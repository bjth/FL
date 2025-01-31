using Arch.Core;
using Arch.Core.Extensions;
using FL.Client.Components;
using MessagePipe;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace FL.Client.Messaging.Signals;

public sealed record KeyPressedSignal(int KeyCode, KeyboardKey Key);

public sealed record KeyHeldSignal(int KeyCode, KeyboardKey Key);

public sealed class KeyPressedSignalConsumer
{
    private Entity? _fpsCounter;
    private readonly World _world;

    public KeyPressedSignalConsumer(ILogger<KeyPressedSignalConsumer> logger, World world,
        IAsyncSubscriber<KeyPressedSignal> subscriber)
    {
        _world = world;
        subscriber.Subscribe(Handle);
    }

    public ValueTask Handle(KeyPressedSignal? keyPressedEvent, CancellationToken cancellationToken = default)
    {
        if (keyPressedEvent?.Key != KeyboardKey.F11) return ValueTask.CompletedTask;

        if (_fpsCounter == null)
        {
            _fpsCounter = _world.Create(new Drawable
                { DrawFn = _ => DrawText($"{GetFPS()} fps", 10, 10, 20, Color.Green) });
            return ValueTask.CompletedTask;
        }

        if (_fpsCounter?.IsAlive() ?? false)
        {
            _world.Destroy(_fpsCounter.Value);
        }

        _fpsCounter = null;
        return ValueTask.CompletedTask;
    }
}