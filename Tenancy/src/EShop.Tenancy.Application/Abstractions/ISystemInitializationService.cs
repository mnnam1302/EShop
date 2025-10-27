namespace EShop.Tenancy.Application.Abstractions;

public interface ISystemInitializationService
{
    Task InitializeSystemAsync(CancellationToken cancellationToken = default);
}
