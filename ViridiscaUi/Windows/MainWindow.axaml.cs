using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ViridiscaUi.Windows
{
    public partial class MainWindow : ReactiveWindow<MainViewModel>
    {
        private readonly CompositeDisposable _disposables = new();

        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            
            this.WhenActivated(disposables =>
            {
                StatusLogger.LogInfo("Главное окно приложения инициализировано", "MainWindow");
                // Route the DataContext to ViewModel upon activation
                disposables.Add(this.WhenAnyValue(x => x.DataContext)
                    .Subscribe(dataContext => ViewModel = dataContext as MainViewModel));
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            StatusLogger.LogInfo("Главное окно закрыто", "MainWindow");
            _disposables?.Dispose();
            base.OnClosed(e);
        }
    }
} 