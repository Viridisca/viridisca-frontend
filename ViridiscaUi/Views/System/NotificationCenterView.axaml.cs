using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Views.System
{
    /// <summary>
    /// Представление для центра уведомлений
    /// </summary>
    public partial class NotificationCenterView : ReactiveUserControl<NotificationCenterViewModel>
    {
        public NotificationCenterView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 