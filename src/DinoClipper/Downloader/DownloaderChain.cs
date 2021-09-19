using System;
using System.Collections.Generic;
using DinoClipper.Config;
using DinoClipper.Downloader.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NYoutubeDL;
using PandaDotNet.ChainProcessing.Abstraction;
using WebDav;

namespace DinoClipper.Downloader
{
    public interface IDownloaderChain : ITaskChain<DownloaderChainPayload> { }
    public interface IDownloaderChainTask : ITask<DownloaderChainPayload> { }

    public class DownloaderChain : IDownloaderChain
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DownloaderChain> _logger;
        private readonly DownloaderFlags _downloaderFlags;

        public DownloaderChain(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<DownloaderChain>>();
            _downloaderFlags = _serviceProvider.GetAppConfig().DownloaderFlags;
        }

        public IEnumerable<ITask<DownloaderChainPayload>> GetTasks()
        {
            _logger.LogDebug("Creating new chain");
            yield return new DownloadClipTask(
                _serviceProvider.GetRequiredService<ILogger<DownloadClipTask>>(),
                _serviceProvider.GetRequiredService<YoutubeDL>(),
                _serviceProvider.GetAppConfig());

            if (_downloaderFlags.SkipUpload)
            {
                _logger.LogTrace("Skipped upload tasks because {SkipUploadProperty} is enabled",
                    nameof(_downloaderFlags.SkipUpload));
            }
            else
            {
                yield return new UploadClipTask(
                    _serviceProvider.GetRequiredService<ILogger<UploadClipTask>>(),
                    _serviceProvider.GetRequiredService<IWebDavClient>(),
                    _serviceProvider.GetRequiredService<WebDavConfig>());
            }
        }
    }
}