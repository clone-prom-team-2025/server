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
    private readonly ILogger<StoreCreateRequestRepository> _logger = logger;
    private readonly IMongoCollection<User> _users = mongoDbContext.Users;

    public async Task<bool> Create(StoreCreateRequest model)
    {
        using (_logger.BeginScope("Create"))
        {
            _logger.LogInformation("Create called with model");
            var userFilter = Builders<User>.Filter.Eq(u => u.Id, model.UserId);
            var user = _users.Find(userFilter).FirstOrDefault();
            if (user == null)
            {
                _logger.LogError("User not found");
                return false;
            }

            _logger.LogInformation("User found for UserId={userId}", model.UserId.ToString());
            await _collection.InsertOneAsync(model);
            _logger.LogInformation("Create succeeded for Store={store}", model);
            return true;
        }
    }

    public async Task<bool> Update(StoreCreateRequest model)
    {
        using (_logger.BeginScope("Update"))
        {
            _logger.LogInformation("Update called with model");
            var userFilter = Builders<User>.Filter.Eq(u => u.Id, model.UserId);
            var user = _users.Find(userFilter).FirstOrDefault();
            if (user == null)
            {
                _logger.LogError("User not found");
                return false;
            }

            _logger.LogInformation("User found for UserId={userId}", model.UserId.ToString());
            var filter = Builders<StoreCreateRequest>.Filter.Eq(u => u.Id, model.Id);
            await _collection.ReplaceOneAsync(filter, model);
            _logger.LogInformation("Update succeeded for Store={store}", model);
            return true;
        }
    }

    public async Task<bool> Delete(ObjectId requestId)
    {
        using (_logger.BeginScope("Delete"))
        {
            _logger.LogInformation("Delete called with requestId={requestId}", requestId.ToString());
            var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.Id, requestId);
            var result = await _collection.DeleteOneAsync(filter);
            _logger.LogInformation("Delete result for RequestId={requestId}: {IsAcknowledged}", requestId,
                result.IsAcknowledged);
            return result.IsAcknowledged;
        }
    }

    public async Task<List<StoreCreateRequest>?> GetAll()
    {
        using (_logger.BeginScope("GetAll"))
        {
            _logger.LogInformation("GetAll called");
            var filter = Builders<StoreCreateRequest>.Filter.Empty;
            var result = await _collection.Find(filter).ToListAsync();
            _logger.LogInformation("GetAll return {Count} requests", result.Count);
            return result;
        }
    }

    public async Task<StoreCreateRequest?> GetById(ObjectId id)
    {
        using (_logger.BeginScope("GetById"))
        {
            _logger.LogInformation("GetById called with id={id}", id.ToString());
            var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.Id, id);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            _logger.LogInformation("GetById result for id={id}: {result}", id.ToString(), result);
            return result;
        }
    }

    public async Task<StoreCreateRequest?> GetByUserId(ObjectId userId)
    {
        using (_logger.BeginScope("GetByUserId"))
        {
            _logger.LogInformation("GetByUserId called with userId={userId}", userId.ToString());
            var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.UserId, userId);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            _logger.LogInformation("GetByUserId result for UserId={userId}: {result}", userId.ToString(), result);
            return result;
        }
    }

    public async Task<List<StoreCreateRequest>?> GetApprovedByAdminId(ObjectId adminId)
    {
        using (_logger.BeginScope("GetApprovedByAdminId"))
        {
            _logger.LogInformation("GetApprovedByAdminId called with userId={adminId}", adminId.ToString());
            var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.ApprovedByAdminId, adminId);
            var result = await _collection.Find(filter).ToListAsync();
            _logger.LogInformation("GetApprovedByAdminId return {Count} requests", result.Count);
            return result;
        }
    }

    public async Task<List<StoreCreateRequest>?> GetRejectedByAdminId(ObjectId adminId)
    {
        using (_logger.BeginScope("GetRejectedByAdminId"))
        {
            _logger.LogInformation("GetRejectedByAdminId called with userId={adminId}", adminId.ToString());
            var filter = Builders<StoreCreateRequest>.Filter.Eq(s => s.RejectedByAdminId, adminId);
            var result = await _collection.Find(filter).ToListAsync();
            _logger.LogInformation("GetRejectedByAdminId return {Count} requests", result.Count);
            return result;
        }
    }
}