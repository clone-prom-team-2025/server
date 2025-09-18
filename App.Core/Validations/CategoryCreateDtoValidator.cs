using App.Core.DTOs.Categoty;
using FluentValidation;

namespace App.Core.Validations;

public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotNull();

        RuleFor(x => x.ParentId)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Id must be a 24-character hex string")
            .When(x => x != null);
    }
}