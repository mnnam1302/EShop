using EShop.Shared.Contracts.Shared;
using FluentValidation;

namespace EShop.Shared.Contracts.Services.Tenancy.Tenants.Validators;

public class CreateTenantInternalValidator : AbstractValidator<Command.CreateTenantCommandInternal>
{
    public CreateTenantInternalValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .MaximumLength(ModelConstants.ShortText);

        RuleFor(x => x.TenantName)
            .NotEmpty()
            .MaximumLength(ModelConstants.ShortMediumText);

        RuleFor(x => x.OwnerUsername)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);

        RuleFor(x => x.OwnerDisplayName)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);

        RuleFor(x => x.OwnerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(ModelConstants.MediumLongText);
    }
}
