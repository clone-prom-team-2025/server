using App.Core.Models.User;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IUserBanRepository
{
    Task<List<UserBan>?> GetAllAsync();
    Task<List<UserBan>?> GetByUserIdAsync(ObjectId userId);
    Task<UserBan?> GetByIdAsync(string banId);
    Task<bool> CreateAsync(UserBan userBan);

    Task<bool> UpdateAsync(UserBan userBan);
    Task<bool> DeleteByUserIdAsync(ObjectId userId);
    Task<bool> DeleteByIdAsync(ObjectId id);
}