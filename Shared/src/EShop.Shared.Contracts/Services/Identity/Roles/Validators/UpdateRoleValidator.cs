using FluentValidation;

namespace EShop.Shared.Contracts.Services.Identity.Roles.Validators;

public class UpdateRoleValidator : AbstractValidator<Command.UpdateRole>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}