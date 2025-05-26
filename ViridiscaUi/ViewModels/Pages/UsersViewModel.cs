using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.ViewModels.Pages;

/// <summary>
/// ViewModel для страницы с пользователями
/// </summary>
public class UsersViewModel : ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment => "users";
    public IScreen HostScreen { get; }

    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IDialogService _dialogService;
    private readonly IStatusService _statusService;

    public string Title => "Пользователи";
    public string Description => "Управление пользователями системы";

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }

    public UsersViewModel(
        IUserService userService,
        IRoleService roleService,
        IDialogService dialogService,
        IStatusService statusService,
        IScreen hostScreen)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        HostScreen = hostScreen ?? throw new ArgumentNullException(nameof(hostScreen));

        // Инициализация команд
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadUsers);
        ClearFiltersCommand = ReactiveCommand.Create(() => { /* Очистка фильтров */ });
        
        // Загрузка пользователей при инициализации
        LoadUsers();
    }

    private async Task LoadUsers()
    {
        try
        {
            _statusService.ShowInfo("Загрузка пользователей...", "UsersViewModel");
            // Здесь будет логика загрузки пользователей
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка загрузки пользователей: {ex.Message}", "UsersViewModel");
        }
    }
} 