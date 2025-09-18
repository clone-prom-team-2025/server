using App.Core.DTOs.Product.Review;

namespace App.Core.Interfaces;

public interface IProductReviewService
{
    Task AddCommentToReviewByProductId(string productId, ProductReviewCommentCreateDto comment);
    Task RemoveCommentFromReviewByProductId(string productId, string userId);
    Task<ProductReviewDto?> GetReviewByProductId(string productId);
    Task<ProductReviewDto?> GetReviewById(string reviewId);
    Task ClearAllReviewsByProductId(string productId);
    Task SetReactionToReviewComment(string reviewId, string commentUserId, string reactionUserId, bool reaction);
    Task DeleteReactionToReviewComment(string reviewId, string commentUserId, string reactionUserId);
    Task<IEnumerable<ProductReviewCommentDto>?> GetAllCommentsByProductId(string productId);
}