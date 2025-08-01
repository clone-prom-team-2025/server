using App.Core.Interfaces;
using App.Core.Models.Email;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Imap;
using MailKit;
using MailKit.Net.Smtp;

namespace App.Services;

public class EmailService : IEmailService
{
    private readonly Dictionary<string, EmailAccountSettings> _accounts;

    public EmailService(IConfiguration config)
    {
        _accounts = config
            .GetSection("EmailSettings:Accounts")
            .GetChildren()
            .ToDictionary(
                acc => acc.Key,
                acc => acc.Get<EmailAccountSettings>()!
            );
    }

    public async Task SendEmailAsync(EmailMessage message)
    {
        if (!_accounts.TryGetValue(GetAccountKeyFromEmail(message.From), out var sender))
            throw new InvalidOperationException($"Sender '{message.From}' is not configured");

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(sender.DisplayName, sender.Email));
        email.To.AddRange(message.To.Select(to => MailboxAddress.Parse(to)));
        email.Subject = message.Subject;

        email.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(sender.SmtpServer, sender.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(sender.Username, sender.Password);
        await client.SendAsync(email);
        await client.DisconnectAsync(true);
    }

    public async Task<List<MimeMessage>> GetInboxAsync(string from)
    {
        if (!_accounts.TryGetValue(GetAccountKeyFromEmail(from), out var acc))
            throw new InvalidOperationException($"Account '{from}' not found");

        using var client = new ImapClient();
        await client.ConnectAsync(acc.ImapServer, 993, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(acc.Username, acc.Password);
        await client.Inbox.OpenAsync(FolderAccess.ReadOnly);

        var uids = await client.Inbox.SearchAsync(MailKit.Search.SearchQuery.All);
        var messages = await client.Inbox.FetchAsync(uids, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
        
        var result = new List<MimeMessage>();
        foreach (var uid in uids)
        {
            var msg = await client.Inbox.GetMessageAsync(uid);
            result.Add(msg);
        }

        await client.DisconnectAsync(true);
        return result;
    }

    private string GetAccountKeyFromEmail(string email)
    {
        return _accounts.FirstOrDefault(kvp => kvp.Value.Email == email).Key
            ?? throw new InvalidOperationException($"Email '{email}' not configured");
    }
}