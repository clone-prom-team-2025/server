using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product.Review;

/// <summary>
///     Represents a collection of user reviews for a specific product variation,
///     including an average rating and individual review comments.
/// </summary>
public class ProductReview
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProductReview" /> class
    ///     for a specific product and model variation.
    /// </summary>
    /// <param name="productId">The ID of the related product.</param>
    /// <param name="modelId">The model id of the variation.</param>
    public ProductReview(ObjectId productId)
    {
        ProductId = productId;
    }

    /// <summary>
    ///     The unique identifier of this review document (MongoDB ObjectId).
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    /// <summary>
    ///     The identifier of the associated product.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId ProductId { get; set; }

    /// <summary>
    ///     The list of individual user review comments.
    /// </summary>
    public List<ProductReviewComment> Comments { get; set; } = [];
}