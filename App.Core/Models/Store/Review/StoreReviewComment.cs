namespace App.Core.Models.Store.Review;

/// <summary>
/// Represents a single user review comment with rating, content,
/// reactions, and metadata.
/// </summary>
public class StoreReviewComment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductReviewComment"/> class
    /// with a rating, user ID, and comment text.
    /// </summary>
    /// <param name="rating">The rating given by the user (typically 1–5).</param>
    /// <param name="userId">The identifier of the user who left the review.</param>
    /// <param name="comment">The content of the comment.</param>
    public StoreReviewComment(double rating, string userId, string comment)
    {
        Rating = rating;
        UserId = userId;
        Comment = comment;
    }
    
    /// <summary>
    /// The unique identifier of the comment.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The rating value submitted by the user.
    /// </summary>
    public double Rating { get; set; }

    /// <summary>
    /// The identifier of the user who made the comment.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The content of the review comment.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// The UTC timestamp of when the comment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// A list of reactions (likes/dislikes) left by other users.
    /// </summary>
    public List<StoreReviewCommentReaction> Reactions { get; set; } = new();

    /// <summary>
    /// Gets the number of positive (like) reactions.
    /// </summary>
    public int PositiveCount => Reactions.Count(r => r.Positive);

    /// <summary>
    /// Gets the number of negative (dislike) reactions.
    /// </summary>
    public int NegativeCount => Reactions.Count(r => !r.Positive);
    
    /// <summary>
    /// Adds a user reaction (like/dislike) to this comment.
    /// </summary>
    /// <param name="reaction">The reaction to add.</param>
    public void AddReaction(StoreReviewCommentReaction reaction)
    {
        Reactions.Add(reaction);
    }
}