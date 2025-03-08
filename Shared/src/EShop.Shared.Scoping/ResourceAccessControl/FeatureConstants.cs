using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Shared.Scoping.ResourceAccessControl
{
    public static class FeatureConstants
    {
        // Identity
        public const string Identity_ExternalApplicationIntegration_FeatureId = "Identity_ExternalApplicationIntegration";
        public const string Identity_UserInvites_FeatureId = "Identity_UserInvites";
        public const string Identity_OrganisationRingFencing_FeatureId = "Identity_OrganisationRingFencing";
        public const string Identity_CustomRoles_FeatureId = "Identity_CustomRoles";
    }

    public enum FeatureModules
    {
        EShop_Identity,
    }

    public enum FeatureState
    {
        NotAvailable,
        Enabled,
        Disabled
    }

    public enum FeatureCategory
    {
        Permanent,
        Release,
        Experiment,
        Operational
    }
}
