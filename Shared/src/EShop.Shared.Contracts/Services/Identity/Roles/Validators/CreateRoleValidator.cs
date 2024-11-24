using FluentValidation;

namespace EShop.Shared.Contracts.Services.Identity.Roles.Validators
{
    public class CreateRoleValidator : AbstractValidator<Command.CreateRole>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();
        }
    }
}