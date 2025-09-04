using App.Core.Enums;
using App.Core.Models.FileStorage;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Store;

public class StoreCreateRequest
{
    [BsonId] public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public string Name { get; set; } = string.Empty;
    public ObjectId UserId { get; set; }
    public BaseFile Avatar { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ObjectId? ApprovedByAdminId { get; set; } = null;
    public ObjectId? RejectedByAdminId { get; set; } = null;
    public List<StoreRequestComment> Comments { get; set; } = [];
    public StorePlans Plan { get; set; } = StorePlans.None;
}