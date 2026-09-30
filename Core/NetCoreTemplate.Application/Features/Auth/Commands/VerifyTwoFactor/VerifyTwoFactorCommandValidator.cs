using FluentValidation;

namespace NetCoreTemplate.Application.Features.Auth.Commands.VerifyTwoFactor;

public class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MinimumLength(4);
    }
}
