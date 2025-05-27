using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.Windows
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            
            this.WhenActivated(disposables =>
            {
                // Устанавливаем ViewLocator программно
                if (MainRouterViewHost != null && ViewModel?.ViewLocator != null)
                {
                    MainRouterViewHost.ViewLocator = ViewModel.ViewLocator;
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
            if (MainRouterViewHost != null && ViewModel?.ViewLocator != null)
            {
                MainRouterViewHost.ViewLocator = ViewModel.ViewLocator;
            }
        }
    }
} 