using System;
using Avalonia;
using Avalonia.ReactiveUI;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.Desktop;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Отладочная информация о конфигурации
            Console.WriteLine("=== VIRIDISCA UI DESKTOP STARTUP ===");
            Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
            Console.WriteLine($"Base Directory: {AppContext.BaseDirectory}");
            Console.WriteLine($"Args: {string.Join(", ", args)}");
            
            // Проверяем файлы конфигурации
            var appSettingsPath = System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var appSettingsDevPath = System.IO.Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json");
            
            Console.WriteLine($"appsettings.json path: {appSettingsPath}");
            Console.WriteLine($"appsettings.json exists: {System.IO.File.Exists(appSettingsPath)}");
            Console.WriteLine($"appsettings.Development.json path: {appSettingsDevPath}");
            Console.WriteLine($"appsettings.Development.json exists: {System.IO.File.Exists(appSettingsDevPath)}");
            
            if (System.IO.File.Exists(appSettingsPath))
            {
                var content = System.IO.File.ReadAllText(appSettingsPath);
                Console.WriteLine($"appsettings.json content length: {content.Length} characters");
            }
            
            Console.WriteLine("=== STARTING AVALONIA APP ===");
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
