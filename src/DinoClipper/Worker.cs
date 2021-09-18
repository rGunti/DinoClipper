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
using PandaDotNet.Cache.Abstraction;
using PandaDotNet.Time;
using PandaDotNet.Utils;

namespace DinoClipper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DinoClipperConfiguration _config;
        private readonly IClipRepository _clipRepository;
        private readonly IClipApi _clipApi;
        private readonly ICache<User, string> _userCache;

        private DateTime? _newestClipFound = null;

        public Worker(
            ILogger<Worker> logger,
            DinoClipperConfiguration config,
            IClipRepository clipRepository,
            IClipApi clipApi,
            ICache<User, string> userCache)
        {
            _logger = logger;
            _config = config;
            _clipRepository = clipRepository;
            _clipApi = clipApi;
            _userCache = userCache;
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

            if (_config.RestoreDateFilter)
            {
                _logger.LogInformation("Restoring date filter from database ...");
                _newestClipFound = _clipRepository.GetDateOfNewestClip(_config.Twitch.ChannelId);
                
                _logger.LogTrace("Restored date filter to {NewClipDate}", _newestClipFound);
            }

            if (_config.RestoreCacheFromDatabase)
            {
                _logger.LogInformation("Restoring caches from database ...");

                _logger.LogDebug("Restoring Users from Clips database ...");
                List<User> users = _clipRepository.All
                    .SelectMany(c => new[] { c.Broadcaster, c.Creator })
                    .Where(u => u != null)
                    .ToList();
                foreach (User user in users)
                {
                    if (!_userCache.IsCached(user.Id))
                    {
                        _logger.LogTrace("Caching user #{UserId} ...", user.Id);
                        _userCache.CacheObject(user, user.Id);
                    }
                    else
                    {
                        _logger.LogTrace("Did not cache user #{UserId} because it is already cached", user.Id);
                    }
                }

                CacheMetrics metrics = _userCache.GetMetrics();
                _logger.LogInformation("Restored {CachedObjects} user(s) from database",
                    metrics.CachedObjects);
            }
        }

        private async Task CheckForClips(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Discovering clips ...");
            List<Clip> clips = (await _clipApi.GetClipsOfBroadcasterAsync(
                _config.Twitch.ChannelId, _newestClipFound))
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

                if (_newestClipFound == null || _newestClipFound < clip.CreatedAt)
                {
                    _logger.LogTrace("Updating date of newest found clip to {NewClipDate}", clip.CreatedAt);
                    _newestClipFound = clip.CreatedAt;
                }
            }
            
            _logger.LogInformation("Discovery completed, found {NewClipCount} new clip(s)",
                newClips);
        }
    }
}