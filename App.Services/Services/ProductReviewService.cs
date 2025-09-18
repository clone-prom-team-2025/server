using App.Core.DTOs.Product.Review;
using App.Core.Interfaces;
using App.Core.Models.Product.Review;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
///     Provides services for managing product reviews and comments.
/// </summary>
public class ProductReviewService(
    IProductReviewRepository repository,
    IMapper mapper,
    IProductRepository productRepository, ILogger<ProductReviewService> logger) : IProductReviewService
{
    private readonly IMapper _mapper = mapper;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IProductReviewRepository _repository = repository;
    private readonly ILogger<ProductReviewService> _logger = logger;

    public async Task AddCommentToReviewByProductId(string productId, ProductReviewCommentCreateDto comment)
    {
        using (_logger.BeginScope("AddCommentToReviewByProductId")){
            _logger.LogInformation("AddCommentToReviewByProductId called");
            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
            if (product == null)
                throw new KeyNotFoundException("Product not found");
            var review = await _repository.GetByProductId(product.Id);
            if (review == null)
            {
                review = new ProductReview(product.Id);
                await _repository.CreateReview(review);
            }

            if (review.Comments.Any(c => c.UserId == ObjectId.Parse(comment.UserId)))
                throw new InvalidOperationException("Comment already exists");

            review.Comments.Add(_mapper.Map<ProductReviewComment>(comment));

            var result = await _repository.UpdateReview(review);
            if (!result)
            {
                _logger.LogError("UpdateReviewAsync failed");
                throw new InvalidOperationException("Update review failed");
            }
            _logger.LogInformation("UpdateReviewAsync success");
        }
    }

    public async Task RemoveCommentFromReviewByProductId(string productId, string userId)
    {
        using (_logger.BeginScope("RemoveCommentFromReviewByProductId")){
            _logger.LogInformation("RemoveCommentFromReviewByProductId called");
            var review = await _repository.GetByProductId(ObjectId.Parse(productId));
            if (review == null)
                throw new KeyNotFoundException("Review not found");

            var removedCount = review.Comments.RemoveAll(c => c.UserId == ObjectId.Parse(productId));

            if (removedCount == 0)
                throw new InvalidOperationException("Comment doesn't exist");

            var result = await _repository.UpdateReview(review);
            if (!result)
            {
                _logger.LogError("UpdateReviewAsync failed");
                throw new InvalidOperationException("Update review failed");
            }
            _logger.LogInformation("UpdateReviewAsync success");
        }
    }

    public async Task<ProductReviewDto?> GetReviewByProductId(string productId)
    {
        using (_logger.BeginScope("GetReviewByProductId")) {
            _logger.LogInformation("GetReviewByProductId called");
            var review = await _repository.GetByProductId(ObjectId.Parse(productId));
            if (review == null)
                throw new KeyNotFoundException("Product not found");

            var dto = _mapper.Map<ProductReviewDto>(review);


            dto.Rating = new ProductReviewRatingListDto
            {
                OneStar = review.Comments.Count(c => c.Rating == 1),
                TwoStar = review.Comments.Count(c => c.Rating == 2),
                ThreeStar = review.Comments.Count(c => c.Rating == 3),
                FourStar = review.Comments.Count(c => c.Rating == 4),
                FiveStar = review.Comments.Count(c => c.Rating == 5)
            };

            var averageRating = review.Comments.Any()
                ? review.Comments.Average(c => c.Rating)
                : 0;

            dto.AverageRating = averageRating;

            _logger.LogInformation("GetReviewByProductId success");
            return dto;
        }
    }

    public async Task<ProductReviewDto?> GetReviewById(string reviewId)
    {
        using (_logger.BeginScope("GetReviewById")) {
            _logger.LogInformation("GetReviewById called");
            var review = await _repository.GetReviewById(ObjectId.Parse(reviewId));
            if (review == null)
                throw new KeyNotFoundException("Review not found");

            var dto = _mapper.Map<ProductReviewDto>(review);

            dto.Rating = new ProductReviewRatingListDto
            {
                OneStar = review.Comments.Count(c => c.Rating == 1),
                TwoStar = review.Comments.Count(c => c.Rating == 2),
                ThreeStar = review.Comments.Count(c => c.Rating == 3),
                FourStar = review.Comments.Count(c => c.Rating == 4),
                FiveStar = review.Comments.Count(c => c.Rating == 5)
            };

            var averageRating = review.Comments.Any()
                ? review.Comments.Average(c => c.Rating)
                : 0;

            dto.AverageRating = averageRating;

            _logger.LogInformation("GetReviewById success");
            return dto;
        }
    }

    public async Task ClearAllReviewsByProductId(string productId)
    {
        using (_logger.BeginScope("ClearAllReviewsByProductId")){
            _logger.LogInformation("ClearAllReviewsByProductId called");
            var deleteResult = await _repository.DeleteReview(ObjectId.Parse(productId));
            if (!deleteResult)
                throw new InvalidOperationException("Can't delete review");
            var result = await _repository.CreateReview(new ProductReview(ObjectId.Parse(productId)));
            if (!result)
            {
                _logger.LogError("CreateReviewAsync failed");
                throw new InvalidOperationException("Create review failed");
            }
            _logger.LogInformation("CreateReviewAsync success");
        }
    }

    public async Task SetReactionToReviewComment(string productId, string commentUserId, string reactionUserId,
        bool reaction)
    {
        using (_logger.BeginScope("SetReactionToReviewComment")){
            _logger.LogInformation("SetReactionToReviewComment called");
            var review = await _repository.GetByProductId(ObjectId.Parse(productId));
            if (review == null)
                throw new KeyNotFoundException("Review not found");

            var comment = review.Comments.FirstOrDefault(c => c.UserId == ObjectId.Parse(commentUserId));
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            comment.Reactions[reactionUserId] = reaction;
            var result = await _repository.UpdateReview(review);
            if (!result)
            {
                _logger.LogError("UpdateReviewAsync failed");
                throw new InvalidOperationException("Update review failed");
            }
            _logger.LogInformation("SetReactionToReviewComment success");
        }
    }

    public async Task DeleteReactionToReviewComment(string productId, string commentUserId, string reactionUserId)
    {
        using (_logger.BeginScope("DeleteReactionToReviewComment")){
            _logger.LogInformation("DeleteReactionToReviewComment called");
            var review = await _repository.GetByProductId(ObjectId.Parse(productId));
            if (review == null)
                throw new KeyNotFoundException("Review not found");

            var comment = review.Comments.FirstOrDefault(c => c.UserId == ObjectId.Parse(commentUserId));
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (!comment.Reactions.ContainsKey(reactionUserId))
                throw new InvalidOperationException("Invalid reaction");

            comment.Reactions.Remove(reactionUserId);

            var result = await _repository.UpdateReview(review);
            if (!result)
            {
                _logger.LogError("UpdateReviewAsync failed");
                throw new InvalidOperationException("Update review failed");
            }
            _logger.LogInformation("UpdateReviewAsync success");
        }
    }

    public async Task<IEnumerable<ProductReviewCommentDto>?> GetAllCommentsByProductId(string productId)
    {
        using  (_logger.BeginScope("GetAllCommentsByProductId"))
        {
            _logger.LogInformation("GetAllCommentsByProductId called");
            var review = await _repository.GetByProductId(ObjectId.Parse(productId));
            if (review == null || !review.Comments.Any())
                throw new KeyNotFoundException("Review not found");

            var commentDtos = _mapper.Map<IEnumerable<ProductReviewCommentDto>>(review.Comments);
            _logger.LogInformation("GetAllCommentsByProductId success");
            return commentDtos;
        }
    }
}