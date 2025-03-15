using AutoMapper;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using MassTransit;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public class FeatureEventConsumers : IConsumer<SupportedFeaturesUpdated>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public FeatureEventConsumers(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<SupportedFeaturesUpdated> context)
    {
        // Hanlde idempotent

        // Convert context.Message to command
        var command = _mapper.Map<Command.UpdateSupportFeaturesCommand>(context.Message);

        // Send command to command handler
        await _sender.Send(command);
    }
}