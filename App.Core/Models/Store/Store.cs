using System.ComponentModel.DataAnnotations;
using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Store;

public class Store
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [Required] [MaxLength(50)] 
    public string Name { get; set; } = null!;

    [Url] 
    public string AvatarUrl { get; set; } = String.Empty;
    
    public List<StoreRole> Roles { get; set; } = new List<StoreRole>();

    public Store()
    {
        Id = ObjectId.GenerateNewId().ToString();
        
    }
}