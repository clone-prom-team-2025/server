using App.Core.Models.Email;
using MimeKit;

namespace App.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message, byte[]? file = null, string? fileName = null);
    Task<List<MimeMessage>> GetInboxAsync(string from);
}