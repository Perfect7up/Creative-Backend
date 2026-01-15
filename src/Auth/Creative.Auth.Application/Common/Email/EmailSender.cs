using Creative.Auth.Application.Common.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Creative.Auth.Application.Common.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly string _frontendUrl;

    public EmailSender(
        IOptions<EmailSettings> settings,
        IOptions<FrontendSettings> frontendSettings)
    {
        _settings = settings.Value;
        _frontendUrl = frontendSettings.Value.Url;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Email));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Email, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public string CreateVerificationLink(string token)
    {
        return $"{_frontendUrl}/verify-email?token={token}";
    }

    public string CreateResetPasswordLink(string token)
    {
        return $"{_frontendUrl}/reset-password?token={token}";
    }
}