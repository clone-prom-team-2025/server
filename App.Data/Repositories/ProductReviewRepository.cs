using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

/// <summary>
///     Repository for managing product reviews and related comments in MongoDB.
///     Provides CRUD operations for reviews, comments, and reactions.
/// </summary>
public class ProductReviewRepository(ProductRepository productRepository, MongoDbContext mongoDbContext)
    : IProductReviewRepository
{
    private readonly IMongoCollection<ProductReview> _reviews = mongoDbContext.ProductReviews;

    /// <summary>
    ///     Retrieves all product reviews associated with all products of a specific seller.
    /// </summary>
    /// <param name="sellerId">The ID of the seller whose products' reviews to fetch.</param>
    /// <returns>List of product reviews or null if none found.</returns>
    public async Task<List<ProductReview>?> GetReviewsBySellerIdAsync(ObjectId sellerId)
    {
        var products = await productRepository.GetBySellerIdAsync(sellerId, new ProductFilterRequest());
        if (products == null || !products.Any()) return null;

        List<ProductReview>? reviews = new();
        foreach (var product in products)
        {
            var filter = Builders<ProductReview>.Filter.Eq(r => r.ProductId, product.Id);
            var temp = await _reviews.Find(r => r.ProductId.Equals(product.Id)).ToListAsync();
            foreach (var review in temp) reviews.Add(review);
        }

        return reviews;
    }

    /// <summary>
    ///     Retrieves a product review by its unique review ID.
    /// </summary>
    /// <param name="reviewId">The review ID (MongoDB ObjectId as string).</param>
    /// <returns>The product review or null if not found.</returns>
    public async Task<ProductReview?> GetReviewByIdAsync(ObjectId reviewId)
    {
        var filter = Builders<ProductReview>.Filter.Eq(r => r.Id, reviewId);
        return await _reviews.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Retrieves a product review by the associated product ID.
    /// </summary>
    /// <param name="productId">The product ID to find the review for.</param>
    /// <returns>The product review or null if not found.</returns>
    public async Task<ProductReview?> GetReviewByProductIdAsync(ObjectId productId)
    {
        var filters = Builders<ProductReview>.Filter.Eq(r => r.ProductId, productId);
        return await _reviews.Find(filters).FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Inserts a new product review document into the database.
    /// </summary>
    /// <param name="review">The product review to create.</param>
    public async Task CreateReviewAsync(ProductReview review)
    {
        await _reviews.InsertOneAsync(review);
    }

    /// <summary>
    ///     Updates an existing product review document.
    /// </summary>
    /// <param name="review">The updated product review.</param>
    /// <returns>True if update was acknowledged and modified a document; otherwise false.</returns>
    public async Task<bool> UpdateReviewAsync(ProductReview review)
    {
        var filter = Builders<ProductReview>.Filter.Eq(r => r.Id, review.Id);
        var result = await _reviews.ReplaceOneAsync(filter, review);
        return result.IsAcknowledged;
    }

    /// <summary>
    ///     Deletes a product review by its review ID.
    /// </summary>
    /// <param name="reviewId">The review ID to delete.</param>
    /// <returns>True if deletion was acknowledged and deleted a document; otherwise false.</returns>
    public async Task<bool> DeleteReviewAsync(ObjectId reviewId)
    {
        var filter = Builders<ProductReview>.Filter.Eq(r => r.Id, reviewId);
        var result = await _reviews.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    /// <summary>
    ///     Retrieves all comments for a given product review by review ID.
    /// </summary>
    /// <param name="reviewId">The review ID whose comments to retrieve.</param>
    /// <returns>List of review comments or null if review not found.</returns>
    public async Task<List<ProductReviewComment>?> GetCommentsByReviewIdAsync(ObjectId reviewId)
    {
        var filter = Builders<ProductReview>.Filter.Eq(r => r.Id, reviewId);
        var projection = Builders<ProductReview>.Projection.Expression(r => r.Comments);
        var comments = await _reviews.Find(filter).Project(projection).FirstOrDefaultAsync();

        return comments ?? null;
    }

    /// <summary>
    ///     Adds a new comment to an existing product review.
    /// </summary>
    /// <param name="reviewId">The review ID to add the comment to.</param>
    /// <param name="comment">The comment to add.</param>
    /// <returns>True if update was successful; otherwise false.</returns>
    public async Task<bool> AddCommentToReviewAsync(ObjectId reviewId, ProductReviewComment comment)
    {
        var productReview = await GetReviewByIdAsync(reviewId);
        if (productReview == null) return false;
        productReview.Comments.Add(comment);

        return await UpdateReviewAsync(productReview);
    }

    /// <summary>
    ///     Updates an existing comment within a product review.
    /// </summary>
    /// <param name="reviewId">The review ID containing the comment.</param>
    /// <param name="comment">The updated comment object (must have existing comment ID).</param>
    /// <returns>True if update was successful; otherwise false.</returns>
    public async Task<bool> UpdateCommentInReviewAsync(ObjectId reviewId, ProductReviewComment comment)
    {
        var productReview = await GetReviewByIdAsync(reviewId);
        if (productReview == null) return false;

        var index = productReview.Comments.FindIndex(c => c.Id == comment.Id);
        if (index == -1)
            return false;

        productReview.Comments[index] = comment;

        return await UpdateReviewAsync(productReview);
    }

    /// <summary>
    ///     Deletes a comment from a product review.
    /// </summary>
    /// <param name="reviewId">The review ID containing the comment.</param>
    /// <param name="commentId">The comment ID to delete.</param>
    /// <returns>True if deletion was successful; otherwise false.</returns>
    public async Task<bool> DeleteCommentFromReviewAsync(ObjectId reviewId, string commentId)
    {
        var productReview = await GetReviewByIdAsync(reviewId);
        if (productReview == null) return false;

        var index = productReview.Comments.FindIndex(c => c.Id == commentId);

        if (index == -1)
            return false;

        productReview.Comments.RemoveAt(index);
        return await UpdateReviewAsync(productReview);
    }

    /// <summary>
    ///     Adds a reaction (like/dislike) to a specific comment in a product review.
    /// </summary>
    /// <param name="reviewId">The review ID containing the comment.</param>
    /// <param name="commentId">The comment ID to add the reaction to.</param>
    /// <param name="reaction">The reaction to add.</param>
    /// <returns>True if update was successful; otherwise false.</returns>
    public async Task<bool> AddReactionToCommentAsync(ObjectId reviewId, string commentId,
        ProductReviewCommentReaction reaction)
    {
        var productReview = await GetReviewByIdAsync(reviewId);
        if (productReview == null)
            return false;

        var comment = productReview.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null)
            return false;

        comment.AddReaction(reaction);

        return await UpdateReviewAsync(productReview);
    }
}