using App.Core.DTOs;
using FluentValidation;

namespace App.Core.Validations;

public class FileArrayItemDtoValidator : AbstractValidator<FileArrayItemDto>
{
    public FileArrayItemDtoValidator()
    {
        RuleFor(x => x.FileName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("FileName must be non-empty.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Type)
            .NotEmpty()
            .NotNull();
    }
}