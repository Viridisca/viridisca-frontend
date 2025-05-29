using Avalonia.Controls.ApplicationLifetimes;
using ViridiscaUi.Infrastructure;
using Avalonia.Markup.Xaml;
using Avalonia; 

namespace ViridiscaUi;

public partial class App : Application
{ 
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
