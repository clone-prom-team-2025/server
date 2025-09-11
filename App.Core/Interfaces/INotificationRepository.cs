using App.Core.Models.Notification;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>?> GetAllNotificationsAsync();
    Task<Notification?> GetNotificationByIdAsync(ObjectId id);
    Task<List<Notification>?> GetAllNotificationsByUserIdAsync(ObjectId userId);
    Task<List<Notification>?> GetSeenNotificationsAsync(ObjectId userId);
    Task<List<Notification>?> GetUnseenNotificationsAsync(ObjectId userId);
    Task CreateNotificationAsync(Notification notification);
    Task<bool> DeleteNotificationAsync(ObjectId id);
    Task<bool> DeleteAllNotificationsAsync();
    Task<bool> DeleteAllNotificationsByUserIdAsync(ObjectId userId);
    Task CreateSeenNotificationAsync(NotificationSeen notificationSeen);
    Task<bool> DeleteSeenNotificationAsync(ObjectId id);
    Task<bool> DeleteAllSeenNotificationsAsync();
    Task<bool> DeleteAllSeenNotificationsByUserIdAsync(ObjectId userId);
    Task<bool> DeleteAllSeenNotificationsByNotificationIdAsync(ObjectId notificationId);
    Task<bool> UpdateNotificationSeenAsync(NotificationSeen notificationSeen);
    Task<bool> HasSeenNotificationAsync(ObjectId userId, ObjectId notificationId);
}