using FluentValidation;
using HardLock.Shared.Models;

namespace HardLock.Identity.Validators;

public class UserLoginValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255)
            .WithMessage("Valid email address is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");

        RuleFor(x => x.MfaCode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.MfaCode))
            .WithMessage("MFA code cannot exceed 10 characters");
    }
} 