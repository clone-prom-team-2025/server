namespace App.Core.DTOs.Product.Review;

public class ProductReviewCommentCreateDto
{
    public ProductReviewCommentCreateDto(double rating, string userId, string comment)
    {
        Rating = rating;
        UserId = userId;
        Comment = comment;
    }

    public double Rating { get; set; }

    public string UserId { get; set; }

    public string Comment { get; set; }
}
