using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models;

public class ProductMedia
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; }

    public string Url { get; set; }

    [BsonRepresentation(BsonType.String)]
    public MediaType Type { get; set; }

    public int Order { get; set; }

    public ProductMedia(string productId, string url, MediaType type, int order)
    {
        Id = ObjectId.GenerateNewId().ToString();
        ProductId = productId;
        Url = url;
        Type = type;
        Order = order;
    }
}