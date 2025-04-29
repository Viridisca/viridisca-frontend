using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using System;
using ViridiscaUi.DI;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Views;

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
        Locator.CurrentMutable.Register<IViewLocator>(() => new AppViewLocator());
        
        // Настройка ReactiveUI планировщика для Avalonia
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        
        // Create and build the service provider
        var services = new ServiceCollection();
        services.AddViridiscaServices();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
