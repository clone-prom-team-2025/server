namespace App.Core.DTOs.Product.Review;

public class ProductReviewCommentCreateDto
{
    public ProductReviewCommentCreateDto(double rating, string userId, string comment, DateTime createdAt)
    {
        Rating = rating;
        UserId = userId;
        Comment = comment;
        CreatedAt = createdAt;
    }

    public double Rating { get; set; }

    public string UserId { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
