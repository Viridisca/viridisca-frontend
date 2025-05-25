using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using ViridiscaUi.Configuration;
using ViridiscaUi.DI;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Windows;

namespace ViridiscaUi;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Регистрация ViewLocator для ReactiveUI
        Locator.CurrentMutable.RegisterViewsForViewModels(typeof(App).Assembly);

        // Настройка ReactiveUI планировщика для Avalonia
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Создание конфигурации
            var configuration = BuildConfiguration();

            // Create and build the service provider
            var services = new ServiceCollection();

            // Create MainWindow first
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;

            // Register MainWindow in DI container
            services.AddSingleton(mainWindow);

            // Register other services with configuration
            services.AddViridiscaServices(configuration);
            Services = services.BuildServiceProvider();
              
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            // Создание конфигурации
            var configuration = BuildConfiguration();

            var services = new ServiceCollection();
            services.AddViridiscaServices(configuration);
            Services = services.BuildServiceProvider();
             
            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Создает конфигурацию приложения
    /// </summary>
    /// <returns>Конфигурация приложения</returns>
    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Настройки по умолчанию для разработки
                ["Logging:EnableSensitiveDataLogging"] = "true",
                ["Logging:EnableDetailedErrors"] = "true"
            });

        return builder.Build();
    }    
}
