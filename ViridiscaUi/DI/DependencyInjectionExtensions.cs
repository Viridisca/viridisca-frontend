using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services)
    {
        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        
        // Register data services
        services.AddSingleton<LocalDbContext>();
        
        // Register other services as they are added
        // services.AddSingleton<ISomeService, SomeService>();
        
        return services;
    }
} 