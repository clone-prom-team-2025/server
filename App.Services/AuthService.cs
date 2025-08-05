using System.Reflection;
using System.Security.Claims;
using App.Core.DTOs.Auth;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Auth;
using App.Core.Models.Email;
using App.Core.Models.User;
using App.Core.Utils;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;

namespace App.Services;

/// <summary>
/// Service for handling authentication and user sessions.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    IMapper mapper,
    IFileService fileService,
    IEmailService emailService,
    IMemoryCache memoryCache,
    IUserSessionRepository sessionRepository)
    : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IFileService _fileService = fileService;
    private readonly IEmailService _emailService = emailService;
    private readonly IMemoryCache _cache = memoryCache;
    private readonly IUserSessionRepository _sessionRepository = sessionRepository;
    private static readonly Random Random = new();

    /// <summary>
    /// Logs in a user using email or username and returns a session token.
    /// </summary>
    /// <param name="model">Login credentials.</param>
    /// <returns>Session ID as a string if successful; otherwise, null.</returns>
    public async Task<string?> LoginAsync(LoginDto model)
    {
        var user = await _userRepository.GetUserByEmailAsync(model.Login) 
                   ?? await _userRepository.GetUserByUsernameAsync(model.Login);

        if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash!))
            return null;

        string deviceInfo = model.DeviceInfo ?? "Unknown device";
        var session = await _sessionRepository.CreateSessionAsync(user.Id, deviceInfo);
        return session.Id.ToString();
    }

    /// <summary>
    /// Registers a new user, saves their avatar, and logs them in.
    /// </summary>
    /// <param name="model">Registration data.</param>
    /// <returns>Session ID as a string if successful; otherwise, null.</returns>
    public async Task<string?> RegisterAsync(RegisterDto model)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(model.Email);
        if (existingUser != null) return null;

        int index = model.Email.IndexOf('@');
        string username = model.Email.Substring(0, index);

        await using (Stream image = AvatarGenerator.ByteToStream(AvatarGenerator.CreateAvatar(model.FullName)))
        {
            var (url, fileName) = await _fileService.SaveImageFullHdAsync(image, username + "-avatar", "user-avatars");
            var user = new User(username, model.Password, model.Email,
                new UserAvatar(url, fileName), new List<UserRole> { UserRole.User });

            await _userRepository.CreateUserAsync(user);
        }

        return await LoginAsync(new LoginDto { Login = model.Email, Password = model.Password });
    }
    
    /// <summary>
    /// Revokes an active session (logout).
    /// </summary>
    /// <param name="sessionId">The ID of the session to revoke.</param>
    /// <returns>True if the session was successfully revoked; otherwise, false.</returns>
    public async Task<bool> LogoutAsync(string sessionId)
    {
        if (!ObjectId.TryParse(sessionId, out var objectId))
            return false;

        var session = await _sessionRepository.GetSessionAsync(objectId);
        if (session == null || session.IsRevoked)
            return false;

        await _sessionRepository.RevokeSessionAsync(objectId);
        return true;
    }

    /// <summary>
    /// Sends an email with a verification code to the user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the email was sent successfully; otherwise, false.</returns>
    public async Task<bool> SendEmailVerificationCodeAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null || user.EmailConfirmed) return false;

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("App.Services.EmailTemplates.ConfirmEmail.html");
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
        return true;
    }

    /// <summary>
    /// Saves the generated verification code in memory cache.
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
    /// Retrieves a verification code from memory cache.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <returns>The verification code if found; otherwise, null.</returns>
    public string? GetVerificationCode(string email)
    {
        _cache.TryGetValue($"verify:{email}", out string? code);
        return code;
    }

    /// <summary>
    /// Removes the verification code from memory cache.
    /// </summary>
    /// <param name="email">User's email address.</param>
    public void RemoveVerificationCode(string email)
    {
        _cache.Remove($"verify:{email}");
    }

    /// <summary>
    /// Verifies the user's input code with the one stored in cache.
    /// If valid, marks the email as confirmed.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="inputCode">The verification code entered by the user.</param>
    /// <returns>True if the code is correct and email is confirmed; otherwise, false.</returns>
    public async Task<bool> VerifyCode(string userId, string inputCode)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return false;

        var cacheKey = $"verify:{user.Email}";

        if (_cache.TryGetValue(cacheKey, out string storedCode))
        {
            if (string.Equals(storedCode, inputCode, StringComparison.OrdinalIgnoreCase))
            {
                _cache.Remove(cacheKey);
                user.EmailConfirmed = true;
                await _userRepository.UpdateUserAsync(user);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Generates a random alphanumeric code.
    /// </summary>
    /// <param name="length">The length of the code.</param>
    /// <returns>A string containing the generated code.</returns>
    private static string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Next(chars.Length)]).ToArray());
    }
}
