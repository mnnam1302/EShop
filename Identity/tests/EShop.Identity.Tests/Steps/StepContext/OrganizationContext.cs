using EShop.Identity.Tests.Setups;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Identity.Tests.Steps.StepContext
{
    public class OrganizationContext
    {
        private readonly ApiContext _apiContext;
        private readonly ILogger<OrganizationContext> _logger;

        public OrganizationContext(ApiContext apiContext)
        {
            _apiContext = apiContext;
            _logger = _apiContext.ServiceProvider.GetRequiredService<ILogger<OrganizationContext>>();
        }


    }
}
