using System;
using System.Collections.Generic;
using DinoClipper.ClipStorage;
using DinoClipper.Config;
using DinoClipper.Downloader.Tasks;
using DinoClipper.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NYoutubeDL;
using PandaDotNet.ChainProcessing.Abstraction;

namespace DinoClipper.Downloader;

public interface IDownloaderChain : ITaskChain<DownloaderChainPayload>;
public interface IDownloaderChainTaskBase : ITaskBase<DownloaderChainPayload>;
public interface IDownloaderChainTask : IDownloaderChainTaskBase, ITask<DownloaderChainPayload>;
public interface IDownloaderChainAsyncTask : IDownloaderChainTaskBase, IAsyncTask<DownloaderChainPayload>;

public class DownloaderChain : IDownloaderChain
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DownloaderChain> _logger;
    private readonly DinoClipperConfiguration _config;
    private readonly DownloaderFlags _downloaderFlags;

    public DownloaderChain(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<DownloaderChain>>();
        _config = _serviceProvider.GetAppConfig();
        _downloaderFlags = _config.DownloaderFlags;
    }

    public IEnumerable<ITaskBase<DownloaderChainPayload>> GetTasks()
    {
        _logger.LogDebug("Creating new chain");
        yield return new DownloadClipTask(
            _serviceProvider.GetRequiredService<ILogger<DownloadClipTask>>(),
            _serviceProvider.GetRequiredService<YoutubeDL>());

        if (_downloaderFlags.UploadOriginal && !_downloaderFlags.SkipUpload)
        {
            yield return new UploadClipTask(
                _serviceProvider.GetRequiredService<IClipStorageService>(),
                true);
        }
        else
        {
            _logger.LogTrace(
                "Skipped uploading original clip because either {UploadOriginalFlag} was disabled " +
                "(enabled={UploadOriginalEnabled}) or {SkipUploadFlag} was enabled (enabled={SkipUploadEnabled})",
                nameof(_downloaderFlags.UploadOriginal), _downloaderFlags.UploadOriginal,
                nameof(_downloaderFlags.SkipUpload), _downloaderFlags.SkipUpload);
        }

        if (!_config.TitleInjection.Enabled)
        {
            _logger.LogTrace("Skipped title injection because it was disabled in the app configuration");
        }
        else
        {
            yield return new InjectTitleTask(
                _serviceProvider.GetRequiredService<ILogger<InjectTitleTask>>(),
                _serviceProvider.GetAppConfig());
        }

        if (_downloaderFlags.SkipUpload)
        {
            _logger.LogTrace("Skipped upload tasks because {SkipUploadProperty} is enabled",
                nameof(_downloaderFlags.SkipUpload));
        }
        else
        {
            yield return new UploadClipTask(
                _serviceProvider.GetRequiredService<IClipStorageService>());
        }

        yield return new SaveClipTask(_serviceProvider.GetRequiredService<IClipRepository>());
    }
}