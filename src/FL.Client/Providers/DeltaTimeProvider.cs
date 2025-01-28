using System.Diagnostics;

namespace FL.Client.Providers;

public class DeltaTimeProvider
{
    private readonly Stopwatch _timer = Stopwatch.StartNew();
    public float DeltaTime { get; private set; }

    private float _previousTime = 0;
    public ValueTask CalculateDeltaTimeAsync()
    {
        DeltaTime = (float)_timer.Elapsed.TotalSeconds - _previousTime;
        _previousTime = (float)_timer.Elapsed.TotalSeconds;
        return ValueTask.CompletedTask;
    }
}