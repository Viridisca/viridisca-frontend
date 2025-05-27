using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using ViridiscaUi.DI;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Windows;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Централизованный класс для инициализации приложения
/// Следует принципу единственной ответственности и обеспечивает чистую архитектуру
/// </summary>
public static class ApplicationBootstrapper
{
    private static IServiceProvider? _services;

    public static IServiceProvider Services => _services ?? throw new InvalidOperationException("Application not initialized");

    /// <summary>
    /// Инициализирует приложение для Desktop платформы
    /// </summary>
    public static void InitializeDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            Initialize();
            
            var mainWindow = CreateMainWindow();
            desktop.MainWindow = mainWindow;

            StatusLogger.LogSuccess("Desktop application initialized successfully", "ApplicationBootstrapper");
        }
        catch (Exception ex)
        {
            HandleInitializationError(ex, desktop);
        }
    }

    /// <summary>
    /// Инициализирует приложение для Single View платформы
    /// </summary>
    public static void InitializeSingleView(ISingleViewApplicationLifetime singleView)
    {
        try
        {
            Initialize();

            singleView.MainView = new MainView
            {
                DataContext = _services!.GetRequiredService<MainViewModel>()
            };

            StatusLogger.LogSuccess("Single view application initialized successfully", "ApplicationBootstrapper");
        }
        catch (Exception ex)
        {
            HandleSingleViewInitializationError(ex, singleView);
        }
    }

    /// <summary>
    /// Настраивает ReactiveUI для работы с Avalonia
    /// </summary>
    public static void ConfigureReactiveUI()
    {
        // Регистрация ViewLocator для ReactiveUI
        Locator.CurrentMutable.RegisterViewsForViewModels(typeof(App).Assembly);

        // Настройка ReactiveUI планировщика для Avalonia
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    }

    /// <summary>
    /// Основная инициализация приложения
    /// </summary>
    private static void Initialize()
    {
        var configuration = BuildConfiguration();
        _services = ConfigureServices(configuration);
        
        InitializeStatusLogger();
        StatusLogger.LogInfo("Application services configured successfully", "ApplicationBootstrapper");
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static IServiceProvider ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddViridiscaServices(configuration);
        return services.BuildServiceProvider();
    }

    private static void InitializeStatusLogger()
    {
        StatusLogger.Initialize(_services!);
    }

    private static MainWindow CreateMainWindow()
    {
        var mainWindow = _services!.GetRequiredService<MainWindow>();
        var mainViewModel = _services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;
        return mainWindow;
    }

    private static void HandleInitializationError(Exception ex, IClassicDesktopStyleApplicationLifetime desktop)
    {
        // Логируем критическую ошибку в консоль как fallback
        Console.WriteLine($"Critical initialization error: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");

        try
        {
            // Создаем минимальную конфигурацию для аварийного запуска
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddLogging(builder => builder.AddConsole());
            fallbackServices.AddSingleton<MainWindow>();
            fallbackServices.AddSingleton<MainViewModel>();
            
            _services = fallbackServices.BuildServiceProvider();

            // Пытаемся инициализировать StatusLogger даже в fallback режиме
            try
            {
                InitializeStatusLogger();
                StatusLogger.LogError($"Application started in fallback mode: {ex.Message}", "ApplicationBootstrapper");
            }
            catch
            {
                // Если StatusLogger не работает, используем Console
                Console.WriteLine("StatusLogger initialization failed in fallback mode");
            }

            var mainWindow = _services.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _services.GetRequiredService<MainViewModel>();
            desktop.MainWindow = mainWindow;
        }
        catch (Exception fallbackEx)
        {
            Console.WriteLine($"Fallback initialization also failed: {fallbackEx.Message}");
            throw; // Если даже fallback не работает, прекращаем выполнение
        }
    }

    private static void HandleSingleViewInitializationError(Exception ex, ISingleViewApplicationLifetime singleView)
    {
        // Логируем критическую ошибку в консоль как fallback
        Console.WriteLine($"Critical single view initialization error: {ex.Message}");

        try
        {
            // Создаем минимальную конфигурацию
            var fallbackServices = new ServiceCollection();
            fallbackServices.AddLogging(builder => builder.AddConsole());
            fallbackServices.AddSingleton<MainViewModel>();
            
            _services = fallbackServices.BuildServiceProvider();

            // Пытаемся инициализировать StatusLogger даже в fallback режиме
            try
            {
                InitializeStatusLogger();
                StatusLogger.LogError($"Single view application started in fallback mode: {ex.Message}", "ApplicationBootstrapper");
            }
            catch
            {
                Console.WriteLine("StatusLogger initialization failed in single view fallback mode");
            }

            singleView.MainView = new MainView
            {
                DataContext = _services.GetRequiredService<MainViewModel>()
            };
        }
        catch (Exception fallbackEx)
        {
            Console.WriteLine($"Single view fallback initialization also failed: {fallbackEx.Message}");
            throw;
        }
    }
} 