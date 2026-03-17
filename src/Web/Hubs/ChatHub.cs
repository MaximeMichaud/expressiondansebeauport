using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> UserConnections = new();

    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            lock (UserConnections)
            {
                UserConnections[userId] = Context.ConnectionId;
            }
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            lock (UserConnections)
            {
                UserConnections.Remove(userId);
            }
        }
        return base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionId(string userId)
    {
        lock (UserConnections)
        {
            return UserConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }
    }
}
