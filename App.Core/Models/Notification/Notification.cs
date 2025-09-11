using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Notification;

public class Notification
{
    [BsonId]
    public ObjectId Id { get; set; }

    public NotificationType Type { get; set; } = NotificationType.Info;

    public string Message { get; set; } = null!;

    public ObjectId? From { get; set; }
    
    public ObjectId? To { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? MetadataUrl { get; set; }

    public bool IsHighPriority { get; set; } = false;
}