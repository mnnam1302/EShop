using EShop.Shared.Scoping;
using FluentValidation;

namespace EShop.Shared.Contracts.Services.Identity.Users.Validators;

public class CreateUserRequestValidator : AbstractValidator<Command.CreateUserCommand>
{
    public CreateUserRequestValidator()
    {
        // Username not null, and must not equal UserData.SystemUser
        RuleFor(x => x.Username)
            .NotEmpty()
            .NotEqual(UserData.SystemUsername)
            .WithMessage("Invalid username.");

        RuleFor(x => x.Password)
            .NotEmpty();

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