using App.Core.DTOs.Notification;

namespace App.Core.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
    Task<NotificationDto> GetNotificationAsync(string id);
    Task<IEnumerable<NotificationDto>> GetAllNotificationsByUserIdAsync(string userId);
    Task<IEnumerable<NotificationDto>> GetSeenNotificationsAsync(string userId);
    Task<IEnumerable<NotificationDto>> GetUnSeenNotificationsAsync(string userId);
    Task SendNotificationAsync(NotificationCreateDto notificationCreateDto);
    Task DeleteNotificationAsync(string id);
    Task DeleteAllNotificationsAsync(string userId);
    Task DeleteAllNotificationsByUserIdAsync(string userId);
    Task DeleteSeenNotificationAsync(string id);
    Task DeleteAllSeenNotificationsAsync();
    Task DeleteAllSeenNotificationsByUserIdAsync(string userId);
    Task DeleteAllSeenNotificationsByNotificationIdAsync(string notificationId);
    Task SeeNotificationAsync(string notificationId, string userId);
}