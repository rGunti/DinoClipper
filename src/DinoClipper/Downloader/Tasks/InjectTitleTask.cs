using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinoClipper.Config;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace DinoClipper.Downloader.Tasks
{
    public class InjectTitleTask : IDownloaderChainTask
    {
        private readonly ILogger<InjectTitleTask> _logger;
        private readonly DinoClipperConfiguration _config;

        public InjectTitleTask(
            ILogger<InjectTitleTask> logger,
            DinoClipperConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public bool CanRun(DownloaderChainPayload payload)
        {
            return payload?.Clip != null && !string.IsNullOrWhiteSpace(payload.DownloadedFile);
        }

        public bool Run(DownloaderChainPayload payload)
        {
            string inputFile = payload.DownloadedFile;
            string outputPath = Path.Combine(_config.TempStorage,
                $"{Path.GetFileNameWithoutExtension(payload.DownloadedFile)}.convert.mp4");

            _logger.LogTrace("Getting Media Info from {InputFilePath}", inputFile);
            Task<IMediaInfo> mediaInfoTask = FFmpeg.GetMediaInfo(payload.DownloadedFile);
            while (!mediaInfoTask.IsCompleted)
            {
                Thread.Sleep(500);
            }

            if (mediaInfoTask.Exception != null)
            {
                throw mediaInfoTask.Exception;
            }

            IMediaInfo mediaInfo = mediaInfoTask.Result;

            IVideoStream videoStream = mediaInfo.VideoStreams.First().SetCodec(VideoCodec.h264);
            IAudioStream audioStream = mediaInfo.AudioStreams.First().SetCodec(AudioCodec.aac);

            string textInjectionFile = WriteTextToDrawToTempFile($"clipped at {payload.Clip.CreatedAt:yyyy-MM-dd} by {payload.Clip.Creator?.Name ?? "somebody that we used to know"}");

            IConversion conversion = FFmpeg.Conversions.New()
                .AddStream(videoStream)
                .AddStream(audioStream)
                .SetOutput(outputPath)
                .AddParameter($"-vf \"drawtext=textfile='{textInjectionFile}':x=0:y=0:fontsize=24:fontcolor=white:box=1:boxcolor=black@0.6\"");
            conversion.OnProgress += ConversionOnOnProgress;

            IConversionResult response = conversion.Start().RunSync();

            payload.DownloadedFile = outputPath;
            return true;
        }

        private void ConversionOnOnProgress(object sender, ConversionProgressEventArgs args)
        {
            _logger.LogDebug("[PID={PID}] Conversion in progress; {Percent}%; Position: {Duration}/{TotalLength}",
                args.ProcessId, args.Percent, args.Duration, args.TotalLength);
        }

        private string WriteTextToDrawToTempFile(string textToDraw)
        {
            string newPath = Path.Combine(_config.TempStorage, $"generated_text.{Guid.NewGuid()}.txt");
            File.WriteAllText(newPath, textToDraw);
            return newPath;
        }
    }
}