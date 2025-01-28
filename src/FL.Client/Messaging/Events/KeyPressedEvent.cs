
using Arch.Core;
using FL.Client.EntityData;
using Microsoft.Extensions.Logging;
using Raylib_cs;

namespace FL.Client.Messaging.Events;

public sealed record KeyPressedEvent(int KeyCode, KeyboardKey Key);

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

public sealed class MovePlayerEventHandler(ILogger<MovePlayerEventHandler> logger, World world) : IEventHandler<KeyPressedEvent>
{
    private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<Position>();
    
    public ValueTask Handle(KeyPressedEvent? keyPressedEvent, CancellationToken cancellationToken = default)
    {
        const int velocity = 8;
        world.Query(in _queryDescription, (Entity entity, ref Position pos) =>
        {
            switch (keyPressedEvent?.Key)
            {
                case KeyboardKey.A:
                {
                    pos.X -= 1 * velocity;
                    break;
                }
                case KeyboardKey.D:
                {
                    pos.X += 1 * velocity;
                    break;
                }
                case KeyboardKey.W:
                {
                    pos.Y -= 1 * velocity;
                    break;
                }
                case KeyboardKey.S:
                {
                    pos.Y += 1 * velocity;
                    break;
                }
            }
        });
        
        return ValueTask.CompletedTask;
    }
}