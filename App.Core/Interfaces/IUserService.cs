using App.Core.Enums;
using App.Core.Models.User;
using App.Core.DTOs;
using App.Core.DTOs.User;

namespace App.Core.Interfaces;

public interface IUserService
{
    Task<List<UserDto>?> GetAllUsersAsync();
    Task<List<UserDto>?> GetAllUsersAsync(int pageNumber, int pageSize);
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<User> GetUserByAvatarUrlAsync(string avatarUrl);
    Task<User> GetUserByPhoneNumberAsync(string phoneNumber);
    Task<List<User>?> GetUsersByRoleAsync(string role);
    Task<List<User>?> GetUsersByRoleAsync(string role, int pageNumber, int pageSize);
    Task CreateUserAsync(UserCreateDto user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    
    // additionalInfo
    Task<bool> UpdateUserAdditionalInfoByUserIdAsync(string userId, UserAdditionalInfo userAdditionalInfo);
    Task<bool> DeleteUserAdditionalInfoByUserIdAsync(string userId);
    Task<bool> SetUserPhoneNumberConfirmedAsync(string userId, string phoneNumber);
    Task<bool> SetUserEmailConfirmedAsync(string userId, string email);
    Task<UserAdditionalInfo?> GetUserAdditionalInfoByUserIdAsync(string userId);
    
    // block info
    Task SetUserBlockInfoByUserIdAsync(string userId, UserBlockInfo userBlockInfo);
    Task<bool> UpdateUserBlockInfoByUserIdAsync(string userId, UserBlockInfo userBlockInfo);
    Task<UserBlockInfo?> GetUserBlockInfoByUserIdAsync(string userId);
}