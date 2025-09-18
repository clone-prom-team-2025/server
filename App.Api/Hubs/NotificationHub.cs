using System.Security.Claims;
using App.Core.DTOs.Notification;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace App.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    // Ключ: userId, значення: HashSet<connectionId>
    public static readonly Dictionary<string, HashSet<string>> UserConnections = new();
    private readonly ILogger<NotificationHub> _logger;
    private readonly IUserSessionRepository _sessionRepository;

    public NotificationHub(ILogger<NotificationHub> logger, IUserSessionRepository sessionRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
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

        var userId = session.UserId.ToString();
        lock (UserConnections)
        {
            if (!UserConnections.ContainsKey(userId))
                UserConnections[userId] = new HashSet<string>();
            UserConnections[userId].Add(Context.ConnectionId);
        }

        _logger.LogDebug("UserConnections: {conn}", UserConnections.ToJson());
        await Clients.Caller.SendAsync("Registered", "Session registered successfully");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (UserConnections)
        {
            foreach (var kvp in UserConnections)
                if (kvp.Value.Contains(Context.ConnectionId))
                {
                    kvp.Value.Remove(Context.ConnectionId);
                    if (kvp.Value.Count == 0)
                        UserConnections.Remove(kvp.Key);
                    break;
                }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendNotification(NotificationDto notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        notification.From = string.IsNullOrEmpty(notification.From) ? "System" : notification.From;

        if (!string.IsNullOrEmpty(notification.To))
        {
            List<string> connectionsCopy;
            lock (UserConnections)
            {
                if (!UserConnections.TryGetValue(notification.To, out var connections) || connections.Count == 0)
                {
                    _logger.LogDebug("User {To} not connected, notification skipped", notification.To);
                    return;
                }

                // копіюємо поточні connectionId, щоб не працювати з HashSet поза lock
                connectionsCopy = connections.ToList();
            }

            foreach (var connectionId in connectionsCopy)
                if (!string.IsNullOrEmpty(connectionId))
                    try
                    {
                        _logger.LogInformation("Sending notification to {connectionId}", connectionId);
                        await Clients.Client(connectionId)?.SendAsync("ReceiveNotification", notification)!;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send notification to connection {connectionId}",
                            connectionId);
                    }
        }
        else
        {
            _logger.LogDebug("Broadcasting notification: {not}", notification.ToJson());
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }
    }

    private string? GetSessionIdFromClaims()
    {
        return Context.User?.FindFirst(ClaimTypes.Sid)?.Value;
    }
}