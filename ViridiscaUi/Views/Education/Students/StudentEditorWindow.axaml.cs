using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViridiscaUi.ViewModels.Students;

namespace ViridiscaUi.Views.Education.Students
{
    public partial class StudentEditorWindow : ReactiveWindow<StudentEditorViewModel>
    {
        public StudentEditorWindow()
        {
            InitializeComponent();
            
            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.Title, v => v.Title));
                
                // Bind buttons by name
                var saveButton = this.FindControl<Button>("SaveButton");
                var cancelButton = this.FindControl<Button>("CancelButton");
                
                if (saveButton != null)
                    d(this.BindCommand(ViewModel, vm => vm.SaveCommand, v => saveButton));
                
                if (cancelButton != null)
                    d(this.BindCommand(ViewModel, vm => vm.CancelCommand, v => cancelButton));
            });
        }
    }
} 