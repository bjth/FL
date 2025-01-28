
using Arch.Core;
using FL.Client.EntityData;
using FL.Client.Providers;
using Microsoft.Extensions.Logging;
using Raylib_cs;

namespace FL.Client.Messaging.Events;

public sealed record KeyPressedEvent(int KeyCode, KeyboardKey Key);
public sealed record KeyHeldEvent(int KeyCode, KeyboardKey Key);

public sealed class KeyPressedEventHandler(ILogger<KeyPressedEventHandler> logger, WindowManager windowManager) : IEventHandler<KeyPressedEvent>
{
    public ValueTask Handle(KeyPressedEvent? keyPressedEvent, CancellationToken cancellationToken = default)
    {
        if (keyPressedEvent?.Key != KeyboardKey.F11) return ValueTask.CompletedTask;
        windowManager.ToggleBorderlessWindowed();
        return ValueTask.CompletedTask;
    }
}

public sealed class SpawnPlayerEventHandler(ILogger<SpawnPlayerEventHandler> logger, World world) : IEventHandler<KeyPressedEvent>
{
    public ValueTask Handle(KeyPressedEvent? keyPressedEvent, CancellationToken cancellationToken = default)
    {
        if (keyPressedEvent?.Key == KeyboardKey.F1)
        {
            world.Create(new Position(0, 0));
        }
        return ValueTask.CompletedTask;
    }
}

public sealed class MovePlayerEventHandler(DeltaTimeProvider deltaTimeProvider, World world) : IEventHandler<KeyHeldEvent>
{
    private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<Position>();
    
    public ValueTask Handle(KeyHeldEvent? keyPressedEvent, CancellationToken cancellationToken = default)
    {
        if (keyPressedEvent is null) return ValueTask.CompletedTask;
        var velocity = 20f * deltaTimeProvider.DeltaTime;
        var distance = 20f * velocity;
        
        world.Query(in _queryDescription, (ref Position pos) =>
        {
            switch (keyPressedEvent.Key)
            {
                case KeyboardKey.A:
                {
                    pos.X -= distance;
                    break;
                }
                case KeyboardKey.D:
                {
                    pos.X += distance;
                    break;
                }
                case KeyboardKey.W:
                {
                    pos.Y -= distance;
                    break;
                }
                case KeyboardKey.S:
                {
                    pos.Y += distance;
                    break;
                }
            }
        });
        
        return ValueTask.CompletedTask;
    }
}