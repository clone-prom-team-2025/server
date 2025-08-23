using App.Core.Models.Auth;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSession?> CreateSessionAsync(ObjectId userId, string deviceInfo);
    Task<bool> ReplaceSessionsAsync(ObjectId userId, List<UserSession> sessions);
    Task<UserSession?> GetSessionAsync(ObjectId sessionId);
    Task<List<UserSession>?> GetSessionsAsync(ObjectId userId);
    Task RevokeSessionAsync(ObjectId sessionId);
}
