using FL.Client.Messaging.Events;
using MessagePipe;
using Raylib_cs;

namespace FL.Client;

public class InputManager(IAsyncPublisher<KeyPressedEvent> keyPressedEventProducer, IAsyncPublisher<KeyHeldEvent> keyHeldEventProducer)
{
    private readonly HashSet<KeyboardKey> _pressedKeys = new();

    public async Task HandleInputAsync()
    {
        int pressedKey;
        do
        {
            pressedKey = Raylib.GetKeyPressed();
            if (pressedKey == 0) continue;
            _pressedKeys.Add((KeyboardKey)pressedKey);
            await keyPressedEventProducer.PublishAsync(new KeyPressedEvent(pressedKey, (KeyboardKey)pressedKey));
            
        } while (pressedKey != 0);

        foreach (var keyPressedEvent in _pressedKeys)
        {
            if (Raylib.IsKeyDown(keyPressedEvent))
            {
                await keyHeldEventProducer.PublishAsync(new KeyHeldEvent((int)keyPressedEvent, keyPressedEvent));
                continue;
            }
            _pressedKeys.Remove(keyPressedEvent);
        }
    }
}