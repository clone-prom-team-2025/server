using App.Core.DTOs.Product;
using FluentValidation;

namespace App.Core.Validations;

public class ProductDtoValidator : AbstractValidator<ProductDto>
{
    public ProductDtoValidator()
    {
        RuleFor(x => x.Id)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Id must be a 24-character hex string");

        RuleFor(x => x.Name)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Name must be non-empty.")
            .MaximumLength(128);

        RuleFor(x => x.ProductType)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("ProductType must be non-empty.")
            .MaximumLength(50);

        RuleFor(x => x.CategoryPath)
            .NotNull()
            .Must(name => name.All(item => !string.IsNullOrWhiteSpace(item)))
            .Must(path => path.Count > 0).WithMessage("At least one category path must be specified.")
            .ForEach(pathRule =>
            {
                pathRule
                    .Matches("^[a-fA-F0-9]{24}$")
                    .WithMessage("CategoryId must be a 24-character hex string");
            });

        RuleFor(x => x.SellerId)
            .NotEmpty()
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("SellerId must be a 24-character hex string");
    }
}