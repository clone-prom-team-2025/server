using App.Core.Interfaces;
using App.Core.Models.Auth;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Data.Repositories;

public class UserSessionRepository(MongoDbContext mongoDbContext) : IUserSessionRepository
{
    private readonly IMongoCollection<UserSession> _collection = mongoDbContext.UserSessions;
    
    public async Task<UserSession> CreateSessionAsync(ObjectId userId, string deviceInfo)
    {
        var session = new UserSession()
        {
            CreatedAt = DateTime.UtcNow, 
            DeviceInfo = deviceInfo, 
            ExpiresAt = DateTime.UtcNow.AddHours(168),
            Id = ObjectId.GenerateNewId(), 
            UserId = userId, 
            IsRevoked = false
        };
        await _collection.InsertOneAsync(session);
        return session;
    }

    public async Task<UserSession> GetSessionAsync(ObjectId sessionId)
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
}