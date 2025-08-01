namespace App.Core.Models.Email;

public class EmailAccountSettings
{
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string SmtpServer { get; set; } = null!;
    public string ImapServer { get; set; } = null!;
    public int Port { get; set; }
}