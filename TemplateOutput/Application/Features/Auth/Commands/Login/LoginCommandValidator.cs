using FluentValidation;

namespace $safeprojectname$.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName).NotEmpty().WithMessage("Email or Username is required");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
