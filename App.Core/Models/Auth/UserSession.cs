using MongoDB.Bson;

namespace App.Core.Models.Auth;

public class UserSession
{
    public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string DeviceInfo { get; set; }
    public bool IsRevoked { get; set; }
}
