using System.Linq.Expressions;
using System.Text.Json;
using System.Reflection;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<RegisterResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserActivityLogger _activityLogger;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.AppUsers.IsEmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            throw new BusinessException($"Email {request.Email} is already in use");

        if (await _unitOfWork.AppUsers.IsUserNameExistsAsync(request.UserName, cancellationToken: cancellationToken))
            throw new BusinessException($"Username {request.UserName} is already in use");

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new AppUser(request.UserName, request.Email, passwordHash);
        user.Profile.Update(request.FirstName ?? string.Empty, request.LastName ?? string.Empty, null, null, null, null, null, null, null);

        var createdUser = await _unitOfWork.AppUsers.AddAsync(user, cancellationToken);
        await _activityLogger.LogAsync(createdUser.Id, UserActivityType.Register, $"New user registered from {request.IpAddress}", ipAddress: request.IpAddress, cancellationToken: cancellationToken);

        var toEmail = request.Email;
        var userName = request.UserName;
        var subject = "Hoş Geldiniz!";
        var htmlString = $@"
<html>
<body style=""font-family: Arial, sans-serif; padding: 20px;"">
    <h2>Hoş Geldiniz, {userName}!</h2>
    <p>Hesabınız başarıyla oluşturuldu. Keyifli kullanımlar dileriz.</p>
    <p>Saygılarımızla,<br/>NetCoreTemplate Ekibi</p>
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

        var dto = new RegisterResponseDto(createdUser.Id, createdUser.UserName, createdUser.Email);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Kayıt başarılı.");
    }

    private static void EnqueueEmailJob(Guid jobLogId, string toEmail, string subject, string htmlString)
    {
        var jobType = Type.GetType("NetCoreTemplate.Infrastructure.Jobs.EmailSenderJob, NetCoreTemplate.Infrastructure");
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