using App.Core.Interfaces;
using App.Core.Models.Notification;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class NotificationRepository(MongoDbContext context) : INotificationRepository
{
    private readonly IMongoCollection<Notification> _notifications = context.Notifications;
    private readonly IMongoCollection<NotificationSeen> _seenNotifications = context.NotificationSees;

    public async Task<List<Notification>?> GetAllNotificationsAsync()
    {
        return await _notifications.Find(FilterDefinition<Notification>.Empty).ToListAsync();
    }

    public async Task<Notification?> GetNotificationByIdAsync(ObjectId id)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
        return await _notifications.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Notification>?> GetAllNotificationsByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.To, userId);
        return await _notifications.Find(filter).ToListAsync();
    }

    public async Task<List<Notification>?> GetSeenNotificationsAsync(ObjectId userId)
    {
        var pipeline = _notifications.Aggregate()
            .Lookup<Notification, NotificationSeen, NotificationWithSeen>(
                _seenNotifications,
                n => n.Id,
                s => s.NotificationId,
                r => r.SeenInfo)
            .Match(n => n.SeenInfo.Any(s => s.UserId == userId));

        var result = await pipeline.ToListAsync();
        return result.Cast<Notification>().ToList();
    }

    public async Task<List<Notification>?> GetUnseenNotificationsAsync(ObjectId userId)
    {
        var pipeline = _notifications.Aggregate()
            .Lookup<Notification, NotificationSeen, NotificationWithSeen>(
                _seenNotifications,
                n => n.Id,
                s => s.NotificationId,
                r => r.SeenInfo)
            .Match(n => n.SeenInfo.All(s => s.UserId != userId));

        var result = await pipeline.ToListAsync();
        return result.Cast<Notification>().ToList();
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }
    
    public async Task<bool> DeleteNotificationAsync(ObjectId id)
    {
        var filter =  Builders<Notification>.Filter.Eq(n => n.Id, id);
        return (await _notifications.DeleteOneAsync(filter)).DeletedCount != 0;
    }

    public async Task<bool> DeleteAllNotificationsAsync()
    {
        return (await _notifications.DeleteManyAsync(FilterDefinition<Notification>.Empty)).DeletedCount != 0;
    }

    public async Task<bool> DeleteAllNotificationsByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.To, userId);
        return (await _notifications.DeleteManyAsync(filter)).DeletedCount != 0;
    }

    public async Task CreateSeenNotificationAsync(NotificationSeen notificationSeen)
    {
        await _seenNotifications.InsertOneAsync(notificationSeen);
    }

    public async Task<bool> DeleteSeenNotificationAsync(ObjectId id)
    {
        var filter = Builders<NotificationSeen>.Filter.Eq(n => n.Id, id);
        return (await _seenNotifications.DeleteOneAsync(filter)).DeletedCount != 0;
    }

    public async Task<bool> DeleteAllSeenNotificationsAsync()
    {
        return (await _seenNotifications.DeleteManyAsync(FilterDefinition<NotificationSeen>.Empty)).DeletedCount != 0;
    }

    public async Task<bool> DeleteAllSeenNotificationsByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<NotificationSeen>.Filter.Eq(n => n.UserId, userId);
        return (await _seenNotifications.DeleteManyAsync(filter)).DeletedCount != 0;
    }

    public async Task<bool> DeleteAllSeenNotificationsByNotificationIdAsync(ObjectId notificationId)
    {
        var filter = Builders<NotificationSeen>.Filter.Eq(n => n.NotificationId, notificationId);
        return (await _seenNotifications.DeleteManyAsync(filter)).DeletedCount != 0;
    }

    public async Task<bool> UpdateNotificationSeenAsync(NotificationSeen notificationSeen)
    {
        var filter = Builders<NotificationSeen>.Filter.Eq(n => n.Id, notificationSeen.Id);
        return (await _seenNotifications.ReplaceOneAsync(filter, notificationSeen)).MatchedCount != 0;
    }

    public async Task<bool> HasSeenNotificationAsync(ObjectId userId, ObjectId notificationId)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.To, userId),
            Builders<Notification>.Filter.Eq(n => n.Id, notificationId)
        );
        return await _notifications.Find(filter).AnyAsync();
    }

    private class NotificationWithSeen : Notification
    {
        public List<NotificationSeen> SeenInfo { get; set; } = new();
    }
}