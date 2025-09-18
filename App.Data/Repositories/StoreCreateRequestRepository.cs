using App.Core.Interfaces;
using App.Core.Models.Store;
using App.Core.Models.User;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class StoreCreateRequestRepository(MongoDbContext mongoDbContext, ILogger<StoreCreateRequestRepository> logger)
    : IStoreCreateRequestRepository
{
    private readonly IMongoCollection<StoreCreateRequest> _collection = mongoDbContext.StoreCreateRequests;
    private readonly IMongoCollection<User> _users = mongoDbContext.Users;

    public async Task<bool> Create(StoreCreateRequest model)
    {
        var userFilter = Builders<User>.Filter.Eq(u => u.Id, model.UserId);
        var user = _users.Find(userFilter).FirstOrDefault();
        if (user == null) return false;

        await _collection.InsertOneAsync(model);
        return true;
    }

    public async Task<bool> Update(StoreCreateRequest model)
    {
        var userFilter = Builders<User>.Filter.Eq(u => u.Id, model.UserId);
        var user = _users.Find(userFilter).FirstOrDefault();
        if (user == null) return false;

        var filter = Builders<StoreCreateRequest>.Filter.Eq(u => u.Id, model.Id);
        await _collection.ReplaceOneAsync(filter, model);
        return true;
    }

    public async Task<bool> Delete(ObjectId requestId)
    {
        var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.Id, requestId);
        var result = await _collection.DeleteOneAsync(filter);
        return result.IsAcknowledged;
    }

    public async Task<List<StoreCreateRequest>?> GetAll()
    {
        var filter = Builders<StoreCreateRequest>.Filter.Empty;
        var result = await _collection.Find(filter).ToListAsync();
        return result;
    }

    public async Task<StoreCreateRequest?> GetById(ObjectId id)
    {
        var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.Id, id);
        var result = await _collection.Find(filter).FirstOrDefaultAsync();
        return result;
    }

    public async Task<StoreCreateRequest?> GetByUserId(ObjectId userId)
    {
        var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.UserId, userId);
        var result = await _collection.Find(filter).FirstOrDefaultAsync();
        return result;
    }

    public async Task<List<StoreCreateRequest>?> GetApprovedByAdminId(ObjectId adminId)
    {
        var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.ApprovedByAdminId, adminId);
        var result = await _collection.Find(filter).ToListAsync();
        return result;
    }

    public async Task<List<StoreCreateRequest>?> GetRejectedByAdminId(ObjectId adminId)
    {
        var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.RejectedByAdminId, adminId);
        var result = await _collection.Find(filter).ToListAsync();
        return result;
    }
}