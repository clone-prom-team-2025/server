using App.Core.Interfaces;
using App.Core.Models.Email;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace App.Services.Services;

/// <summary>
/// Service class responsible for sending and receiving emails using configured accounts.
/// </summary>
public class EmailService : IEmailService
{
    private readonly Dictionary<string, EmailAccountSettings> _accounts;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="config">The application configuration containing email account settings.</param>
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

    /// <summary>
    /// Sends an email asynchronously using the configured sender account.
    /// </summary>
    /// <param name="message">The email message to be sent.</param>
    public async Task SendEmailAsync(EmailMessage message)
    {
        if (!_accounts.TryGetValue(GetAccountKeyFromEmail(message.From), out var sender))
            return;

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

    /// <summary>
    /// Retrieves all emails from the inbox of a specified account asynchronously.
    /// </summary>
    /// <param name="from">The sender email to identify the account to use.</param>
    public async Task<List<MimeMessage>> GetInboxAsync(string from)
    {
        if (!_accounts.TryGetValue(GetAccountKeyFromEmail(from), out var acc))
            return [];

        using var client = new ImapClient();
        await client.ConnectAsync(acc.ImapServer, 993, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(acc.Username, acc.Password);
        await client.Inbox.OpenAsync(FolderAccess.ReadOnly);

        var uids = await client.Inbox.SearchAsync(SearchQuery.All);
        //var messages = await client.Inbox.FetchAsync(uids, MessageSummaryItems.Full | MessageSummaryItems.UniqueId);

        var result = new List<MimeMessage>();
        foreach (var uid in uids)
        {
            var msg = await client.Inbox.GetMessageAsync(uid);
            result.Add(msg);
        }

        await client.DisconnectAsync(true);
        return result;
    }

    /// <summary>
    /// Retrieves the account key corresponding to the specified email address.
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    private string GetAccountKeyFromEmail(string email)
    {
        return _accounts.FirstOrDefault(kvp => kvp.Value.Email == email).Key
               ?? "";
    }
}