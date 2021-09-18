using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Interfaces;
using Clip = DinoClipper.Storage.Clip;

namespace DinoClipper.TwitchApi
{
    public interface IClipApi
    {
        public Task<IEnumerable<Clip>> GetClipsOfBroadcasterAsync(string broadcasterId, DateTime? startedAt = null);
    }
    
    public class ClipApi : IClipApi
    {
        private readonly ILogger<ClipApi> _logger;
        private readonly ITwitchAPI _twitchAPI;
        private readonly IUserApi _userApi;

        public ClipApi(
            ILogger<ClipApi> logger,
            ITwitchAPI twitchAPI,
            IUserApi userApi)
        {
            _logger = logger;
            _twitchAPI = twitchAPI;
            _userApi = userApi;
        }
        
        
        public async Task<IEnumerable<Clip>> GetClipsOfBroadcasterAsync(string broadcasterId, DateTime? startedAt = null)
        {
            var twitchClips = new List<TwitchLib.Api.Helix.Models.Clips.GetClips.Clip>();
            
            GetClipsResponse clipsResponse = null;
            do
            {
                _logger.LogDebug(
                    "Retrieving clips for channel {ChannelId} (currently having {ClipCount}, should be created after {StartedAt}) ...",
                    broadcasterId, twitchClips.Count, startedAt);

                clipsResponse = await GetClips(broadcasterId, clipsResponse?.Pagination.Cursor, startedAt);
                _logger.LogTrace("{ChannelId}: Got response, new cursor is {Cursor}",
                    broadcasterId, clipsResponse.Pagination.Cursor);
                twitchClips.AddRange(clipsResponse.Clips);
                await Task.Delay(1000);
            } while (clipsResponse.Clips.Any() && clipsResponse.Pagination.Cursor != null);

            return await ConvertToClips(twitchClips);
        }

        private async Task<GetClipsResponse> GetClips(string broadcasterId, string cursor, DateTime? startedAt)
        {
            _logger.LogTrace("Querying clips from Twitch for Broadcaster #{Broadcaster} with Cursor {Cursor} and time {StartedAt} ...",
                broadcasterId, cursor, startedAt?.ToUniversalTime());
            return await _twitchAPI.Helix.Clips.GetClipsAsync(
                broadcasterId: broadcasterId,
                after: cursor,
                first: 50,
                startedAt: startedAt?.ToUniversalTime(),
                endedAt: startedAt.HasValue ? DateTime.UtcNow : null);
        }

        private async Task<IEnumerable<Clip>> ConvertToClips(IEnumerable<TwitchLib.Api.Helix.Models.Clips.GetClips.Clip> clips)
        {
            var c = new List<Clip>();
            foreach (var clip in clips)
                c.Add(await ConvertToClip(clip));
            return c;
        }
        
        private async Task<Clip> ConvertToClip(TwitchLib.Api.Helix.Models.Clips.GetClips.Clip clip) =>
            new()
            {
                Id = clip.Id,
                Name = clip.Title,
                Views = clip.ViewCount,
                Creator = await _userApi.GetUserByIdAsync(clip.CreatorId),
                Broadcaster = await _userApi.GetUserByIdAsync(clip.BroadcasterId),
                Duration = clip.Duration,
                Language = clip.Language,
                Url = clip.Url,
                CreatedAt = DateTime.Parse(clip.CreatedAt)
            };

    }
}