using App.Core.DTOs.Product;
using FluentValidation;

namespace App.Core.Validations;

public class ProductFeatureDtoValidator : AbstractValidator<ProductFeatureDto>
{
    public ProductFeatureDtoValidator()
    {
        RuleFor(x => x.Category)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Features)
            .NotNull()
            .Must(f => f.Count > 0).WithMessage("At least one feature must be specified.");
    }
}