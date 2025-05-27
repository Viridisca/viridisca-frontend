using Avalonia.ReactiveUI;
using Avalonia;
using System;
using ViridiscaUi;

namespace ViridiscaUi.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .UseReactiveUI();
}
