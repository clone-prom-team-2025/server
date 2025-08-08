using App.Core.DTOs.Product;
using FluentValidation;

namespace App.Core.Validations;

public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Name must be non-empty.")
            .MaximumLength(128);

        RuleFor(x => x.ProductType)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Product type must be non-empty.")
            .MaximumLength(50);

        RuleFor(x => x.Category)
            .NotEmpty()
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Category must be a 24-character hex string");

        RuleFor(x => x.SellerId)
            .NotEmpty()
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("SellerId must be a 24-character hex string");
    }
}