using App.Core.DTOs.Notification;

namespace App.Core.Interfaces;

public interface INotificationHubNotifier
{
    Task SendNotificationAsync(NotificationDto notification);
}