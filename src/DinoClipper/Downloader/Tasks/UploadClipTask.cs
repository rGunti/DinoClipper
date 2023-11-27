using System.Threading.Tasks;
using DinoClipper.ClipStorage;

namespace DinoClipper.Downloader.Tasks
{
    public class UploadClipTask : IDownloaderChainAsyncTask
    {
        private readonly IClipStorageService _clipStorageService;
        private readonly bool _isOriginalClip;

        public UploadClipTask(IClipStorageService clipStorageService, bool isOriginalClip = false)
        {
            _clipStorageService = clipStorageService;
            _isOriginalClip = isOriginalClip;
        }

        public bool CanRun(DownloaderChainPayload payload)
        {
            return payload?.Clip != null && !string.IsNullOrWhiteSpace(payload.DownloadedFile);
        }

        public Task<bool> Run(DownloaderChainPayload payload)
        {
            return _clipStorageService.StoreClip(payload, _isOriginalClip);
        }
    }
}