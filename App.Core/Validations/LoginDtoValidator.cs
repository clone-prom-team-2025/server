using App.Core.DTOs.Auth;
using FluentValidation;

namespace App.Core.Validations;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9!&*$_%@]+$")
            .WithMessage("Password can only contain letters, numbers, and the following symbols: ! & * $ _ % @")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Password must be non-empty.");

        RuleFor(x => x.Login)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Either Username or Email is required.");
    }
}