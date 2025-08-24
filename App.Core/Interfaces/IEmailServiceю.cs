using App.Core.Models.Email;
using MimeKit;

namespace App.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message);
    Task<List<MimeMessage>> GetInboxAsync(string from);
}