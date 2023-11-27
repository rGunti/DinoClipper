using System.Threading.Tasks;
using DinoClipper.Downloader;

namespace DinoClipper.ClipStorage;

public interface IClipStorageService
{
    Task<bool> StoreClip(DownloaderChainPayload payload, bool isOriginalClip);
}