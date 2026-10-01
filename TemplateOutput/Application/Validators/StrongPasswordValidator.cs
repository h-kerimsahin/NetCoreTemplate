using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FluentValidation;
using $safeprojectname$.Application.DTOs.Settings;

namespace $safeprojectname$.Application.Validators;

public abstract class StrongPasswordValidator<T> : AbstractValidator<T>
{
    protected StrongPasswordValidator(Expression<Func<T, string>> passwordSelector, PasswordSettings? settings = null)
    {
        var passwordSettings = settings ?? new PasswordSettings();

        RuleFor(passwordSelector)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(passwordSettings.MinLength)
            .WithMessage($"Şifre en az {passwordSettings.MinLength} karakter olmalıdır.");

        if (passwordSettings.RequireUppercase)
        {
            RuleFor(passwordSelector)
                .Matches(@"[A-Z]")
                .WithMessage("Şifre en az bir büyük harf içermelidir.");
        }

        if (passwordSettings.RequireLowercase)
        {
            RuleFor(passwordSelector)
                .Matches(@"[a-z]")
                .WithMessage("Şifre en az bir küçük harf içermelidir.");
        }

        if (passwordSettings.RequireDigit)
        {
            RuleFor(passwordSelector)
                .Matches(@"[0-9]")
                .WithMessage("Şifre en az bir rakam içermelidir.");
        }

        if (passwordSettings.RequireSpecialChar)
        {
            RuleFor(passwordSelector)
                .Matches(@"[" + Regex.Escape(passwordSettings.SpecialChars) + @"]")
                .WithMessage("Şifre en az bir özel karakter içermelidir.");
        }
    }
}
