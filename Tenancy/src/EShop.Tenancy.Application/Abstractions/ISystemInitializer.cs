namespace EShop.Tenancy.Application.Abstractions;

public interface ISystemInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
