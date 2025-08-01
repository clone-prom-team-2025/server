using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.DTOs.Product.Review;

public class ProductReviewDto
{
    public ProductReviewDto(string id, string productId, string modelId, double averageRating, List<ProductReviewCommentDto> comments)
    {
        Id = id;
        ProductId = productId;
        AverageRating = averageRating;
        ModelId = modelId;
        Comments = comments;
    }

    public ProductReviewDto()
    {
        
    }

    public string Id { get; set; }

    public string ProductId { get; set; }

    public string ModelId { get; set; }

    public double AverageRating { get; set; }

    public List<ProductReviewCommentDto> Comments { get; set; } = new();

    public void AddComment(ProductReviewCommentDto comment)
    {
        Comments.Add(comment);
        CalculateAverageRating();
    }

    public void CalculateAverageRating()
    {
        if (Comments.Count == 0)
        {
            AverageRating = 0.0;
            return;
        }

        var totalRating = Comments.Sum(item => item.Rating);
        AverageRating = totalRating / Comments.Count;
    }
}
