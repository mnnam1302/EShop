using FluentValidation;

namespace EShop.Shared.Contracts.Services.Identity.Users.Validators;

public class RegisterUserValidator : AbstractValidator<Command.RegisterUser>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}