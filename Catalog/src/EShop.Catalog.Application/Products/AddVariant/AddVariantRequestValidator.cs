using EShop.Shared.Contracts.Shared;
using FluentValidation;

namespace EShop.Catalog.Application.Products.AddVariant;

public sealed class AddVariantRequestValidator : AbstractValidator<AddVariantRequest>
{
    public AddVariantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DiscountPrice)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.Price);

        RuleForEach(x => x.DimensionValues).ChildRules(dv =>
        {
            dv.RuleFor(x => x.Name).NotEmpty();
            dv.RuleFor(x => x.Value).NotEmpty();
        });
    }
}
