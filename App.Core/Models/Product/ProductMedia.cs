using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product;

/// <summary>
/// Represents media (image, video) associated with a product.
/// </summary>
public class ProductMedia
{
    /// <summary>
    /// Unique media identifier (MongoDB ObjectId).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Id of the product this media belongs to.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; }

    /// <summary>
    /// URL link to the media resource.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Type of the media (image, video, etc.).
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public MediaType Type { get; set; }

    /// <summary>
    /// Ordering index for display priority.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Creates a new ProductMedia instance.
    /// </summary>
    /// <param name="productId">Associated product Id.</param>
    /// <param name="url">URL of the media.</param>
    /// <param name="type">Type of media.</param>
    /// <param name="order">Display order.</param>
    public ProductMedia(string productId, string url, MediaType type, int order)
    {
        Id = ObjectId.GenerateNewId().ToString();
        ProductId = productId;
        Url = url;
        Type = type;
        Order = order;
    }
}