using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.Models.Store.Review;

/// <summary>
///     Represents a user reaction (like or dislike) to a review comment.
/// </summary>
public class StoreReviewCommentReaction
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StoreReviewCommentReaction" /> class.
    /// </summary>
    /// <param name="userId">The identifier of the user who reacted.</param>
    /// <param name="positive">Indicates whether the reaction is positive (like) or negative (dislike).</param>
    public StoreReviewCommentReaction(string userId, bool positive)
    {
        UserId = userId;
        Positive = positive;
    }

    /// <summary>
    ///     The ID of the user who gave the reaction.
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }

    /// <summary>
    ///     Indicates whether the reaction is positive (true = like) or negative (false = dislike).
    /// </summary>
    public bool Positive { get; set; }
}