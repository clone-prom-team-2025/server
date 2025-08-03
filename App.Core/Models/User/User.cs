using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using App.Core.Enums;
using App.Core.Utils;

namespace App.Core.Models.User;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    public string Username { get; set; }
    
    public string? Email { get; set; }
    
    public bool EmailConfirmed { get; set; }
    
    public UserAvatar AvatarUrl { get; set; }

    public string? PasswordHash { get; set; }
    
    public List<UserRole> Roles { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    public UserAdditionalInfo? AdditionalInfo { get; set; }

    public UserBlockInfo? BlockInfo { get; set; }
    
    public User(string username, UserAvatar avatarUrl, List<UserRole> roles)
    {
        Id = ObjectId.GenerateNewId();
        this.Username = username;
        this.AvatarUrl = avatarUrl;
        this.Roles = new List<UserRole>(roles);
        CreatedAt = DateTime.UtcNow;
    }
    
    public User(string username, string password, string email, UserAvatar avatarUrl, List<UserRole> roles)
    {
        Id = ObjectId.GenerateNewId();
        this.Username = username;
        this.PasswordHash = PasswordHasher.HashPassword(password);
        this.Email = email;
        this.AvatarUrl = avatarUrl;
        this.Roles = new List<UserRole>(roles);
        CreatedAt = DateTime.UtcNow;
    }
}