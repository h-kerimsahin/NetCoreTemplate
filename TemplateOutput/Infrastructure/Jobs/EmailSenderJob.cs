using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Infrastructure.Services;
using Polly;

namespace $safeprojectname$.Infrastructure.Jobs;

public class EmailSenderJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmailSettings _emailSettings;

    public EmailSenderJob(IUnitOfWork unitOfWork, IOptions<EmailSettings> emailSettings)
    {
        _unitOfWork = unitOfWork;
        _emailSettings = emailSettings.Value;
    }

    public async Task Execute(Guid jobLogId, string toEmail, string subject, string body, bool isHtml = true, CancellationToken ct = default)
    {
        var jobLog = await _unitOfWork.BackgroundJobLogs.GetByIdAsync(jobLogId, ct);
        if (jobLog == null) return;

        jobLog.Start();
        await _unitOfWork.SaveChangesAsync(ct);

        var retryPolicy = Policy
            .Handle<SmtpCommandException>()
            .Or<SmtpProtocolException>()
            .Or<IOException>()
            .WaitAndRetryAsync(3,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (ex, _, attempt, _) =>
                {
                    jobLog.ScheduleRetry(ex.ToString());
                    _unitOfWork.BackgroundJobLogs.Update(jobLog);
                    _ = _unitOfWork.SaveChangesAsync(ct).GetAwaiter().GetResult();
                });

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;
                message.Body = isHtml
                    ? new BodyBuilder { HtmlBody = body }.ToMessageBody()
                    : new TextPart("plain") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None, ct);

                if (!string.IsNullOrEmpty(_emailSettings.SmtpUserName))
                    await client.AuthenticateAsync(_emailSettings.SmtpUserName, _emailSettings.SmtpPassword, ct);

                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);
            });

            jobLog.Succeed();
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            jobLog.Fail(ex.ToString());
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
