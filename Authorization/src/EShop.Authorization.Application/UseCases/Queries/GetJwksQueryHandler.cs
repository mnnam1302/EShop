using EShop.Authorization.Application.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed record GetJwksQuery(string TenantId) : IQuery<JwksResponse>;

public sealed record JwksResponse
{
    public required JwkKey[] Keys { get; init; }
}

public sealed record JwkKey
{
    public required string Kty { get; init; } = "RSA";
    public required string Use { get; init; } = "sig";
    public required string Kid { get; init; }
    public required string Alg { get; init; } = "RS256";
    public required string N { get; init; }
    public required string E { get; init; }
}

internal sealed class GetJwksQueryHandler : IQueryHandler<GetJwksQuery, JwksResponse>
{
    private readonly IRsaKeyManager _rsaKeyManager;

    public GetJwksQueryHandler(IRsaKeyManager rsaKeyManager)
    {
        _rsaKeyManager = rsaKeyManager;
    }

    public async Task<Result<JwksResponse>> HandleAsync(GetJwksQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyPair = await _rsaKeyManager.GetActiveKeyPairAsync(query.TenantId);

            if (keyPair == null)
            {
                return Result.Failure<JwksResponse>(new Error("JWKS.NotFound", $"No active keys found for tenant {query.TenantId}"));
            }

            var rsaParams = keyPair.PublicKey.ExportParameters(false);

            var jwkKey = new JwkKey
            {
                Kty = "RSA",
                Use = "sig",
                Alg = "RS256",
                Kid = keyPair.KeyId,
                N = Convert.ToBase64String(rsaParams.Modulus!).TrimEnd('=').Replace('+', '-').Replace('/', '_'),
                E = Convert.ToBase64String(rsaParams.Exponent!).TrimEnd('=').Replace('+', '-').Replace('/', '_')
            };

            var response = new JwksResponse
            {
                Keys = [jwkKey]
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<JwksResponse>(new Error("JWKS.RetrievalFailed", ex.Message));
        }
    }
}