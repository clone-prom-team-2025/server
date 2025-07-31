namespace App.Core.DTOs.Product.Review;

public class ProductReviewCreateDto
{
    public ProductReviewCreateDto(string productId, string modelId, double averageRating)
    {
        ProductId = productId;
        AverageRating = averageRating;
        ModelId = modelId;
    }
    public string ProductId { get; set; }

    public string ModelId { get; set; }

    public double AverageRating { get; set; }
}
