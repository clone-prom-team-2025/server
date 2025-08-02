using App.Core.DTOs.Product;
using FluentValidation;

namespace App.Core.Validations;

public class ProductFilterRequestDtoValidator :  AbstractValidator<ProductFilterRequestDto>
{
    public ProductFilterRequestDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("CategoryId must be a 24-character hex string");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(0);
    }
}