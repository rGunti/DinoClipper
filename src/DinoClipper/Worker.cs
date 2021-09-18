using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinoClipper.Config;
using DinoClipper.Exceptions;
using DinoClipper.Storage;
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

        public Worker(ILogger<Worker> logger, DinoClipperConfiguration config, IClipRepository clipRepository)
        {
            _logger = logger;
            _config = config;
            _clipRepository = clipRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ValidateConfiguration();
            PrepareEnvironment();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
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
    }
}