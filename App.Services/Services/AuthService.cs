using System.Reflection;
using App.Core.Constants;
using App.Core.DTOs.Auth;
using App.Core.Exceptions;
using App.Core.Interfaces;
using App.Core.Models.Auth;
using App.Core.Models.Email;
using App.Core.Models.Favorite;
using App.Core.Models.FileStorage;
using App.Core.Models.User;
using App.Core.Utils;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
///     Service for handling authentication and user sessions.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    IMapper mapper,
    IFileService fileService,
    IEmailService emailService,
    IMemoryCache memoryCache,
    IUserSessionRepository sessionRepository,
    IOptions<SessionsOptions> options,
    ISessionHubNotifier sessionHubNotifier,
    IFavoriteSellerRepository favoriteSellerRepository,
    IFavoriteProductRepository favoriteProductRepository)
    : IAuthService
{
    private static readonly Random Random = new();
    private readonly IMemoryCache _cache = memoryCache;
    private readonly IEmailService _emailService = emailService;
    private readonly IFavoriteProductRepository _favoriteProductRepository = favoriteProductRepository;
    private readonly IFavoriteSellerRepository _favoriteSellerRepository = favoriteSellerRepository;
    private readonly IFileService _fileService = fileService;
    private readonly IMapper _mapper = mapper;
    private readonly SessionsOptions _options = options.Value;
    private readonly ISessionHubNotifier _sessionHubNotifier = sessionHubNotifier;
    private readonly IUserSessionRepository _sessionRepository = sessionRepository;
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Authenticates a user by email or username and issues a new or existing session token.
    /// </summary>
    /// <param name="model">The login request containing credentials.</param>
    /// <param name="deviceInfo">The device information for the session.</param>
    public async Task<string?> LoginAsync(LoginDto model, DeviceInfo deviceInfo)
    {
        var user = await _userRepository.GetUserByEmailAsync(model.Login)
                   ?? await _userRepository.GetUserByUsernameAsync(model.Login);

        if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash!))
            return null;

        var sessions = await _sessionRepository.GetSessionsAsync(user.Id) ?? new List<UserSession>();

        var existingSession = sessions.FirstOrDefault(s =>
            s.DeviceInfo.Browser == deviceInfo.Browser &&
            s.DeviceInfo.Os == deviceInfo.Os &&
            s.DeviceInfo.Device == deviceInfo.Device
        );

        if (existingSession != null)
        {
            if (existingSession.IsRevoked || existingSession.ExpiresAt <= DateTime.UtcNow)
            {
                var newSession = await _sessionRepository.CreateSessionAsync(user.Id, deviceInfo);
                return newSession?.Id.ToString();
            }

            existingSession.ExpiresAt = DateTime.UtcNow.AddHours(_options.ExpiresIn);
            await _sessionRepository.ReplaceSessionsAsync(user.Id, sessions);
            return existingSession.Id.ToString();
        }

        {
            var newSession = await _sessionRepository.CreateSessionAsync(user.Id, deviceInfo);
            return newSession?.Id.ToString();
        }
    }

    /// <summary>
    /// Registers a new user, generates an avatar, initializes default favorites, and logs them in.
    /// </summary>
    /// <param name="model">The registration request data.</param>
    /// <param name="deviceInfo">The device information for the session.</param>
    public async Task<string?> RegisterAsync(RegisterDto model, DeviceInfo deviceInfo)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(model.Email);
        if (existingUser != null) return null;

        var index = model.Email.IndexOf('@');
        var username = model.Email.Substring(0, index);

        var normalizedEmail = model.Email.ToLower();

        await using (var image = AvatarGenerator.ByteToStream(AvatarGenerator.CreateAvatar(model.FullName)))
        {
            BaseFile file = new();
            var id = ObjectId.GenerateNewId();
            (file.SourceUrl, file.CompressedFileName, file.SourceFileName, file.CompressedFileName) =
                await _fileService.SaveImageAsync(image, id + "-avatar", "user-avatars");
            var user = new User(username, model.Password, normalizedEmail,
                file, [RoleNames.User], model.FullName)
            {
                Id = id
            };

            await _userRepository.CreateUserAsync(user);
            await _favoriteSellerRepository.CreateAsync(new FavoriteSeller(id,
                DefaultFavoriteNames.DefaultSellerCollectionName));
            await _favoriteSellerRepository.CreateAsync(new FavoriteSeller(id,
                DefaultFavoriteNames.DefaultSellerCollectionName));
        }

        return await LoginAsync(new LoginDto { Login = model.Email, Password = model.Password }, deviceInfo);
    }

    /// <summary>
    ///     Revokes an active session (logout).
    /// </summary>
    /// <param name="sessionId">The ID of the session to revoke.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the session does not exist or is already revoked.</exception>

    public async Task LogoutAsync(string sessionId)
    {
        var session = await _sessionRepository.GetSessionAsync(ObjectId.Parse(sessionId));
        if (session == null || session.IsRevoked)
            throw new KeyNotFoundException("Session not found");

        await _sessionHubNotifier.ForceLogoutAsync(sessionId);

        await _sessionRepository.RevokeSessionAsync(ObjectId.Parse(sessionId));
    }

    /// <summary>
    /// Revokes a session belonging to a specific user.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="userId">The ID of the user owning the session.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user or session is not found.</exception>
    /// <exception cref="AccessDeniedException">Thrown if the session does not belong to the user.</exception>
    public async Task LogoutAsync(string sessionId, string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
            throw new KeyNotFoundException("User not found");
        var session = await _sessionRepository.GetSessionAsync(ObjectId.Parse(sessionId));
        if (session == null || session.IsRevoked)
            throw new KeyNotFoundException("Session not found");
        if (session.UserId.ToString() != userId)
            throw new AccessDeniedException("It's not your session");

        await _sessionHubNotifier.ForceLogoutAsync(sessionId);

        await _sessionRepository.RevokeSessionAsync(ObjectId.Parse(sessionId));
    }

    /// <summary>
    /// Sends a password reset email containing a verification code.
    /// </summary>
    /// <param name="login">The email or username of the account.</param>
    public async Task<string?> SendPasswordReset(string login)
    {
        var user = await _userRepository.GetUserByEmailAsync(login)
                   ?? await _userRepository.GetUserByUsernameAsync(login);

        if (user == null)
            return null;

        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("App.Services.EmailTemplates.ResetPassword.html");
        using var reader = new StreamReader(stream!);
        var html = await reader.ReadToEndAsync();

        var resetToken = Guid.NewGuid().ToString("N");
        var code = GenerateCode(6);
        var readyHtml = html.Replace("__CODE__", code).Replace("__TIME__", "15");

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(15));
        var resetData = new ResetPassData
        {
            Code = code,
            UserId = user.Id.ToString()
        };
        _cache.Set($"reset-pass:{resetToken}", resetData, cacheEntryOptions);

        var mail = new EmailMessage
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [user.Email!],
            Subject = "Reset Password",
            HtmlBody = readyHtml
        };

        await _emailService.SendEmailAsync(mail);
        return resetToken;
    }

    /// <summary>
    /// Validates a password reset code.
    /// </summary>
    /// <param name="resetToken">The reset token issued to the user.</param>
    /// <param name="inputCode">The verification code entered by the user.</param>
    public async Task<string?> VerifyPasswordCodeAsync(string resetToken, string inputCode)
    {
        var cacheKey = $"reset-pass:{resetToken}";

        if (_cache.TryGetValue(cacheKey, out ResetPassData? stored))
            if (string.Equals(stored?.Code, inputCode, StringComparison.OrdinalIgnoreCase))
            {
                _cache.Remove(cacheKey);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                var newAccessCode = Guid.NewGuid().ToString("N");
                _cache.Set($"reset-pass-access-code:{newAccessCode}", stored?.UserId, cacheEntryOptions);

                return newAccessCode;
            }

        return null;
    }

    /// <summary>
    /// Resets a user's password using a valid access code.
    /// </summary>
    /// <param name="password">The new password to set.</param>
    /// <param name="accessCode">The access code obtained from verification.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the access code is invalid.</exception>
    public async Task ResetPassword(string password, string accessCode)
    {
        var cacheKey = $"reset-pass-access-code:{accessCode}";

        if (_cache.TryGetValue(cacheKey, out string? userId))
        {
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
                throw new KeyNotFoundException("User not found");

            _cache.Remove(cacheKey);
            user.PasswordHash = PasswordHasher.HashPassword(password);
            await _userRepository.UpdateUserAsync(user);
        }

        throw new InvalidOperationException("Invalid code");
    }

    /// <summary>
    /// Sends an email verification code to the user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the email is already confirmed.</exception>
    public async Task SendEmailVerificationCodeAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (user.EmailConfirmed)
            throw new InvalidOperationException("Email is already confirmed");

        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("App.Services.EmailTemplates.ConfirmEmail.html");
        using var reader = new StreamReader(stream!);
        var html = await reader.ReadToEndAsync();

        var code = GenerateCode(6);
        var readyHtml = html.Replace("__CODE__", code).Replace("__TIME__", "15");

        SaveVerificationCode(user.Email, code, 15);

        var mail = new EmailMessage
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [user.Email],
            Subject = "Confirm your email address",
            HtmlBody = readyHtml
        };

        await _emailService.SendEmailAsync(mail);
    }

    /// <summary>
    /// Verifies an email confirmation code and marks the email as confirmed if valid.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="inputCode">The verification code entered by the user.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the code is invalid.</exception>
    public async Task VerifyCode(string userId, string inputCode)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null) throw new KeyNotFoundException("User not found");

        var cacheKey = $"verify:{user.Email}";

        if (_cache.TryGetValue(cacheKey, out string storedCode))
            if (string.Equals(storedCode, inputCode, StringComparison.OrdinalIgnoreCase))
            {
                _cache.Remove(cacheKey);
                user.EmailConfirmed = true;
                await _userRepository.UpdateUserAsync(user);
            }

        throw new InvalidOperationException("Invalid code");
    }

    /// <summary>
    /// Retrieves all active sessions for a given user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user is not found.</exception>
    public async Task<IEnumerable<UserSessionDto>?> GetActiveSessions(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var sessions = await _sessionRepository.GetSessionsAsync(ObjectId.Parse(userId));

        return _mapper.Map<IEnumerable<UserSessionDto>>(sessions);
    }

    /// <summary>
    /// Revokes all sessions associated with a given user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <exception cref="KeyNotFoundException">Thrown if the user is not found.</exception>
    public async Task RevokeAllSessions(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
            throw new KeyNotFoundException("User not found");
        var sessions = await _sessionRepository.GetSessionsAsync(ObjectId.Parse(userId));
        foreach (var session in sessions)
            session.IsRevoked = true;
        if (!await _userRepository.UpdateUserAsync(user))
            throw new KeyNotFoundException("User not found");
    }

    /// <summary>
    /// Saves a verification code for email confirmation in memory cache.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="code">The generated verification code.</param>
    /// <param name="expires">Expiration time in minutes.</param>
    public void SaveVerificationCode(string email, string code, int expires)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(expires));
        _cache.Set($"verify:{email}", code, cacheEntryOptions);
    }

    /// <summary>
    /// Retrieves a stored verification code from memory cache.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    public string? GetVerificationCode(string email)
    {
        _cache.TryGetValue($"verify:{email}", out string? code);
        return code;
    }

    /// <summary>
    ///     Removes the verification code from memory cache.
    /// </summary>
    /// <param name="email">User's email address.</param>
    public void RemoveVerificationCode(string email)
    {
        _cache.Remove($"verify:{email}");
    }

    /// <summary>
    ///     Generates a random alphanumeric code.
    /// </summary>
    /// <param name="length">The length of the code.</param>
    /// <returns>A string containing the generated code.</returns>
    private static string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Next(chars.Length)]).ToArray());
    }

    private class ResetPassData
    {
        public string Code { get; set; } = default!;
        public string UserId { get; set; } = default!;
    }
}