using MassTransit;
using SerilogContext = Serilog.Context.LogContext;

namespace EShop.Shared.EventBus.Filters;

/// <summary>
/// Pushes MassTransit's envelope CorrelationId into Serilog's LogContext so all log entries
/// within a consumer automatically carry the CorrelationId property.
/// Works for any message type — no domain-specific interface needed.
/// </summary>
public sealed class CorrelationIdLogEnrichFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (context.CorrelationId.HasValue)
        {
            using (SerilogContext.PushProperty("CorrelationId", context.CorrelationId.Value))
                await next.Send(context);

            return;
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context) => context.CreateFilterScope("correlationEnrich");
}
