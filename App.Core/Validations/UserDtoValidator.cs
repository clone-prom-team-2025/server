using FluentValidation;
using App.Core.DTOs;

namespace App.Core.Validations;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.Id)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Id must be a 24-character hex string");

        RuleFor(x => x.Username)
            .MinimumLength(5)
            .MaximumLength(20)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Username must be non-empty.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x != null);

        RuleFor(x => x.AvatarUrl)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("AvatarUrl must be non-empty.");

        RuleFor(x => x.Roles)
            .Must(x => x.Count > 0).WithMessage("At least one role must be specified.");

        RuleFor(x => x.CreatedAt)
            .NotNull();
    }
}