using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
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
    private static IServiceProvider _services = null!;

    public static IServiceProvider Services => _services ?? throw new InvalidOperationException("Application not initialized");

    /// <summary>
    /// Инициализирует приложение для Desktop платформы
    /// </summary>
    public static void InitializeDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            Initialize();

            var mainWindow = _services.GetRequiredService<MainWindow>();
            
            try
            {
                var mainViewModel = _services.GetRequiredService<MainViewModel>();
                mainWindow.DataContext = mainViewModel;
                StatusLogger.LogInfo("Главная модель представления создана и привязана к окну", "ApplicationBootstrapper");
            }
            catch (Exception vmEx)
            {
                StatusLogger.LogError($"Ошибка создания MainViewModel: {vmEx.Message}", "ApplicationBootstrapper");
                throw;
            }
            
            desktop.MainWindow = mainWindow;

            StatusLogger.LogSuccess("Десктопное приложение инициализировано успешно", "ApplicationBootstrapper");
        }
        catch (Exception fallbackEx)
        {
            StatusLogger.LogError($"Критическая ошибка инициализации приложения: {fallbackEx.Message}", "ApplicationBootstrapper");
            Console.WriteLine($"Fallback initialization also failed: {fallbackEx.Message}");
            throw; // Если даже fallback не работает, прекращаем выполнение
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
        catch (Exception fallbackEx)
        {
            Console.WriteLine($"Fallback initialization also failed: {fallbackEx.Message}"); 
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
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            services.AddViridiscaServices(configuration);

            _services = services.BuildServiceProvider();

            StatusLogger.Initialize(_services!);
            StatusLogger.LogInfo("Application services configured successfully", "ApplicationBootstrapper");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Initialize: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}