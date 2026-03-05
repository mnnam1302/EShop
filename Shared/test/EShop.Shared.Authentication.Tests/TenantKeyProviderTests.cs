using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Authentication.Managers.RsaKey;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EShop.Shared.Authentication.Tests;

public sealed class TenantKeyProviderTests
{
    private readonly Mock<ITenantKeyCachingService> _mockCachingService;
    private readonly Mock<ILogger<TenantKeyProvider>> _mockLogger;
    private readonly Mock<IOptionsMonitor<TenantKeyOptions>> _mockKeyOptions;
    private readonly TenantKeyProvider _sut;

    public TenantKeyProviderTests()
    {
        _mockCachingService = new Mock<ITenantKeyCachingService>();
        _mockLogger = new Mock<ILogger<TenantKeyProvider>>();
        _mockKeyOptions = new Mock<IOptionsMonitor<TenantKeyOptions>>();

        // Setup default options
        _mockKeyOptions.Setup(x => x.CurrentValue).Returns(new TenantKeyOptions
        {
            KeySizeInBits = 2048,
            KeyExpiryInDays = 365,
            PreviousKeyTtlMinutes = 15
        });

        _sut = new TenantKeyProvider(
            _mockCachingService.Object,
            _mockLogger.Object,
            _mockKeyOptions.Object
        );
    }

    /// <summary>
    /// `RotateKeyPairAsync` no longer throws `NotImplementedException`
    /// </summary>
    [Fact]
    public async Task RotateKeyPairAsync_ShouldCompleteSuccessfully_WithoutThrowingNotImplementedException()
    {
        // Arrange
        var tenantId = "tenant-123";
        var existingActiveKey = CreateTestKeyPair(tenantId, "old-key-id");

        _mockCachingService
            .Setup(x => x.GetActiveKeyAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingActiveKey);

        _mockCachingService
            .Setup(x => x.SetPreviousKeyAsync(tenantId, existingActiveKey, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCachingService
            .Setup(x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _sut.RotateKeyPairAsync(tenantId, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<NotImplementedException>();
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// After rotation, new tokens are signed with the new active key
    /// </summary>
    [Fact]
    public async Task RotateKeyPairAsync_ShouldGenerateAndStoreNewActiveKey()
    {
        // Arrange
        var tenantId = "tenant-123";
        var oldActiveKey = CreateTestKeyPair(tenantId, "old-key-id");
        RsaKeyPair? capturedNewKey = null;

        _mockCachingService
            .Setup(x => x.GetActiveKeyAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldActiveKey);

        _mockCachingService
            .Setup(x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()))
            .Callback<string, RsaKeyPair, CancellationToken>((_, keyPair, _) => capturedNewKey = keyPair)
            .Returns(Task.CompletedTask);

        _mockCachingService
            .Setup(x => x.SetPreviousKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.RotateKeyPairAsync(tenantId, CancellationToken.None);

        // Assert
        capturedNewKey.Should().NotBeNull();
        capturedNewKey!.KeyId.Should().NotBe(oldActiveKey.KeyId, "a new key should be generated");
        capturedNewKey.TenantId.Should().Be(tenantId);
        capturedNewKey.PrivateKeyPem.Should().NotBeNullOrEmpty();
        capturedNewKey.PublicKeyPem.Should().NotBeNullOrEmpty();

        _mockCachingService.Verify(
            x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "new key should be stored as active"
        );
    }

    /// <summary>
    /// After rotation, tokens signed with the previous key still validate via fallback
    /// </summary>
    [Fact]
    public async Task RotateKeyPairAsync_ShouldMoveCurrentActiveKeyToPreviousKey()
    {
        // Arrange
        var tenantId = "tenant-123";
        var currentActiveKey = CreateTestKeyPair(tenantId, "current-key-id");
        RsaKeyPair? capturedPreviousKey = null;
        TimeSpan capturedTtl = TimeSpan.Zero;

        _mockCachingService
            .Setup(x => x.GetActiveKeyAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentActiveKey);

        _mockCachingService
            .Setup(x => x.SetPreviousKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<string, RsaKeyPair, TimeSpan, CancellationToken>((_, keyPair, ttl, _) =>
            {
                capturedPreviousKey = keyPair;
                capturedTtl = ttl;
            })
            .Returns(Task.CompletedTask);

        _mockCachingService
            .Setup(x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.RotateKeyPairAsync(tenantId, CancellationToken.None);

        // Assert
        capturedPreviousKey.Should().NotBeNull();
        capturedPreviousKey!.KeyId.Should().Be(currentActiveKey.KeyId, "current active key should become previous key");
        capturedPreviousKey.TenantId.Should().Be(currentActiveKey.TenantId);

        _mockCachingService.Verify(
            x => x.SetPreviousKeyAsync(tenantId, currentActiveKey, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "current active key should be stored as previous"
        );
    }

    /// <summary>
    /// After previous key TTL expires, tokens signed with it are rejected
    /// </summary>
    [Fact]
    public async Task RotateKeyPairAsync_ShouldSetPreviousKeyTTL_ToPreviousKeyTtlMinutes()
    {
        // Arrange
        var tenantId = "tenant-123";
        var currentActiveKey = CreateTestKeyPair(tenantId, "current-key-id");
        var expectedTtl = TimeSpan.FromMinutes(15);
        TimeSpan actualTtl = TimeSpan.Zero;

        _mockCachingService
            .Setup(x => x.GetActiveKeyAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentActiveKey);

        _mockCachingService
            .Setup(x => x.SetPreviousKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<string, RsaKeyPair, TimeSpan, CancellationToken>((_, _, ttl, _) => actualTtl = ttl)
            .Returns(Task.CompletedTask);

        _mockCachingService
            .Setup(x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.RotateKeyPairAsync(tenantId, CancellationToken.None);

        // Assert
        actualTtl.Should().Be(expectedTtl, "TTL should match PreviousKeyTtlMinutes configuration");

        _mockCachingService.Verify(
            x => x.SetPreviousKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), expectedTtl, It.IsAny<CancellationToken>()),
            Times.Once,
            "previous key should expire after PreviousKeyTtlMinutes"
        );
    }

    /// <summary>
    /// In-memory cache contains both active and previous keys after rotation
    /// </summary>
    [Fact]
    public async Task RotateKeyPairAsync_ShouldStoreActiveAndPreviousKeys_InMemoryCache()
    {
        // Arrange
        var tenantId = "tenant-123";
        var currentActiveKey = CreateTestKeyPair(tenantId, "current-key-id");

        _mockCachingService
            .Setup(x => x.GetActiveKeyAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentActiveKey);

        _mockCachingService
            .Setup(x => x.SetPreviousKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockCachingService
            .Setup(x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act - Perform rotation
        await _sut.RotateKeyPairAsync(tenantId, CancellationToken.None);

        // Simulate Redis failure by throwing exception (not InvalidOperationException, as that's reserved for missing keys)
        _mockCachingService
            .Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        _mockCachingService
            .Setup(x => x.GetPreviousKeyAsync(tenantId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        // Assert - Should retrieve from in-memory cache (active key)
        var activeKeyFromCache = await _sut.GetKeyPairAsync(tenantId, CancellationToken.None);
        activeKeyFromCache.Should().NotBeNull("active key should be cached in-memory");

        // Assert - Should retrieve from in-memory cache (previous key)
        var previousKeyFromCache = await _sut.GetPreviousKeyPairAsync(tenantId, CancellationToken.None);
        previousKeyFromCache.Should().NotBeNull("previous key should be cached in-memory");
        previousKeyFromCache!.KeyId.Should().Be(currentActiveKey.KeyId, "cached previous key should match the old active key");
    }

    /// <summary>
    /// First rotation (no existing previous key) completes without error
    /// </summary>
    [Fact]
    public async Task RotateKeyPairAsync_ShouldCompleteSuccessfully_WhenNoExistingActiveKey()
    {
        // Arrange
        var tenantId = "new-tenant-123";

        _mockCachingService
            .Setup(x => x.GetActiveKeyAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RsaKeyPair?)null); // No existing active key

        _mockCachingService
            .Setup(x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _sut.RotateKeyPairAsync(tenantId, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync("first rotation should complete without errors");

        _mockCachingService.Verify(
            x => x.SetActiveKeyAsync(tenantId, It.IsAny<RsaKeyPair>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "new active key should be stored even on first rotation"
        );

        _mockCachingService.Verify(
            x => x.SetPreviousKeyAsync(It.IsAny<string>(), It.IsAny<RsaKeyPair>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "previous key should NOT be set when there is no existing active key"
        );
    }

    private static RsaKeyPair CreateTestKeyPair(string tenantId, string keyId)
    {
        using var rsa = System.Security.Cryptography.RSA.Create(2048);

        return new RsaKeyPair
        {
            KeyId = keyId,
            TenantId = tenantId,
            PrivateKeyPem = rsa.ExportRSAPrivateKeyPem(),
            PublicKeyPem = rsa.ExportRSAPublicKeyPem(),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(365)
        };
    }
}
