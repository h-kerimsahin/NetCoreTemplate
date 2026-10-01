using System.Linq.Expressions;
using System.Text.Json;
using System.Reflection;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.Auth.Commands.EnableTwoFactor;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;
    private readonly ITokenService _tokenService;

    public EnableTwoFactorCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<bool>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdAsync(request.UserId, cancellationToken) ?? throw new NotFoundException(nameof(AppUser), request.UserId);
        var type = Enum.TryParse<TwoFactorType>(request.Type, true, out var t) ? t : TwoFactorType.Email;

        user.EnableTwoFactor(type);
        await _activityLogger.LogAsync(user.Id, UserActivityType.TwoFactorEnabled, $"2FA enabled via {type}", cancellationToken: cancellationToken);

        if (type == TwoFactorType.Email)
        {
            var code = _tokenService.GenerateRandomToken(6);
            var codeToken = new AppUserRefreshToken(user.Id, code, DateTime.UtcNow.AddMinutes(5), TokenType.TwoFactor);
            await _unitOfWork.AppUserRefreshTokens.AddAsync(codeToken, cancellationToken);

            var toEmail = user.Email;
            var subject = "İki Faktörlü Doğrulama Kodu";
            var htmlString = $@"
<html>
<body style=""font-family: Arial, sans-serif; padding: 20px;"">
    <h2>İki Faktörlü Doğrulama</h2>
    <p>Hesabınız için 2FA etkinleştirildi. Giriş yaparken kullanacağınız doğrulama kodu:</p>
    <p style=""font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px; background: #f5f5f5; display: inline-block; border-radius: 8px;"">{code}</p>
    <p>Bu kod 5 dakika boyunca geçerlidir.</p>
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

            return ApiResponse.Success(true, StatusCodes.Status200OK, "İki faktörlü doğrulama etkinleştirildi.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success(true, StatusCodes.Status200OK, "İki faktörlü doğrulama etkinleştirildi.");
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