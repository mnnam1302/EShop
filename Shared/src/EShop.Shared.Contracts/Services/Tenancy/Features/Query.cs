using EShop.Shared.Contracts.Abstractions.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public static class Query
{
    public record GetFeaturesQuery() : IQuery<Response.FeatureResponseInternal>;
}
