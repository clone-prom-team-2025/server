using App.Core.DTOs.Auth;
using FluentValidation;

namespace App.Core.Validations;

public class RegisterDtoValidation : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email must be provided.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password must be non-empty.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[!&*$_%@]").WithMessage("Password must contain at least one special character: ! & * $ _ % @")
            .Matches("^[a-zA-Z0-9!&*$_%@]+$")
            .WithMessage("Password can only contain letters, numbers, and the following symbols: ! & * $ _ % @");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name must be provided.")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters long.");
    }
}