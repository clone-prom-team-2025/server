using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Auth;
using App.Core.Models.User;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class UserSessionRepository(MongoDbContext mongoDbContext, IOptions<SessionsOptions> options)
    : IUserSessionRepository
{
    private readonly IMongoCollection<UserSession> _collection = mongoDbContext.UserSessions;
    private readonly SessionsOptions _options = options.Value;

    public async Task<UserSession?> CreateSessionAsync(ObjectId userId, DeviceInfo deviceInfo)
    {
        var userFilter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var user = await mongoDbContext.Users.Find(userFilter).FirstOrDefaultAsync();
        if (user == null) return null;
        var session = new UserSession
        {
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = deviceInfo,
            ExpiresAt = DateTime.UtcNow.AddHours(_options.ExpiresIn),
            Id = ObjectId.GenerateNewId(),
            UserId = userId,
            IsRevoked = false,
            Roles = [..user.Roles],
            Banned = BanType.Comments,
            BannedUntil = null
        };
        await _collection.InsertOneAsync(session);
        return session;
    }

    public async Task<bool> ReplaceSessionsAsync(ObjectId userId, List<UserSession> sessions)
    {
        var filter = Builders<UserSession>.Filter.Eq(u => u.UserId, userId);
        await _collection.DeleteManyAsync(filter);
        if (sessions.Count > 0)
            await _collection.InsertManyAsync(sessions);
        return true;
    }

    public async Task<UserSession?> GetSessionAsync(ObjectId sessionId)
    {
        var filter = Builders<UserSession>.Filter.Eq(s => s.Id, sessionId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<UserSession>?> GetSessionsAsync(ObjectId userId)
    {
        var filter = Builders<UserSession>.Filter.Eq(s => s.UserId, userId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task RevokeSessionAsync(ObjectId sessionId)
    {
        var filter = Builders<UserSession>.Filter.Eq(s => s.Id, sessionId);
        var update = Builders<UserSession>.Update.Set(s => s.IsRevoked, true);
        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteSessionAsync(ObjectId sessionId)
    {
        var filter = Builders<UserSession>.Filter.Eq(s => s.Id, sessionId);
        await _collection.DeleteOneAsync(filter);
    }

    public async Task DeleteSessionsAsync(ObjectId userId)
    {
        var filter = Builders<UserSession>.Filter.Eq(s => s.UserId, userId);
        await _collection.DeleteManyAsync(filter);
    }

    public async Task<bool> DeleteAllSessionsAsync(ObjectId userId)
    {
        var filter = Builders<UserSession>.Filter.Eq(s => s.UserId, userId);
        return (await _collection.DeleteManyAsync(filter)).DeletedCount > 0;
    }
}