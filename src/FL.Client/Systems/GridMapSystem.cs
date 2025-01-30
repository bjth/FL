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
        world.Create(_grid, new Drawable { DrawFn = DrawBoard, ZIndex = -1});
        return default;
    }
    
    private readonly Random _random = new();
    public GridPosition? TakeEmptyGridPosition(Entity entity, int padding = 0)
    {
        if (_grid == null) return null;
        var gridPosition = new GridPosition();
        do
        {
            gridPosition.Column = _random.Next(padding, _grid.GetLength(0) - padding);
            gridPosition.Row = _random.Next(padding, _grid.GetLength(1) - padding);
        } while (GetEntity(gridPosition) is not null);
        AddEntity(entity, gridPosition);
        return gridPosition;
    }

    public Entity? GetEntity(GridPosition gridPosition)
    {
        return _grid?[gridPosition.Column, gridPosition.Row];
    }

    public void AddEntity(Entity entity, GridPosition gridPosition)
    {
        if (_grid == null) return;
        Console.WriteLine($"Adding Entity: {entity}");
        _grid[gridPosition.Column, gridPosition.Row] = entity;
    }
    
    public Entity? MoveEntity(GridPosition from, GridPosition to)
    {
        if (_grid == null) return null;
        var fromEntity = GetEntity(from);
        var currentEntity = GetEntity(to);
        _grid[from.Column, from.Row] = null;
        _grid[to.Column, to.Row] = fromEntity;
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