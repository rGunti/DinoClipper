using DinoClipper.Storage;

namespace DinoClipper.Downloader.Tasks
{
    public class SaveClipTask : IDownloaderChainTask
    {
        private readonly IClipRepository _clipRepository;

        public SaveClipTask(IClipRepository clipRepository)
        {
            _clipRepository = clipRepository;
        }

        public bool CanRun(DownloaderChainPayload payload)
        {
            return payload.Clip != null;
        }

        public bool Run(DownloaderChainPayload payload)
        {
            payload.Clip = _clipRepository.Insert(payload.Clip);
            return true;
        }
    }
}