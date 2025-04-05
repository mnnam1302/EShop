using FluentValidation;

namespace EShop.Shared.Contracts.Services.Identity.Organizations.Validators;

public class CreateOrganizationCommandValidator : AbstractValidator<Command.CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.OrganizationNumber)
            .NotEmpty()
            .Matches(@"^[0-9]{50}$");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{10,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.ParentOrganizationId)
            .NotEmpty();
    }
}