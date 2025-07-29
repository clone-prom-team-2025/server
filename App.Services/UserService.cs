using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.User;
using AutoMapper;

namespace App.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        this._userRepository = userRepository;
        this._mapper = mapper;
    }
    
    public async Task<List<UserDto>?> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users?.Select(u => _mapper.Map<UserDto>(u)).ToList();
    }

    public async Task<List<UserDto>?> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        var users = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);
        return users?.Select(u => _mapper.Map<UserDto>(u)).ToList();
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);
        return _mapper.Map<UserDto>(user);
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