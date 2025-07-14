using App.Core.Models.Product.Review;

namespace App.Core.Interfaces;

public interface IProductReviewRepository
{
    Task<List<ProductReview>?> GetReviewsBySellerIdAsync(string sellerId);
    Task<ProductReview?> GetReviewByIdAsync(string reviewId);
    Task<ProductReview?> GetReviewByProductIdAsync(string productId);
    Task<ProductReview?> GetReviewByModelIdAsync(string modelId);
    Task CreateReviewAsync(ProductReview review);
    Task<bool> UpdateReviewAsync(ProductReview review);
    Task<bool> DeleteReviewAsync(string reviewId);
    Task<List<ProductReviewComment>?> GetCommentsByReviewIdAsync(string reviewId);
    Task<bool> AddCommentToReviewAsync(string reviewId, ProductReviewComment comment);
    Task<bool> UpdateCommentInReviewAsync(string reviewId, ProductReviewComment comment);
    Task<bool> DeleteCommentFromReviewAsync(string reviewId, string commentId);
    Task<bool> AddReactionToCommentAsync(string reviewId, string commentId, ProductReviewCommentReaction reaction);
}