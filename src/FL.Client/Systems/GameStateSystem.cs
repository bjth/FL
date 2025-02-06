using System.Text;
using Arch.Core;
using FL.Client.Components;
using FL.Client.Providers;
using static Raylib_cs.Raylib;

namespace FL.Client.Systems;
public class GameStateSystem(World world, WindowProvider windowProvider) : IGameSystem
{
    private const int FontSize = 32;
    
    public ValueTask InitializeAsync()
    {
        world.Create(new Drawable { DrawFn = DrawUi, ZIndex = 999});
        return default;
    }

    private void DrawUi(Entity entity)
    {
        
    }

    private static uint GetTextLength(string text)
    {
        uint length;
        unsafe
        {
            fixed (byte* p = Encoding.ASCII.GetBytes(text))
            {
                length = TextLength((sbyte*)p);
            }
        }

        return length;
    }
}