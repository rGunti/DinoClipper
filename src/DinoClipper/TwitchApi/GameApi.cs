using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PandaDotNet.Cache.Abstraction;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Interfaces;
using Game = DinoClipper.Storage.Game;

namespace DinoClipper.TwitchApi
{
    public interface IGameApi
    {
        Task<Game> GetGameByIdAsync(string gameId);
    }

    public class GameApi : IGameApi
    {
        private readonly ILogger<GameApi> _logger;
        private readonly ITwitchAPI _twitchAPI;
        private readonly ICache<Game, string> _cache;

        public GameApi(
            ILogger<GameApi> logger,
            ITwitchAPI twitchAPI,
            ICache<Game, string> cache)
        {
            _logger = logger;
            _twitchAPI = twitchAPI;
            _cache = cache;
        }

        public async Task<Game> GetGameByIdAsync(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId))
                return default;
            return await _cache.GetObjectForKeyAsync(gameId, GetGameByIdFromApi);
        }

        private async Task<Game> GetGameByIdFromApi(string gameId)
        {
            _logger.LogTrace("Getting game #{TwitchGameId} from Twitch API", gameId);
            GetGamesResponse gameResponse = await _twitchAPI.Helix.Games.GetGamesAsync(new List<string> { gameId });
            TwitchLib.Api.Helix.Models.Games.Game game = gameResponse.Games.FirstOrDefault();
            return game != null ? ConvertToGame(game) : null;
        }

        private Game ConvertToGame([NotNull] TwitchLib.Api.Helix.Models.Games.Game game) =>
            new()
            {
                Id = $"{game.Id}",
                Name = game.Name
            };
    }
}