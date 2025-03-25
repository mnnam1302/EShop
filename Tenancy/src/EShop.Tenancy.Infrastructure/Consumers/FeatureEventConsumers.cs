using AutoMapper;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus;
using EShop.Tenancy.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Infrastructure.Consumers;

public class FeatureEventConsumers 
    : IConsumer<SupportedFeaturesUpdated>, IConsumer<TenantFeaturesUpdated>
{
    private readonly TenancyDbContext _dbContext;
    private readonly ISender _sender;

    public FeatureEventConsumers(TenancyDbContext dbContext, ISender sender)
    {
        _dbContext = dbContext;
        _sender = sender;
    }

    public async Task Consume(ConsumeContext<SupportedFeaturesUpdated> context)
    {
        var existsingInbox = await _dbContext.InboxMessages
            .AnyAsync(x => x.MessageId == context.Message.EventId);

        if (!existsingInbox)
        {
            var command = new Command.UpdateSupportedFeaturesCommand
            {
                SourceSystemReference = context.Message.SourceSystemReference,
                Features = context.Message.Features,
                Action = context.Message.Action,
                TenantId = context.Message.TenantId,
                ActionUserId = context.Message.ActionUserId
            };

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

    public async Task Consume(ConsumeContext<TenantFeaturesUpdated> context)
    {
        var existsingInbox = await _dbContext.InboxMessages
            .AnyAsync(x => x.MessageId == context.Message.EventId);

        if (!existsingInbox)
        {
            var command = new Command.UpdateTenantFeaturesCommand(context.Message.TenantId);

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