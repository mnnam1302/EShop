using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.DependencyInjections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EShop.Shared.Authentication.Managers.RsaKey;

internal sealed class TenantKeyProvider : ITenantKeyProvider
{
    private readonly ITenantKeyCachingService _tenantKeyCachingService;
    private readonly ILogger<TenantKeyProvider> _logger;
    private readonly TenantKeyOptions _keyOptions;

    // In-memory fallback cache for RSA key pairs loaded during the current process lifetime
    // Key format: "{tenantId}:active" or "{tenantId}:previous"
    private readonly ConcurrentDictionary<string, RsaKeyPair> _inMemoryCache = new();

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
        try
        {
            // Try to get from Redis (GetAsync tries active key first, falls back to legacy)
            var existingKeyPair = await _tenantKeyCachingService.GetAsync(tenantId, cancellationToken);
            if (existingKeyPair != null)
            {
                // Cache in-memory for fallback using active key format
                _inMemoryCache.TryAdd($"{tenantId}:active", existingKeyPair);
                return existingKeyPair;
            }

            // Generate new key pair if not found
            var newKeyPair = CreateRsaKeyPair(tenantId);
            await _tenantKeyCachingService.AddAsync(tenantId, newKeyPair, cancellationToken);

            // Cache in-memory for fallback using active key format
            _inMemoryCache.TryAdd($"{tenantId}:active", newKeyPair);

            _logger.LogInformation("Generated new RSA key pair for tenant {TenantId} with KeyId {KeyId}", tenantId, newKeyPair.KeyId);

            return newKeyPair;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable, attempting in-memory fallback for tenant {TenantId}", tenantId);

            // Fall back to in-memory cache if Redis fails (try active key format first, then legacy)
            if (_inMemoryCache.TryGetValue($"{tenantId}:active", out var cachedKeyPair) ||
                _inMemoryCache.TryGetValue(tenantId, out cachedKeyPair))
            {
                _logger.LogWarning("Operating in degraded mode: using in-memory cached RSA key for tenant {TenantId}", tenantId);
                return cachedKeyPair;
            }

            _logger.LogError("Failed to retrieve RSA key for tenant {TenantId}: Redis unavailable and no in-memory fallback exists", tenantId);
            throw new InvalidOperationException($"RSA key pair for tenant {tenantId} is not available (Redis unavailable and no cached key)", ex);
        }
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
        try
        {
            var rsaKeyPair = await _tenantKeyCachingService.GetAsync(tenantId, cancellationToken)
                ?? throw new InvalidOperationException($"RSA key pair for tenant {tenantId} is not found.");

            // Cache in-memory for fallback using active key format
            _inMemoryCache.TryAdd($"{tenantId}:active", rsaKeyPair);
            return rsaKeyPair;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogWarning(ex, "Redis unavailable, attempting in-memory fallback for tenant {TenantId}", tenantId);

            // Fall back to in-memory cache if Redis fails (try active key format first, then legacy)
            if (_inMemoryCache.TryGetValue($"{tenantId}:active", out var cachedKeyPair) ||
                _inMemoryCache.TryGetValue(tenantId, out cachedKeyPair))
            {
                _logger.LogWarning("Operating in degraded mode: using in-memory cached RSA key for tenant {TenantId}", tenantId);
                return cachedKeyPair;
            }

            // No fallback available - cannot proceed
            _logger.LogError("Failed to retrieve RSA key for tenant {TenantId}: Redis unavailable and no in-memory fallback exists", tenantId);
            throw new InvalidOperationException($"RSA key pair for tenant {tenantId} is not available (Redis unavailable and no cached key)", ex);
        }
    }

    public async Task<RsaKeyPair?> GetPreviousKeyPairAsync(string tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var rsaKeyPair = await _tenantKeyCachingService.GetPreviousKeyAsync(tenantId, cancellationToken);
            if (rsaKeyPair != null)
            {
                // Cache in-memory for fallback
                _inMemoryCache.TryAdd($"{tenantId}:previous", rsaKeyPair);
            }

            return rsaKeyPair;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable, attempting in-memory fallback for previous key for tenant {TenantId}", tenantId);

            // Fall back to in-memory cache if Redis fails
            if (_inMemoryCache.TryGetValue($"{tenantId}:previous", out var cachedKeyPair))
            {
                _logger.LogWarning("Operating in degraded mode: using in-memory cached previous RSA key for tenant {TenantId}", tenantId);
                return cachedKeyPair;
            }

            // Previous key not available (either doesn't exist or Redis is down and not cached)
            _logger.LogDebug("Previous RSA key for tenant {TenantId} not found", tenantId);
            return null;
        }
    }

    public async Task RotateKeyPairAsync(string tenantId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RSA key rotation for tenant {TenantId}", tenantId);

        try
        {
            // Get current active key
            var currentActiveKey = await _tenantKeyCachingService.GetActiveKeyAsync(tenantId, cancellationToken);

            // Generate new key pair
            var newKeyPair = CreateRsaKeyPair(tenantId);

            // Move current active to previous with TTL = PreviousKeyTtlMinutes
            if (currentActiveKey != null)
            {
                var ttl = TimeSpan.FromMinutes(_keyOptions.PreviousKeyTtlMinutes);
                await _tenantKeyCachingService.SetPreviousKeyAsync(tenantId, currentActiveKey, ttl, cancellationToken);

                // Update in-memory cache for previous key
                _inMemoryCache.TryAdd($"{tenantId}:previous", currentActiveKey);

                _logger.LogInformation("Moved current active key {OldKeyId} to previous for tenant {TenantId} with TTL {TTL} minutes",
                    currentActiveKey.KeyId, tenantId, _keyOptions.PreviousKeyTtlMinutes);
            }
            else
            {
                _logger.LogInformation("No existing active key found for tenant {TenantId}, this is the first key rotation", tenantId);
            }

            // Store new as active
            await _tenantKeyCachingService.SetActiveKeyAsync(tenantId, newKeyPair, cancellationToken);

            // Update in-memory cache for active key
            _inMemoryCache.AddOrUpdate($"{tenantId}:active", newKeyPair, (_, _) => newKeyPair);

            _logger.LogInformation("RSA key rotation completed for tenant {TenantId}. New active key: {NewKeyId}",
                tenantId, newKeyPair.KeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate RSA key for tenant {TenantId}", tenantId);
            throw;
        }
    }
}