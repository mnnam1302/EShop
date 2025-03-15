using AutoMapper;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus;
using EShop.Tenancy.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Infrastructure.Consumers;

public class FeatureEventConsumers : IConsumer<SupportedFeaturesUpdated>
{
    private readonly TenancyDbContext _dbContext;
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public FeatureEventConsumers(TenancyDbContext dbContext, ISender sender, IMapper mapper)
    {
        _dbContext = dbContext;
        _sender = sender;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<SupportedFeaturesUpdated> context)
    {
        var existsingInbox = await _dbContext.InboxMessages
            .AnyAsync(x => x.MessageId == context.Message.EventId);

        if (!existsingInbox)
        {
            var command = _mapper.Map<Command.UpdateSupportedFeaturesCommand>(context.Message);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                var consumerId = $"{typeof(FeatureEventConsumers).Name}:{context.Message.GetType().Name}";
                var inboxMessage = new InboxMessage
                {
                    MessageId = context.Message.EventId,
                    MessageType = context.Message.GetType().Name,
                    ConsumerId = consumerId,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _dbContext.InboxMessages.Add(inboxMessage);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}