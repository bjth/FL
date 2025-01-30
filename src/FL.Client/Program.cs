using Arch.Core;
using FL.Client.Messaging.Signals;
using FL.Client.Providers;
using FL.Client.Systems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using static Raylib_cs.Raylib;

const int screenWidth = 800;
const int screenHeight = 480;

//Setup services.
var services = new ServiceCollection();
services.AddLogging(b => b.AddSimpleConsole());

//Setup world and Library Managers
services.AddSingleton(World.Create());
services.AddScoped<WindowProvider>();
services.AddSingleton<DeltaTimeProvider>();

//Setup Systems.
services.AddGameSystem<DeltaTimeSystem>();
services.AddGameSystem<InputSystem>();
services.AddGameSystem<GridMapSystem>();
services.AddGameSystem<SnakeSystem>();
services.AddGameSystem<AppleSystem>();
services.AddGameSystem<DrawSystem>();

//Setup Signals and Consumers
services.AddMessagePipe();
services.AddSignalConsumer<KeyPressedSignal, KeyPressedSignalConsumer>();
services.AddSignalConsumer<KeyPressedSignal, SnakeSystem>();
services.AddSignalConsumer<EntityCollisionSignal, AppleSystem>();

var provider = services.BuildServiceProvider();

InitWindow(screenWidth, screenHeight, "Mondo Snake");
SetTargetFPS(240);

await using (var asyncScope = provider.CreateAsyncScope())
using (_ = asyncScope.ServiceProvider.GetRequiredService<World>())
{
    asyncScope.ServiceProvider.UseSignalConsumers();
    var gameSystems = asyncScope.ServiceProvider.GetServices<IGameSystem>().ToList();
    foreach (var gameSystem in gameSystems)
    {
        await gameSystem.InitializeAsync();
    }
    
    while (!WindowShouldClose())
    {
        BeginDrawing();
        ClearBackground(GetColor(0x000000FF));

        foreach (var gameSystem in gameSystems)
        {
            await gameSystem.UpdateAsync();
        }
        
        EndDrawing();
    }
}


CloseWindow();