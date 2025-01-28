using Arch.Core;
using FL.Client;
using FL.Client.EntityData;
using FL.Client.Messaging.Events;
using FL.Client.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raylib_cs;

//Setup services.
var services = new ServiceCollection();
services.AddLogging(b => b.AddSimpleConsole());

//Setup world and Library Managers
services.AddSingleton(World.Create());
services.AddSingleton<WindowManager>();
services.AddSingleton<InputManager>();
services.AddSingleton<DeltaTimeProvider>();

//Setup event handlers. -- TODO move this to its own configuration somewhere.
services.AddMessagePipe();
services.AddEventHandler<KeyPressedEvent, KeyPressedEventHandler>();
services.AddEventHandler<KeyPressedEvent, SpawnPlayerEventHandler>();
services.AddEventHandler<KeyHeldEvent, MovePlayerEventHandler>();

//Build services
var provider = services.BuildServiceProvider();
provider.SubscribeAsyncEventHandlers();

Raylib.InitWindow(800, 480, "Hello World");

using (var scope = provider.CreateScope())
using (var world = scope.ServiceProvider.GetRequiredService<World>())
{
    var query = new QueryDescription().WithAll<Position>();
    var deltaTimeProvider = scope.ServiceProvider.GetRequiredService<DeltaTimeProvider>();
    var inputManager = scope.ServiceProvider.GetRequiredService<InputManager>();
    
    while (!Raylib.WindowShouldClose())
    {
        await deltaTimeProvider.CalculateDeltaTimeAsync();
        await inputManager.HandleInputAsync();
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.White);
        Raylib.DrawText("Hello, world!", 12, 12, 20, Color.Black);

        world.Query(in query,
            (Entity entity, ref Position pos) => { Raylib.DrawRectangle((int)Math.Ceiling(pos.X), (int)Math.Ceiling(pos.Y), 32, 32, Color.Red); });

        Raylib.EndDrawing();
    }
}


Raylib.CloseWindow();