using App.Core.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace App.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }
    
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
    
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