using FluentValidation;

namespace EShop.Shared.Contracts.Services.Identity.Users.Validators;

public class CreateUserValidator : AbstractValidator<Command.CreateUserCommand>
{
    public CreateUserValidator()
    {
        // Username not null, and must not equal UserData.SystemUser
        RuleFor(x => x.Username)
            .NotEmpty()
            .NotEqual("System")
            .NotEqual("system")
            .WithMessage("Invalid username.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.DisplayName)
            .NotEmpty();

        RuleFor(x => x.RoleIds)
            .NotEmpty();

        RuleFor(x => x.OrganizationId)
            .NotEmpty();
    }
}