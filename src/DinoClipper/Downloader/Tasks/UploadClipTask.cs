using System;
using System.IO;
using DinoClipper.Config;
using DinoClipper.Storage;
using Microsoft.Extensions.Logging;
using WebDav;

namespace DinoClipper.Downloader.Tasks
{
    public class UploadClipTask : IDownloaderChainTask
    {
        private readonly ILogger<UploadClipTask> _logger;
        private readonly IWebDavClient _webDavClient;
        private readonly WebDavConfig _webDavConfig;
        private readonly Uri _baseUrl;

        public UploadClipTask(
            ILogger<UploadClipTask> logger,
            IWebDavClient webDavClient,
            WebDavConfig webDavConfig)
        {
            _logger = logger;
            _webDavClient = webDavClient;
            _webDavConfig = webDavConfig;
            _baseUrl = new Uri(_webDavConfig.Url);
        }

        public bool CanRun(DownloaderChainPayload payload)
        {
            return payload?.Clip != null && !string.IsNullOrWhiteSpace(payload.DownloadedFile);
        }

        public bool Run(DownloaderChainPayload payload)
        {
            Clip clip = payload.Clip;
            _logger.LogDebug("Uploading clip {ClipId} from {ClipPath} to WebDav share ...",
                clip.Id, payload.DownloadedFile);

            string originalExtension = Path.GetExtension(payload.DownloadedFile);
            string fileName = $"{clip.CreatedAt:yyyy-MM-dd}_{clip.Creator.Name}_{clip.Name}.{originalExtension}".MakeSafe();
            var uploadUrls = new Uri(_baseUrl, fileName);
            _logger.LogTrace("Uploading to url {UploadUrl} ...", uploadUrls);

            using var fs = new FileStream(payload.DownloadedFile!, FileMode.Open);
            WebDavResponse response = _webDavClient.PutFile(uploadUrls, fs).GetAwaiter().GetResult();

            _logger.LogTrace("Request completed with response code {WebDavResponseCode} {WebDavResponse}",
                response.StatusCode, response.Description);
            if (response.IsSuccessful)
            {
                _logger.LogInformation("Clip {ClipId} ({FileName}) upload",
                    payload.Clip.Id, fileName);
            }
            else
            {
                _logger.LogError("Upload failed with error code {WebDavErrorCode}: {WebDavErrorDescription}",
                    response.StatusCode, response.Description);
            }
            return response.IsSuccessful;
        }
    }
}