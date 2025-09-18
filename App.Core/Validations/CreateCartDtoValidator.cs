using App.Core.DTOs.Cart;
using FluentValidation;

namespace App.Core.Validations;

public class CreateCartDtoValidator : AbstractValidator<CreateCartDto>
{
    public CreateCartDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("Id must be a 24-character hex string");

        RuleFor(x => x.Pcs)
            .Must(f => f > 0).WithMessage("Pcs must be a positive number.");
    }
}