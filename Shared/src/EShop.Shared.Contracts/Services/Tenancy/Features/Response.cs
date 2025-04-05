using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public static class Response
{
    public record FeatureResponseInternal
    {
        public string[] FeatureIds { get; set; } = Array.Empty<string>();
    }
}
