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
    private readonly IMongoCollection<Product> _productCollection = mongoDbContext.Products;
    private readonly IMongoCollection<ProductReview> _reviewCollection = mongoDbContext.ProductReviews;

    public async Task<bool> CreateReview(ProductReview review)
    {
        var productFilter = Builders<Product>.Filter.Where(p => p.Id == review.ProductId);
        if (!await _productCollection.Find(productFilter).Limit(1).AnyAsync()) return false;

        await _reviewCollection.InsertOneAsync(review);
        return true;
    }

    public async Task<bool> DeleteReview(ObjectId id)
    {
        var reviewFilter = Builders<ProductReview>.Filter.Where(p => p.Id == id);
        var result = await _reviewCollection.DeleteOneAsync(reviewFilter);

        if (result.IsAcknowledged && result.DeletedCount > 0) return true;

        return false;
    }

    public async Task<bool> UpdateReview(ProductReview review)
    {
        var productFilter = Builders<Product>.Filter.Where(p => p.Id == review.ProductId);
        if (!await _productCollection.Find(productFilter).Limit(1).AnyAsync()) return false;


        var reviewFilter = Builders<ProductReview>.Filter.Where(p => p.Id == review.Id);
        var result = await _reviewCollection.ReplaceOneAsync(reviewFilter, review);

        if (result.IsAcknowledged && result.ModifiedCount > 0) return true;

        return false;
    }

    public async Task<ProductReview?> GetReviewById(ObjectId id)
    {
        var review = await _reviewCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        return review;
    }

    public async Task<ProductReview?> GetByProductId(ObjectId productId)
    {
        var productFilter = Builders<Product>.Filter.Where(p => p.Id == productId);
        if (!await _productCollection.Find(productFilter).Limit(1).AnyAsync()) return null;

        var reviewFilter = Builders<ProductReview>.Filter.Where(r => r.ProductId == productId);
        var review = await _reviewCollection.Find(reviewFilter).FirstOrDefaultAsync();
        return review;
    }

    public async Task<List<ProductReview>?> GetAll()
    {
        var reviews = await _reviewCollection.Find(FilterDefinition<ProductReview>.Empty).ToListAsync();
        return reviews;
    }
}