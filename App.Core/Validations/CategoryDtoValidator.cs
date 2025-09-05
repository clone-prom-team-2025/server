using App.Core.DTOs.Categoty;
using FluentValidation;

namespace App.Core.Validations;

public class CategoryDtoValidator : AbstractValidator<CategoryDto>
{
    public CategoryDtoValidator()
    {
        RuleFor(x => x.Id)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Id must be a 24-character hex string");

        RuleFor(x => x.Name)
            .NotNull();

        RuleFor(x => x.ParentId)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Id must be a 24-character hex string")
            .When(x => x != null);
    }
}