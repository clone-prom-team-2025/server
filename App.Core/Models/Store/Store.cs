using System.ComponentModel.DataAnnotations;
using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Store;

public class Store
{
    [BsonId]
    public ObjectId Id { get; set; }

    [Required] [MaxLength(50)] 
    public string Name { get; set; } = null!;

    [Url] 
    public string AvatarUrl { get; set; } = String.Empty;
    
    public List<StoreRole> Roles { get; set; } = new List<StoreRole>();

    public Store(string name, string avatarUrl, List<StoreRole> roles)
    {
        Id = ObjectId.GenerateNewId();
        Name = name;
        AvatarUrl = avatarUrl;
        Roles = new List<StoreRole>(roles);
    }
}