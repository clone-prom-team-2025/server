using App.Core.DTOs.Product;
using FluentValidation;

namespace App.Core.Validations;

public class ProductMediaCreateDtoValidator : AbstractValidator<ProductMediaCreateDto>
{
    public ProductMediaCreateDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Url)
            .NotNull()
            .NotEmpty();
    }
}