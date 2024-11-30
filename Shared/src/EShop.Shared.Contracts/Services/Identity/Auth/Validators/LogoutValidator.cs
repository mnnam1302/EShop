using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Shared.Contracts.Services.Identity.Auth.Validators
{
    public class LogoutValidator : AbstractValidator<Command.Logout>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User id is required.");
        }
    }
}
