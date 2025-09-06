using App.Core.DTOs.Product.Review;
using App.Core.Interfaces;
using App.Core.Models.Product.Review;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services;

/// <summary>
///     Provides services for managing product reviews and comments.
/// </summary>
public class ProductReviewService(
    IProductReviewRepository repository,
    IMapper mapper,
    IProductRepository productRepository) : IProductReviewService
{
    private readonly IMapper _mapper = mapper;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IProductReviewRepository _repository = repository;

    public async Task AddCommentToReviewByProductId(string productId, ProductReviewCommentCreateDto comment)
    {
        var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
        if (product == null)
            throw new Exception("Product not found");
        var review = await _repository.GetByProductId(product.Id);
        if (review == null)
        {
            review = new ProductReview(product.Id);
            await _repository.CreateReview(review);
        }

        if (review.Comments.Any(c => c.UserId == ObjectId.Parse(comment.UserId)))
           throw new Exception("Comment already exists");

        review.Comments.Add(_mapper.Map<ProductReviewComment>(comment));

        var result = await _repository.UpdateReview(review);
    }

    public async Task RemoveCommentFromReviewByProductId(string productId, string userId)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            throw new Exception("Review not found");

        var removedCount = review.Comments.RemoveAll(c => c.UserId == ObjectId.Parse(productId));

        if (removedCount == 0)
            throw new Exception("Comment doesn't exist");

        await _repository.UpdateReview(review);
    }

    public async Task<ProductReviewDto?> GetReviewByProductId(string productId)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            throw new Exception("Product not found");

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

        return dto;
    }

    public async Task<ProductReviewDto?> GetReviewById(string reviewId)
    {
        var review = await _repository.GetReviewById(ObjectId.Parse(reviewId));
        if (review == null)
            throw new Exception("Review not found");

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

        return dto;
    }

    public async Task ClearAllReviewsByProductId(string productId)
    {
        var deleteResult = await _repository.DeleteReview(ObjectId.Parse(productId));
        if (!deleteResult)
            throw new Exception("Can't delete review");
        var createResult = await _repository.CreateReview(new ProductReview(ObjectId.Parse(productId)));
    }

    public async Task SetReactionToReviewComment(string productId, string commentUserId, string reactionUserId,
        bool reaction)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            throw new Exception("Review not found");

        var comment = review.Comments.FirstOrDefault(c => c.UserId == ObjectId.Parse(commentUserId));
        if (comment == null)
            throw new Exception("Comment not found");

        comment.Reactions[reactionUserId] = reaction;

        await _repository.UpdateReview(review);
    }

    public async Task DeleteReactionToReviewComment(string productId, string commentUserId, string reactionUserId)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null)
            throw new Exception("Review not found");

        var comment = review.Comments.FirstOrDefault(c => c.UserId == ObjectId.Parse(commentUserId));
        if (comment == null)
            throw new Exception("Comment not found");

        if (!comment.Reactions.ContainsKey(reactionUserId))
            throw new Exception("Invalid reaction");

        comment.Reactions.Remove(reactionUserId);

        await _repository.UpdateReview(review);
    }

    public async Task<IEnumerable<ProductReviewCommentDto>?> GetAllCommentsByProductId(string productId)
    {
        var review = await _repository.GetByProductId(ObjectId.Parse(productId));
        if (review == null || !review.Comments.Any())
            throw new Exception("Review not found");

        var commentDtos = _mapper.Map<IEnumerable<ProductReviewCommentDto>>(review.Comments);

        return commentDtos;
    }
}