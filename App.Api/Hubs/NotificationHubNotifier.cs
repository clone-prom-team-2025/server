using App.Core.DTOs.Notification;
using App.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace App.Api.Hubs;

public class NotificationHubNotifier : INotificationHubNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(NotificationDto notification)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));
        notification.From ??= "System";

        if (!string.IsNullOrEmpty(notification.To))
        {
            // Беремо connectionId'и для користувача
            List<string> connectionsCopy;
            lock (NotificationHub.UserConnections)
            {
                if (!NotificationHub.UserConnections.TryGetValue(notification.To, out var connections) ||
                    connections.Count == 0)
                    return;

                connectionsCopy = connections.ToList(); // копіюємо щоб не працювати з HashSet поза lock
            }

            foreach (var connectionId in connectionsCopy)
            {
                if (string.IsNullOrEmpty(connectionId)) continue;

                try
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", notification);
                }
                catch
                {
                    // Якщо клієнт відключився, можна видалити connectionId або ігнорувати
                }
            }
        }
        else
        {
            // Broadcast всім
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        }
    }
}