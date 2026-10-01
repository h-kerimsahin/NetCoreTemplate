using FluentValidation;

namespace $safeprojectname$.Application.Features.UserProfile.Commands.Update;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        RuleFor(x => x.City).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.City));
        RuleFor(x => x.Country).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Country));
    }
}
