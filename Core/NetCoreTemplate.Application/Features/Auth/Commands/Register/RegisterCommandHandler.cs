using MediatR;
using Microsoft.AspNetCore.Http;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<RegisterResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserActivityLogger _activityLogger;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IUserActivityLogger activityLogger, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _activityLogger = activityLogger;
        _emailService = emailService;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try { await _emailService.SendWelcomeEmailAsync(request.Email, request.UserName, cancellationToken); } catch { }

        var dto = new RegisterResponseDto(createdUser.Id, createdUser.UserName, createdUser.Email);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Kayıt başarılı.");
    }
}
