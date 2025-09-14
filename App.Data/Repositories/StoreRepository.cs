using App.Core.Interfaces;
using App.Core.Models.Store;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class StoreRepository(MongoDbContext context) : IStoreRepository
{
    private readonly IMongoCollection<Store> _storeCollection = context.Stores;

    public async Task CreateStore(Store model)
    {
        await _storeCollection.InsertOneAsync(model);
    }

    public async Task<bool> UpdateStore(Store model)
    {
        var filter = Builders<Store>.Filter.Eq(x => x.Id, model.Id);
        return (await _storeCollection.ReplaceOneAsync(filter, model)).IsAcknowledged;
    }

    public async Task<bool> DeleteStore(ObjectId storeId)
    {
        var filter = Builders<Store>.Filter.Eq(x => x.Id, storeId);
        var result = await _storeCollection.DeleteOneAsync(filter);
        return result.IsAcknowledged && result.DeletedCount > 0;
        ;
    }

    public async Task<Store?> GetStoreById(ObjectId storeId)
    {
        var filter = Builders<Store>.Filter.Eq(s => s.Id, storeId);
        return await _storeCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Store?> GetStoreByUserId(ObjectId userId)
    {
        var filter = Builders<Store>.Filter.Exists($"Roles.{userId}");
        return await _storeCollection.Find(filter).FirstOrDefaultAsync();
    }


    public async Task<List<Store>?> GetStores()
    {
        return await _storeCollection.Find(Builders<Store>.Filter.Empty).ToListAsync();
    }
}