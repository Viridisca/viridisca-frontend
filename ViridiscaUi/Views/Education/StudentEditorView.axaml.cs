using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Students;

namespace ViridiscaUi.Views.Education;

public partial class StudentEditorView : ReactiveUserControl<StudentEditorViewModel>
{
    public StudentEditorView()
    {
        InitializeComponent();
    }
}
