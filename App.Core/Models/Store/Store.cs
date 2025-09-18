using App.Core.Enums;
using App.Core.Models.FileStorage;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Store;

public class Store
{
    public Store(string name, BaseFile avatar, Dictionary<string, StoreRole> roles, StorePlans plan = StorePlans.None)
    {
        Id = ObjectId.GenerateNewId();
        Name = name;
        Avatar = avatar;
        Roles = roles;
        Plan = plan;
        CreatedAt = DateTime.UtcNow;
    }

    [BsonId] public ObjectId Id { get; set; }

    public string Name { get; set; }

    public BaseFile Avatar { get; set; }

    public Dictionary<string, StoreRole> Roles { get; set; }

    public StorePlans Plan { get; set; }

    public DateTime CreatedAt { get; set; }
}