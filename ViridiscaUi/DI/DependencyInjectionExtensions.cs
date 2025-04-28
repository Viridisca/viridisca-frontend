using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services)
    {
        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        
        // Register other services as they are added
        // services.AddSingleton<ISomeService, SomeService>();
        
        return services;
    }
} 