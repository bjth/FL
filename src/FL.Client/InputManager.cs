using FL.Client.Messaging.Events;
using MessagePipe;
using Raylib_cs;

namespace FL.Client;

public class InputManager(IAsyncPublisher<KeyPressedEvent> keyPressedEventProducer)
{
    public async Task HandleInputAsync()
    {
        int pressedKey;
        do
        {
            pressedKey = Raylib.GetKeyPressed();
            if (pressedKey == 0) continue;
            // do
            // {
                await keyPressedEventProducer.PublishAsync(new KeyPressedEvent(pressedKey,
                    (KeyboardKey)pressedKey));
            // } while (Raylib.IsKeyDown((KeyboardKey)pressedKey));
        } while (pressedKey != 0);
    }
}