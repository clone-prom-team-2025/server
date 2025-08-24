using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Store.Review;

/// <summary>
///     Represents a collection of user reviews for store,
///     including an average rating and individual review comments.
/// </summary>
public class StoreReview
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StoreReview" /> class
    ///     for a specific product and model variation.
    /// </summary>
    /// <param name="storeId">The ID of the related product.</param>
    /// <param name="modelId">The model id of the variation.</param>
    public StoreReview(string storeId)
    {
        Id = ObjectId.GenerateNewId();
        StoreId = storeId;
        AverageRating = 0.0;
    }

    /// <summary>
    ///     The unique identifier of this review document (MongoDB ObjectId).
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    ///     The identifier of the associated product.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string StoreId { get; set; }

    /// <summary>
    ///     The average rating calculated from all submitted comments.
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    ///     The list of individual user review comments.
    /// </summary>
    public List<StoreReviewComment> Comments { get; set; } = new();

    /// <summary>
    ///     Adds a new comment to the review and updates the average rating.
    /// </summary>
    /// <param name="comment">The review comment to add.</param>
    public void AddComment(StoreReviewComment comment)
    {
        Comments.Add(comment);
        CalculateAverageRating();
    }

    /// <summary>
    ///     Recalculates the average rating based on the current list of comments.
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