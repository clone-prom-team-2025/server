using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Core.DTOs.Product.Review;

public class ProductReviewCommentReactionDto
{
    public ProductReviewCommentReactionDto(string userId, bool positive)
    {
        UserId = userId;
        Positive = positive;
    }

    public string UserId { get; set; }

    public bool Positive { get; set; }
}