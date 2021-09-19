using System;
using System.Net;
using DinoClipper.Config;
using DinoClipper.Downloader;
using DinoClipper.Exceptions;
using DinoClipper.Storage;
using DinoClipper.TwitchApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NYoutubeDL;
using PandaDotNet.Cache.Abstraction;
using PandaDotNet.Cache.ExpiringCache;
using PandaDotNet.ChainProcessing.Abstraction;
using PandaDotNet.DI.Configuration;
using PandaDotNet.Repo.Drivers.LiteDB;
using PandaDotNet.Time;
using Serilog;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Interfaces;
using WebDav;

namespace DinoClipper
{
    public class Program
    {
        public static int Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                host.Run();
            }
            catch (DinoClipperException ex)
            {
                logger.LogCritical(ex,
                    "Encountered unhandled exception with Exit Code 0x{ExitCode:x3}! Application must terminate!",
                    ex.ExitCode);
#if DEBUG
                // If we run in a debugger, rethrow it
                throw;
#endif
                return ex.ExitCode;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Encountered unhandled exception");
#if DEBUG
                // If we run in a debugger, rethrow it
                throw;
#endif
                return -1;
            }

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(l => l.AddSerilog())
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        // Configuration
                        .AddConfigObject<DinoClipperConfiguration>("DinoClipper")
                        .AddSingleton<WebDavConfig>(s => s.GetAppConfig().UploadTarget)
                        .AddSingleton<TwitchConfig>(s => s.GetAppConfig().Twitch)
                        // Storage
                        .AddLiteDbDriver(hostContext.Configuration.GetConnectionString("litedb"))
                        .AddSingleton<IClipRepository, ClipRepository>()
                        // Cache
                        .AddSingleton<IClock, Clock>()
                        .AddSingleton<ICache<User, string>, ExpiringMemoryCache<User, string>>(s =>
                            new ExpiringMemoryCache<User, string>(
                                TimeSpan.FromMinutes(s.GetAppConfig().MaxCacheAge),
                                s.GetRequiredService<IClock>()))
                        // Twitch
                        .AddSingleton<ITwitchAPI>(s =>
                        {
                            var cfg = s.GetRequiredService<TwitchConfig>();
                            return new TwitchAPI(settings: new ApiSettings
                            {
                                ClientId = cfg?.ClientId,
                                Secret = cfg?.ClientSecret
                            });
                        })
                        // YoutubeDL
                        .AddTransient(s =>
                        {
                            var logger = s.GetRequiredService<ILogger<YoutubeDL>>();
                            var inst = new YoutubeDL(s.GetAppConfig().YouTubeDlPath);
                            inst.StandardOutputEvent += (_, e) =>
                            {
                                logger.LogDebug("ytdl stdout: {Stdout}", e);
                            };
                            inst.StandardErrorEvent += (_, e) =>
                            {
                                logger.LogDebug("ytdl stderr: {Stderr}", e);
                            };
                            return inst;
                        })
                        // WebDav
                        .AddTransient<WebDavClientParams>(s =>
                        {
                            var cfg = s.GetRequiredService<WebDavConfig>();
                            var inst = new WebDavClientParams
                            {
                                BaseAddress = new Uri(cfg.Url),
                                Credentials = new NetworkCredential(cfg.Username, cfg.Password)
                            };
                            return inst;
                        })
                        .AddTransient<IWebDavClient>(s =>
                            new WebDavClient(s.GetRequiredService<WebDavClientParams>()))
                        // APIs
                        .AddSingleton<IUserApi, UserApi>()
                        .AddSingleton<IClipApi, ClipApi>()
                        // Chains
                        .AddSingleton<IDownloaderChain, DownloaderChain>()
                        .AddSingleton<ITaskChainProcessor<DownloaderChainPayload>>(s =>
                        {
                            var logger = s.GetRequiredService<ILogger<YoutubeDL>>();
                            var inst = new DownloaderProcessor(
                                s.GetRequiredService<IDownloaderChain>());
                            inst.OnChainAborted += (_, e) =>
                            {
                                logger.LogInformation("TRANS={TransactionId}: Chain was aborted at task {Task}",
                                    e.TransactionId, e.CurrentTask);
                            };
                            inst.OnChainCompleted += (_, e) =>
                            {
                                logger.LogDebug("TRANS={TransactionId}: Chain was completed",
                                    e.TransactionId);
                            };
                            inst.OnTaskSkipped += (_, e) =>
                            {
                                logger.LogInformation("TRANS={TransactionId}: Task {Task} has been skipped",
                                    e.TransactionId, e.CurrentTask);
                            };
                            return inst;
                        })
                        // Worker
                        .AddHostedService<Worker>();
                })
                .UseSerilog((ctx, services, logConfig) => logConfig
                    .ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}"));
    }
}