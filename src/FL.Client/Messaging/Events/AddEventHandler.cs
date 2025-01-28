using System.Reflection;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace FL.Client.Messaging.Events;

public static class AddServiceCollectionEventHandlerExtensions
{
    public static IServiceCollection AddEventHandler<T, THandler>(this IServiceCollection services)
        where THandler : class, IEventHandler<T>
    {
        services.AddScoped<IEventHandler<T>, THandler>();
        services.AddScoped<IEventHandler, THandler>();
        return services;
    }

    public static IServiceProvider SubscribeAsyncEventHandlers(this IServiceProvider serviceProvider)
    {
        var eventTypes = new HashSet<Type>();
        var eventHandlers = serviceProvider.GetServices<IEventHandler>();

        //Get all the registered event handlers, and the EventType it's handling.
        //Build a unique set of Event Types.
        foreach (var e in eventHandlers)
        {
            var eventType = e.GetType().GetInterface("IEventHandler`1")?.GenericTypeArguments[0];
            if (eventType == null) continue;
            eventTypes.Add(eventType);
        }

        //Loop through all the event types, and Find the equivalent AsyncSubscribers, and wire up the subscribe events to the event handlers.
        foreach (var eventType in eventTypes)
        {
            typeof(AddServiceCollectionEventHandlerExtensions)
                .GetMethod(nameof(SubscribeAsyncEventHandlers), BindingFlags.NonPublic | BindingFlags.Static)?
                .MakeGenericMethod(eventType).Invoke(null, [serviceProvider]);
        }

        return serviceProvider;
    }

    private static IServiceProvider SubscribeAsyncEventHandlers<T>(this IServiceProvider serviceProvider)
    {
        var eventHandlers = serviceProvider.GetServices<IEventHandler<T>>();
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