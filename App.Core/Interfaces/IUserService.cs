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
    Task<bool> DeleteUserAsync(string userId, string code);
    Task<bool> SendDeleteAccountCodeAsync(string userId);
    Task<bool> BanUser(UserBanCreateDto userBlockInfo, string adminId);
    Task<bool> UnbanUserByBanId(string banId, string adminId);
    Task<IEnumerable<UserBanDto>> GetUserBansByUserId(string userId);

    Task<bool> UpdateUser(string userId, UpdateUserDto dto);
}