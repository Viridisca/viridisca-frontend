using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Views.System;

/// <summary>
/// View for managing departments with full CRUD functionality
/// </summary>
public partial class DepartmentsView : ReactiveUserControl<DepartmentsViewModel>
{
    public DepartmentsView()
    {
        InitializeComponent();
        
        this.WhenActivated(disposables =>
        {
            // Здесь можно добавить дополнительную логику активации
        });
    }
} 