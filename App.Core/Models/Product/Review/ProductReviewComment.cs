using MongoDB.Bson;

namespace App.Core.Models.Product.Review;

/// <summary>
///     Represents a single user review comment with rating, content,
///     reactions, and metadata.
/// </summary>
public class ProductReviewComment
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProductReviewComment" /> class
    ///     with a rating, user ID, and comment text.
    /// </summary>
    /// <param name="rating">The rating given by the user (typically 1–5).</param>
    /// <param name="userId">The identifier of the user who left the review.</param>
    /// <param name="comment">The content of the comment.</param>
    public ProductReviewComment(int rating, ObjectId userId, string comment)
    {
        Rating = rating;
        UserId = userId;
        Comment = comment;
    }

    /// <summary>
    ///     The rating value submitted by the user.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    ///     The identifier of the user who made the comment.
    /// </summary>
    public ObjectId UserId { get; set; }

    /// <summary>
    ///     The content of the review comment.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     The UTC timestamp of when the comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     A list of reactions (likes/dislikes) left by other users.
    /// </summary>
    public Dictionary<string, bool> Reactions { get; set; } = [];
}