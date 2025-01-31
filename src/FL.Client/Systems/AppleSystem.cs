using Arch.Core;
using Arch.Core.Extensions;
using FL.Client.Components;
using FL.Client.Messaging.Signals;
using FL.Client.Providers;
using MessagePipe;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace FL.Client.Systems;

public class AppleSystem(
    World world,
    DeltaTimeProvider deltaTimeProvider,
    GridMapSystem gridMapSystem,
    IAsyncSubscriber<EntityCollisionSignal> entityCollisionSubscriber)
    : IGameSystem
{
    private readonly HashSet<Entity> _apples = [];
    private float _timePassed;
    private const float SpawnSpeed = 8f;
    private const int MaxApples = 1;

    private ValueTask SpawnApple()
    {
        var apple = world.Create(new Apple(), new Drawable { DrawFn = Draw, ZIndex = 10 });
        _apples.Add(apple);

        var gridPosition = gridMapSystem.TakeEmptyGridPosition(apple, 4);
        if (gridPosition == null) return ValueTask.CompletedTask; //Could not spawn Apple.
        apple.Add(gridPosition.Value);
        _timePassed = 0;
        return ValueTask.CompletedTask;
    }

    public ValueTask InitializeAsync()
    {
        entityCollisionSubscriber.Subscribe(Handle);
        return SpawnApple();
    }

    public ValueTask UpdateAsync()
    {
        if (_apples.Count >= MaxApples) return ValueTask.CompletedTask;
        _timePassed += deltaTimeProvider.DeltaTime;
        if (_timePassed < SpawnSpeed) return ValueTask.CompletedTask;
        _timePassed = 0f;
        return SpawnApple();
    }

    private void Draw(Entity entity)
    {
        foreach (var screenPosition in _apples.Select(apple =>
                     GridMapSystem.GetScreenPosition(apple.Get<GridPosition>())))
        {
            DrawRectangle((int)screenPosition.X, (int)screenPosition.Y, 16, 16, Color.Green);
        }
    }

    public ValueTask Handle(EntityCollisionSignal? signal, CancellationToken token = default)
    {
        if (signal == null) return ValueTask.CompletedTask;
        if (!signal.Entity.Has<Apple>()) return ValueTask.CompletedTask;

        _apples.RemoveWhere(apple => apple.Id == signal.Entity.Id);
        world.Destroy(signal.Entity);

        return _apples.Count == 0 ? SpawnApple() : ValueTask.CompletedTask;
    }
}