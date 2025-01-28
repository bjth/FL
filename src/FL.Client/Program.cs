using Arch.Core;
using FL.Client;
using FL.Client.EntityData;
using FL.Client.Messaging.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raylib_cs;

//Setup services.
var services = new ServiceCollection();
services.AddLogging(b => b.AddSimpleConsole());

//Setup events.
services.AddSingleton(World.Create());
services.AddSingleton(new WindowManager());
services.AddSingleton<InputManager>();

services.AddMessagePipe();
services.AddEventHandler<KeyPressedEvent, KeyPressedEventHandler>();
services.AddEventHandler<KeyPressedEvent, SpawnPlayerEventHandler>();
services.AddEventHandler<KeyPressedEvent, MovePlayerEventHandler>();

//Build services
var provider = services.BuildServiceProvider();
var inputManager = provider.GetRequiredService<InputManager>();
provider.SubscribeAsyncEventHandlers();

Raylib.InitWindow(800, 480, "Hello World");

using (var scope = provider.CreateScope())
using (var world = scope.ServiceProvider.GetRequiredService<World>())
{
    var query = new QueryDescription().WithAll<Position>();
    while (!Raylib.WindowShouldClose())
    {
        await inputManager.HandleInputAsync();
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
        Raylib.DrawText("Hello, world!", 12, 12, 20, Color.Black);

        world.Query(in query,
            (Entity entity, ref Position pos) => { Raylib.DrawRectangle(pos.X, pos.Y, 32, 32, Color.Red); });

        Raylib.EndDrawing();
    }
}


Raylib.CloseWindow();