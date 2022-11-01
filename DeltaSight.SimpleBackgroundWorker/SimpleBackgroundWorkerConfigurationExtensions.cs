using Microsoft.Extensions.DependencyInjection;

namespace DeltaSight.SimpleBackgroundWorker;

public static class SimpleBackgroundWorkerConfigurationExtensions
{
    public static IServiceCollection AddSimpleBackgroundWorker(this IServiceCollection services, Action<SimpleBackgroundWorkerHostOptions> configure)
    {
        var queue = new SimpleBackgroundWorker(null);
        
        services.Configure(configure);
        services.AddSingleton<ISimpleBackgroundWorkerReader>(queue);
        services.AddSingleton<ISimpleBackgroundWorkerWriter>(queue);
        services.AddHostedService<SimpleBackgroundWorkerHost>();

        return services;
    }
}