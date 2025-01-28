namespace FL.Client.Messaging.Events;

public interface IEventHandler<in T> : IEventHandler
{
    ValueTask Handle(T? time, CancellationToken token = default);
}

public interface IEventHandler
{
    
}