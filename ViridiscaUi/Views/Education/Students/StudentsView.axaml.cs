using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ViridiscaUi.ViewModels.Students;

namespace ViridiscaUi.Views.Education.Students
{
    public partial class StudentsView : ReactiveUserControl<StudentsViewModel>
    {
        public StudentsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 