using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PandaDotNet.Cache.Abstraction;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Interfaces;
using User = DinoClipper.Storage.User;

namespace DinoClipper.TwitchApi
{
    public interface IUserApi
    {
        public Task<User> GetUserByUsernameAsync(string username);
        public Task<User> GetUserByIdAsync(string userId);
    }

    public class UserApi : IUserApi
    {
        private readonly ILogger<UserApi> _logger;
        private readonly ITwitchAPI _twitchAPI;
        private readonly ICache<User, string> _cache;

        public UserApi(
            ILogger<UserApi> logger,
            ITwitchAPI twitchAPI,
            ICache<User, string> cache)
        {
            _logger = logger;
            _twitchAPI = twitchAPI;
            _cache = cache;
        }
        
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _cache.GetObjectForKeyAsync(username, GetUserByUsernameFromApi);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _cache.GetObjectForKeyAsync(userId, GetUserByIdFromApi);
        }

        private async Task<User> GetUserByUsernameFromApi(string username)
        {
            _logger.LogTrace("Fetching user {TwitchUsername} by username from Twitch API", username);
            GetUsersResponse users = await _twitchAPI.Helix.Users.GetUsersAsync(
                logins: new List<string> { username });
            TwitchLib.Api.Helix.Models.Users.GetUsers.User user = users.Users.FirstOrDefault();
            return ConvertToUser(user);
        }

        private async Task<User> GetUserByIdFromApi(string userId)
        {
            _logger.LogTrace("Fetching user #{TwitchUserId} from Twitch API", userId);
            GetUsersResponse users = await _twitchAPI.Helix.Users.GetUsersAsync(
                ids: new List<string> { userId });
            TwitchLib.Api.Helix.Models.Users.GetUsers.User user = users.Users.FirstOrDefault();
            return user != null ? ConvertToUser(user) : null;
        }

        private User ConvertToUser(TwitchLib.Api.Helix.Models.Users.GetUsers.User user) =>
            new()
            {
                Id = user.Id,
                Name = user.DisplayName
            };
    }
}