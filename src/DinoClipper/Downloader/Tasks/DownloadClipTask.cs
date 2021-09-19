using System;
using System.IO;
using DinoClipper.Config;
using DinoClipper.Storage;
using Microsoft.Extensions.Logging;
using NYoutubeDL;

namespace DinoClipper.Downloader.Tasks
{
    public class DownloadClipTask : IDownloaderChainTask
    {
        private readonly ILogger<DownloadClipTask> _logger;
        private readonly YoutubeDL _youtubeDl;
        private readonly DinoClipperConfiguration _config;

        public DownloadClipTask(
            ILogger<DownloadClipTask> logger,
            YoutubeDL youtubeDl,
            DinoClipperConfiguration config)
        {
            _logger = logger;
            _youtubeDl = youtubeDl;
            _config = config;
        }

        public bool CanRun(DownloaderChainPayload payload)
        {
            return payload?.Clip != null;
        }

        public bool Run(DownloaderChainPayload payload)
        {
            Clip clip = payload.Clip;
            string downloadPath = Path.Combine(_config.TempStorage, $"{clip.Id}.mp4");
            _logger.LogInformation("Downloading clip {ClipId} to {ClipPath} ...", clip.Id, downloadPath);
            _youtubeDl.Options.FilesystemOptions.Output = downloadPath;
            try
            {
                _youtubeDl.Download(clip.Url);
                payload.DownloadedFile = downloadPath;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Download of clip {ClipId} failed due to an exception",
                    clip.Id);
                return false;
            }
        }
    }
}