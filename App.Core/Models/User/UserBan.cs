using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.User;

public class UserBan
{
    [BsonId] public ObjectId Id { get; set; }

    public ObjectId UserId { get; set; }

    public ObjectId AdminId { get; set; }

    public string Reason { get; set; } = null!;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? BannedUntil { get; set; }

    public BanType Types { get; set; } = BanType.Comments;
}