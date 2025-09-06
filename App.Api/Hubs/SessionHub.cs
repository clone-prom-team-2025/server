using System.Security.Claims;
using App.Core.DTOs.Auth;
using App.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace App.Api.Hubs;

[Authorize]
public class SessionHub : Hub
{
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IMapper _mapper;

    public static readonly Dictionary<string, string> SessionConnections = new();

    public SessionHub(IUserSessionRepository sessionRepository, IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _mapper = mapper;
    }

    public async Task<UserSessionDto?> RequestSessionData()
    {
        var sessionId = GetSessionIdFromClaims();
        if (sessionId == null) 
        {
            await Clients.Caller.SendAsync("Error", "Session not found in token");
            Context.Abort();
            return null;
        }

        if (!ObjectId.TryParse(sessionId, out var objectId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid sessionId format");
            Context.Abort();
            return null;
        }

        var session = await _sessionRepository.GetSessionAsync(objectId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("Error", "Session does not exist");
            Context.Abort();
            return null;
        }

        if (session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
        {
            await Clients.Caller.SendAsync("Error", "Session has expired");
            Context.Abort();
            return null;
        }

        return _mapper.Map<UserSessionDto>(session);
    }

    public async Task ReRegisterSession()
    {
        var sessionId = GetSessionIdFromClaims();
        if (sessionId == null)
        {
            await Clients.Caller.SendAsync("Error", "Session not found in token");
            Context.Abort();
            return;
        }

        await HandleSessionRegistration(sessionId);
    }

    private async Task HandleSessionRegistration(string sessionId)
    {
        if (!ObjectId.TryParse(sessionId, out var objectId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid sessionId format");
            Context.Abort();
            return;
        }

        var session = await _sessionRepository.GetSessionAsync(objectId);
        if (session == null || session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
        {
            await Clients.Caller.SendAsync("Error", "Session invalid or expired");
            Context.Abort();
            return;
        }

        lock (SessionConnections)
        {
            SessionConnections[sessionId] = Context.ConnectionId;
        }

        await Clients.Caller.SendAsync("Registered", "Session registered successfully");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (SessionConnections)
        {
            var item = SessionConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(item.Key))
            {
                SessionConnections.Remove(item.Key);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
    
    public override async Task OnConnectedAsync()
    {
        var sessionId = GetSessionIdFromClaims();

        if (string.IsNullOrEmpty(sessionId))
        {
            await Clients.Caller.SendAsync("Error", "Session not found in token");
            Context.Abort();
            return;
        }

        if (!ObjectId.TryParse(sessionId, out var objectId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid sessionId format");
            Context.Abort();
            return;
        }

        var session = await _sessionRepository.GetSessionAsync(objectId);
        if (session == null || session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
        {
            await Clients.Caller.SendAsync("Error", "Session invalid or expired");
            Context.Abort();
            return;
        }

        lock (SessionConnections)
        {
            SessionConnections[sessionId] = Context.ConnectionId;
        }

        await Clients.Caller.SendAsync("Registered", "Session registered successfully");

        await base.OnConnectedAsync();
    }


    public async Task ForceLogoutLocal(string sessionId)
    {
        if (SessionConnections.TryGetValue(sessionId, out var connectionId) 
            && connectionId == Context.ConnectionId)
        {
            await Clients.Caller.SendAsync("ForceLogout", "Your session was terminated");
            Context.Abort();
        }
    }
    
    private string? GetSessionIdFromClaims()
    {
        return Context.User?.FindFirst(ClaimTypes.Sid)?.Value;
    }
}
