using System;
using System.IO;
using System.Threading.Tasks;
using DinoClipper.Config;
using DinoClipper.Downloader;
using Microsoft.Extensions.Logging;
using WebDav;

namespace DinoClipper.ClipStorage;

public class WebDavClipStorageService : IClipStorageService
{
    private readonly ILogger<WebDavClipStorageService> _logger;
    private readonly Uri _baseUrl;
    private readonly IWebDavClient _webDavClient;

    public WebDavClipStorageService(
        ILogger<WebDavClipStorageService> logger,
        IWebDavClient webDavClient,
        Uri baseUrl)
    {
        _logger = logger;
        _webDavClient = webDavClient;
        _baseUrl = baseUrl;
    }

    public async Task<bool> StoreClip(DownloaderChainPayload payload, bool isOriginalClip)
    {
        var clip = payload.Clip;
        var originalExtension = Path.GetExtension(payload.DownloadedFile);
        var uploadDir = isOriginalClip ? "original" : "processed";
        var fileName = $"{clip.CreatedAt:yyyy-MM-dd}_{clip.Creator?.Name ?? "UnknownUser"}_{clip.Id}.{originalExtension}".MakeSafe();
        var uploadUrls = new Uri(_baseUrl, $"{uploadDir}/{fileName}");

        try
        {
            _logger.LogDebug("Uploading clip {ClipId} from {ClipPath} to WebDav share ...",
                clip.Id, payload.DownloadedFile);
            _logger.LogTrace("Uploading to url {UploadUrl} ...", uploadUrls);

            await using var fs = new FileStream(payload.DownloadedFile!, FileMode.Open);
            var response = await _webDavClient.PutFile(uploadUrls, fs);

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