using App.Core.Interfaces;
using App.Core.Models.User;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class UserBanRepository(MongoDbContext mongoDbContext) : IUserBanRepository
{
    private readonly IMongoCollection<UserBan> _userBans = mongoDbContext.UserBans;
    private readonly IMongoCollection<User> _users = mongoDbContext.Users;

    public async Task<List<UserBan>?> GetAllAsync()
    {
        var bans = await _userBans.Find(FilterDefinition<UserBan>.Empty).ToListAsync();
        return bans;
    }

    public async Task<List<UserBan>?> GetByUserIdAsync(ObjectId userId)
    {
        var filter = Builders<UserBan>.Filter.Eq(u => u.UserId, userId);
        var bans = await _userBans.Find(filter).ToListAsync();
        return bans;
    }

    public async Task<UserBan?> GetByIdAsync(string banId)
    {
        var filter = Builders<UserBan>.Filter.Eq(u => u.Id, ObjectId.Parse(banId));
        var ban = await _userBans.Find(filter).FirstOrDefaultAsync();
        return ban;
    }

    public async Task<bool> CreateAsync(UserBan userBan)
    {
        var userFilter = Builders<User>.Filter.Eq(u => u.Id, userBan.UserId);
        var user = await _users.Find(userFilter).FirstOrDefaultAsync();
        if (user == null) return false;

        await _userBans.InsertOneAsync(userBan);
        return true;
    }

    public async Task<bool> UpdateAsync(UserBan userBan)
    {
        var userFilter = Builders<User>.Filter.Eq(u => u.Id, userBan.Id);
        var user = await _users.Find(userFilter).FirstOrDefaultAsync();
        if (user == null) return false;

        var userBanFilter = Builders<UserBan>.Filter.Eq(u => u.Id, userBan.Id);
        var result = await _userBans.ReplaceOneAsync(userBanFilter, userBan);

        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteByUserIdAsync(ObjectId userId)
    {
        var userBanFilter = Builders<UserBan>.Filter.Eq(u => u.UserId, userId);
        var result = await _userBans.DeleteOneAsync(userBanFilter);

        return result.IsAcknowledged;
    }

    public async Task<bool> DeleteByIdAsync(ObjectId id)
    {
        var filter = Builders<UserBan>.Filter.Eq(u => u.Id, id);
        var result = await _userBans.DeleteOneAsync(filter);

        return result.IsAcknowledged;
    }
}