using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using App.Core.DTOs.Auth;
using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Auth;
using App.Core.Models.User;
using App.Core.Utils;
using AutoMapper;
using MongoDB.Bson;


namespace App.Services;

public class AuthService(IUserRepository userRepository, IMapper mapper, IJwtService jwtService, IFileService fileService) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IFileService _fileService = fileService;
    
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
        return await LoginAsync(new LoginDto{Login = model.Email, Password = model.Password});
    }
}