using FL.Client.Messaging.Signals;
using MessagePipe;
using Raylib_cs;

namespace FL.Client.Systems;

public class InputSystem(IAsyncPublisher<KeyPressedSignal> keyPressedEventProducer, IAsyncPublisher<KeyHeldSignal> keyHeldEventProducer) : IGameSystem
{
    private readonly HashSet<KeyboardKey> _pressedKeys = [];

    public async ValueTask UpdateAsync()
    {
        int pressedKey;
        do
        {
            pressedKey = Raylib.GetKeyPressed();
            if (pressedKey == 0) continue;
            _pressedKeys.Add((KeyboardKey)pressedKey);
            await keyPressedEventProducer.PublishAsync(new KeyPressedSignal(pressedKey, (KeyboardKey)pressedKey));
            
        } while (pressedKey != 0);

        foreach (var keyPressedEvent in _pressedKeys)
        {
            if (Raylib.IsKeyDown(keyPressedEvent))
            {
                await keyHeldEventProducer.PublishAsync(new KeyHeldSignal((int)keyPressedEvent, keyPressedEvent));
                continue;
            }
            _pressedKeys.Remove(keyPressedEvent);
        }
    }
}