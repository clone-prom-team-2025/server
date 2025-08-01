using FluentValidation;
using App.Core.DTOs.Product;

namespace App.Core.Validations;

public class ProductFeatureItemDtoValidator : AbstractValidator<ProductFeatureItemDto>
{
    public ProductFeatureItemDtoValidator()
    {
        RuleFor(x => x.Value)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Type)
            .NotNull()
            .NotEmpty();
    }
}