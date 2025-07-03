using App.Core.Models;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
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
    
    /// <summary>
    ///  Gets the MongoDB collection for <see cref="ProductReview" /> documents.
    /// </summary>
    public IMongoCollection<ProductReview> ProductReviews => _database.GetCollection<ProductReview>("ProductReviews");

    /// <summary>
    ///     Ensures that the necessary indexes for the Category collection are created.
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
    /// Ensures that the necessary indexes for the Product collection are created.
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
}