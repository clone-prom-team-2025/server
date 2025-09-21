namespace App.Core.DTOs.Auth;

public class RegisterDto
{
    public RegisterDto(string firstName, string lastName, string email, string password)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
    }

    public RegisterDto()
    {
    }

    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}