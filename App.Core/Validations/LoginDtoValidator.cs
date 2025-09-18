using App.Core.DTOs.Auth;
using FluentValidation;

namespace App.Core.Validations;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password must be non-empty.")
            .Matches("^[a-zA-Z0-9!&*$_%@]+$")
            .WithMessage("Password can only contain letters, numbers, and the following symbols: ! & * $ _ % @");

        RuleFor(x => x.Login)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Either Username or Email is required.");
    }
}