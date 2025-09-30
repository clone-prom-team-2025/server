using App.Core.Interfaces;
using App.Core.Models.Notification;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
        var lookup = new BsonDocument
        {
            {
                "$lookup", new BsonDocument
                {
                    { "from", _seenNotifications.CollectionNamespace.CollectionName },
                    { "localField", "_id" },
                    { "foreignField", "NotificationId" },
                    { "as", "SeenInfo" }
                }
            }
        };

        var match = new BsonDocument
        {
            { "$match", new BsonDocument("SeenInfo.UserId", userId) }
        };

        var project = new BsonDocument
        {
            {
                "$project", new BsonDocument
                {
                    { "SeenInfo", 0 }
                }
            }
        };

        var pipeline = new[] { lookup, match, project };

        var docs = await _notifications.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return docs.Select(d => BsonSerializer.Deserialize<Notification>(d)).ToList();
    }

    public async Task<List<Notification>?> GetUnseenNotificationsAsync(ObjectId userId)
    {
        var lookup = new BsonDocument
        {
            {
                "$lookup", new BsonDocument
                {
                    { "from", _seenNotifications.CollectionNamespace.CollectionName },
                    { "localField", "_id" },
                    { "foreignField", "NotificationId" },
                    { "as", "SeenInfo" }
                }
            }
        };

        var match = new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    {
                        "$expr", new BsonDocument
                        {
                            {
                                "$not", new BsonDocument
                                {
                                    {
                                        "$in", new BsonArray { new BsonObjectId(userId), "$SeenInfo.UserId" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var project = new BsonDocument
        {
            {
                "$project", new BsonDocument
                {
                    { "SeenInfo", 0 }
                }
            }
        };

        var pipeline = new[] { lookup, match, project };

        var docs = await _notifications.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return docs.Select(d => BsonSerializer.Deserialize<Notification>(d)).ToList();
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }

    public async Task<bool> DeleteNotificationAsync(ObjectId id)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
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
        var lookup = new BsonDocument
        {
            {
                "$lookup", new BsonDocument
                {
                    { "from", _seenNotifications.CollectionNamespace.CollectionName },
                    { "localField", "_id" },
                    { "foreignField", "NotificationId" },
                    { "as", "SeenInfo" }
                }
            }
        };

        var match = new BsonDocument
        {
            {
                "$match", new BsonDocument
                {
                    { "_id", notificationId },
                    {
                        "SeenInfo", new BsonDocument
                        {
                            {
                                "$not", new BsonDocument
                                {
                                    { "$elemMatch", new BsonDocument("UserId", userId) }
                                }
                            }
                        }
                    }
                }
            }
        };

        var pipeline = new[] { lookup, match };

        var exists = await _notifications.Aggregate<BsonDocument>(pipeline).AnyAsync();
        return exists;
    }

    private class NotificationWithSeen : Notification
    {
        public List<NotificationSeen> SeenInfo { get; } = new();
    }
}