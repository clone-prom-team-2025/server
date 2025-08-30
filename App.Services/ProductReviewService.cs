using App.Core.DTOs.Product.Review;
using App.Core.Interfaces;
using App.Core.Models.Product.Review;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services;

/// <summary>
///     Provides services for managing product reviews and comments.
/// </summary>
public class ProductReviewService(IProductReviewRepository repository, IMapper mapper, IProductRepository productRepository) : IProductReviewService
{
    private readonly IMapper _mapper = mapper;
    private readonly IProductReviewRepository _repository = repository;
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<bool> AddCommentToReviewByProductId(string productId, ProductReviewCommentCreateDto comment)
    {
        var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
        if (product == null)
            return false;
        var review = await _repository.GetByProductId(product.Id);
        if (review == null)
        {
            review = new ProductReview(product.Id);
            await _repository.CreateReview(review);
        }

        if (review.Comments.Any(c => c.UserId == ObjectId.Parse(comment.UserId)))
            return false;

        review.Comments.Add(_mapper.Map<ProductReviewComment>(comment));

        var result = await _repository.UpdateReview(review);
        return result;
    }

    public async Task<bool> RemoveCommentFromReviewByProductId(string productId, string userId)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            return false;

        if (!ObjectId.TryParse(userId, out var objectUserId))
            return false;

        var removedCount = review.Comments.RemoveAll(c => c.UserId == objectUserId);

        if (removedCount == 0)
            return false;

        await _repository.UpdateReview(review);

        return true;
    }
    
    public async Task<ProductReviewDto?> GetReviewByProductId(string productId)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            return null;

        var dto = _mapper.Map<ProductReviewDto>(review);

        
        
        dto.Rating = new ProductReviewRatingListDto
        {
            OneStar = review.Comments.Count(c => c.Rating == 1),
            TwoStar = review.Comments.Count(c => c.Rating == 2),
            ThreeStar = review.Comments.Count(c => c.Rating == 3),
            FourStar = review.Comments.Count(c => c.Rating == 4),
            FiveStar = review.Comments.Count(c => c.Rating == 5),
        };
        
        var averageRating = review.Comments.Any()
            ? review.Comments.Average(c => c.Rating)
            : 0;
        
        dto.AverageRating = averageRating;

        return dto;
    }

    public async Task<ProductReviewDto?> GetReviewById(string reviewId)
    {
        var review = await _repository.GetReviewById(ObjectId.Parse(reviewId));
        if (review == null)
            return null;

        var dto = _mapper.Map<ProductReviewDto>(review);

        dto.Rating = new ProductReviewRatingListDto
        {
            OneStar = review.Comments.Count(c => c.Rating == 1),
            TwoStar = review.Comments.Count(c => c.Rating == 2),
            ThreeStar = review.Comments.Count(c => c.Rating == 3),
            FourStar = review.Comments.Count(c => c.Rating == 4),
            FiveStar = review.Comments.Count(c => c.Rating == 5),
        };
        
        var averageRating = review.Comments.Any()
            ? review.Comments.Average(c => c.Rating)
            : 0;
        
        dto.AverageRating = averageRating;

        return dto;
    }

    public async Task<bool> ClearAllReviewsByProductId(string productId)
    {
        var deleteResult = await _repository.DeleteReview(ObjectId.Parse(productId));
        if (!deleteResult)
            return false;
        var createResult = await _repository.CreateReview(new ProductReview(ObjectId.Parse(productId)));
        return createResult;
    }

    public async Task<bool> SetReactionToReviewComment(string productId, string commentUserId, string reactionUserId, bool reaction)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            return false;

        if (!ObjectId.TryParse(commentUserId, out var commentUserObjectId))
            return false;

        var comment = review.Comments.FirstOrDefault(c => c.UserId == commentUserObjectId);
        if (comment == null)
            return false;

        comment.Reactions[reactionUserId] = reaction;

        await _repository.UpdateReview(review);

        return true;
    }
    
    public async Task<bool> DeleteReactionToReviewComment(string productId, string commentUserId, string reactionUserId)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            return false;

        if (!ObjectId.TryParse(commentUserId, out var commentUserObjectId))
            return false;

        var comment = review.Comments.FirstOrDefault(c => c.UserId == commentUserObjectId);
        if (comment == null)
            return false;

        if (!comment.Reactions.ContainsKey(reactionUserId))
            return false;

        comment.Reactions.Remove(reactionUserId);

        await _repository.UpdateReview(review);

        return true;
    }

    public async Task<IEnumerable<ProductReviewCommentDto>?> GetAllCommentsByProductId(string productId)
    {
        if (!ObjectId.TryParse(productId, out var productObjectId))
            return Enumerable.Empty<ProductReviewCommentDto>();

        var review = await _repository.GetByProductId(productObjectId);
        if (review == null || !review.Comments.Any())
            return Enumerable.Empty<ProductReviewCommentDto>();

        var commentDtos = _mapper.Map<IEnumerable<ProductReviewCommentDto>>(review.Comments);

        return commentDtos;
    }
}