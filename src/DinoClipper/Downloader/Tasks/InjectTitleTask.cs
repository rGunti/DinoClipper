using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinoClipper.Config;
using DinoClipper.Ffmpeg;
using DinoClipper.Storage;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace DinoClipper.Downloader.Tasks
{
    public class InjectTitleTask : IDownloaderChainAsyncTask
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

        public async Task<bool> Run(DownloaderChainPayload payload)
        {
            string inputFile = payload.DownloadedFile;
            string outputPath = Path.Combine(payload.WorkingDirectory,
                $"{Path.GetFileNameWithoutExtension(payload.DownloadedFile)}.convert.mp4");

            _logger.LogTrace("Generating filter script ...");
            string script = FilterScriptGenerator.RenderFilterScriptToText(
                GenerateScriptForClip(payload.Clip, payload.WorkingDirectory, _config.TitleInjection.Avatar.Add));
            string scriptPath = WriteTextToTempFile(script, payload.WorkingDirectory);

            IConversion conversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{inputFile}\"");
            if (_config.TitleInjection.Avatar.Add)
            {
                _logger.LogTrace("Avatar injection is enabled, adding from source file {AvatarSourceFilePath}",
                    _config.TitleInjection.Avatar.Source);
                conversion = conversion
                    .AddParameter($"-i \"{_config.TitleInjection.Avatar.Source}\"");
            }
            conversion = conversion
                .AddParameter($"-filter_complex_script \"{scriptPath}\"")
                .SetOverwriteOutput(true)
                .SetOutput(outputPath);
            conversion.OnProgress += ConversionOnOnProgress;

            try
            {
                _logger.LogDebug("Starting ffmpeg with parameters {FfmpegParameters}",
                    conversion.Build());
                IConversionResult result = await conversion.Start();
                _logger.LogInformation("Title injection completed, took {ConversionTime}",
                    result.EndTime - result.StartTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process clip {ClipId} due to exception",
                    payload.Clip.Id);
                return false;
            }

            payload.DownloadedFile = outputPath;
            return true;
        }

        private IEnumerable<FilterDefinition> GenerateScriptForClip(Clip clip, string workingDirectory, bool includeAvatar = false)
        {
            const string BUFFER = "clip";
            const string AVATAR_BUFFER = "avatar";
            const string FONT_COLOR = "lime";

            // Buffer In
            yield return new FilterDefinition("null", "0:v", BUFFER);

            // Rescale to 1280x720
            yield return new FilterDefinition("scale", BUFFER, BUFFER)
                .AddParam("size", "hd720");

            // Box
            yield return new FilterDefinition("drawbox", BUFFER, BUFFER)
                .AddParam("x", 0)
                .AddParam("y", 0)
                .AddParam("w", "in_w")
                .AddParam("h", 35)
                .AddParam("c", "black@0.6")
                .AddParam("t", "fill");

            // "clipped at"
            yield return new FilterDefinition("drawtext", BUFFER, BUFFER)
                .AddParam("x", 5)
                .AddParam("y", 10)
                .AddParam("fontsize", 20)
                .AddParam("fontcolor", FONT_COLOR)
                .AddParam("text", "'clipped at'");
            // Clip Date
            yield return new FilterDefinition("drawtext", BUFFER, BUFFER)
                .AddParam("x", 110)
                .AddParam("y", 7)
                .AddParam("fontsize", 24)
                .AddParam("fontcolor", FONT_COLOR)
                .AddFileParam("textfile", $"{clip.CreatedAt:yyyy-MM-dd}", workingDirectory);

            // "by"
            yield return new FilterDefinition("drawtext", BUFFER, BUFFER)
                .AddParam("x", 255)
                .AddParam("y", 10)
                .AddParam("fontsize", 20)
                .AddParam("fontcolor", FONT_COLOR)
                .AddParam("text", "'by'");
            // Clip Creator
            yield return new FilterDefinition("drawtext", BUFFER, BUFFER)
                .AddParam("x", 285)
                .AddParam("y", 7)
                .AddParam("fontsize", 24)
                .AddParam("fontcolor", FONT_COLOR)
                .AddFileParam("textfile", clip.Creator?.Name ?? "somebody we used to know", workingDirectory);

            // Game Title
            yield return new FilterDefinition("drawtext", BUFFER, BUFFER)
                .AddParam("x", "w-text_w-5")
                .AddParam("y", 7)
                .AddParam("fontsize", 24)
                .AddParam("fontcolor", FONT_COLOR)
                .AddFileParam("textfile", clip.Game?.Name ?? "some game that we used to know", workingDirectory);
            
            // (optional): Avatar
            if (includeAvatar)
            {
                // Load avatar into separate buffer
                yield return new FilterDefinition("null", "1:v", AVATAR_BUFFER);
                // Scale buffer to desired size
                yield return new FilterDefinition("scale", AVATAR_BUFFER, AVATAR_BUFFER)
                    .AddParam("w", 35)
                    .AddParam("h", 35);
                // Overlay avatar on video buffer
                yield return new FilterDefinition("overlay", new []{ BUFFER, AVATAR_BUFFER }, BUFFER)
                    .AddParam("x", "(main_w/2)-(overlay_w/2)")
                    .AddParam("y", 0);
            }

            // Buffer Out
            yield return new FilterDefinition("null", BUFFER);
        }

        private void ConversionOnOnProgress(object sender, ConversionProgressEventArgs args)
        {
            _logger.LogDebug("[PID={PID}] Conversion in progress; {Percent}%; Position: {Duration}/{TotalLength}",
                args.ProcessId, args.Percent, args.Duration, args.TotalLength);
        }

        private string WriteTextToTempFile(string textToDraw, string workingDirectory)
        {
            string newPath = Path.Combine(workingDirectory, $"generated.{Guid.NewGuid()}.txt");
            File.WriteAllText(newPath, textToDraw);
            return newPath;
        }
    }
}