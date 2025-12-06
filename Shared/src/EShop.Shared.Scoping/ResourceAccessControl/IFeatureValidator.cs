using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Shared.Scoping.ResourceAccessControl;

public interface IFeatureValidator
{
    Task<bool> HasFeatureAsync(string featureId);

    Task<bool> HasAtLeastOneOfSpecifiedFeaturesEnabledAsync(params string[] featureIds);
}

public sealed class CurrentUserFeaturesValidator : IFeatureValidator
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantFeaturesProvider _userFeaturesProvider;

    public CurrentUserFeaturesValidator(IUserDetailsProvider userDetailsProvider, ITenantFeaturesProvider userFeaturesProvider)
    {
        _userDetailsProvider = userDetailsProvider;
        _userFeaturesProvider = userFeaturesProvider;
    }

    public Task<bool> HasFeatureAsync(string featureId)
    {
        return HasAtLeastOneOfSpecifiedFeaturesEnabledAsync(featureId);
    }

    public async Task<bool> HasAtLeastOneOfSpecifiedFeaturesEnabledAsync(params string[] featureIds)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return false;
        }

        var tenantId = _userDetailsProvider.AuthenticatedUser.TenantId;

        var currentFeatures = await _userFeaturesProvider.GetFeatures(tenantId);
        return featureIds.Any(featureId => currentFeatures.Contains(featureId));
    }
}