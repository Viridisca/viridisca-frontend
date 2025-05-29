using System;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для диалога просмотра деталей департамента
/// </summary>
public class DepartmentDetailsDialogViewModel : ViewModelBase
{
    [Reactive] public Department Department { get; set; }
    [Reactive] public DepartmentStatistics Statistics { get; set; }
    [Reactive] public string Title { get; set; }

    public ReactiveCommand<Unit, bool> CloseCommand { get; private set; }

    public DepartmentDetailsDialogViewModel(Department department, DepartmentStatistics statistics)
    {
        Department = department ?? throw new ArgumentNullException(nameof(department));
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        Title = $"Детали департамента: {department.Name}";

        CloseCommand = ReactiveCommand.Create(() => true);
    }
} 