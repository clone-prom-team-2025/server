namespace App.Core.DTOs.Product.Review;

public class ProductReviewCommentDto
{
    public ProductReviewCommentDto(double rating, string userId, string comment, DateTime createdAt,
        Dictionary<string, bool> reactions)
    {
        Rating = rating;
        UserId = userId;
        Comment = comment;
        CreatedAt = createdAt;
        Reactions = reactions;
    }

    public double Rating { get; set; }

    public string UserId { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Dictionary<string, bool> Reactions { get; set; } = [];

    public int PositiveCount => Reactions.Count(r => r.Value);

    public int NegativeCount => Reactions.Count(r => r.Value == false);
}