using App.Core.DTOs.Product.Review;

namespace App.Core.Interfaces;

public interface IProductReviewService
{
    Task<bool> AddCommentToReviewByProductId(string productId, ProductReviewCommentCreateDto comment);
    Task<bool> RemoveCommentFromReviewByProductId(string productId, string userId);
    Task<ProductReviewDto?> GetReviewByProductId(string productId);
    Task<ProductReviewDto?> GetReviewById(string reviewId);
    Task<bool> ClearAllReviewsByProductId(string productId);

    Task<bool> SetReactionToReviewComment(string reviewId, string commentUserId, string reactionUserId, bool reaction);
    Task<bool> DeleteReactionToReviewComment(string reviewId, string commentUserId, string reactionUserId);
    Task<IEnumerable<ProductReviewCommentDto>?> GetAllCommentsByProductId(string productId);
}