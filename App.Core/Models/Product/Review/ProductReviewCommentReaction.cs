using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Product.Review;

/// <summary>
/// Represents a user reaction (like or dislike) to a review comment.
/// </summary>
public class ProductReviewCommentReaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductReviewCommentReaction"/> class.
    /// </summary>
    /// <param name="userId">The identifier of the user who reacted.</param>
    /// <param name="positive">Indicates whether the reaction is positive (like) or negative (dislike).</param>
    public ProductReviewCommentReaction(ObjectId userId, bool positive)
    {
        UserId = userId;
        Positive = positive;
    }

    /// <summary>
    /// The ID of the user who gave the reaction.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }

    /// <summary>
    /// Indicates whether the reaction is positive (true = like) or negative (false = dislike).
    /// </summary>
    public bool Positive { get; set; }
}