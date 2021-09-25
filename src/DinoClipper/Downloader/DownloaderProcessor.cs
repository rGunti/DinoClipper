using PandaDotNet.ChainProcessing;

namespace DinoClipper.Downloader
{
    public class DownloaderProcessor : DefaultAsyncTaskChainProcessor<DownloaderChainPayload>
    {
        public DownloaderProcessor(IDownloaderChain chain) : base(chain)
        {
        }

        protected override string GenerateTransactionId(DownloaderChainPayload payload)
        {
            return payload.Clip?.Id;
        }
    }
}