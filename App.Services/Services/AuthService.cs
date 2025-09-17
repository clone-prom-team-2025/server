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
    ///     Logs in a user using email or username and returns a session token.
    /// </summary>
    /// <param name="model">Login credentials.</param>
    /// <returns>Session ID as a string if successful; otherwise, null.</returns>
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
    ///     Registers a new user, saves their avatar, and logs them in.
    /// </summary>
    /// <param name="model">Registration data.</param>
    /// <returns>Session ID as a string if successful; otherwise, null.</returns>
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
    /// <returns>True if the session was successfully revoked; otherwise, false.</returns>
    public async Task LogoutAsync(string sessionId)
    {
        var session = await _sessionRepository.GetSessionAsync(ObjectId.Parse(sessionId));
        if (session == null || session.IsRevoked)
            throw new KeyNotFoundException("Session not found");

        await _sessionHubNotifier.ForceLogoutAsync(sessionId);

        await _sessionRepository.RevokeSessionAsync(ObjectId.Parse(sessionId));
    }

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

    public async Task<string?> VerifyPasswordCodeAsync(string resetToken, string inputCode)
    {
        var cacheKey = $"reset-pass:{resetToken}";

        if (_cache.TryGetValue(cacheKey, out ResetPassData? stored))
            if (string.Equals(stored.Code, inputCode, StringComparison.OrdinalIgnoreCase))
            {
                _cache.Remove(cacheKey);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                var newAccessCode = Guid.NewGuid().ToString("N");
                _cache.Set($"reset-pass-access-code:{newAccessCode}", stored.UserId, cacheEntryOptions);

                return newAccessCode;
            }

        return null;
    }

    public async Task ResetPassword(string password, string accessCode)
    {
        var cacheKey = $"reset-pass-access-code:{accessCode}";

        if (_cache.TryGetValue(cacheKey, out string userId))
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
    ///     Sends an email with a verification code to the user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the email was sent successfully; otherwise, false.</returns>
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
    ///     Verifies the user's input code with the one stored in cache.
    ///     If valid, marks the email as confirmed.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="inputCode">The verification code entered by the user.</param>
    /// <returns>True if the code is correct and email is confirmed; otherwise, false.</returns>
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

    public async Task<IEnumerable<UserSessionDto>?> GetActiveSessions(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var sessions = await _sessionRepository.GetSessionsAsync(ObjectId.Parse(userId));

        return _mapper.Map<IEnumerable<UserSessionDto>>(sessions);
    }

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
    ///     Saves the generated verification code in memory cache.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="code">The verification code.</param>
    /// <param name="expires">Expiration time in minutes.</param>
    public void SaveVerificationCode(string email, string code, int expires)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(expires));
        _cache.Set($"verify:{email}", code, cacheEntryOptions);
    }

    /// <summary>
    ///     Retrieves a verification code from memory cache.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <returns>The verification code if found; otherwise, null.</returns>
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