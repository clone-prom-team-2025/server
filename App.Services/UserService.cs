using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.User;
using App.Core.DTOs;
using App.Core.DTOs.User;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUserBanRepository _userBanRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IUserSessionRepository userSessionRepository, IUserBanRepository userBanRepository, IMapper mapper, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userSessionRepository = userSessionRepository;
        _userBanRepository = userBanRepository;
        _logger = logger;
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

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        return _mapper.Map<UserDto>(await _userRepository.GetUserByUsernameAsync(username));
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        return _mapper.Map<UserDto?>(await _userRepository.GetUserByEmailAsync(email));
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

    public async Task CreateUserAsync(UserCreateDto user)
    {
        await _userRepository.CreateUserAsync(_mapper.Map<User>(user));
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
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

    public Task<bool> DeleteUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> BanUser(UserBanCreateDto userBlockInfo, string adminId)
    {
        if (string.IsNullOrWhiteSpace(adminId) || string.IsNullOrWhiteSpace(userBlockInfo?.UserId))
        {
            _logger.LogWarning("BanUser called with null or empty adminId/UserId");
            return false;
        }

        using (_logger.BeginScope("BanUser: AdminId={AdminId}, UserId={UserId}", adminId, userBlockInfo.UserId))
        {
            _logger.LogInformation("BanUser called");

            if (userBlockInfo.UserId == adminId)
            {
                _logger.LogWarning("Admin tried to ban themselves");
                return false;
            }

            _logger.LogDebug("Creating ban for UserId={UserId} until {BannedUntil} with reason: {Reason}",
                userBlockInfo.UserId ?? "null",
                userBlockInfo.BannedUntil?.ToString("o") ?? "null",
                userBlockInfo.Reason ?? "null");

            var parsedUserId = ObjectId.Parse(userBlockInfo.UserId);
            var ban = new UserBan
            {
                AdminId = ObjectId.Parse(adminId),
                BannedAt = DateTime.UtcNow,
                BannedUntil = userBlockInfo.BannedUntil,
                Id = ObjectId.GenerateNewId(),
                Reason = userBlockInfo.Reason ?? string.Empty,
                Types = userBlockInfo.Types,
                UserId = parsedUserId
            };
            
            _logger.LogDebug("Ban object to insert: {@Ban}", _mapper.Map<UserBanDto>(ban));

            var result = await _userBanRepository.CreateAsync(ban);
            _logger.LogInformation("Ban created successfully: {Result}", result);

            return result;
        }
    }

    public async Task<bool> UnbanUserByBanId(string banId, string adminId)
    {
        if (string.IsNullOrWhiteSpace(adminId) || string.IsNullOrWhiteSpace(banId))
        {
            _logger.LogWarning("UnbanUserByBanId called with null or empty adminId/BanId");
            return false;
        }

        using (_logger.BeginScope("UnbanUserByBanId: AdminId={AdminId}, BanId={BanId}", adminId, banId))
        {
            _logger.LogInformation("UnbanUserByBanId called");

            var ban = await _userBanRepository.GetByIdAsync(banId);
            if (ban == null)
            {
                _logger.LogWarning("Ban not found for BanId={BanId}", banId);
                return false;
            }

            if (ban.UserId.ToString() == adminId)
            {
                _logger.LogWarning("Admin tried to unban themselves");
                return false;
            }

            var result = await _userBanRepository.DeleteByIdAsync(ObjectId.Parse(banId));
            _logger.LogInformation("Unban result for BanId={BanId}: {Result}", banId, result);

            return result;
        }
    }

    public async Task<IEnumerable<UserBanDto>> GetUserBansByUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetUserBansByUserId called with null or empty UserId");
            return Enumerable.Empty<UserBanDto>();
        }

        using (_logger.BeginScope("GetUserBansByUserId: UserId={UserId}", userId))
        {
            _logger.LogInformation("GetUserBansByUserId called");

            var bans = await _userBanRepository.GetByUserIdAsync(ObjectId.Parse(userId));
            var dtos = _mapper.Map<IEnumerable<UserBanDto>>(bans);

            _logger.LogDebug("Fetched {Count} bans for UserId={UserId}", dtos.Count(), userId ?? "null");

            return dtos;
        }
    }
}