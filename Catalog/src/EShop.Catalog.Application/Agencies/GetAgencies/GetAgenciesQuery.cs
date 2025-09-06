
using EShop.Shared.CQRS.Query;

namespace EShop.Catalog.Application.Agencies.GetAgencies;

public sealed class GetAgenciesQuery : IQuery<List<GetAgenciesResponse>>;
