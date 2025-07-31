using App.Core.DTOs.Product.Review;
using App.Core.Interfaces;
using App.Core.Models.Product.Review;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services;

/// <summary>
/// Provides services for managing product reviews and comments.
/// </summary>
public class ProductReviewService(IProductReviewRepository repository, IMapper mapper) : IProductReviewService
{
    private readonly IProductReviewRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Adds a new comment to an existing product review.
    /// </summary>
    /// <param name="reviewId">The ID of the review to which the comment is added.</param>
    /// <param name="comment">The comment data to add.</param>
    public async Task AddCommentToReviewAsync(string reviewId, ProductReviewCommentCreateDto comment)
    {
        var newComment = _mapper.Map<ProductReviewComment>(comment);
        newComment.Id = Guid.NewGuid().ToString();
        newComment.CreatedAt = DateTime.UtcNow;
        await _repository.AddCommentToReviewAsync(reviewId, newComment);
    }

    /// <summary>
    /// Adds a reaction to a specific comment within a product review.
    /// </summary>
    /// <param name="reviewId">The ID of the review containing the comment.</param>
    /// <param name="commentId">The ID of the comment to react to.</param>
    /// <param name="reaction">The reaction data.</param>
    public async Task AddReactionToCommentAsync(string reviewId, string commentId, ProductReviewCommentReactionDto reaction)
    {
        await _repository.AddReactionToCommentAsync(reviewId, commentId, _mapper.Map<ProductReviewCommentReaction>(reaction));
    }

    /// <summary>
    /// Creates a new product review.
    /// </summary>
    /// <param name="review">The review data to create.</param>
    public async Task CreateReviewAsync(ProductReviewCreateDto review)
    {
        var newReview = _mapper.Map<ProductReview>(review);
        newReview.Id = ObjectId.GenerateNewId();
        await _repository.CreateReviewAsync(newReview);
    }

    /// <summary>
    /// Deletes a comment from a product review.
    /// </summary>
    /// <param name="reviewId">The ID of the review containing the comment.</param>
    /// <param name="commentId">The ID of the comment to delete.</param>
    /// <returns>True if deleted successfully, otherwise false.</returns>
    public async Task<bool> DeleteCommentFromReviewAsync(string reviewId, string commentId)
    {
        return await _repository.DeleteCommentFromReviewAsync(reviewId, commentId);
    }

    /// <summary>
    /// Deletes a product review by its ID.
    /// </summary>
    /// <param name="reviewId">The ID of the review to delete.</param>
    /// <returns>True if deleted successfully, otherwise false.</returns>
    public async Task<bool> DeleteReviewAsync(string reviewId)
    {
        return await _repository.DeleteReviewAsync(reviewId);
    }

    /// <summary>
    /// Gets the list of comments for a specific review.
    /// </summary>
    /// <param name="reviewId">The ID of the review.</param>
    /// <returns>List of comments or null if not found.</returns>
    public async Task<List<ProductReviewCommentDto>?> GetCommentsByReviewIdAsync(string reviewId)
    {
        return _mapper.Map<List<ProductReviewCommentDto>?>(await _repository.GetCommentsByReviewIdAsync(reviewId));
    }

    /// <summary>
    /// Gets a product review by its ID.
    /// </summary>
    /// <param name="reviewId">The ID of the review.</param>
    /// <returns>The review data or null if not found.</returns>
    public async Task<ProductReviewDto?> GetReviewByIdAsync(string reviewId)
    {
        return _mapper.Map<ProductReviewDto?>(await _repository.GetReviewByIdAsync(reviewId));
    }

    /// <summary>
    /// Gets a product review by the model ID.
    /// </summary>
    /// <param name="modelId">The ID of the model associated with the review.</param>
    /// <returns>The review data or null if not found.</returns>
    public async Task<ProductReviewDto?> GetReviewByModelIdAsync(string modelId)
    {
        return _mapper.Map<ProductReviewDto?>(await _repository.GetReviewByModelIdAsync(modelId));
    }

    /// <summary>
    /// Gets a product review by the product ID.
    /// </summary>
    /// <param name="productId">The ID of the product associated with the review.</param>
    /// <returns>The review data or null if not found.</returns>
    public async Task<ProductReviewDto?> GetReviewByProductIdAsync(string productId)
    {
        return _mapper.Map<ProductReviewDto?>(await _repository.GetReviewByProductIdAsync(productId));
    }

    /// <summary>
    /// Gets a list of reviews for a specific seller.
    /// </summary>
    /// <param name="sellerId">The ID of the seller.</param>
    /// <returns>List of reviews or null if none found.</returns>
    public async Task<List<ProductReviewDto>?> GetReviewsBySellerIdAsync(string sellerId)
    {
        return _mapper.Map<List<ProductReviewDto>?>(await _repository.GetReviewsBySellerIdAsync(sellerId));
    }

    /// <summary>
    /// Updates a comment in a review.
    /// </summary>
    /// <param name="reviewId">The ID of the review containing the comment.</param>
    /// <param name="comment">The updated comment data.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    public async Task<bool> UpdateCommentInReviewAsync(string reviewId, ProductReviewCommentDto comment)
    {
        return await _repository.UpdateCommentInReviewAsync(reviewId, _mapper.Map<ProductReviewComment>(comment));
    }

    /// <summary>
    /// Updates an existing product review.
    /// </summary>
    /// <param name="review">The updated review data.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    public async Task<bool> UpdateReviewAsync(ProductReviewDto review)
    {
        return await _repository.UpdateReviewAsync(_mapper.Map<ProductReview>(review));
    }
}