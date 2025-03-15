using MassTransit;
using Serilog;

namespace EShop.Tenancy.Infrastructure.PipelineObservers;

public class LoggingConsumeObserver : IConsumeObserver
{
    public async Task PreConsume<T>(ConsumeContext<T> context)
        where T : class
    {
        await Task.Yield();

        Log.Information("Consuming {Message} message from {Namespace}, CorrelationId: {CorrelationId}", typeof(T).Name, typeof(T).Namespace, context.CorrelationId);
    }

    public async Task PostConsume<T>(ConsumeContext<T> context)
        where T : class
    {
        await Task.CompletedTask;
    }

    public async Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
        where T : class
    {
        await Task.CompletedTask;
    }
}