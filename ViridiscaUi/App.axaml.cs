using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Splat;
using System;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi;

public partial class App : Application
{
    public static IServiceProvider Services => ApplicationBootstrapper.Services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Настройка ReactiveUI
        ApplicationBootstrapper.ConfigureReactiveUI();

        // Инициализация в зависимости от типа платформы
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                ApplicationBootstrapper.InitializeDesktop(desktop);
                break;
                
            case ISingleViewApplicationLifetime singleView:
                ApplicationBootstrapper.InitializeSingleView(singleView);
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
