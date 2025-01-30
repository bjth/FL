using Arch.Core;
using Arch.Core.Extensions;
using FL.Client.Components;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace FL.Client.Messaging.Signals;

public sealed record KeyPressedSignal(int KeyCode, KeyboardKey Key);
public sealed record KeyHeldSignal(int KeyCode, KeyboardKey Key);

public sealed class KeyPressedSignalConsumer(ILogger<KeyPressedSignalConsumer> logger, World world) : ISignalConsumer<KeyPressedSignal>
{
    private Entity? _fpsCounter;
    public ValueTask Handle(KeyPressedSignal? keyPressedEvent, CancellationToken cancellationToken = default)
    {
        if (keyPressedEvent?.Key != KeyboardKey.F11) return ValueTask.CompletedTask;

        if (_fpsCounter == null)
        {
            _fpsCounter = world.Create(new Drawable
                { DrawFn = _ => DrawText($"{GetFPS()} fps", 10, 10, 20, Color.Green) });
            return ValueTask.CompletedTask;
        }
        
        if (_fpsCounter?.IsAlive() ?? false)
        {
            world.Destroy(_fpsCounter.Value);
        }

        _fpsCounter = null;
        return ValueTask.CompletedTask;
    }
}