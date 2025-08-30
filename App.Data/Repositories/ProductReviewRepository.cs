using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class ProductReviewRepository(MongoDbContext mongoDbContext, ILogger<ProductReviewRepository> logger)
    : IProductReviewRepository
{
    private readonly IMongoCollection<ProductReview> _reviewCollection = mongoDbContext.ProductReviews;
    private readonly IMongoCollection<Product> _productCollection = mongoDbContext.Products;
    private readonly ILogger<ProductReviewRepository> _logger = logger;

    public async Task<bool> CreateReview(ProductReview review)
    {
        _logger.LogInformation("CreateReview called for ProductId={ProductId}", review.ProductId);

        var productFilter = Builders<Product>.Filter.Where(p => p.Id == review.ProductId);
        if (!await _productCollection.Find(productFilter).Limit(1).AnyAsync())
        {
            _logger.LogWarning("CreateReview failed: ProductId={ProductId} not found", review.ProductId);
            return false;
        }

        await _reviewCollection.InsertOneAsync(review);
        _logger.LogInformation("CreateReview completed for ProductId={ProductId}, ReviewId={ReviewId}", review.ProductId, review.Id);
        return true;
    }

    public async Task<bool> DeleteReview(ObjectId id)
    {
        _logger.LogInformation("DeleteReview called for ReviewId={ReviewId}", id);

        var reviewFilter = Builders<ProductReview>.Filter.Where(p => p.Id == id);
        var result = await _reviewCollection.DeleteOneAsync(reviewFilter);

        if (result.IsAcknowledged && result.DeletedCount > 0)
        {
            _logger.LogInformation("DeleteReview succeeded for ReviewId={ReviewId}", id);
            return true;
        }
        else
        {
            _logger.LogWarning("DeleteReview failed for ReviewId={ReviewId}", id);
            return false;
        }
    }

    public async Task<bool> UpdateReview(ProductReview review)
    {
        _logger.LogInformation("UpdateReview called for ReviewId={ReviewId}, ProductId={ProductId}", review.Id, review.ProductId);

        var productFilter = Builders<Product>.Filter.Where(p => p.Id == review.ProductId);
        if (!await _productCollection.Find(productFilter).Limit(1).AnyAsync())
        {
            _logger.LogWarning("UpdateReview failed: ProductId={ProductId} not found", review.ProductId);
            return false;
        }

        var reviewFilter = Builders<ProductReview>.Filter.Where(p => p.Id == review.Id);
        var result = await _reviewCollection.ReplaceOneAsync(reviewFilter, review);

        if (result.IsAcknowledged && result.ModifiedCount > 0)
        {
            _logger.LogInformation("UpdateReview succeeded for ReviewId={ReviewId}", review.Id);
            return true;
        }
        else
        {
            _logger.LogWarning("UpdateReview failed for ReviewId={ReviewId}", review.Id);
            return false;
        }
    }

    public async Task<ProductReview?> GetReviewById(ObjectId id)
    {
        _logger.LogInformation("GetReviewById called for ReviewId={ReviewId}", id);
        var review = await _reviewCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        _logger.LogInformation("GetReviewById returned {Found}", review != null);
        return review;
    }

    public async Task<ProductReview?> GetByProductId(ObjectId productId)
    {
        _logger.LogInformation("GetByProductId called for ProductId={ProductId}", productId);

        var productFilter = Builders<Product>.Filter.Where(p => p.Id == productId);
        if (!await _productCollection.Find(productFilter).Limit(1).AnyAsync())
        {
            _logger.LogWarning("GetByProductId failed: ProductId={ProductId} not found", productId);
            return null;
        }

        var reviewFilter = Builders<ProductReview>.Filter.Where(r => r.ProductId == productId);
        var review = await _reviewCollection.Find(reviewFilter).FirstOrDefaultAsync();
        _logger.LogInformation("GetByProductId returned {Found}", review != null);
        return review;
    }

    public async Task<List<ProductReview>?> GetAll()
    {
        _logger.LogInformation("GetAll called");
        var reviews = await _reviewCollection.Find(FilterDefinition<ProductReview>.Empty).ToListAsync();
        _logger.LogInformation("GetAll returned {Count} reviews", reviews?.Count ?? 0);
        return reviews;
    }
}
