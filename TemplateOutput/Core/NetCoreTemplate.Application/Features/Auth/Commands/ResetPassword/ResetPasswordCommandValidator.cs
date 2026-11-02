using FluentValidation;
using NetCoreTemplate.Application.Validators;

namespace NetCoreTemplate.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator : StrongPasswordValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator() : base(x => x.NewPassword)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token boş olamaz.");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("Şifreler eşleşmiyor.");
    }
}
