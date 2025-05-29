using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using System.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Domain.Models.System;
using Microsoft.Extensions.Logging;
using DynamicData;
using DynamicData.Binding;
using System.Reactive.Linq;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для управления департаментами с полной реализацией CRUD операций
/// </summary>
[Route("departments", DisplayName = "Отделы", IconKey = "Building", Order = 1, Group = "Система", ShowInMenu = true)]
public class DepartmentsViewModel : RoutableViewModelBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DepartmentsViewModel> _logger;
    private readonly SourceList<Department> _departments = new();

    #region Properties

    [Reactive] public string Title { get; set; } = "Управление отделами";
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool HasErrors { get; set; }
    [Reactive] public new string? ErrorMessage { get; set; }
    [Reactive] public Department? SelectedDepartment { get; set; }
    
    // Filter Properties
    [Reactive] public bool ShowActiveOnly { get; set; } = false;
    [Reactive] public bool ShowInactiveOnly { get; set; } = false;
    
    // Statistics Properties
    [Reactive] public int TotalDepartments { get; set; } = 0;
    [Reactive] public int ActiveDepartments { get; set; } = 0;
    [Reactive] public int InactiveDepartments { get; set; } = 0;

    // Collections
    public ReadOnlyObservableCollection<Department> Departments => _filteredDepartments;
    private readonly ReadOnlyObservableCollection<Department> _filteredDepartments;

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> LoadDepartmentsCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CreateDepartmentCommand { get; private set; }
    public ReactiveCommand<Department, Unit> EditDepartmentCommand { get; private set; }
    public ReactiveCommand<Department, Unit> DeleteDepartmentCommand { get; private set; }
    public ReactiveCommand<Department, Unit> ViewDepartmentDetailsCommand { get; private set; }
    public ReactiveCommand<Department, Unit> ToggleActiveStatusCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ExportToCsvCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; }

    #endregion

    public DepartmentsViewModel(
        IDepartmentService departmentService,
        IDialogService dialogService,
        INotificationService notificationService,
        ILogger<DepartmentsViewModel> logger,
        IScreen hostScreen,
        IUnifiedNavigationService navigationService)
        : base(hostScreen)
    {
        var _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Setup filtered collection
        var filterPredicate = this.WhenAnyValue(
                x => x.SearchText,
                x => x.ShowActiveOnly,
                x => x.ShowInactiveOnly)
            .Select(CreateFilter);

        _departments.Connect()
            .Filter(filterPredicate)
            .Sort(SortExpressionComparer<Department>.Ascending(d => d.Name))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _filteredDepartments)
            .Subscribe(_ => UpdateStatistics());

        SetupCommands();
        
        // Initial load
        LoadDepartmentsCommand.Execute().Subscribe();
    }

    private void SetupCommands()
    {
        // Can execute predicates
        var canExecuteWithDepartment = this.WhenAnyValue(x => x.SelectedDepartment)
            .Select(dept => dept != null);

        var canExecuteWhenNotLoading = this.WhenAnyValue(x => x.IsLoading)
            .Select(loading => !loading);

        // Main CRUD commands
        LoadDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadDepartmentsAsync, canExecuteWhenNotLoading);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync, canExecuteWhenNotLoading);
        CreateDepartmentCommand = ReactiveCommand.CreateFromTask(CreateDepartment, canExecuteWhenNotLoading);
        EditDepartmentCommand = ReactiveCommand.CreateFromTask<Department>(EditDepartment, canExecuteWhenNotLoading);
        DeleteDepartmentCommand = ReactiveCommand.CreateFromTask<Department>(DeleteDepartment, canExecuteWhenNotLoading);
        ViewDepartmentDetailsCommand = ReactiveCommand.CreateFromTask<Department>(ViewDepartmentDetails, canExecuteWhenNotLoading);
        ToggleActiveStatusCommand = ReactiveCommand.CreateFromTask<Department>(ToggleActiveStatus, canExecuteWhenNotLoading);

        // Export commands
        ExportToExcelCommand = ReactiveCommand.CreateFromTask(ExportToExcel, canExecuteWhenNotLoading);
        ExportToCsvCommand = ReactiveCommand.CreateFromTask(ExportToCsv, canExecuteWhenNotLoading);

        // Filter commands
        ClearFiltersCommand = ReactiveCommand.Create(ClearFilters);

        // Error handling
        LoadDepartmentsCommand.ThrownExceptions.Subscribe(HandleError);
        CreateDepartmentCommand.ThrownExceptions.Subscribe(HandleError);
        EditDepartmentCommand.ThrownExceptions.Subscribe(HandleError);
        DeleteDepartmentCommand.ThrownExceptions.Subscribe(HandleError);
    }

    #region Command Implementations

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            IsLoading = true;
            HasErrors = false;
            ErrorMessage = null;

            _logger.LogInformation("Loading departments...");

            var departments = await _departmentService.GetAllDepartmentsAsync();
            
            _departments.Clear();
            _departments.AddRange(departments);

            _logger.LogInformation("Loaded {Count} departments", departments.Count());
            
            _notificationService.ShowSuccess("Данные департаментов успешно загружены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading departments");
            HandleError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadDepartmentsAsync();
    }

    private async Task CreateDepartment()
    {
        try
        {
            IsLoading = true;
            HasErrors = false;

            var newDepartment = new Department
            {
                Uid = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            var createViewModel = new DepartmentEditDialogViewModel(newDepartment, _departmentService, false);
            var result = await _dialogService.ShowDialogAsync<bool>(createViewModel);
            
            if (result == true)
            {
                var createdDepartment = await _departmentService.CreateDepartmentAsync(createViewModel.Department);
                await LoadDepartmentsAsync();
                _notificationService.ShowSuccess($"Департамент '{createdDepartment.Name}' успешно создан");
                LogInfo("Департамент создан: {DepartmentName}", createdDepartment.Name);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EditDepartment(Department department)
    {
        if (department == null) return;

        try
        {
            IsLoading = true;
            HasErrors = false;

            // Создаем копию для редактирования
            var departmentCopy = new Department
            {
                Uid = department.Uid,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                IsActive = department.IsActive,
                HeadOfDepartmentUid = department.HeadOfDepartmentUid,
                CreatedAt = department.CreatedAt,
                LastModifiedAt = DateTime.UtcNow
            };

            var editViewModel = new DepartmentEditDialogViewModel(departmentCopy, _departmentService, true);
            var result = await _dialogService.ShowDialogAsync<bool>(editViewModel);
            
            if (result == true)
            {
                await _departmentService.UpdateDepartmentAsync(editViewModel.Department);
                await LoadDepartmentsAsync();
                _notificationService.ShowSuccess($"Департамент '{editViewModel.Department.Name}' успешно обновлен");
                LogInfo("Департамент обновлен: {DepartmentName}", editViewModel.Department.Name);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteDepartment(Department department)
    {
        if (department == null) return;

        try
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить департамент '{department.Name}'?");

            if (!confirmed) return;

            IsLoading = true;
            HasErrors = false;

            await _departmentService.DeleteDepartmentAsync(department.Uid);
            await LoadDepartmentsAsync();
            
            _notificationService.ShowSuccess($"Департамент '{department.Name}' успешно удален");
            LogInfo("Департамент удален: {DepartmentName}", department.Name);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewDepartmentDetails(Department department)
    {
        if (department == null) return;

        try
        {
            var statistics = await _departmentService.GetDepartmentStatisticsAsync(department.Uid);
            var detailsViewModel = new DepartmentDetailsDialogViewModel(department, statistics);
            await _dialogService.ShowDialogAsync<bool>(detailsViewModel);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }

    private async Task ToggleActiveStatus(Department department)
    {
        if (department == null) return;

        try
        {
            var action = department.IsActive ? "деактивировать" : "активировать";
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение действия",
                $"Вы уверены, что хотите {action} департамент '{department.Name}'?");

            if (!confirmed) return;

            IsLoading = true;
            HasErrors = false;

            var newStatus = !department.IsActive;
            await _departmentService.SetDepartmentActiveStatusAsync(department.Uid, newStatus);
            await LoadDepartmentsAsync();
            
            var status = newStatus ? "активирован" : "деактивирован";
            _notificationService.ShowSuccess($"Департамент '{department.Name}' успешно {status}");
            LogInfo("Изменен статус департамента: {DepartmentName} -> {Status}", department.Name, newStatus);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExportToExcel()
    {
        try
        {
            _notificationService.ShowInfo("Экспорт в Excel находится в разработке");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }

    private async Task ExportToCsv()
    {
        try
        {
            _notificationService.ShowInfo("Экспорт в CSV находится в разработке");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }

    #endregion

    #region Helper Methods

    private Func<Department, bool> CreateFilter((string searchText, bool showActiveOnly, bool showInactiveOnly) filterParams)
    {
        return department =>
        {
            // Text filter
            if (!string.IsNullOrWhiteSpace(filterParams.searchText))
            {
                var searchLower = filterParams.searchText.ToLowerInvariant();
                if (!department.Name.ToLowerInvariant().Contains(searchLower) &&
                    !department.Code.ToLowerInvariant().Contains(searchLower) &&
                    !department.Description.ToLowerInvariant().Contains(searchLower))
                {
                    return false;
                }
            }

            // Status filters
            if (filterParams.showActiveOnly && !department.IsActive)
                return false;
            
            if (filterParams.showInactiveOnly && department.IsActive)
                return false;

            return true;
        };
    }

    private void UpdateStatistics()
    {
        var allDepartments = _departments.Items.ToList();
        TotalDepartments = allDepartments.Count;
        ActiveDepartments = allDepartments.Count(d => d.IsActive);
        InactiveDepartments = allDepartments.Count(d => !d.IsActive);
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        ShowInactiveOnly = false;
    }

    private void HandleError(Exception ex)
    {
        HasErrors = true;
        ErrorMessage = ex.Message;
        _logger.LogError(ex, "Error in DepartmentsViewModel");
    }

    #endregion
} 