namespace App.Core.DTOs.Product.Review;

public class ProductReviewCommentDto
{
    public ProductReviewCommentDto(string id, double rating, string userId, string comment, DateTime createdAt,
        List<ProductReviewCommentReactionDto> reactions)
    {
        Id = id;
        Rating = rating;
        UserId = userId;
        Comment = comment;
        CreatedAt = createdAt;
        Reactions = reactions;
    }

    public string Id { get; set; }
    public double Rating { get; set; }

    public string UserId { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ProductReviewCommentReactionDto> Reactions { get; set; } = new();

    public int PositiveCount => Reactions.Count(r => r.Positive);

    public int NegativeCount => Reactions.Count(r => !r.Positive);

    public void AddReaction(ProductReviewCommentReactionDto reaction)
    {
        Reactions.Add(reaction);
    }
}