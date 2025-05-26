using Avalonia.Controls;
using ReactiveUI;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Views.Education;

/// <summary>
/// Представление для управления курсами
/// </summary>
public partial class CoursesView : UserControl, IViewFor<CoursesViewModel>
{
    public CoursesView()
    {
        InitializeComponent();
    }

    public CoursesViewModel? ViewModel
    {
        get => DataContext as CoursesViewModel;
        set => DataContext = value;
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = value as CoursesViewModel;
    }
} 