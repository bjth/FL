using Raylib_cs;

namespace FL.Client.Providers;

public class WindowProvider
{
    public void ToggleFullScreenBorderless()
    {
        Raylib.ToggleBorderlessWindowed();
    }
    
    public int ScreenWidth => Raylib.GetScreenWidth();
    public int ScreenHeight => Raylib.GetScreenHeight();
}