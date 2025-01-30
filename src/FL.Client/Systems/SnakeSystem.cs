using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using FL.Client.Components;
using FL.Client.Messaging.Signals;
using FL.Client.Providers;
using MessagePipe;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace FL.Client.Systems;

public class SnakeSystem(
    World world,
    DeltaTimeProvider deltaTimeProvider,
    GridMapSystem gridMapSystem,
    IAsyncPublisher<EntityCollisionSignal> appleCollisionPublisher)
    : IGameSystem, ISignalConsumer<KeyPressedSignal>
{
    private Entity? _snakeEntity;
    private float _timePassed;
    private float _snakeSpeed = 12f;
    private const float SnakeSpeedIncrement = 1.5f;
    private const float SpeedThreshold = 1f;

    private const int StartingSegmentCount = 3;
    private readonly List<(GridPosition position, Direction direction)> _segments = new();
    private Direction _changeDirection = Direction.Right;

    private readonly Dictionary<KeyboardKey, Direction> _directionMap = new()
    {
        { KeyboardKey.A, Direction.Left },
        { KeyboardKey.D, Direction.Right },
        { KeyboardKey.W, Direction.Up },
        { KeyboardKey.S, Direction.Down },
    };

    public ValueTask Handle(KeyPressedSignal? keyPressedEvent, CancellationToken token = default)
    {
        if (keyPressedEvent is null) return ValueTask.CompletedTask;
        if (keyPressedEvent.Key == KeyboardKey.F1) return SpawnSnake();
        if (!_directionMap.TryGetValue(keyPressedEvent.Key, out var newDirection)) return ValueTask.CompletedTask;
        _changeDirection = newDirection;
        return ValueTask.CompletedTask;
    }

    private ValueTask SpawnSnake()
    {
        if (_snakeEntity != null) return ValueTask.CompletedTask;
        _snakeEntity = world.Create(new Snake(), new Drawable { DrawFn = Draw, ZIndex = 10 }, Direction.Right);
        var gridPosition = gridMapSystem.TakeEmptyGridPosition(_snakeEntity.Value, 4);
        if (gridPosition == null) return ValueTask.CompletedTask;

        _segments.Add((gridPosition.Value, Direction.Right)); //Head
        for (var i = 0; i <= StartingSegmentCount; i++)
        {
            AddSnakeSegment(1);
        }

        _snakeEntity.Value.Add(gridPosition.Value);
        return ValueTask.CompletedTask;
    }

    private void AddSnakeSegment(int distance)
    {
        if (_snakeEntity is null) return;
        var nextTo = _segments.Last();
        var pos = GetNewPosition(nextTo.position, OppositeDirection(nextTo.direction), distance);
        gridMapSystem.AddEntity(_snakeEntity.Value, pos);
        _segments.Add((pos, nextTo.direction));
    }

    private static Direction SanitizedDirection(Direction current, Direction next)
    {
        return current switch
        {
            Direction.Down when next == Direction.Up => Direction.Down,
            Direction.Up when next == Direction.Down => Direction.Up,
            Direction.Left when next == Direction.Right => Direction.Left,
            Direction.Right when next == Direction.Left => Direction.Right,
            _ => next
        };
    }

    private static Direction OppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Down => Direction.Up,
            Direction.Up => Direction.Down,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.Right
        };
    }

    private static GridPosition GetNewPosition(GridPosition currentPosition, Direction direction, int distance)
    {
        var row = currentPosition.Row;
        var column = currentPosition.Column;

        switch (direction)
        {
            case Direction.Up:
                row -= distance;
                break;
            case Direction.Down:
                row += distance;
                break;
            case Direction.Left:
                column -= distance;
                break;
            case Direction.Right:
                column += distance;
                break;
        }

        return new GridPosition(column, row);
    }

    private void ResetSnake()
    {
        //Reset the game, as we have lost.
        if (_snakeEntity is null) return;
        world.Destroy(_snakeEntity.Value);
        _snakeEntity = null;
        _changeDirection = Direction.Right;
        foreach (var segment in _segments)
        {
            gridMapSystem.RemoveEntity(segment.position);
        }

        _segments.Clear();
    }

    private async ValueTask HandleEntityCollision(Entity? entity)
    {
        if (entity is null) return;
        if (entity.Value.Has<Snake>())
        {
            ResetSnake();
            return;
        }

        if (entity.Value.Has<Apple>())
        {
            await EatApple(entity.Value);
        }
    }

    private async ValueTask EatApple(Entity apple)
    {
        if (_snakeEntity is null) return;
        var snake = _snakeEntity.Value.Get<Snake>();
        var applesEaten = snake.ApplesEaten + 1;
        if (applesEaten % 5 == 0)
        {
            _snakeSpeed += SnakeSpeedIncrement;
        }

        _snakeEntity.Value.Add(new Snake { ApplesEaten = applesEaten });
        await appleCollisionPublisher.PublishAsync(new EntityCollisionSignal(apple));
        AddSnakeSegment(1);
    }

    public async ValueTask UpdateAsync()
    {
        if (_snakeEntity == null) return;
        _timePassed += deltaTimeProvider.DeltaTime * _snakeSpeed;
        if (_timePassed < SpeedThreshold) return;
        _timePassed = 0f;

        var hitEntity = MoveSnake();
        await HandleEntityCollision(hitEntity);
    }

    private Entity? MoveSnake()
    {
        if (_snakeEntity == null) return null;
        var currentDirection = _snakeEntity.Value.Get<Direction>();
        var newDirection = SanitizedDirection(currentDirection, _changeDirection);
        _snakeEntity.Value.Add(newDirection);

        const int spacesToMove = 1;
        var headPosition = _segments[0];
        var newHeadPosition = GetNewPosition(headPosition.position, newDirection, spacesToMove);

        var headCollision = gridMapSystem.MoveEntity(headPosition.position, ref newHeadPosition);
        _segments[0] = (newHeadPosition, newDirection);

        //Shift and Move the body
        var previousPosition = headPosition;
        for (var i = 1; i < _segments.Count; i++)
        {
            var segment = _segments[i];
            gridMapSystem.MoveEntity(segment.position, ref previousPosition.position);
            _segments[i] = previousPosition;
            previousPosition = segment;
        }

        return headCollision;
    }

    private void Draw(Entity entity)
    {
        if (_snakeEntity == null) return;
        if (entity.Id != _snakeEntity?.Id) return;

        var entities = gridMapSystem.GetEntitiesById(_snakeEntity.Value.Id);
        foreach (var e in entities)
        {
            var bodyScreenPos = GridMapSystem.GetScreenPosition(e.Item2);
            DrawRectangle((int)bodyScreenPos.X, (int)bodyScreenPos.Y, 16, 16, Color.Red); //The Body Parts
        }
    }
}