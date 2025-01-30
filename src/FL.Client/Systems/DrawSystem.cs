using Arch.Core;
using FL.Client.Components;

namespace FL.Client.Systems;

public class DrawSystem(World world) : IGameSystem
{
    
    private readonly QueryDescription _drawableQueryDescription = new QueryDescription().WithAll<Drawable>();

    public ValueTask UpdateAsync()
    {
        var drawActions = new Dictionary<Entity, Drawable>();
        world.Query(in _drawableQueryDescription, (Entity entity, ref Drawable drawable) => { drawActions.Add(entity, drawable); });
        foreach (var keyValuePair in drawActions.OrderBy(x => x.Value.ZIndex))
        {
            keyValuePair.Value.DrawFn(keyValuePair.Key);
        }
        return default;
    }
}