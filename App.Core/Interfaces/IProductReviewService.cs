using App.Core.DTOs.Product.Review;
using App.Core.Models.Product.Review;

namespace App.Core.Interfaces;

public interface IProductReviewService
{
    Task<List<ProductReviewDto>?> GetReviewsBySellerIdAsync(string sellerId);
    Task<ProductReviewDto?> GetReviewByIdAsync(string reviewId);
    Task<ProductReviewDto?> GetReviewByProductIdAsync(string productId);
    Task<ProductReviewDto?> GetReviewByModelIdAsync(string modelId);
    Task CreateReviewAsync(ProductReviewCreateDto review);
    Task<bool> UpdateReviewAsync(ProductReviewDto review);
    Task<bool> DeleteReviewAsync(string reviewId);
    Task<List<ProductReviewCommentDto>?> GetCommentsByReviewIdAsync(string reviewId);
    Task AddCommentToReviewAsync(string reviewId, ProductReviewCommentCreateDto comment);
    Task<bool> UpdateCommentInReviewAsync(string reviewId, ProductReviewCommentDto comment);
    Task<bool> DeleteCommentFromReviewAsync(string reviewId, string commentId);
    Task AddReactionToCommentAsync(string reviewId, string commentId, ProductReviewCommentReactionDto reaction);
}