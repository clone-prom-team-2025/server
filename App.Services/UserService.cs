using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.User;

namespace App.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        this._userRepository = userRepository;
    }
    
    public async Task<List<User>?> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<List<User>?> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        return await _userRepository.GetAllUsersAsync(pageNumber, pageSize);
    }

    public async Task<User> GetUserByIdAsync(string userId)
    {
        return await _userRepository.GetUserByIdAsync(userId);
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<User> GetUserByAvatarUrlAsync(string avatarUrl)
    {
        throw new NotImplementedException();
    }

    public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        throw new NotImplementedException();
    }

    public async Task<List<User>?> GetUsersByRoleAsync(string role)
    {
        throw new NotImplementedException();
    }

    public async Task<List<User>?> GetUsersByRoleAsync(string role, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task CreateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<List<UserRole>> GetUserRolesAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserAdditionalInfoByUserIdAsync(string id, UserAdditionalInfo userAdditionalInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteUserAdditionalInfoByUserIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SetUserPhoneNumberConfirmedAsync(string userId, string phoneNumber)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SetUserEmailConfirmedAsync(string userId, string email)
    {
        throw new NotImplementedException();
    }

    public async Task<UserAdditionalInfo?> GetUserAdditionalInfoByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task SetUserBlockInfoByUserIdAsync(string userId, UserBlockInfo userBlockInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserBlockInfoByUserIdAsync(string userId, UserBlockInfo userBlockInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<UserBlockInfo?> GetUserBlockInfoByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserAsync(string userId)
    {
        throw new NotImplementedException();
    }
}