using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using FL.Client.Components;
using FL.Client.Providers;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace FL.Client.Systems;
public class GameStateSystem(World world, WindowProvider windowProvider) : IGameSystem
{
    private readonly QueryDescription _snakeQuery = new QueryDescription().WithAll<Snake>();
    private int _lastScore;
    private const int FontSize = 32;
    
    public ValueTask InitializeAsync()
    {
        world.Create(new Drawable { DrawFn = DrawUi, ZIndex = 999});
        return default;
    }

    private void DrawUi(Entity entity)
    {
        var listOfSnakes = new List<Entity>();
        world.GetEntities(_snakeQuery, listOfSnakes);

        if (listOfSnakes.Count == 0)
        {
            DrawStartGame();   
        }
        
        DrawScore(listOfSnakes);
    }
    
    private void DrawScore(List<Entity> snakes)
    {
        var score = snakes.Count == 0 ? _lastScore : snakes.Sum(x => x.Get<Snake>().ApplesEaten);
        _lastScore = score;
        var scoreText = "Score: " + score;
        var length = GetTextLength(scoreText);
        var xPost = Convert.ToInt32(windowProvider.ScreenWidth - 30 - length * FontSize / 2);
        DrawText(scoreText, xPost, 10, FontSize, Color.White);
    }

    private void DrawStartGame()
    {
        const string startText = "Press F1 to Start";
        var length = GetTextLength(startText);
        var xPost = Convert.ToInt32(windowProvider.ScreenWidth / 1.5 - length * FontSize / 2);
        var yPost = Convert.ToInt32(windowProvider.ScreenHeight / 2 - FontSize / 2);
        DrawText(startText, xPost, yPost, FontSize, Color.White);
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