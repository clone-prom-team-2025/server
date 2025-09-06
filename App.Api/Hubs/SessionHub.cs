using App.Core.DTOs.Auth;
using App.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace App.Api.Hubs;

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
    
    public async Task RegisterSession(string sessionId)
    {
        if (!ObjectId.TryParse(sessionId, out var objectId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid sessionId format");
            Context.Abort();
            return;
        }

        var session = await _sessionRepository.GetSessionAsync(objectId);
        if (session == null || session.IsRevoked)
        {
            await Clients.Caller.SendAsync("Error", "Session not found or revoked");
            Context.Abort();
            return;
        }

        lock (SessionConnections)
        {
            SessionConnections[sessionId] = Context.ConnectionId;
        }

        await Clients.Caller.SendAsync("Registered", "Session registered successfully");
    }
    
    public async Task<UserSessionDto?> RequestSessionData(string sessionId)
    {
        if (!ObjectId.TryParse(sessionId, out var objectId))
            return null;

        var session = await _sessionRepository.GetSessionAsync(objectId);
        return session == null ? null : _mapper.Map<UserSessionDto>(session);
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

    public async Task ForceLogoutLocal(string sessionId)
    {
        if (SessionConnections.TryGetValue(sessionId, out var connectionId) 
            && connectionId == Context.ConnectionId)
        {
            await Clients.Caller.SendAsync("ForceLogout", "Your session was terminated");
            Context.Abort();
        }
    }
}