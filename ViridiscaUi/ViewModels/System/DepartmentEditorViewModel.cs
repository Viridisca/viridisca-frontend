using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для редактирования департамента
/// </summary>
public class DepartmentEditorViewModel : NavigatableViewModelBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IDialogService _dialogService;

    #region Properties

    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string? Description { get; set; }
    [Reactive] public bool IsEditing { get; set; }
    [Reactive] public Department? Department { get; set; }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    #endregion

    public DepartmentEditorViewModel(
        IScreen hostScreen,
        IUnifiedNavigationService navigationService,
        IDepartmentService departmentService,
        IDialogService dialogService) : base(hostScreen, navigationService)
    {
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    public void SetDepartment(Department? department)
    {
        Department = department;
        IsEditing = department != null;

        if (department != null)
        {
            Name = department.Name ?? string.Empty;
            Code = department.Code ?? string.Empty;
            Description = department.Description;
        }
        else
        {
            Name = string.Empty;
            Code = string.Empty;
            Description = null;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if (IsEditing && Department != null)
            {
                Department.Name = Name;
                Department.Code = Code;
                Department.Description = Description;
                await _departmentService.UpdateDepartmentAsync(Department);
            }
            else
            {
                var newDepartment = new Department
                {
                    Uid = Guid.NewGuid(),
                    Name = Name,
                    Code = Code,
                    Description = Description,
                    CreatedAt = DateTime.UtcNow
                };
                await _departmentService.CreateDepartmentAsync(newDepartment);
            }

            // Close dialog or navigate back
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка сохранения департамента: {ex.Message}");
        }
    }

    private void Cancel()
    {
        // Close dialog or navigate back
    }
} 