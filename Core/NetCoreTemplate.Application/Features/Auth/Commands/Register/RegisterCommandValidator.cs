using FluentValidation;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).Matches(@"[A-Z]").Matches(@"[a-z]").Matches(@"[0-9]").WithMessage("Password must contain at least one uppercase, lowercase letter and a number");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
    }
}
