using FL.Client.Providers;

namespace FL.Client.Systems;

public class DeltaTimeSystem(DeltaTimeProvider deltaTimeProvider) : IGameSystem
{
    public ValueTask UpdateAsync()
    {
        return deltaTimeProvider.CalculateDeltaTimeAsync();
    }
}