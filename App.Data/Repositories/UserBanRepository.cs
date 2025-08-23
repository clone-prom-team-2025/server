using App.Core.Interfaces;
using App.Core.Models.User;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace App.Data.Repositories;

public class UserBanRepository : IUserBanRepository
{
    private readonly IMongoCollection<UserBan> _userBans;
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UserBanRepository> _logger;

    public UserBanRepository(MongoDbContext mongoDbContext, ILogger<UserBanRepository> logger)
    {
        _userBans = mongoDbContext.UserBans;
        _users = mongoDbContext.Users;
        _logger = logger;
    }
    
    public async Task<List<UserBan>?> GetAllAsync()
    {
        _logger.LogInformation("GetAllAsync called");
        var bans = await _userBans.Find(FilterDefinition<UserBan>.Empty).ToListAsync();
        _logger.LogInformation("GetAllAsync returned {Count} bans", bans?.Count ?? 0);
        return bans;
    }

    public async Task<List<UserBan>?> GetByUserIdAsync(ObjectId userId)
    {
        _logger.LogInformation("GetByUserIdAsync called for UserId={UserId}", userId);
        var filter = Builders<UserBan>.Filter.Eq(u => u.UserId, userId);
        var bans = await _userBans.Find(filter).ToListAsync();
        _logger.LogInformation("GetByUserIdAsync found {Count} bans for UserId={UserId}", bans?.Count ?? 0, userId);
        return bans;
    }

    public async Task<UserBan?> GetByIdAsync(string banId)
    {
        _logger.LogInformation("GetByIdAsync called for BanId={BanId}", banId);
        var filter = Builders<UserBan>.Filter.Eq(u => u.Id, ObjectId.Parse(banId));
        var ban = await _userBans.Find(filter).FirstOrDefaultAsync();
        _logger.LogInformation("GetByIdAsync result: {@Ban}", ban);
        return ban;
    }

    public async Task<bool> CreateAsync(UserBan userBan)
    {
        _logger.LogInformation("CreateAsync called for UserId={UserId}", userBan.UserId);

        var userFilter = Builders<User>.Filter.Eq(u => u.Id, userBan.UserId);
        var user = await _users.Find(userFilter).FirstOrDefaultAsync();
        if (user == null)
        {
            _logger.LogWarning("CreateAsync failed: user not found for Id={UserBanId}", userBan.UserId);
            return false;
        }

        await _userBans.InsertOneAsync(userBan);
        _logger.LogInformation("CreateAsync succeeded for UserBan: {@UserBan}", userBan);
        return true;
    }

    public async Task<bool> UpdateAsync(UserBan userBan)
    {
        _logger.LogInformation("UpdateAsync called for UserBanId={UserBanId}", userBan.Id);

        var userFilter = Builders<User>.Filter.Eq(u => u.Id, userBan.Id);
        var user = await _users.Find(userFilter).FirstOrDefaultAsync();
        if (user == null)
        {
            _logger.LogWarning("UpdateAsync failed: user not found for UserBanId={UserBanId}", userBan.Id);
            return false;
        }

        var userBanFilter = Builders<UserBan>.Filter.Eq(u => u.Id, userBan.Id);
        var result = await _userBans.ReplaceOneAsync(userBanFilter, userBan);
        _logger.LogInformation("UpdateAsync result for UserBanId={UserBanId}: {IsAcknowledged}", userBan.Id, result.IsAcknowledged);

        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteByUserIdAsync(ObjectId userId)
    {
        _logger.LogInformation("DeleteByUserIdAsync called for UserId={UserId}", userId);

        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var user = await _users.Find(filter).FirstOrDefaultAsync();
        if (user == null)
        {
            _logger.LogWarning("DeleteByUserIdAsync failed: user not found for UserId={UserId}", userId);
            return false;
        }

        var userBanFilter = Builders<UserBan>.Filter.Eq(u => u.UserId, userId);
        var result = await _userBans.DeleteOneAsync(userBanFilter);
        _logger.LogInformation("DeleteByUserIdAsync result for UserId={UserId}: {IsAcknowledged}", userId, result.IsAcknowledged);

        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteByIdAsync(ObjectId id)
    {
        _logger.LogInformation("DeleteByIdAsync called for UserBanId={UserBanId}", id);

        var filter = Builders<UserBan>.Filter.Eq(u => u.Id, id);
        var result = await _userBans.DeleteOneAsync(filter);

        _logger.LogInformation("DeleteByIdAsync result for UserBanId={UserBanId}: {IsAcknowledged}", id, result.IsAcknowledged);
        return result.IsAcknowledged;
    }
}
