using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NotificationService.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();
        private static readonly Dictionary<string, string> _userLanguages = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var language = Context.User?.FindFirst("language")?.Value ?? "en";
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                _userLanguages[userId] = language;
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "all");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections.Remove(userId);
                }
                if (_userLanguages.ContainsKey(userId))
                {
                    _userLanguages.Remove(userId);
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public static string? GetConnectionId(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }

        public static string GetUserLanguage(string userId)
        {
            return _userLanguages.TryGetValue(userId, out var language) ? language : "en";
        }

        public static IEnumerable<string> GetAllConnectionIds()
        {
            return _userConnections.Values;
        }

        public static Dictionary<string, string> GetAllUserLanguages()
        {
            return new Dictionary<string, string>(_userLanguages);
        }
    }
} 