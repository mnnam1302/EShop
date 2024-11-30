using EShop.Shared.Contracts.Abstractions.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Shared.Contracts.Services.Identity.Auth
{
    public static class Command
    {
        public record Logout(string UserId) : ICommand;
    }
}
