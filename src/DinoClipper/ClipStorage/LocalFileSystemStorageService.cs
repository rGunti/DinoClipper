using System;
using System.IO;
using System.Threading.Tasks;
using DinoClipper.Downloader;
using Microsoft.Extensions.Logging;

namespace DinoClipper.ClipStorage;

public class LocalFileSystemStorageService : IClipStorageService
{
    private readonly ILogger<LocalFileSystemStorageService> _logger;
    private readonly string _storageDir;

    public LocalFileSystemStorageService(ILogger<LocalFileSystemStorageService> logger, string storageDir)
    {
        _logger = logger;
        _storageDir = Path.GetFullPath(storageDir);
        if (!Directory.Exists(_storageDir))
        {
            _logger.LogInformation("Creating output directory {OutputDir} ...", _storageDir);
            Directory.CreateDirectory(_storageDir);
        }
    }

    public Task<bool> StoreClip(DownloaderChainPayload payload, bool isOriginalClip)
    {
        var clip = payload.Clip;
        var originalExtension = Path.GetExtension(payload.DownloadedFile)!.Replace(".", "");
        var storageDir = Path.Combine(_storageDir, isOriginalClip ? "original" : "processed");
        var fileName = $"{clip.CreatedAt:yyyy-MM-dd}_{clip.Creator?.Name ?? "UnknownUser"}_{clip.Id}.{originalExtension}".MakeSafe();
        var destinationFilePath = Path.Combine(storageDir, fileName);
        
        try
        {
            _logger.LogDebug("Storing clip {ClipId} from {ClipPath} to local file system ...",
                clip.Id, payload.DownloadedFile);
            _logger.LogTrace("Storing at path {FilePath} ...", destinationFilePath);

            var parentDir = Path.GetDirectoryName(destinationFilePath);
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir!);
            }
            
            // Copy the file to destination path asynchronously
            // Replace with Move if you want to transfer the file instead of creating a copy
            File.Copy(payload.DownloadedFile!, destinationFilePath, true);

            _logger.LogInformation("Clip {ClipId} ({FileName}) stored",
                payload.Clip.Id, fileName);
        
            // Indicate a successful operation since there was no exception thrown within the try block
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storing file has failed with an exception");
            return Task.FromResult(false);
        }
    }
}