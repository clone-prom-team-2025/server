using FluentValidation;
using App.Core.DTOs.Product;

namespace App.Core.Validations;

public class ProductVariationDtoValidator : AbstractValidator<ProductVariationDto>
{
    public ProductVariationDtoValidator()
    {
        RuleFor(x => x.ModelName)
            .MinimumLength(4)
            .MaximumLength(50)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("ModelName must be non-empty.")
            .When(x => x.ModelName != null);


        RuleFor(x => x.ModelId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

    }
}