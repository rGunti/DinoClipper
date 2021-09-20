using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinoClipper.Config;
using DinoClipper.Downloader;
using DinoClipper.Exceptions;
using DinoClipper.Storage;
using DinoClipper.TwitchApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PandaDotNet.Cache.Abstraction;
using PandaDotNet.ChainProcessing.Abstraction;
using PandaDotNet.Time;
using PandaDotNet.Utils;
using Xabe.FFmpeg;

namespace DinoClipper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DinoClipperConfiguration _config;
        private readonly IClipRepository _clipRepository;
        private readonly IClipApi _clipApi;
        private readonly ICache<User, string> _userCache;
        private readonly ICache<Game, string> _gameCache;
        private readonly ITaskChainProcessor<DownloaderChainPayload> _clipDownloader;

        private DateTime? _newestClipFound = null;

        public Worker(
            ILogger<Worker> logger,
            DinoClipperConfiguration config,
            IClipRepository clipRepository,
            IClipApi clipApi,
            ICache<User, string> userCache,
            ICache<Game, string> gameCache,
            ITaskChainProcessor<DownloaderChainPayload> clipDownloader)
        {
            _logger = logger;
            _config = config;
            _clipRepository = clipRepository;
            _clipApi = clipApi;
            _userCache = userCache;
            _gameCache = gameCache;
            _clipDownloader = clipDownloader;
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
                RestoreUserCache();
                RestoreGameCache();
            }

            if (!string.IsNullOrWhiteSpace(_config.FfmpegPath))
            {
                _logger.LogInformation("Setting ffmpeg Path to {FfmpegPath}", _config.FfmpegPath);
                FFmpeg.SetExecutablesPath(_config.FfmpegPath);
            }

            ClearTempDirectory();
        }

        private void RestoreUserCache()
        {
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

        private void RestoreGameCache()
        {
            _logger.LogDebug("Restoring Games from Clips database ...");
            List<Game> games = _clipRepository.All
                .Select(c => c.Game)
                .Where(g => g != null)
                .ToList();
            foreach (Game game in games)
            {
                if (!_gameCache.IsCached(game.Id))
                {
                    _logger.LogTrace("Caching game #{GameId} ...", game.Id);
                    _gameCache.CacheObject(game, game.Id);
                }
                else
                {
                    _logger.LogTrace("Did not cache game #{GameId} because it is already cached", game.Id);
                }
            }

            CacheMetrics metrics = _gameCache.GetMetrics();
            _logger.LogInformation("Restored {CachedObjects} game(s) from database",
                metrics.CachedObjects);
        }

        private void ClearTempDirectory()
        {
            if (!Directory.Exists(_config.TempStorage))
            {
                return;
            }
            
            _logger.LogInformation("Clearing temp directory ...");
            foreach (string dir in Directory.GetDirectories(_config.TempStorage))
            {
                _logger.LogTrace("Deleting directory {Directory}", dir);
                Directory.Delete(dir, true);
            }

            foreach (string file in Directory.GetFiles(_config.TempStorage))
            {
                _logger.LogTrace("Deleting file {File}", file);
                File.Delete(file);
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

            var newClips = new List<Clip>();
            foreach (Clip clip in clips)
            {
                if (!_clipRepository.ExistsWithId(clip.Id))
                {
                    _logger.LogDebug("Found new clip {ClipId}", clip.Id);

                    bool completed = RunClipProcess(clip, cancellationToken);
                    if (completed)
                    {
                        _logger.LogTrace("Saving clip {ClipId} into database", clip.Id);
                        Clip newClip = _clipRepository.Insert(clip);
                        newClips.Add(newClip);
                    }
                    else if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Cancellation was requested, stopped processing");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to process clip {ClipId}, check logs for errors", clip.Id);
                    }
                }

                if (_newestClipFound == null || _newestClipFound < clip.CreatedAt)
                {
                    _logger.LogTrace("Updating date of newest found clip to {NewClipDate}", clip.CreatedAt);
                    _newestClipFound = clip.CreatedAt;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;
            }
            
            _logger.LogInformation("Discovery completed, found {NewClipCount} new clip(s)",
                newClips.Count);
        }

        private bool RunClipProcess(Clip clip, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            _logger.LogTrace("Start processing clip {ClipId}", clip.Id);
            bool completed = _clipDownloader.Process(new DownloaderChainPayload
            {
                Clip = clip
            });
            if (cancellationToken.IsCancellationRequested)
                return false;

            if (!_config.DownloaderFlags.SkipClearingTempDirectory)
                ClearTempDirectory();

            return completed;
        }
    }
}