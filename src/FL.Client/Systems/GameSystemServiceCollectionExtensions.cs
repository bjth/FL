
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FL.Client.Systems;

public static class GameSystemServiceCollectionExtensions
{
    public static IServiceCollection AddGameSystem<T>(this IServiceCollection services)
        where T : class, IGameSystem
    {
        services.TryAddScoped<T>();
        services.AddScoped<IGameSystem, T>(sp => sp.GetRequiredService<T>());
        return services;
    }
}