using EShop.Authorization.Application.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed record GetPublicKeyQuery(string TenantId, string? KeyId = null) : IQuery<PublicKeyResponse>;

public sealed record PublicKeyResponse
{
    public required string KeyId { get; init; }
    public required string TenantId { get; init; }
    public required string PublicKeyPem { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required string Algorithm { get; init; } = "RS256";
}

internal sealed class GetPublicKeyQueryHandler : IQueryHandler<GetPublicKeyQuery, PublicKeyResponse>
{
    private readonly IRsaKeyManager _rsaKeyManager;

    public GetPublicKeyQueryHandler(IRsaKeyManager rsaKeyManager)
    {
        _rsaKeyManager = rsaKeyManager;
    }

    public async Task<Result<PublicKeyResponse>> HandleAsync(GetPublicKeyQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            RsaKeyPair? keyPair;

            if (!string.IsNullOrEmpty(query.KeyId))
            {
                // Get specific key by ID
                var publicKey = await _rsaKeyManager.GetPublicKeyAsync(query.TenantId, query.KeyId);
                // Note: This would require extending IRsaKeyManager to get full key pair by KeyId
                // For now, we'll get the active key pair
                keyPair = await _rsaKeyManager.GetActiveKeyPairAsync(query.TenantId);

                if (keyPair == null || keyPair.KeyId != query.KeyId)
                {
                    return Result.Failure<PublicKeyResponse>(new Error("PublicKey.NotFound", $"Public key with ID {query.KeyId} not found for tenant {query.TenantId}"));
                }
            }
            else
            {
                // Get active key pair
                keyPair = await _rsaKeyManager.GetActiveKeyPairAsync(query.TenantId);

                if (keyPair == null)
                {
                    return Result.Failure<PublicKeyResponse>(new Error("PublicKey.NotFound", $"No active public key found for tenant {query.TenantId}"));
                }
            }

            var publicKeyPem = keyPair.PublicKey.ExportSubjectPublicKeyInfoPem();

            var response = new PublicKeyResponse
            {
                KeyId = keyPair.KeyId,
                TenantId = keyPair.TenantId,
                PublicKeyPem = publicKeyPem,
                ExpiresAt = keyPair.ExpiresAt,
                Algorithm = "RS256"
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<PublicKeyResponse>(new Error("PublicKey.RetrievalFailed", ex.Message));
        }
    }
}