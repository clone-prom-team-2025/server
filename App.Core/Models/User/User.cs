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

    public User(string username, string password, string email, BaseFile avatar, List<string> roles, string fullName,
        string? phoneNumber = null, string? gender = null, DateTime? dateOfBirth = null)
    {
        Id = ObjectId.GenerateNewId();
        Username = username;
        PasswordHash = PasswordHasher.HashPassword(password);
        Email = email;
        Avatar = avatar;
        Roles = new List<string>(roles);
        CreatedAt = DateTime.UtcNow;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Gender = gender;
        DateOfBirth = dateOfBirth;
    }

    [BsonId] public ObjectId Id { get; set; }

    public string FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? PhoneNumberConfirmed { get; set; } = false;

    public string? Gender { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DateOfBirth { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public BaseFile Avatar { get; set; }

    public string? PasswordHash { get; set; }

    public List<string> Roles { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
}