using App.Core.DTOs.Product;
using FluentValidation;

namespace App.Core.Validations;

public class ProductMediaDtoValidator : AbstractValidator<ProductMediaDto>
{
    public ProductMediaDtoValidator()
    {
        RuleFor(x => x.Id)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Id must be a 24-character hex string");

        RuleFor(x => x.ProductId)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("ProductId must be a 24-character hex string");

        RuleFor(x => x.Type)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0);
    }
}