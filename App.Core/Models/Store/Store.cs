using System.ComponentModel.DataAnnotations;
using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Store;

public class Store
{
    public Store(string name, string avatarUrl, List<StoreRole> roles, StorePlans plan = StorePlans.None)
    {
        Id = ObjectId.GenerateNewId();
        Name = name;
        AvatarUrl = avatarUrl;
        Roles = new List<StoreRole>(roles);
        Plan = plan;
        CreatedAt = DateTime.UtcNow;
    }

    [BsonId] public ObjectId Id { get; set; }

    [Required] public string Name { get; set; }

    public string AvatarUrl { get; set; }

    public List<StoreRole> Roles { get; set; }

    public StorePlans Plan { get; set; }

    public DateTime CreatedAt { get; set; }
}