using DinoClipper.Storage;

namespace DinoClipper.Downloader
{
    public class DownloaderChainPayload
    {
        public Clip Clip { get; set; }
        public string WorkingDirectory { get; set; }
        public string DownloadedFile { get; set; }
    }
}