using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.Windows;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {
            // Устанавливаем ViewLocator программно
            if (RouterViewHost != null && ViewModel?.ViewLocator != null)
            {
                RouterViewHost.ViewLocator = ViewModel.ViewLocator;
            }
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        // Устанавливаем ViewLocator при изменении DataContext
        if (RouterViewHost != null && ViewModel?.ViewLocator != null)
        {
            RouterViewHost.ViewLocator = ViewModel.ViewLocator;
        }
    }
}
