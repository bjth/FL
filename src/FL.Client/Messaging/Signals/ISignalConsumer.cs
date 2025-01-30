namespace FL.Client.Messaging.Signals;

public interface ISignalConsumer<in T> : ISignalConsumer
{
    ValueTask Handle(T? signal, CancellationToken token = default);
}

public interface ISignalConsumer;