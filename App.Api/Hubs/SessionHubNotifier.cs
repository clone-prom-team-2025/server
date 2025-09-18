using App.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace App.Api.Hubs;

public class SessionHubNotifier : ISessionHubNotifier
{
    private readonly IHubContext<SessionHub> _hubContext;

    public SessionHubNotifier(IHubContext<SessionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task ForceLogoutAsync(string sessionId)
    {
        if (SessionHub.SessionConnections.TryGetValue(sessionId, out var connectionId))
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ForceLogout", "Your session was terminated");
    }
}