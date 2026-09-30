using FluentValidation;

namespace NetCoreTemplate.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.Token).NotEmpty().MinimumLength(32);
}
