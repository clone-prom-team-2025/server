using App.Core.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product;

/// <summary>
///     Represents media (image, video, etc.) linked to a product.
/// </summary>
public class ProductMedia
{
    /// <summary>
    ///     Initializes a new instance of ProductMedia.
    /// </summary>
    /// <param name="productId">The product ID the media is associated with.</param>
    /// <param name="url">The media URL.</param>
    /// <param name="type">The type of the media.</param>
    /// <param name="order">Ordering index for display.</param>
    public ProductMedia(ObjectId productId, string url, MediaType type, int order)
    {
        Id = ObjectId.GenerateNewId();
        ProductId = productId;
        Url = url;
        Type = type;
        Order = order;
    }

    /// <summary>
    ///     Unique identifier for the media.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    ///     ID of the associated product.
    /// </summary>
    public ObjectId ProductId { get; set; }

    /// <summary>
    ///     URL to the media file.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Media type (e.g., image, video).
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public MediaType Type { get; set; }

    /// <summary>
    ///     Display order for the media.
    /// </summary>
    public int Order { get; set; }
}
