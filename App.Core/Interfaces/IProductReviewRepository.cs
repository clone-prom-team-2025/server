using App.Core.Models.Product.Review;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IProductReviewRepository
{
    Task<List<ProductReview>?> GetReviewsBySellerIdAsync(ObjectId sellerId);
    Task<ProductReview?> GetReviewByIdAsync(ObjectId reviewId);
    Task<ProductReview?> GetReviewByProductIdAsync(ObjectId productId);
    Task CreateReviewAsync(ProductReview review);
    Task<bool> UpdateReviewAsync(ProductReview review);
    Task<bool> DeleteReviewAsync(ObjectId reviewId);
    Task<List<ProductReviewComment>?> GetCommentsByReviewIdAsync(ObjectId reviewId);
    Task<bool> AddCommentToReviewAsync(ObjectId reviewId, ProductReviewComment comment);
    Task<bool> UpdateCommentInReviewAsync(ObjectId reviewId, ProductReviewComment comment);
    Task<bool> DeleteCommentFromReviewAsync(ObjectId reviewId, string commentId);
    Task<bool> AddReactionToCommentAsync(ObjectId reviewId, string commentId, ProductReviewCommentReaction reaction);
}