using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Views.Common.System;

public partial class ProfileView : ReactiveUserControl<ProfileViewModel>
{
    public ProfileView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 