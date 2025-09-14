using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Notification;

public class NotificationSeen
{
    [BsonId]
    public ObjectId Id { get; set; }

    public ObjectId NotificationId { get; set; }

    public ObjectId UserId { get; set; }

    public DateTime SeenAt { get; set; } = DateTime.UtcNow;
}