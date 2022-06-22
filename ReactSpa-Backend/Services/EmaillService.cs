namespace ReactSpa_Backend.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using ReactSpa_Backend.Helpers;
using ReactSpa_Backend.IServices;

public class EmailService : IEmailService
{
    private readonly AppSettings _appSettings;

    public EmailService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public void Send(string to, string subject, string html, string from = null)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from ?? _appSettings.EmailFrom));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html)
        {
            Text = html
        };

        using var client = new SmtpClient();
        client.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
        client.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
        client.Send(message);
        client.Disconnect(true);
    }
}