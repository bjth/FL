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
}