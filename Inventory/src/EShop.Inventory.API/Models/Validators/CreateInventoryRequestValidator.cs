using EShop.Shared.Contracts.Shared;
using FluentValidation;

namespace EShop.Inventory.API.Models.Validators;

public sealed class CreateInventoryRequestValidator : AbstractValidator<CreateInventoryRequest>
{
    public CreateInventoryRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.SkuId)
            .NotEmpty();

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);

        RuleFor(x => x.StockAvailable)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MinimumStock)
            .GreaterThanOrEqualTo(0);
    }
}
