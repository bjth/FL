namespace FL.Client.Systems;

public interface IGameSystem
{
    public ValueTask InitializeAsync()
    {
        return default;
    }
    
    public ValueTask UpdateAsync()
    {
        return default;
    }

    public ValueTask DrawAsync()
    {
        return default;
    }

    public ValueTask DrawUIAsync()
    {
        return default;
    }
}