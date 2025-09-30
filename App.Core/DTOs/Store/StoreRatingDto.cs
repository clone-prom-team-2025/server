using App.Core.DTOs.Product.Review;

namespace App.Core.DTOs.Store;

public class StoreRatingDto
{
    public ProductReviewRatingListDto Ratings { get; set; } = new();
    public double AverageRating { get; set; } = 0.0;
    public int TotalReviews { get; set; } = 0;
    public List<ProductReviewDto> Reviews { get; set; } = new();
}