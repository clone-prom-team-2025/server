namespace App.Core.DTOs.Auth;

public class RegisterDto
{
    public RegisterDto(string fullName, string email, string password)
    {
        FullName = fullName;
        Email = email;
        Password = password;
    }

    public RegisterDto()
    {
    }

    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}