namespace $safeprojectname$.Domain.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendResetPasswordEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
    Task SendEmailConfirmationEmailAsync(string toEmail, string confirmLink, CancellationToken cancellationToken = default);
    Task SendTwoFactorCodeEmailAsync(string toEmail, string twoFactorCode, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);
}
