using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Pages;

namespace ViridiscaUi.Views.Pages;

public partial class CoursesView : ReactiveUserControl<CoursesViewModel>
{
    public CoursesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 