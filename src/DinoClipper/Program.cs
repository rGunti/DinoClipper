using System;
using DinoClipper.Config;
using DinoClipper.Exceptions;
using DinoClipper.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PandaDotNet.DI.Configuration;
using PandaDotNet.Repo.Drivers.LiteDB;
using Serilog;

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
                        .AddConfigObject<DinoClipperConfiguration>("DinoClipper")
                        .AddLiteDbDriver(hostContext.Configuration.GetConnectionString("litedb"))
                        .AddSingleton<IClipRepository, ClipRepository>()
                        .AddHostedService<Worker>();
                })
                .UseSerilog((ctx, services, logConfig) => logConfig
                    .ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}"));
    }
}