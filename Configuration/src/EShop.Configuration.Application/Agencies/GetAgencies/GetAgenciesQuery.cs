using EShop.Shared.CQRS.Query;

namespace EShop.Configuration.Application.Agencies.GetAgencies;

public sealed class GetAgenciesQuery : IQuery<List<GetAgenciesResponse>>;
