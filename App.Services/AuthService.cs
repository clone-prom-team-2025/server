using System.IdentityModel.Tokens.Jwt;
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
using MongoDB.Bson;


namespace App.Services;

public class AuthService(IUserRepository userRepository, IMapper mapper, IJwtService jwtService, IFileService fileService, IEmailService emailService) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IFileService _fileService = fileService;
    private readonly IEmailService _emailService = emailService;
    
    public async Task<string?> LoginAsync(LoginDto model)
    {
        var user = await _userRepository.GetUserByEmailAsync(model.Login) ?? await _userRepository.GetUserByUsernameAsync(model.Login);
        if (user == null  || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash!))
            return null;
        
        return _jwtService.GenerateToken(user);
    }

    public async Task<string?> RegisterAsync(RegisterDto model)
    {
        int index = model.Email.IndexOf('@');
        string username = model.Email.Substring(0, index);
        await using (Stream image = AvatarGenerator.ByteToStream(AvatarGenerator.CreateAvatar(model.FullName)))
        {
            var (url, fileName) = await _fileService.SaveImageFullHdAsync(image, username + "-avatar", "user-avatars");
            var user = new User(username, model.Password, model.Email, new UserAvatar(url, fileName), new List<UserRole> { UserRole.User });
            await _userRepository.CreateUserAsync(user);
        }

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("App.Services.EmailTemplates.ConfirmEmail.html");
        using var reader = new StreamReader(stream!);
        var html = await reader.ReadToEndAsync();
        var readyHtml = html.Replace("__CODE__", "test");
        var email = new EmailMessage() {From = "no-reply@sellpoint.pp.ua", To = new List<string> { model.Email }, Subject = "Confirm your email address", HtmlBody = readyHtml};
        await _emailService.SendEmailAsync(email);
        return await LoginAsync(new LoginDto{Login = model.Email, Password = model.Password});
    }
    
    public async Task<bool> DeleteAccountAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) return false;
        await _fileService.DeleteFileAsync("user-avatars", user.AvatarUrl.FileName);
        
        return await _userRepository.DeleteUserAsync(user.Id.ToString());
    } 
}