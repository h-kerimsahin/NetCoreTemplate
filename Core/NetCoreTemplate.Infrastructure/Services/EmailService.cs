using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings) => _emailSettings = emailSettings.Value;

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;
        message.Body = isHtml ? new BodyBuilder { HtmlBody = body }.ToMessageBody() : new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

        if (!string.IsNullOrEmpty(_emailSettings.SmtpUserName))
            await client.AuthenticateAsync(_emailSettings.SmtpUserName, _emailSettings.SmtpPassword, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendResetPasswordEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        var body = $"<h3>Şifre Sıfırlama</h3><p>Şifrenizi sıfırlamak için <a href=\"{resetLink}\">buraya tıklayın</a></p><p>Bu bağlantı 1 saat geçerlidir.</p>";
        await SendEmailAsync(toEmail, "Şifre Sıfırlama Talebi", body, true, cancellationToken);
    }

    public async Task SendEmailConfirmationEmailAsync(string toEmail, string confirmLink, CancellationToken cancellationToken = default)
    {
        var body = $"<h3>Email Doğrulama</h3><p>Hesabınızı doğrulamak için <a href=\"{confirmLink}\">buraya tıklayın</a></p>";
        await SendEmailAsync(toEmail, "Emailinizi Doğrulayın", body, true, cancellationToken);
    }

    public async Task SendTwoFactorCodeEmailAsync(string toEmail, string twoFactorCode, CancellationToken cancellationToken = default)
    {
        var body = $"<h3>İki Adımlı Doğrulama Kodu</h3><p>Giriş kodunuz: <strong style=\"font-size:24px\">{twoFactorCode}</strong></p><p>Bu kod 5 dakika geçerlidir.</p>";
        await SendEmailAsync(toEmail, "2FA Giriş Kodunuz", body, true, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        var body = $"<h3>Hoş Geldiniz {userName}!</h3><p>NetCoreTemplate sistemine başarılı şekilde kayıt oldunuz.</p>";
        await SendEmailAsync(toEmail, "Hoş Geldiniz!", body, true, cancellationToken);
    }
}
