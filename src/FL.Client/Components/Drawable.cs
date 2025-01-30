using Arch.Core;

namespace FL.Client.Components;

public record struct Drawable
{
    public Drawable() { }
    public required Action<Entity> DrawFn { get; init; }
    public int ZIndex { get; init; } = 0;
}