using System.Reflection;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FL.Client.Messaging.Signals;

public static class SignalsServiceCollectionExtensions
{
    public static IServiceCollection AddSignalConsumer<T, THandler>(this IServiceCollection services)
        where THandler : class, ISignalConsumer<T>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ISignalConsumer<T>, THandler>(sp => sp.GetRequiredService<THandler>());
        services.AddScoped<ISignalConsumer, THandler>(sp => sp.GetRequiredService<THandler>());
        return services;
    }
    
    public static IServiceProvider UseSignalConsumers(this IServiceProvider serviceProvider)
    {
        var eventTypes = new HashSet<Type>();
        var eventHandlers = serviceProvider.GetServices<ISignalConsumer>();

        //Get all the registered event handlers, and the EventType it's handling.
        //Build a unique set of Event Types.
        foreach (var e in eventHandlers)
        {
            var eventType = e.GetType().GetInterface("ISignalConsumer`1")?.GenericTypeArguments[0];
            if (eventType == null) continue;
            eventTypes.Add(eventType);
        }

        //Loop through all the event types, and Find the equivalent AsyncSubscribers, and wire up the subscribe events to the event handlers.
        foreach (var eventType in eventTypes)
        {
            typeof(SignalsServiceCollectionExtensions)
                .GetMethod(nameof(SubscribeAsyncEventHandlers), BindingFlags.NonPublic | BindingFlags.Static)?
                .MakeGenericMethod(eventType).Invoke(null, [serviceProvider]);
        }

        return serviceProvider;
    }

    private static IServiceProvider SubscribeAsyncEventHandlers<T>(this IServiceProvider serviceProvider)
    {
        var eventHandlers = serviceProvider.GetServices<ISignalConsumer<T>>();
        serviceProvider.GetRequiredService<IAsyncSubscriber<T>>().Subscribe(async (@event, token) =>
        {
            foreach (var handler in eventHandlers)
            {
                await handler.Handle(@event, token).ConfigureAwait(false);
            }
        });
        return serviceProvider;
    }
}