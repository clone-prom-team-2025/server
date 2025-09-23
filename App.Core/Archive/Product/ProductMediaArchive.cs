using App.Core.Enums;
using App.Core.Models.FileStorage;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Archive.Product;

public class ProductMediaArchive
{
    [BsonId]
    public string Id { get; set; }
    
    public DateTime ArchivedAt { get; set; }
    
    public ObjectId ProductMediaId { get; set; }
    
    public ObjectId ProductId { get; set; }

    public BaseFile Files { get; set; } = new();
    
    [BsonRepresentation(BsonType.String)]
    public MediaType Type { get; set; }
    
    public int Order { get; set; }
}