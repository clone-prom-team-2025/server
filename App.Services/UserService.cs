using System.ComponentModel;
using System.Reflection;
using App.Core.Constants;
using App.Core.DTOs.User;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Email;
using App.Core.Models.FileStorage;
using App.Core.Models.User;
using App.Core.Utils;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services;

public class UserService(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IUserBanRepository userBanRepository,
    IMapper mapper,
    ILogger<UserService> logger,
    IMemoryCache cache,
    IEmailService emailService,
    IFileService fileService,
    ISessionHubNotifier sessionHubNotifier,
    IFavoriteSellerRepository favoriteSellerRepository,
    IFavoriteProductRepository favoriteProductRepository,
    ICartRepository cartRepository) : IUserService
{
    private readonly IMemoryCache _cache = cache;
    private readonly ICartRepository _cartRepository = cartRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly IFavoriteProductRepository _favoriteProductRepository = favoriteProductRepository;
    private readonly IFavoriteSellerRepository _favoriteSellerRepository = favoriteSellerRepository;
    private readonly IFileService _fileService = fileService;
    private readonly ILogger<UserService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly ISessionHubNotifier _sessionHubNotifier = sessionHubNotifier;
    private readonly IUserBanRepository _userBanRepository = userBanRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserSessionRepository _userSessionRepository = userSessionRepository;

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
            return _mapper.Map<IEnumerable<UserDto>?>(users);
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
                throw new KeyNotFoundException("User not found");
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
                throw new KeyNotFoundException("User not found");
            }

            _logger.LogInformation("Fetched user with username {Username}", username);
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
        using (_logger.BeginScope("GetUsersByRoleAsync(Role={role}, PageNumber={pageNumber}, PageSize={pageSize})",
                   role, pageNumber,
                   pageSize))
        {
            _logger.LogInformation("Fetching users with pagination");
            var users = await _userRepository.GetUsersByRoleAsync(role, pageNumber, pageSize);
            if (users == null || !users.Any())
            {
                _logger.LogInformation("No users found on page {PageNumber}", pageNumber);
                return null;
            }

            _logger.LogInformation("Fetched {Count} users on page {PageNumber}", users.Count, pageNumber);
            return _mapper.Map<IEnumerable<UserDto>?>(users);
        }
    }

    public async Task DeleteUserAsync(string userId, string code)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            throw new KeyNotFoundException("User not found");
        }

        var cacheKey = $"delete-account:{userId}";

        if (_cache.TryGetValue(cacheKey, out string? stored))
        {
            if (string.Equals(stored, code, StringComparison.OrdinalIgnoreCase))
            {
                _cache.Remove(cacheKey);

                await _favoriteSellerRepository.DeleteAllByUserIdAsync(user.Id);
                await _favoriteProductRepository.DeleteAllByUserIdAsync(user.Id);
                await _cartRepository.DeleteByUserIdAsync(user.Id);
                await _userSessionRepository.DeleteAllSessionsAsync(user.Id);

                var result = await _userRepository.DeleteUserAsync(user.Id);
                if (!result)
                {
                    _logger.LogWarning("User with ID {UserId} not found", user.Id);
                    throw new KeyNotFoundException("User not found");
                }


                _logger.LogInformation("User with ID {UserId} successfully deleted", user.Id);
            }
            else
            {
                _logger.LogWarning("Invalid code");
            }
        }

        throw new InvalidOperationException("Invalid code");
    }

    public async Task SendDeleteAccountCodeAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
            throw new KeyNotFoundException("User not found");
        }

        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("App.Services.EmailTemplates.DeleteAccount.html");
        using var reader = new StreamReader(stream!);
        var html = await reader.ReadToEndAsync();

        var code = CodeGenerator.GenerateCode(6);
        var readyHtml = html.Replace("__CODE__", code).Replace("__TIME__", "15");


        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(15));
        _cache.Set($"delete-account:{userId}", code, cacheEntryOptions);

        var mail = new EmailMessage
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [user.Email!],
            Subject = "Delete Account",
            HtmlBody = readyHtml
        };

        await _emailService.SendEmailAsync(mail);
    }

    public async Task BanUser(UserBanCreateDto userBlockInfo, string adminId)
    {
        if (string.IsNullOrWhiteSpace(userBlockInfo?.UserId))
        {
            _logger.LogWarning("BanUser called with null or empty adminId/UserId");
            throw new InvalidOperationException("UserId is required");
        }

        using (_logger.BeginScope("BanUser: AdminId={AdminId}, UserId={UserId}", adminId, userBlockInfo.UserId))
        {
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userBlockInfo.UserId));
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userBlockInfo.UserId);
                throw new KeyNotFoundException("User not found");
            }

            _logger.LogInformation("BanUser called");

            if (userBlockInfo.UserId == adminId)
            {
                _logger.LogWarning("Admin tried to ban themselves");
                throw new InvalidOperationException("You can't ban yourself");
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

            var session = await _userSessionRepository.GetSessionAsync(ban.Id);
            if (userBlockInfo.Types.HasFlag(BanType.Login))
                if (session != null)
                    await _sessionHubNotifier.ForceLogoutAsync(session.Id.ToString());

            _logger.LogDebug("Ban object to insert: {@Ban}", _mapper.Map<UserBanDto>(ban));

            var result = await _userBanRepository.CreateAsync(ban);
            _logger.LogInformation("Ban created successfully: {Result}", result);
        }
    }

    public async Task UnbanUserByBanId(string banId, string adminId)
    {
        if (string.IsNullOrWhiteSpace(adminId) || string.IsNullOrWhiteSpace(banId))
            _logger.LogWarning("UnbanUserByBanId called with null or empty adminId/BanId");

        using (_logger.BeginScope("UnbanUserByBanId: AdminId={AdminId}, BanId={BanId}", adminId, banId))
        {
            var ban = await _userBanRepository.GetByIdAsync(banId);
            if (ban == null)
            {
                _logger.LogWarning("Ban not found for BanId={BanId}", banId);
                throw new KeyNotFoundException("Ban not found");
            }

            if (ban.UserId.ToString() == adminId)
            {
                _logger.LogWarning("Admin tried to unban themselves");
                throw new InvalidOperationException("You can't unban yourself");
            }

            var result = await _userBanRepository.DeleteByIdAsync(ObjectId.Parse(banId));
            _logger.LogInformation("Unban result for BanId={BanId}: {Result}", banId, result);
        }
    }

    public async Task<IEnumerable<UserBanDto>> GetUserBansByUserId(string userId)
    {
        using (_logger.BeginScope("GetUserBansByUserId: UserId={UserId}", userId))
        {
            _logger.LogInformation("GetUserBansByUserId called");

            var bans = await _userBanRepository.GetByUserIdAsync(ObjectId.Parse(userId));
            var dtos = _mapper.Map<IEnumerable<UserBanDto>>(bans);

            _logger.LogDebug("Fetched {Count} bans for UserId={UserId}", dtos.Count(), userId ?? "null");

            return dtos;
        }
    }

    public async Task UpdateUser(string userId, UpdateUserDto dto)
    {
        using (_logger.BeginScope("UpdateUser: UserId={userId}, Dto", userId))
        {
            _logger.LogInformation("UpdateUser called");

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User not found");
                throw new KeyNotFoundException("User not found");
            }

            if (dto.Username != null) user.Username = dto.Username;
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.Gender != null) user.Gender = dto.Gender;
            if (dto.DateOfBirth != null) user.DateOfBirth = dto.DateOfBirth;
            if (dto.Avatar != null)
            {
                await _fileService.DeleteFileAsync("user-avatars", user.Avatar.SourceFileName);
                await _fileService.DeleteFileAsync("user-avatars", user.Avatar.CompressedFileName!);
                BaseFile file = new();
                var stream = dto.Avatar.OpenReadStream();
                (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) =
                    await _fileService.SaveImageAsync(stream, dto.Avatar.FileName, "user-avatars");
                user.Avatar = file;
            }

            var result = await _userRepository.UpdateUserAsync(user);
            if (!result)
            {
                _logger.LogWarning("Failed to update user");
                throw new InvalidOperationException("Failed to update user");
            }

            _logger.LogInformation("User successfully updated");
        }
    }

    public async Task<bool> CreateAdminAsync(string email, string password, string fullName, string? username, Stream? file, string? fileName)
    {
        using (_logger.BeginScope("CreateAdminAsync"))
        {
            _logger.LogInformation("Creating admin");
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                _logger.LogInformation("User with email {Email} already exists", email);
                return false;
            }

            var index = email.IndexOf('@');
            username ??= email.Substring(0, index);

            var normalizedEmail = email.ToLower();
            
            var id = ObjectId.GenerateNewId();
            BaseFile avatar = new();

            if (file == null && fileName == null)
            {
                await using var image = AvatarGenerator.ByteToStream(AvatarGenerator.CreateAvatar(fullName));
                (avatar.SourceUrl, avatar.CompressedUrl, avatar.SourceFileName, avatar.CompressedFileName) =
                    await _fileService.SaveImageAsync(image, id + "-avatar", "user-avatars");
            }
            else if (file != null && fileName != null)
            {
                (avatar.SourceUrl, avatar.CompressedUrl, avatar.SourceFileName, avatar.CompressedFileName) =
                    await _fileService.SaveImageAsync(file, fileName, "user-avatars");
            }
            var admin = new User(username, password, normalizedEmail,
                avatar, [RoleNames.User, RoleNames.Admin], fullName)
            {
                Id = id
            };
            
            Console.WriteLine(avatar.ToJson());

            await _userRepository.CreateUserAsync(admin);

            return true;
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
                throw new KeyNotFoundException("User not found");
            }

            _logger.LogInformation("Fetched user with email {Email}", email);
            return _mapper.Map<UserDto?>(user);
        }
    }

    public async Task<bool> UpdateUserAsync(UserDto user)
    {
        using (_logger.BeginScope("UpdateUserAsync(UserDto)"))
        {
            var result = await _userRepository.UpdateUserAsync(_mapper.Map<User>(user));
            if (!result)
            {
                _logger.LogWarning("User with ID {UserId} not found", user.Id);
                throw new InvalidOperationException("Can't update user");
            }

            _logger.LogInformation("Updated user by Id={id}", user.Id);
            return result;
        }
    }

    public async Task SetUserRoleAsync(string userId, string role)
    {
        using (_logger.BeginScope("SetAdminAsync"))
        {
            _logger.LogInformation("SetAdminAsync called");
            if (role != RoleNames.Admin && role != RoleNames.User)
            {
                _logger.LogWarning("Role doesn't exist");    
                throw new InvalidOperationException("Role doesn't exist");
            }

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw new KeyNotFoundException("User not found");
            }

            if (user.Roles.Contains(role))
            {
                _logger.LogInformation("User with ID {UserId} already {role}", user.Id, role);
                throw new InvalidAsynchronousStateException($"User already {role}");
            }
            
            user.Roles.Add(role);
            var result = await _userRepository.UpdateUserAsync(user);
            if (!result)
            {
                _logger.LogWarning("Failed to update user");
                throw new InvalidOperationException("Failed to update user");
            }
            
            var sessions = await _userSessionRepository.GetSessionsAsync(user.Id);
            if (sessions != null)
            {
                foreach (var session in sessions) session.Roles.Add(role);
                await _userSessionRepository.ReplaceSessionsAsync(user.Id, sessions);            
            }
                
            
            _logger.LogInformation("User with ID {UserId} updated", user.Id);
        }
    }
    
    public async Task DeleteUserRoleAsync(string userId, string role)
    {
        using (_logger.BeginScope("DeleteUserRoleAsync"))
        {
            _logger.LogInformation("DeleteUserRoleAsync called");
            if (role != RoleNames.Admin && role != RoleNames.User)
            {
                _logger.LogWarning("Role doesn't exist");    
                throw new InvalidOperationException("Role doesn't exist");
            }

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw new KeyNotFoundException("User not found");
            }

            if (!user.Roles.Contains(role))
            {
                _logger.LogInformation("User with ID {UserId} doesn't have role {role}", user.Id, role);
                throw new InvalidAsynchronousStateException($"User doesn't have role {role}");
            }
            
            user.Roles.Remove(role);
            var result = await _userRepository.UpdateUserAsync(user);
            if (!result)
            {
                _logger.LogWarning("Failed to update user");
                throw new InvalidOperationException("Failed to update user");
            }
            
            var sessions = await _userSessionRepository.GetSessionsAsync(user.Id);
            if (sessions != null)
            {
                foreach (var session in sessions) session.Roles.Add(role);
                await _userSessionRepository.ReplaceSessionsAsync(user.Id, sessions);            
            }
                
            
            _logger.LogInformation("User with ID {UserId} updated", user.Id);
        }
    }

    public async Task DeleteAdminAsync(string userId)
    {
        using (_logger.BeginScope("DeleteAdminAsync"))
        {
            _logger.LogInformation("DeleteAdminAsync called");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw new KeyNotFoundException("User not found");
            }

            if (!user.Roles.Contains(RoleNames.Admin))
            {
                _logger.LogWarning("User is not admin");
                throw new InvalidOperationException("User is not admin");
            }
            await _userSessionRepository.DeleteSessionsAsync(user.Id);
            await _fileService.DeleteFileAsync("user-avatars", user.Avatar.SourceFileName);
            if (user.Avatar.CompressedFileName != null) await _fileService.DeleteFileAsync("user-avatars", user.Avatar.CompressedFileName);
            await _userRepository.DeleteUserAsync(user.Id);
            _logger.LogInformation("User with ID {UserId} deleted", user.Id);
        }
    }
}