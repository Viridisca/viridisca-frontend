using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Pages;

namespace ViridiscaUi.Views.Pages;

public partial class UsersView : ReactiveUserControl<UsersViewModel>
{
    public UsersView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 