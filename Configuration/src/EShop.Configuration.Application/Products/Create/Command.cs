using EShop.Configuration.Application.Shared;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Configuration.Application.Products.Create;

public sealed class Command : ICommand
{
    public required string Name { get; init; }
    public Guid? AgencyId { get; init; }
}

public sealed class CommandHandler(ConfigurationDbContext dbContext) : ICommandHandler<Command>
{
    public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            AgencyId = command.AgencyId
        };

        return Result.Success();
    }
}
