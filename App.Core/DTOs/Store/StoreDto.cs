using System.ComponentModel.DataAnnotations;
using App.Core.Enums;
using App.Core.Models.FileStorage;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.DTOs.Store;

public class StoreDto
{
    public StoreDto(string id, string name, BaseFile avatar, Dictionary<string, StoreRole> roles, StorePlans plan = StorePlans.None)
    {
        Id = id;
        Name = name;
        Avatar = avatar;
        Roles = roles;
        Plan = plan;
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public BaseFile Avatar { get; set; }

    public Dictionary<string, StoreRole> Roles { get; set; }

    public StorePlans Plan { get; set; }

    public DateTime CreatedAt { get; set; }
}