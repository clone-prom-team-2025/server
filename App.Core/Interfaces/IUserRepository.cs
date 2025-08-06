using App.Core.Enums;
using App.Core.Models.User;

namespace App.Core.Interfaces;

public interface IUserRepository
{
    Task<List<User>?> GetAllUsersAsync();
    Task<List<User>?> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByAvatarUrlAsync(UserAvatar avatarUrl);
    Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
    Task<List<User>?> GetUsersByRoleAsync(string role);
    Task<List<User>?> GetUsersByRoleAsync(string role, int pageNumber, int pageSize);
    Task CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    
    // additionalInfo
    Task<bool> UpdateUserAdditionalInfoByUserIdAsync(string userId, UserAdditionalInfo userAdditionalInfo);
    Task<bool> DeleteUserAdditionalInfoByUserIdAsync(string userId);
    Task<UserAdditionalInfo?> GetUserAdditionalInfoByUserIdAsync(string userId);
    
    // block info
    Task<bool> UpdateUserBlockInfoByUserIdAsync(string userId, UserBlockInfo userBlockInfo);
    Task<UserBlockInfo?> GetUserBlockInfoByUserIdAsync(string userId);
}