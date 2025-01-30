using Arch.Core;
using FL.Client.Components;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace FL.Client.Systems;

public record struct GridPosition(int Column, int Row);

public class GridMapSystem(World world) : IGameSystem
{
    private Entity?[,]? _grid;
    private const int CellSize = 20;

    public ValueTask InitializeAsync()
    {
        var columnsCount = GetScreenWidth() / CellSize;
        var rowsCount = GetScreenHeight() / CellSize;

        _grid = new Entity?[columnsCount, rowsCount];
        world.Create(_grid, new Drawable { DrawFn = DrawBoard, ZIndex = -1 });
        return default;
    }

    private readonly Random _random = new();

    public GridPosition? TakeEmptyGridPosition(Entity entity, int padding = 0)
    {
        if (_grid == null) return null;
        GridPosition gridPosition;
        do
        {
            gridPosition = new GridPosition()
            {
                Column = _random.Next(padding, _grid.GetLength(0) - padding),
                Row = _random.Next(padding, _grid.GetLength(1) - padding),
            };
        } while (GetEntity(gridPosition) is not null);

        AddEntity(entity, gridPosition);
        return gridPosition;
    }

    private void SanitizeGridPosition(ref GridPosition gridPosition)
    {
        if (_grid == null) return;
        if (gridPosition.Column >= _grid.GetLength(0))
        {
            gridPosition.Column = 0;
        }

        if (gridPosition.Column < 0)
        {
            gridPosition.Column = _grid.GetLength(0) - 1;
        }

        if (gridPosition.Row >= _grid.GetLength(1))
        {
            gridPosition.Row = 0;
        }

        if (gridPosition.Row < 0)
        {
            gridPosition.Row = _grid.GetLength(1) - 1;
        }
    }

    private Entity? GetEntity(GridPosition gridPosition)
    {
        SanitizeGridPosition(ref gridPosition);
        return _grid?[gridPosition.Column, gridPosition.Row];
    }

    public IEnumerable<(Entity, GridPosition)> GetEntitiesById(int entityId)
    {
        if (_grid == null) yield break;
        for (var column = 0; column < _grid.GetLength(0); column++)
        {
            for (var row = 0; row < _grid.GetLength(1); row++)
            {
                var item = _grid[column, row];
                if (item == null) continue;
                if (item.Value.Id == entityId) yield return (item.Value, new GridPosition(column, row));
            }
        }
    }

    public void AddEntity(Entity entity, GridPosition gridPosition)
    {
        if (_grid == null) return;
        _grid[gridPosition.Column, gridPosition.Row] = entity;
    }

    public void RemoveEntity(GridPosition gridPosition)
    {
        if (_grid == null) return;
        _grid[gridPosition.Column, gridPosition.Row] = null;
    }

    public Entity? MoveEntity(GridPosition from, ref GridPosition to)
    {
        if (_grid == null) return null;
        var fromEntity = GetEntity(from);
        SanitizeGridPosition(ref to);
        var currentEntity = GetEntity(to);
        _grid[to.Column, to.Row] = fromEntity;
        _grid[from.Column, from.Row] = null;
        return currentEntity;
    }

    public static ScreenPosition GetScreenPosition(GridPosition gridPosition)
    {
        return new ScreenPosition(gridPosition.Column * CellSize, gridPosition.Row * CellSize);
    }

    private void DrawBoard(Entity entity)
    {
        if (_grid == null) return;
        for (var column = 0; column < _grid.GetLength(0); column++)
        {
            for (var row = 0; row < _grid.GetLength(1); row++)
            {
                if (_grid[column, row] == null)
                {
                    DrawRectangleRec(new Rectangle(column * CellSize, row * CellSize, 16, 16), GetColor(0x2E2E2EFF));
                }
            }
        }
    }
}