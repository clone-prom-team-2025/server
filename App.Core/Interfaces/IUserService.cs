using App.Core.DTOs.User;

namespace App.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>?> GetAllUsersAsync();
    Task<IEnumerable<UserDto>?> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<UserDto>?> GetUsersByRoleAsync(string role);
    Task<IEnumerable<UserDto>?> GetUsersByRoleAsync(string role, int pageNumber, int pageSize);
    Task DeleteUserAsync(string userId, string code);
    Task SendDeleteAccountCodeAsync(string userId);
    Task BanUser(UserBanCreateDto userBlockInfo, string adminId);
    Task UnbanUserByBanId(string banId, string adminId);
    Task<IEnumerable<UserBanDto>> GetUserBansByUserId(string userId);

    Task UpdateUser(string userId, UpdateUserDto dto);
    Task<bool> CreateAdminAsync(string email, string password, string fullName);
}