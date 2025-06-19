using App.Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace App.Data;

/// <summary>
/// MongoDB database context that provides access to collections
/// and handles initialization tasks like index creation.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoDbContext"/> class.
    /// Establishes connection to the MongoDB database using provided settings.
    /// </summary>
    /// <param name="settings">MongoDB settings including connection string and database name.</param>
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    /// <summary>
    /// Gets the MongoDB collection for <see cref="Category"/> documents.
    /// </summary>
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");

    /// <summary>
    /// Ensures that the necessary indexes for the Category collection are created.
    /// Specifically, creates a unique ascending index on the Name field if it does not already exist.
    /// </summary>
    public async Task CreateCategoryIndexesAsync()
    {
        var collection = Categories;

        var existingIndexesCursor = await collection.Indexes.ListAsync();
        var existingIndexes = await existingIndexesCursor.ToListAsync();

        bool nameIndexExists = existingIndexes
            .Any(index => index["name"] == "Name_1");

        if (!nameIndexExists)
        {
            var indexKeys = Builders<Category>.IndexKeys.Ascending(c => c.Name);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Category>(indexKeys, indexOptions);

            await collection.Indexes.CreateOneAsync(indexModel);
        }
    }
}