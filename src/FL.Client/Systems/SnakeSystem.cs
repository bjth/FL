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
    WindowProvider windowProvider,
    IAsyncPublisher<EntityCollisionSignal> appleCollisionPublisher)
    : IGameSystem, ISignalConsumer<KeyPressedSignal>
{
    private Entity? _snakeEntity;
    private Direction _nextDirection = Direction.Right;

    private float _timePassed;
    private float _snakeSpeed = 8f;
    private const float SnakeSpeedIncrement = 3.5f;
    private const float SpeedThreshold = 1f;

    private const int MaxSegments = 1024;
    private int _currentSegmentsCount = 3;
    private readonly GridPosition[] _segments = new GridPosition[MaxSegments];

    public ValueTask Handle(KeyPressedSignal? keyPressedEvent, CancellationToken token = default)
    {
        if (keyPressedEvent is null) return ValueTask.CompletedTask;
        if (keyPressedEvent.Key == KeyboardKey.F1) return SpawnSnake();
        if (_snakeEntity is null) return ValueTask.CompletedTask;

        switch (keyPressedEvent.Key)
        {
            case KeyboardKey.A:
            {
                _nextDirection = Direction.Left;
                return ValueTask.CompletedTask;
            }
            case KeyboardKey.D:
                _nextDirection = Direction.Right;
                return ValueTask.CompletedTask;
            case KeyboardKey.W:
                _nextDirection = Direction.Up;
                return ValueTask.CompletedTask;
            case KeyboardKey.S:
                _nextDirection = Direction.Down;
                return ValueTask.CompletedTask;
            default:
                return ValueTask.CompletedTask;
        }
    }

    private ValueTask SpawnSnake()
    {
        if (_snakeEntity != null) return ValueTask.CompletedTask;
        _snakeEntity = world.Create(new Snake(), new Drawable { DrawFn = Draw, ZIndex = 10 }, _nextDirection);
        var gridPosition = gridMapSystem.TakeEmptyGridPosition(_snakeEntity.Value, 4);
        if (gridPosition == null) return ValueTask.CompletedTask;

        _segments[0] = gridPosition.Value; //Head
        for (var i = 0; i <= _currentSegmentsCount; i++)
        {
            var bodySegmentPosition = gridPosition.Value with { Column = gridPosition.Value.Column - i };
            gridMapSystem.AddEntity(_snakeEntity.Value, bodySegmentPosition);
            _segments[i + 1] = bodySegmentPosition;
        }

        _snakeEntity.Value.Add(gridPosition.Value);
        return ValueTask.CompletedTask;
    }

    private void AddSnakeSegment()
    {
        if (_snakeEntity is null) return;
        var gridPosition = _segments[_currentSegmentsCount + 1];
        Console.WriteLine($"GridPosition: {gridPosition.Column}, {gridPosition.Row}");
        _currentSegmentsCount++;
        gridMapSystem.AddEntity(_snakeEntity.Value, gridPosition);
    }

    private static Direction SanitizedDirection(Direction current, Direction next)
    {
        switch (current)
        {
            case Direction.Down when next == Direction.Up:
                return Direction.Down;
            case Direction.Up when next == Direction.Down:
                return Direction.Up;
            case Direction.Left when next == Direction.Right:
                return Direction.Left;
            case Direction.Right when next == Direction.Left:
                return Direction.Right;
            default:
                return next;
        }
    }

    public async ValueTask UpdateAsync()
    {
        if (_snakeEntity == null) return;
        _timePassed += deltaTimeProvider.DeltaTime * _snakeSpeed;
        if (_timePassed < SpeedThreshold) return;
        _timePassed = 0f;

        const int spacesToMove = 1;
        var currentHeadPos = _segments[0];
        var newHeadPos = currentHeadPos;
        var currentDirection = _snakeEntity.Value.Get<Direction>();
        _nextDirection = SanitizedDirection(currentDirection, _nextDirection);
        var snakeData = _snakeEntity.Value.Get<Snake>();

        switch (_nextDirection)
        {
            case Direction.Unknown:
                break;
            case Direction.Up:
                newHeadPos.Row -= spacesToMove;
                break;
            case Direction.Down:
                newHeadPos.Row += spacesToMove;
                break;
            case Direction.Left:
                newHeadPos.Column -= spacesToMove;
                break;
            case Direction.Right:
                newHeadPos.Column += spacesToMove;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _segments[0] = newHeadPos;
        var entityReplaced = gridMapSystem.MoveEntity(_segments[0], newHeadPos); //Moves Head
        if (entityReplaced != null)
        {
            //If we hit a snake part we Lose
            if (entityReplaced.Value.Has<Snake>())
            {
                Console.WriteLine("You are lucky, we can't lose yet!");
            }

            if (entityReplaced.Value.Has<Apple>())
            {
                
                snakeData.Score++;
                AddSnakeSegment();
                if (snakeData.Score % 5 == 0)
                {
                    _snakeSpeed += SnakeSpeedIncrement;
                }

                await appleCollisionPublisher.PublishAsync(new EntityCollisionSignal(entityReplaced.Value));
            }
        }

        //Catch the body up.
        var i = _currentSegmentsCount + 1;
        do
        {
            _ = gridMapSystem.MoveEntity(_segments[i], _segments[i - 1]);
            _segments[i] = _segments[i - 1];
            i--;
        } while (i > 0);

        world.Add(_snakeEntity.Value, snakeData, newHeadPos, _nextDirection);
    }

    private void Draw(Entity entity)
    {
        if (_snakeEntity == null) return;
        if (entity.Id != _snakeEntity?.Id) return;
        for (var i = 0; i <= _currentSegmentsCount + 1; i++)
        {
            var bodyScreenPos = GridMapSystem.GetScreenPosition(_segments[i]);
            DrawRectangle((int)bodyScreenPos.X, (int)bodyScreenPos.Y, 16, 16, Color.Red); //The Body Parts
        }
        DrawScore();
    }

    private void DrawScore()
    {
        var scoreText = "Score: " + (_snakeEntity?.Get<Snake>().Score ?? 0);
        uint length;
        unsafe
        {
            fixed (byte* p = Encoding.ASCII.GetBytes(scoreText))
            {
                length = TextLength((sbyte*)p);
            }
        }

        const int fontSize = 32;
        var xPost = Convert.ToInt32(windowProvider.ScreenWidth - 30 - length * fontSize / 2);
        DrawText(scoreText, xPost, 10, fontSize, Color.White);
    }
}