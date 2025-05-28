using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// Простая ViewModel для управления отделами
/// </summary>
[Route("departments", DisplayName = "Отделы", IconKey = "Building", Order = 1, Group = "Система", ShowInMenu = true)]
public class DepartmentsViewModel : RoutableViewModelBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;

    // === PROPERTIES ===
    [Reactive] public string Title { get; set; } = "Управление отделами";
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool HasErrors { get; set; }
    [Reactive] public new string? ErrorMessage { get; set; }
    
    // Statistics Properties
    [Reactive] public int TotalDepartments { get; set; } = 0;
    [Reactive] public int ActiveDepartments { get; set; } = 0;
    [Reactive] public int InactiveDepartments { get; set; } = 0;

    public ObservableCollection<DepartmentItem> Departments { get; } = new();

    // === COMMANDS ===
    public ReactiveCommand<Unit, Unit> LoadDepartmentsCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CreateDepartmentCommand { get; private set; }

    public DepartmentsViewModel(
        IDepartmentService departmentService,
        IDialogService dialogService,
        INotificationService notificationService,
        IScreen hostScreen,
        IUnifiedNavigationService navigationService)
        : base(hostScreen)
    {
        var _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        SetupCommands();
        
        // Initial load
        LoadDepartmentsCommand?.Execute().Subscribe();

        // Добавляем тестовые данные
        Departments.Add(new DepartmentItem { Name = "Информационные технологии", Code = "IT", StudentsCount = 150 });
        Departments.Add(new DepartmentItem { Name = "Математика", Code = "MATH", StudentsCount = 120 });
        Departments.Add(new DepartmentItem { Name = "Физика", Code = "PHYS", StudentsCount = 80 });
    }

    private void SetupCommands()
    {
        LoadDepartmentsCommand = ReactiveCommand.CreateFromTask(LoadDepartmentsAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
        CreateDepartmentCommand = ReactiveCommand.CreateFromTask(CreateDepartmentAsync);
    }

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            IsLoading = true;
            HasErrors = false;
            ErrorMessage = null;

            // Простая заглушка для демонстрации
            await Task.Delay(500);
            
            TotalDepartments = 15;
            ActiveDepartments = 12;
            InactiveDepartments = 3;
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

    private async Task RefreshAsync()
    {
        await LoadDepartmentsAsync();
    }

    private async Task CreateDepartmentAsync()
    {
        try
        {
            // Простая заглушка
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }

    private void HandleError(Exception ex)
    {
        HasErrors = true;
        ErrorMessage = ex.Message;
    }
}

public class DepartmentItem
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
} 