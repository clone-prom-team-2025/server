using App.Core.Models.User;
using MongoDB.Bson;

namespace App.Core.Interfaces;

public interface IUserRepository
{
    Task<List<User>?> GetAllUsersAsync();
    Task<List<User>?> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<User?> GetUserByIdAsync(ObjectId userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
    Task<List<User>?> GetUsersByRoleAsync(string role);
    Task<List<User>?> GetUsersByRoleAsync(string role, int pageNumber, int pageSize);
    Task CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(ObjectId userId);
    Task<List<string>> GetUserRolesAsync(ObjectId userId);

    // additionalInfo
    Task<bool> UpdateUserAdditionalInfoByUserIdAsync(ObjectId userId, UserAdditionalInfo userAdditionalInfo);
    Task<bool> DeleteUserAdditionalInfoByUserIdAsync(ObjectId userId);
    Task<UserAdditionalInfo?> GetUserAdditionalInfoByUserIdAsync(ObjectId userId);
}