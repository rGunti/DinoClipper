using System;
using System.IO;
using System.Threading.Tasks;
using DinoClipper.Storage;
using Microsoft.Extensions.Logging;
using NYoutubeDL;
using PandaDotNet.Time;

namespace DinoClipper.Downloader.Tasks
{
    public class DownloadClipTask : IDownloaderChainTask
    {
        private readonly ILogger<DownloadClipTask> _logger;
        private readonly YoutubeDL _youtubeDl;

        public DownloadClipTask(
            ILogger<DownloadClipTask> logger,
            YoutubeDL youtubeDl)
        {
            _logger = logger;
            _youtubeDl = youtubeDl;
        }

        public bool CanRun(DownloaderChainPayload payload)
        {
            return payload?.Clip != null;
        }

        public bool Run(DownloaderChainPayload payload)
        {
            Clip clip = payload.Clip;
            string downloadPath = Path.Combine(payload.WorkingDirectory, $"{clip.Id}.mp4");
            _logger.LogInformation("Downloading clip {ClipId} to {ClipPath} ...", clip.Id, downloadPath);
            _youtubeDl.Options.FilesystemOptions.Output = downloadPath;
            try
            {
                Task task = Task.Run(() =>
                {
                    _youtubeDl.Download(clip.Url);
                });
                bool done = task.Wait(3.Minutes());
                if (!done)
                {
                    _logger.LogWarning("Clip {ClipId} download has timed out because " +
                                       "the process did not finish within 3 minutes, will now try to use the file " +
                                       "available",
                        clip.Id);

                    if (!File.Exists(downloadPath))
                    {
                        throw new TimeoutException($"Clip download {clip.Id} did not finish in time");
                    }
                }
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