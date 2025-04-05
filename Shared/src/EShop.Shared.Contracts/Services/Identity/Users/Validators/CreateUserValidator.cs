using FluentValidation;

namespace EShop.Shared.Contracts.Services.Identity.Users.Validators;

public class CreateUserValidator : AbstractValidator<Command.CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.DisplayName)
            .NotEmpty();

        RuleFor(x => x.OrganizationId)
            .NotEmpty();
        
        RuleFor(x => x.RoleIds)
            .NotEmpty();
    }
}