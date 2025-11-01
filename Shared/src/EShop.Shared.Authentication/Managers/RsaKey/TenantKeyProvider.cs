using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.DependencyInjections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace EShop.Shared.Authentication.Managers.RsaKey;

internal sealed class TenantKeyProvider : ITenantKeyProvider
{
    private readonly ITenantKeyCachingService _tenantKeyCachingService;
    private readonly ILogger<TenantKeyProvider> _logger;
    private readonly TenantKeyOptions _keyOptions;

    public TenantKeyProvider(
        ITenantKeyCachingService tenantKeyCachingService,
        ILogger<TenantKeyProvider> logger,
        IOptionsMonitor<TenantKeyOptions> rsaKeyOptions)
    {
        _tenantKeyCachingService = tenantKeyCachingService;
        _logger = logger;
        _keyOptions = rsaKeyOptions.CurrentValue;
    }

    public async Task<RsaKeyPair> GetOrCreateKeyPairAsync(string tenantId, CancellationToken cancellationToken)
    {
        var existingKeyPair = await _tenantKeyCachingService.GetAsync(tenantId, cancellationToken);
        if (existingKeyPair != null)
        {
            return existingKeyPair;
        }

        var newKeyPair = CreateRsaKeyPair(tenantId);
        await _tenantKeyCachingService.AddAsync(tenantId, newKeyPair, cancellationToken);

        _logger.LogInformation("Generated new RSA key pair for tenant {TenantId} with KeyId {KeyId}", tenantId, newKeyPair.KeyId);

        return newKeyPair;
    }

    private RsaKeyPair CreateRsaKeyPair(string tenantId)
    {
        using var rsa = RSA.Create(_keyOptions.KeySizeInBits);

        return new RsaKeyPair
        {
            KeyId = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            PrivateKeyPem = rsa.ExportRSAPrivateKeyPem(), // Export as PEM strings instead of RSA instances
            PublicKeyPem = rsa.ExportRSAPublicKeyPem(),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_keyOptions.KeyExpiryInDays)
        };
    }

    public async Task<RsaKeyPair> GetKeyPairAsync(string tenantId, CancellationToken cancellationToken)
    {
        var rsaKeyPair = await _tenantKeyCachingService.GetAsync(tenantId, cancellationToken);
        if (rsaKeyPair == null)
        {
            throw new InvalidOperationException($"RSA key pair for tenant {tenantId} is not found.");
        }

        return rsaKeyPair;
    }

    public Task RotateKeyPairAsync(string tenantId, CancellationToken cancellationToken)
    {
        // TODO: Kodi implement later
        throw new NotImplementedException();
    }
}
