using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Shared.Scoping.ResourceAccessControl;

public interface IFeatureValidator
{
    Task<bool> HasFeatureAsync(string featureId);

    Task<bool> HasAtLeastOneOfSpecifiedFeaturesEnabledAsync(params string[] featureIds);
}

public class CurrentUserFeaturesValidator : IFeatureValidator
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IUserFeaturesProvider _userFeaturesProvider;

    public CurrentUserFeaturesValidator(IUserDetailsProvider userDetailsProvider, IUserFeaturesProvider userFeaturesProvider)
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

        var currentUserId = _userDetailsProvider.AuthenticatedUser.Id;
        var tenantId = _userDetailsProvider.AuthenticatedUser.TenantId;

        var currentFeatures = await _userFeaturesProvider.GetFeatures(currentUserId, tenantId);
        return featureIds.Any(featureId => currentFeatures.Contains(featureId));
    }
}