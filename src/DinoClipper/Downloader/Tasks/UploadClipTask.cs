using System;
using System.IO;
using System.Threading.Tasks;
using DinoClipper.Config;
using DinoClipper.Storage;
using Microsoft.Extensions.Logging;
using WebDav;

namespace DinoClipper.Downloader.Tasks
{
    public class UploadClipTask : IDownloaderChainAsyncTask
    {
        private readonly ILogger<UploadClipTask> _logger;
        private readonly IWebDavClient _webDavClient;
        private readonly WebDavConfig _webDavConfig;
        private readonly Uri _baseUrl;
        private readonly bool _isOriginalClip;

        public UploadClipTask(
            ILogger<UploadClipTask> logger,
            IWebDavClient webDavClient,
            WebDavConfig webDavConfig,
            bool isOriginalClip = false)
        {
            _logger = logger;
            _webDavClient = webDavClient;
            _webDavConfig = webDavConfig;
            _baseUrl = new Uri(_webDavConfig.Url);
            _isOriginalClip = isOriginalClip;
        }

        public bool CanRun(DownloaderChainPayload payload)
        {
            return payload?.Clip != null && !string.IsNullOrWhiteSpace(payload.DownloadedFile);
        }

        public async Task<bool> Run(DownloaderChainPayload payload)
        {
            Clip clip = payload.Clip;
            string originalExtension = Path.GetExtension(payload.DownloadedFile);
            string uploadDir = _isOriginalClip ? "original" : "processed";
            string fileName = $"{clip.CreatedAt:yyyy-MM-dd}_{clip.Creator?.Name ?? "UnknownUser"}_{clip.Name}.{originalExtension}".MakeSafe();
            var uploadUrls = new Uri(_baseUrl, $"{uploadDir}/{fileName}");

            try
            {
                _logger.LogDebug("Uploading clip {ClipId} from {ClipPath} to WebDav share ...",
                    clip.Id, payload.DownloadedFile);
                _logger.LogTrace("Uploading to url {UploadUrl} ...", uploadUrls);

                using var fs = new FileStream(payload.DownloadedFile!, FileMode.Open);
                WebDavResponse response = await _webDavClient.PutFile(uploadUrls, fs);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload has failed with an exception");
                return false;
            }
        }
    }
}