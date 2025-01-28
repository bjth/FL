using Arch.Core;
using FL.Client;
using FL.Client.EntityData;
using FL.Client.Messaging.Events;
using FL.Client.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using static Raylib_cs.Raylib;

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

InitWindow(800, 480, "Hello World");

using (var scope = provider.CreateScope())
using (var world = scope.ServiceProvider.GetRequiredService<World>())
{
    var query = new QueryDescription().WithAll<Position>();
    var deltaTimeProvider = scope.ServiceProvider.GetRequiredService<DeltaTimeProvider>();
    var inputManager = scope.ServiceProvider.GetRequiredService<InputManager>();

    SetTargetFPS(240);
    while (!WindowShouldClose())
    {
        await deltaTimeProvider.CalculateDeltaTimeAsync();
        await inputManager.HandleInputAsync();
        BeginDrawing();
        ClearBackground(Color.White);
        DrawText("Hello, world!", 10, 30, 20, Color.Black);

        world.Query(in query,
            (Entity entity, ref Position pos) =>
            {
                DrawRectangle((int)Math.Ceiling(pos.X), (int)Math.Ceiling(pos.Y), 32, 32, Color.Red);
            });

        DrawText($"{GetFPS()} fps", 10, 10, 20, Color.Green);

        EndDrawing();
    }
}


CloseWindow();