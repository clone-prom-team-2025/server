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

    public async Task<string?> LoginAsync(LoginDto model)
    {
        var user = await _userRepository.GetUserByEmailAsync(model.Login) 
                   ?? await _userRepository.GetUserByUsernameAsync(model.Login);

        if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash!))
            return null;

        // Пристрій або браузер
        string deviceInfo = model.DeviceInfo ?? "Unknown device";
        var session = await _sessionRepository.CreateSessionAsync(user.Id, deviceInfo);
        return session.Id.ToString();
    }

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

    public void SaveVerificationCode(string email, string code, int expires)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(expires));
        _cache.Set($"verify:{email}", code, cacheEntryOptions);
    }

    public string? GetVerificationCode(string email)
    {
        _cache.TryGetValue($"verify:{email}", out string? code);
        return code;
    }

    public void RemoveVerificationCode(string email)
    {
        _cache.Remove($"verify:{email}");
    }

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

    private static string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Next(chars.Length)]).ToArray());
    }
}
