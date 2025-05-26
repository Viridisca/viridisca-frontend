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
            try
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
                
                // Инициализация StatusLogger
                StatusLogger.Initialize(Services);
                  
                var mainViewModel = Services.GetRequiredService<MainViewModel>();
                
                mainWindow.DataContext = mainViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ОШИБКА при инициализации Desktop приложения: {ex.Message} ===");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Fallback initialization if services fail
                var mainWindow = new MainWindow();
                desktop.MainWindow = mainWindow;
                
                // Create minimal services for fallback
                var services = new ServiceCollection();
                services.AddSingleton(mainWindow);
                services.AddSingleton<MainViewModel>();
                Services = services.BuildServiceProvider();
                
                mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
                
                // Show error message
                var errorMessage = $"Ошибка инициализации приложения: {ex.Message}\n\nПриложение запущено в ограниченном режиме.";
                Console.WriteLine(errorMessage);
                
                // You can also show a message box if needed
                // MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            try
            {
                // Создание конфигурации
                var configuration = BuildConfiguration();

                var services = new ServiceCollection();
                services.AddViridiscaServices(configuration);
                Services = services.BuildServiceProvider();
                
                // Инициализация StatusLogger
                StatusLogger.Initialize(Services);
                 
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = Services.GetRequiredService<MainViewModel>()
                };
            }
            catch (Exception ex)
            {
                // Fallback for single view platforms
                var services = new ServiceCollection();
                services.AddSingleton<MainViewModel>();
                Services = services.BuildServiceProvider();
                
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = Services.GetRequiredService<MainViewModel>()
                };
                
                Console.WriteLine($"Ошибка инициализации: {ex.Message}");
            }
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
