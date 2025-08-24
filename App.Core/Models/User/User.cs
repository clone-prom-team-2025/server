using App.Core.Models.FileStorage;
using App.Core.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.User;

public class User
{
    public User(string username, BaseFile avatar, List<string> roles)
    {
        Id = ObjectId.GenerateNewId();
        Username = username;
        Avatar = avatar;
        Roles = new List<string>(roles);
        CreatedAt = DateTime.UtcNow;
    }

    public User(string username, string password, string email, BaseFile avatar, List<string> roles)
    {
        Id = ObjectId.GenerateNewId();
        Username = username;
        PasswordHash = PasswordHasher.HashPassword(password);
        Email = email;
        Avatar = avatar;
        Roles = new List<string>(roles);
        CreatedAt = DateTime.UtcNow;
    }

    [BsonId] public ObjectId Id { get; set; }

    public string Username { get; set; }

    public string? Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public BaseFile Avatar { get; set; }

    public string? PasswordHash { get; set; }

    public List<string> Roles { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    public UserAdditionalInfo? AdditionalInfo { get; set; }
}