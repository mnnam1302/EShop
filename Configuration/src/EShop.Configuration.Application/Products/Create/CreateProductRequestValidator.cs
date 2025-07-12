using EShop.Shared.Contracts.Shared;
using FluentValidation;

namespace EShop.Configuration.Application.Products.Create;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);
    }
}
