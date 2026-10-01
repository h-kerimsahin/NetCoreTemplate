using System.Linq.Expressions;
using System.Text.Json;
using System.Reflection;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IUserActivityLogger _activityLogger;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByEmailAsync(request.Email, cancellationToken);
        if (user != null)
        {
            var resetTokenRaw = _tokenService.GenerateRandomToken(48);
            var resetToken = new AppUserRefreshToken(user.Id, resetTokenRaw, DateTime.UtcNow.AddHours(1), TokenType.ResetPassword);

            await _unitOfWork.AppUserRefreshTokens.AddAsync(resetToken, cancellationToken);
            await _activityLogger.LogAsync(user.Id, UserActivityType.ForgotPasswordRequested, "Password reset requested", cancellationToken: cancellationToken);

            var resetLink = $"https://localhost:7000/reset-password?token={resetTokenRaw}&email={Uri.EscapeDataString(request.Email)}";
            var toEmail = request.Email;
            var subject = "Şifre Sıfırlama Talebi";
            var htmlString = $@"
<html>
<body style=""font-family: Arial, sans-serif; padding: 20px;"">
    <h2>Şifre Sıfırlama Talebi</h2>
    <p>Merhaba, şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
    <p><a href=""{resetLink}"" style=""padding: 10px 20px; background: #007bff; color: white; text-decoration: none; border-radius: 5px; display: inline-block;"">Şifremi Sıfırla</a></p>
    <p>Bu bağlantı 1 saat boyunca geçerlidir. Eğer bu talebi siz yapmadıysanız, bu e-postayı dikkate almayın.</p>
    <p>Saygılarımızla,<br/>$safeprojectname$ Ekibi</p>
</body>
</html>";

            var payload = JsonSerializer.Serialize(new { toEmail, subject, body = htmlString });
            var jobLog = BackgroundJobLog.Create("EmailSenderJob", payload);
            await _unitOfWork.BackgroundJobLogs.AddAsync(jobLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                EnqueueEmailJob(jobLog.Id, toEmail, subject, htmlString);
            }
            catch
            {
            }
        }
        return ApiResponse.Success(true, StatusCodes.Status200OK, "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi (kayıtlı ise).");
    }

    private static void EnqueueEmailJob(Guid jobLogId, string toEmail, string subject, string htmlString)
    {
        var jobType = Type.GetType("$safeprojectname$.Infrastructure.Jobs.EmailSenderJob, $safeprojectname$.Infrastructure");
        if (jobType == null) return;

        var executeMethod = jobType.GetMethod("Execute", new[]
        {
            typeof(Guid), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(CancellationToken)
        });
        if (executeMethod == null) return;

        var param = Expression.Parameter(jobType, "j");
        var callExpr = Expression.Call(
            param,
            executeMethod,
            Expression.Constant(jobLogId, typeof(Guid)),
            Expression.Constant(toEmail, typeof(string)),
            Expression.Constant(subject, typeof(string)),
            Expression.Constant(htmlString, typeof(string)),
            Expression.Constant(true, typeof(bool)),
            Expression.Constant(CancellationToken.None, typeof(CancellationToken))
        );

        var delegateType = typeof(Action<>).MakeGenericType(jobType);
        var lambda = Expression.Lambda(delegateType, callExpr, param);

        var enqueueMethod = typeof(BackgroundJob).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Enqueue" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
            .MakeGenericMethod(jobType);

        enqueueMethod.Invoke(null, new object[] { lambda });
    }
}