using App.Core.Models.Product.Review;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Archive.Product;

public class ProductReviewArchive
{
    [BsonId]
    public string Id { get; set; }
    
    public DateTime ArchivedAt { get; set; }
    
    public ObjectId ProductReviewId { get; set; } = ObjectId.GenerateNewId();

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId ProductId { get; set; }

    public List<ProductReviewComment> Comments { get; set; } = [];
}