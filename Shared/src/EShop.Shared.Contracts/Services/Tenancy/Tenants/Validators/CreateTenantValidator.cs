using EShop.Shared.Contracts.Shared;
using FluentValidation;

namespace EShop.Shared.Contracts.Services.Tenancy.Tenants.Validators;

public class CreateTenantValidator : AbstractValidator<Command.CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(ModelConstants.ShortText);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ModelConstants.ShortMediumText);

        RuleFor(x => x.OwnerUsername)
            .NotEmpty()
            .MaximumLength(ModelConstants.MediumText);

        RuleFor(x => x.OwnerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(ModelConstants.MediumLongText);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(ModelConstants.LongText);

        RuleFor(x => x.Description)
            .MaximumLength(ModelConstants.LongText);
    }
}
