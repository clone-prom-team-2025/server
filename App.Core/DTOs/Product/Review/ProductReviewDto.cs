namespace App.Core.DTOs.Product.Review;

public class ProductReviewDto
{
    public ProductReviewDto(string id, string productId,
        List<ProductReviewCommentDto> comments)
    {
        Id = id;
        ProductId = productId;
        Comments = comments;
    }

    public ProductReviewDto()
    {
    }

    public string Id { get; set; }

    public string ProductId { get; set; }

    public ProductReviewRatingListDto Rating { get; set; }

    public double AverageRating { get; set; }

    public List<ProductReviewCommentDto> Comments { get; set; } = [];
}