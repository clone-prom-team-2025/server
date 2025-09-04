using App.Core.DTOs.User;
using App.Core.Interfaces;
using App.Core.Models.User;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly IUserBanRepository _userBanRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public UserService(IUserRepository userRepository, IUserSessionRepository userSessionRepository,
        IUserBanRepository userBanRepository, IMapper mapper, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _userSessionRepository = userSessionRepository;
        _userBanRepository = userBanRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>?> GetAllUsersAsync()
    {
        using (_logger.BeginScope("GetAllUsersAsync"))
        {
            _logger.LogInformation("Fetching all users without pagination");
            var users = await _userRepository.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                _logger.LogInformation("No users found");
                return null;
            }

            _logger.LogInformation("Fetched {Count} users", users.Count);
            return _mapper.Map<IEnumerable<UserDto>?>(users);
        }
    }

    public async Task<IEnumerable<UserDto>?> GetAllUsersAsync(int pageNumber, int pageSize)
    {
        using (_logger.BeginScope("GetAllUsersAsync(PageNumber={pageNumber}, PageSize={pageSize})", pageNumber,
                   pageSize))
        {
            _logger.LogInformation("Fetching users with pagination");
            var users = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);
            if (users == null || !users.Any())
            {
                _logger.LogInformation("No users found on page {PageNumber}", pageNumber);
                return null;
            }

            _logger.LogInformation("Fetched {Count} users on page {PageNumber}", users.Count, pageNumber);
            return users.Select(u => _mapper.Map<UserDto>(u)).ToList();
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        using (_logger.BeginScope("GetUserByIdAsync(UserId={userId})", userId))
        {
            _logger.LogInformation("Fetching user by ID");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return null;
            }

            _logger.LogInformation("Fetched user with ID {UserId}", userId);
            return _mapper.Map<UserDto?>(user);
        }
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        using (_logger.BeginScope("GetUserByUsernameAsync(Username={username})", username))
        {
            _logger.LogInformation("Fetching user by username");
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User with username {Username} not found", username);
                return null;
            }

            _logger.LogInformation("Fetched user with username {Username}", username);
            return _mapper.Map<UserDto?>(user);
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        using (_logger.BeginScope("GetUserByEmailAsync(Email={email})", email))
        {
            _logger.LogInformation("Fetching user by email");
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", email);
                return null;
            }

            _logger.LogInformation("Fetched user with email {Email}", email);
            return _mapper.Map<UserDto?>(user);
        }
    }

    public async Task<IEnumerable<UserDto>?> GetUsersByRoleAsync(string role)
    {
        using (_logger.BeginScope("GetUsersByRoleAsync(Role={role})", role))
        {
            _logger.LogInformation("Fetching users with pagination");
            var users = await _userRepository.GetUsersByRoleAsync(role);
            if (users == null || !users.Any())
            {
                _logger.LogInformation("No users found");
                return null;
            }

            _logger.LogInformation("Fetched {Count} users", users.Count);
            return _mapper.Map<IEnumerable<UserDto>?>(users);
        }
    }

    public async Task<IEnumerable<UserDto>?> GetUsersByRoleAsync(string role, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
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

    public async Task<bool> DeleteUserAsync(User user)
    {
        throw new NotImplementedException();
    }
}