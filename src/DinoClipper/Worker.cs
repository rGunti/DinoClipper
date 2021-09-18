using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinoClipper.Config;
using DinoClipper.Exceptions;
using DinoClipper.Storage;
using DinoClipper.TwitchApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PandaDotNet.Utils;

namespace DinoClipper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DinoClipperConfiguration _config;
        private readonly IClipRepository _clipRepository;
        private readonly IClipApi _clipApi;

        private DateTime? _newestClipFound = null;

        public Worker(
            ILogger<Worker> logger,
            DinoClipperConfiguration config,
            IClipRepository clipRepository,
            IClipApi clipApi)
        {
            _logger = logger;
            _config = config;
            _clipRepository = clipRepository;
            _clipApi = clipApi;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ValidateConfiguration();
            PrepareEnvironment();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
                await CheckForClips(stoppingToken);
                
                _logger.LogInformation(
                    "Done working, now going to sleep for {SleepInterval} seconds",
                    _config.SleepInterval);
                await Task.Delay(_config.SleepInterval * 1000, stoppingToken);
            }
        }

        private void ValidateConfiguration()
        {
            _logger.LogTrace("Checking configuration data ...");
            if (string.IsNullOrWhiteSpace(_config.Twitch?.ChannelId))
            {
                throw new InitializationException("No Twitch Channel ID provided!");
            }

            if (string.IsNullOrWhiteSpace(_config.Twitch?.ClientId)
                || string.IsNullOrWhiteSpace(_config.Twitch?.ClientSecret))
            {
                throw new InitializationException("No Twitch API credentials provided!");
            }
        }

        private void PrepareEnvironment()
        {
            _logger.LogDebug("Ensuring Temp directory {TempDir} exists", _config.TempStorage);
            _config.TempStorage?.EnsureDirectoryExists();
        }

        private async Task CheckForClips(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discovering clips ...");
            List<Clip> clips = (await _clipApi.GetClipsOfBroadcasterAsync(
                _config.Twitch.ChannelId))
                .ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            int newClips = 0;
            foreach (Clip clip in clips)
            {
                if (!_clipRepository.ExistsWithId(clip.Id))
                {
                    _logger.LogDebug("Found new clip {ClipId}", clip.Id);
                    _clipRepository.Insert(clip);
                    newClips++;
                }
            }
            
            _logger.LogInformation("Discovery completed, found {NewClipCount} new clip(s)",
                newClips);
        }
    }
}