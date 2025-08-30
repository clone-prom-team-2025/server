using App.Core.Models.Product.Review;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IProductReviewRepository
{
    Task<bool> CreateReview(ProductReview review);
    Task<bool> DeleteReview(ObjectId id);
    Task<bool> UpdateReview(ProductReview review);
    Task<ProductReview?> GetReviewById(ObjectId id);
    Task<ProductReview?> GetByProductId(ObjectId productId);
    Task<List<ProductReview>?> GetAll();
}