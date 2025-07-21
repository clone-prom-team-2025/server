using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

public class UserBlockInfo
{
    [Required]
    [MaxLength(255)]
    public string Reason { get; set; } = null!;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? BannedUntil { get; set; }
}
