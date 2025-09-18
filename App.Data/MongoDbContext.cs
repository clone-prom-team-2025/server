using App.Core.Models;
using App.Core.Models.Auth;
using App.Core.Models.AvailableFilters;
using App.Core.Models.Cart;
using App.Core.Models.Favorite;
using App.Core.Models.Notification;
using App.Core.Models.Product;
using App.Core.Models.Product.Review;
using App.Core.Models.Sell;
using App.Core.Models.Store;
using App.Core.Models.User;
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
    ///     Gets the MongoDB collection for <see cref="Product" /> documents.
    /// </summary>
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");

    public IMongoCollection<ProductMedia> ProductMedia => _database.GetCollection<ProductMedia>("ProductMedia");

    /// <summary>
    ///     Gets the MongoDB collection for <see cref="ProductReview" /> documents.
    /// </summary>
    public IMongoCollection<ProductReview> ProductReviews => _database.GetCollection<ProductReview>("ProductReviews");

    /// <summary>
    ///     Gets the MongoDB collection for <see cref="User" /> document
    /// </summary>
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

    /// <summary>
    ///     Gets the MongoDB collection for <see cref="Store" /> document
    /// </summary>
    public IMongoCollection<Store> Stores => _database.GetCollection<Store>("Stores");

    /// <summary>
    ///     Gets the MongoDB collection for <see cref="AvailableFilters" />
    /// </summary>
    public IMongoCollection<AvailableFilters> AvailableFilters =>
        _database.GetCollection<AvailableFilters>("AvailableFilters");

    public IMongoCollection<UserSession> UserSessions => _database.GetCollection<UserSession>("UserSessions");

    public IMongoCollection<UserBan> UserBans => _database.GetCollection<UserBan>("UserBans");

    public IMongoCollection<StoreCreateRequest> StoreCreateRequests =>
        _database.GetCollection<StoreCreateRequest>("StoreCreateRequests");

    public IMongoCollection<Cart> Carts => _database.GetCollection<Cart>("Carts");

    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");

    public IMongoCollection<NotificationSeen> NotificationSees =>
        _database.GetCollection<NotificationSeen>("NotificationSees");

    public IMongoCollection<FavoriteProduct> FavoriteProducts =>
        _database.GetCollection<FavoriteProduct>("FavoriteProducts");

    public IMongoCollection<FavoriteSeller> FavoriteSellers =>
        _database.GetCollection<FavoriteSeller>("FavoriteSellers");
    
    public IMongoCollection<BuyInfo> BuyInfos => _database.GetCollection<BuyInfo>("BuyInfos");


    /// <summary>
    ///     Ensures that the necessary indexes for the <see cref="Category" /> collection are created.
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
            var indexOptions = new CreateIndexOptions { Unique = false };
            var indexModel = new CreateIndexModel<Category>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }

    /// <summary>
    ///     Ensures that the necessary indexes for the <see cref="Product" /> collection are created.
    ///     Specifically, creates non-unique ascending indexes on SellerId and Category fields if they do not already exist.
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
    ///     Ensures that the necessary indexes for the <see cref="ProductReviews" /> collection are created.
    ///     Specifically, creates non-unique ascending indexes on ProductId, ModelId and AverageRating fields if they do not
    ///     already exist.
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
    }

    /// <summary>
    ///     Ensures that the necessary indexes for the <see cref="User" /> collection are created.
    ///     Specifically, creates non-unique ascending indexes on Username and Email fields if they do not already exist.
    /// </summary>
    public async Task CreateUserIndexesAsync()
    {
        var collection = Users;

        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();

        var usernameIndexExists = existingIndexes
            .Any(index => index["name"] == "Username_1");

        var emailIndexExists = existingIndexes
            .Any(index => index["name"] == "Email_1");
    }

    public async Task CreateAvailableFiltersIndexesAsync()
    {
        var collection = AvailableFilters;

        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();

        var categoryIdIndexExists = existingIndexes
            .Any(index => index["name"] == "CategoryId_1");

        if (!categoryIdIndexExists)
        {
            var indexKeys = Builders<AvailableFilters>.IndexKeys.Ascending(p => p.CategoryId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<AvailableFilters>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }

    public async Task CreateStoreCreateRequestsIndexesAsync()
    {
        var collection = StoreCreateRequests;

        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();

        var userIdIndexExists = existingIndexes
            .Any(index => index["name"] == "UserId_1");
        var nameIndexExists = existingIndexes
            .Any(index => index["name"] == "Name_1");

        if (!userIdIndexExists)
        {
            var indexKeys = Builders<StoreCreateRequest>.IndexKeys.Ascending(p => p.UserId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<StoreCreateRequest>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }

        if (!nameIndexExists)
        {
            var indexKeys = Builders<StoreCreateRequest>.IndexKeys.Ascending(p => p.Name);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<StoreCreateRequest>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }
}