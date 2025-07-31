using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product.Review;

/// <summary>
/// Represents a collection of user reviews for a specific product variation,
/// including an average rating and individual review comments.
/// </summary>
public class ProductReview
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductReview"/> class
    /// for a specific product and model variation.
    /// </summary>
    /// <param name="productId">The ID of the related product.</param>
    /// <param name="modelId">The model id of the variation.</param>
    public ProductReview(ObjectId productId, string modelId)
    {
        Id = ObjectId.GenerateNewId();
        ProductId = productId;
        AverageRating = 0.0;
        ModelId = modelId;
    }

    /// <summary>
    /// The unique identifier of this review document (MongoDB ObjectId).
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    /// The identifier of the associated product.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId ProductId { get; set; }

    /// <summary>
    /// The name of the product variation model being reviewed.
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// The average rating calculated from all submitted comments.
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// The list of individual user review comments.
    /// </summary>
    public List<ProductReviewComment> Comments { get; set; } = new();

    /// <summary>
    /// Adds a new comment to the review and updates the average rating.
    /// </summary>
    /// <param name="comment">The review comment to add.</param>
    public void AddComment(ProductReviewComment comment)
    {
        Comments.Add(comment);
        CalculateAverageRating();
    }

    /// <summary>
    /// Recalculates the average rating based on the current list of comments.
    /// </summary>
    public void CalculateAverageRating()
    {
        if (Comments.Count == 0)
        {
            AverageRating = 0.0;
            return;
        }

        var totalRating = Comments.Sum(item => item.Rating);
        AverageRating = totalRating / Comments.Count;
    }
}
