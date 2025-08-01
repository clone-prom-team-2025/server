using App.Core.Models;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using App.Core.Models.User;
using App.Core.Models.Store.Review;
using App.Core.Models.Store;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace App.Data;

/// <summary>
///     MongoDB database context that provides access to collections
///     and handles initialization tasks like index creation.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MongoDbContext" /> class.
    ///     Establishes connection to the MongoDB database using provided settings.
    /// </summary>
    /// <param name="settings">MongoDB settings including connection string and database name.</param>
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    /// <summary>
    ///     Gets the MongoDB collection for <see cref="Category" /> documents.
    /// </summary>
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");

    /// <summary>
    /// Gets the MongoDB collection for <see cref="Product" /> documents.
    /// </summary>
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");

    public IMongoCollection<ProductMedia> ProductMedia => _database.GetCollection<ProductMedia>("ProductMedia");

    /// <summary>
    ///  Gets the MongoDB collection for <see cref="ProductReview" /> documents.
    /// </summary>
    public IMongoCollection<ProductReview> ProductReviews => _database.GetCollection<ProductReview>("ProductReviews");
    
    /// <summary>
    /// Gets the MongoDB collection for <see cref="User"/> document
    /// </summary>
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

    /// <summary>
    /// Gets the MongoDB collection for <see cref="Store"/> document
    /// </summary>
    public IMongoCollection<Store> Stores => _database.GetCollection<Store>("Stores");

    /// <summary>
    /// Gets the MongoDB collection for <see cref="StoreReview"/> document
    /// </summary>
    public IMongoCollection<StoreReview> StoreReviews => _database.GetCollection<StoreReview>("StoreReviews");

    /// <summary>
    ///     Ensures that the necessary indexes for the <see cref="Category"/> collection are created.
    ///     Specifically, creates a unique ascending index on the Name field if it does not already exist.
    /// </summary>
    public async Task CreateCategoryIndexesAsync()
    {
        var collection = Categories;

        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();

        var nameIndexExists = existingIndexes
            .Any(index => index["name"] == "Name_1");

        if (!nameIndexExists)
        {
            var indexKeys = Builders<Category>.IndexKeys.Ascending(c => c.Name);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Category>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }

    /// <summary>
    /// Ensures that the necessary indexes for the <see cref="Product"/> collection are created.
    /// Specifically, creates non-unique ascending indexes on SellerId and CategoryPath fields if they do not already exist.
    /// </summary>
    public async Task CreateProductIndexesAsync()
    {
        var collection = Products;

        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();

        var sellerIdIndexExists = existingIndexes
            .Any(index => index["name"] == "SellerId_1");

        var categoryPathIndexExists = existingIndexes
            .Any(index => index["name"] == "CategoryPath_1");


        if (!sellerIdIndexExists)
        {
            var indexKeys = Builders<Product>.IndexKeys.Ascending(p => p.SellerId);
            var indexOptions = new CreateIndexOptions { Unique = false };
            var indexModel = new CreateIndexModel<Product>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }

        if (!categoryPathIndexExists)
        {
            var indexKeys = Builders<Product>.IndexKeys.Ascending(p => p.CategoryPath);
            var indexOptions = new CreateIndexOptions { Unique = false };
            var indexModel = new CreateIndexModel<Product>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }

    /// <summary>
    /// Ensures that the necessary indexes for the <see cref="ProductReviews"/> collection are created.
    /// Specifically, creates non-unique ascending indexes on ProductId, ModelId and AverageRating fields if they do not already exist.
    /// </summary>
    public async Task CreateProductReviewIndexesAsync()
    {
        var collection = ProductReviews;
        
        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();
        
        var productIdIndexExists = existingIndexes
            .Any(index => index["name"] == "ProductId_1");
        
        var modelIdIndexExists = existingIndexes
            .Any(index => index["name"] == "ModelId_1");
        
        var averageRatingIndexExists = existingIndexes
            .Any(index => index["name"] == "AverageRating_1");

        if (!productIdIndexExists)
        {
            var indexKeys = Builders<ProductReview>.IndexKeys.Ascending(p => p.ProductId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<ProductReview>(indexKeys, indexOptions);
            
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        if (!modelIdIndexExists)
        {
            var indexKeys = Builders<ProductReview>.IndexKeys.Ascending(p => p.ModelId);
            var indexOptions = new CreateIndexOptions { Unique = false };
            var indexModel = new CreateIndexModel<ProductReview>(indexKeys, indexOptions);
            
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        if (!averageRatingIndexExists)
        {
            var indexKeys = Builders<ProductReview>.IndexKeys.Ascending(p => p.AverageRating);
            var indexOptions = new CreateIndexOptions { Unique = false };
            var indexModel = new CreateIndexModel<ProductReview>(indexKeys, indexOptions);
            
            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }

    /// <summary>
    /// Ensures that the necessary indexes for the <see cref="StoreReview"/> collection are created.
    /// Specifically, creates non-unique ascending indexes on StoreId and AverageRating fields if they do not already exist.
    /// </summary>
    public async Task CreateStoreReviewIndexesAsync()
    {
        var collection = StoreReviews;
        
        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();
        
        var storeIdReviewIndexExists = existingIndexes
            .Any(index => index["name"] == "StoreId_1");
        
        var averageRatingIndexExist = existingIndexes
            .Any(index => index["name"] == "AverageRating_1");

        if (!storeIdReviewIndexExists)
        {
            var indexKeys = Builders<StoreReview>.IndexKeys.Ascending(p => p.StoreId);
            var indexOptions = new CreateIndexOptions { Unique = false };
            var indexModel = new CreateIndexModel<StoreReview>(indexKeys, indexOptions);
            
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        if (!averageRatingIndexExist)
        {
            var indexKeys = Builders<StoreReview>.IndexKeys.Ascending(p => p.AverageRating);
            var indexOptions = new CreateIndexOptions { Unique = false };
            var indexModel = new CreateIndexModel<StoreReview>(indexKeys, indexOptions);
            
            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }
}