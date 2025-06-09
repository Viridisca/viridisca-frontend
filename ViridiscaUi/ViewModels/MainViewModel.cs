using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.ViewModels.Components;
using ViridiscaUi.Services.Interfaces;
using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Models.Auth;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.Infrastructure;
using ReactiveUI.Fody.Helpers;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive;
using System.Linq;
using ReactiveUI;
using System;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel приложения, управляющая навигацией и состоянием пользователя
/// </summary>
public class MainViewModel : ViewModelBase, IScreen, IDisposable
{
    private readonly IAuthService _authService;
    private readonly IPersonSessionService _personSessionService;
    private readonly IStatusService _statusService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IStatisticsService _statisticsService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    private readonly CompositeDisposable _disposables = [];

    #region Properties

    /// <summary>
    /// RoutingState для управления навигацией
    /// </summary>
    public RoutingState Router { get; } = new RoutingState();

    /// <summary>
    /// ViewLocator для ReactiveUI навигации
    /// </summary>
    public IViewLocator ViewLocator { get; }

    /// <summary>
    /// StatusBar ViewModel
    /// </summary>
    public StatusBarViewModel StatusBar { get; }

    /// <summary>
    /// Текущий аутентифицированный пользователь
    /// </summary>
    [Reactive] public string? CurrentUser { get; private set; }

    /// <summary>
    /// Полная информация о текущем пользователе
    /// </summary>
    [Reactive] public CurrentUserInfo? CurrentUserInfo { get; private set; }

    /// <summary>
    /// Флаг, указывающий на то, что пользователь авторизован
    /// </summary>
    [Reactive] public bool IsLoggedIn { get; private set; }

    /// <summary>
    /// Роль текущего пользователя
    /// </summary>
    [Reactive] public string? UserRole { get; private set; }

    /// <summary>
    /// Инициалы пользователя для аватара
    /// </summary>
    [Reactive] public string UserInitials { get; private set; } = string.Empty;

    /// <summary>
    /// Флаг возможности навигации назад
    /// </summary>
    [Reactive] public bool CanGoBack { get; private set; }

    /// <summary>
    /// Пункты меню сгруппированные по секциям
    /// </summary>
    [Reactive] public ObservableCollection<MenuGroup> GroupedMenuItems { get; private set; } = new();

    /// <summary>
    /// Общее количество студентов
    /// </summary>
    [Reactive] public int TotalStudents { get; set; } = 0;

    /// <summary>
    /// Общее количество курсов
    /// </summary>
    [Reactive] public int TotalCourses { get; set; } = 0;

    /// <summary>
    /// Общее количество преподавателей
    /// </summary>
    [Reactive] public int TotalTeachers { get; set; } = 0;

    /// <summary>
    /// Общее количество заданий
    /// </summary>
    [Reactive] public int TotalAssignments { get; set; } = 0;

    /// <summary>
    /// Количество пользователей онлайн
    /// </summary>
    [Reactive] public int OnlineUsersCount { get; set; } = 1;

    /// <summary>
    /// Версия приложения
    /// </summary>
    [Reactive] public string AppVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Флаг загрузки статистики
    /// </summary>
    [Reactive] public bool IsLoadingStatistics { get; set; } = false;

    [Reactive] public Person? CurrentPerson { get; set; }

    #endregion

    #region Commands

    /// <summary>
    /// Команда для выхода из системы
    /// </summary>
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; private set; } = null!;

    /// <summary>
    /// Команда для возврата назад
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к маршруту
    /// </summary>
    public ReactiveCommand<string, Unit> NavigateToRouteCommand { get; private set; } = null!;

    /// <summary>
    /// Команда открытия меню пользователя
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenUserMenuCommand { get; private set; } = null!;

    /// <summary>
    /// Команда быстрого действия
    /// </summary>
    public ReactiveCommand<Unit, Unit> QuickActionCommand { get; private set; } = null!;

    /// <summary>
    /// Команда обновления статистики
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshStatisticsCommand { get; private set; } = null!;

    #endregion

    public MainViewModel(
        IAuthService authService,
        IPersonSessionService personSessionService,
        IStatusService statusService,
        IUnifiedNavigationService navigationService,
        IStatisticsService statisticsService,
        IViewLocator viewLocator,
        INotificationService notificationService,
        IDialogService dialogService)
    {
        StatusLogger.LogInfo("Инициализация главной модели представления", "MainViewModel");

        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _personSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        ViewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Initialize collections
        GroupedMenuItems = new ObservableCollection<MenuGroup>();

        // Initialize router
        Router = new RoutingState();

        // Initialize status bar
        StatusBar = new StatusBarViewModel(_statusService);

        // Initialize navigation service with this screen
        _navigationService.Initialize(this);
        _navigationService.ScanAndRegisterRoutes();

        // Initialize commands
        InitializeCommands();

        // Subscribe to user changes
        _personSessionService.CurrentPersonObservable
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(person =>
            {
                CurrentPerson = person;
                if (person != null)
                {
                    HandleUserLoggedIn(person);
                }
                else
                {
                    HandleUserLoggedOut();
                }
            })
            .DisposeWith(_disposables);

        // Initialize navigation
        InitializeNavigation();

        // Load initial statistics
        LoadStatisticsAsync().ConfigureAwait(false);

        StatusLogger.LogInfo($"MainViewModel инициализирована: Пользователь={_personSessionService.CurrentPerson?.Email ?? "не авторизован"}", "MainViewModel");
    }

    #region Private Methods

    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        LogoutCommand = CreateCommand(ExecuteLogoutAsync, null, "Ошибка при выходе из системы");

        // Команда навигации назад
        GoBackCommand = CreateCommand(async () =>
        {
            await _navigationService.GoBackAsync();
        }, null, "Ошибка при навигации назад");

        // Команда навигации к маршруту
        NavigateToRouteCommand = ReactiveCommand.CreateFromTask<string>(async path =>
        {
            await _navigationService.NavigateToAsync(path);
        });

        // Команда открытия меню пользователя
        OpenUserMenuCommand = CreateCommand(async () =>
        {
            await OpenUserMenuAsync();
        }, null, "Ошибка открытия меню пользователя");

        // Команда быстрого действия
        QuickActionCommand = CreateCommand(async () =>
        {
            await ExecuteQuickActionAsync();
        }, null, "Ошибка быстрого действия");

        // Команда обновления статистики
        RefreshStatisticsCommand = CreateCommand(LoadStatisticsAsync, null, "Ошибка обновления статистики");

        // Подписка на изменения навигации для обновления CanGoBack
        this.WhenAnyValue(x => x.Router.NavigationStack.Count)
            .Subscribe(count => CanGoBack = count > 1)
            .DisposeWith(_disposables);
    }

    private void HandleUserLoggedIn(Person person)
    {
        LogInfo("User logged in: {PersonName}", $"{person.FirstName} {person.LastName}");
        
        // Set IsLoggedIn to true to show sidebar
        IsLoggedIn = true;
        
        // Set user information for UI
        CurrentUser = person.Email;
        UserInitials = GetUserInitials(person);
        UserRole = person.PersonRoles?.FirstOrDefault(pr => pr.IsActive)?.Role?.Name ?? "Unknown";
        
        // Create CurrentUserInfo for sidebar display
        CurrentUserInfo = new CurrentUserInfo
        {
            Id = person.Uid,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email,
            PrimaryRole = UserRole,
            Roles = person.PersonRoles?.Where(pr => pr.IsActive)
                .Select(pr => pr.Role?.Name ?? "Unknown")
                .ToArray() ?? Array.Empty<string>(),
            LastLoginAt = DateTime.UtcNow
        };
        
        // Update menu based on user roles
        UpdateMenuItems(person);
        
        // Navigate to appropriate default page
        NavigateToDefaultPage(person);
        
        // Load user-specific data
        LoadUserDataAsync(person);
    }

    private void NavigateToDefaultPage(Person person)
    {
        // Navigate to home page by default
        _navigationService.NavigateToAsync("home");
    }

    private void HandleUserLoggedOut()
    {
        // Set IsLoggedIn to false to hide sidebar
        IsLoggedIn = false;
        
        // Clear user information
        CurrentUser = null;
        CurrentUserInfo = null;
        UserRole = null;
        UserInitials = string.Empty;
        CurrentPerson = null;
        
        // Clear menu items
        GroupedMenuItems.Clear();
        
        ShowInfo("Вы вышли из системы");

        var currentViewModel = Router.GetCurrentViewModel();

        if (currentViewModel is not AuthenticationViewModel)
        {
            _navigationService.NavigateAndResetAsync("auth");
        }
    }

    // Начальная навигация: если пользователь не авторизован и стек пуст, переходим на авторизацию
    private void InitializeNavigation()
    {
        if (_personSessionService.CurrentPerson == null && Router.NavigationStack.Count == 0)
        {
            _navigationService.NavigateToAsync("auth");
        }
    }

    private async Task ExecuteLogoutAsync()
    {
        await _authService.LogoutAsync();
        ShowInfo("Выход из системы выполнен");
    }

    private async void LoadStatistics()
    {
        await LoadStatisticsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            IsLoadingStatistics = true;

            // Загружаем статистику параллельно
            var systemStats = await _statisticsService.GetSystemStatisticsAsync();

            // Обновляем свойства
            TotalStudents = systemStats.TotalStudents;
            TotalCourses = systemStats.TotalCourses;
            TotalTeachers = systemStats.TotalTeachers;
            TotalAssignments = systemStats.TotalAssignments;

            // Симуляция онлайн пользователей (в реальном приложении это будет из SignalR или другого источника)
            OnlineUsersCount = Random.Shared.Next(1, 25);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка загрузки статистики: {ex.Message}", "MainViewModel");
            ShowError("Не удалось загрузить статистику");

            // Устанавливаем значения по умолчанию
            TotalStudents = 0;
            TotalTeachers = 0;
            TotalCourses = 0;
            TotalAssignments = 0;
            OnlineUsersCount = 1;
        }
        finally
        {
            IsLoadingStatistics = false;
        }
    }

    private string GetUserInitials(Person person)
    {
        if (person == null) return "??";
        
        var firstInitial = !string.IsNullOrEmpty(person.FirstName) ? person.FirstName[0].ToString().ToUpper() : "";
        var lastInitial = !string.IsNullOrEmpty(person.LastName) ? person.LastName[0].ToString().ToUpper() : "";
        
        return firstInitial + lastInitial;
    }

    private void UpdateMenuItems(Person? person)
    {
        var userRoles = person?.PersonRoles?.Select(pr => pr.Role?.Name).Where(r => r != null).ToArray() ?? new string[0];
        var menuRoutes = _navigationService.GetMenuRoutes(userRoles);

        // Группировка маршрутов
        var groupedMenuItems = menuRoutes
            .GroupBy(route => route.Group ?? "Основное")
            .OrderBy(group => group.Min(r => r.Order))
            .Select(group => new MenuGroup
            {
                GroupName = group.Key,
                Order = group.Min(r => r.Order),
                Items = new ObservableCollection<NavigationRoute>(
                    group.OrderBy(r => r.Order)
                         .ThenBy(r => r.DisplayName)
                         .Select(r =>
                         {
                             // Создание команды навигации для каждого маршрута
                             r.NavigateCommand = ReactiveCommand.CreateFromTask(async () =>
                             {
                                 await _navigationService.NavigateToAsync(r.Path);
                             });
                             return r;
                         }))
            });

        GroupedMenuItems.Clear();
        foreach (var group in groupedMenuItems)
        {
            GroupedMenuItems.Add(group);
        }
    }

    private async Task LoadUserDataAsync(Person person)
    {
        // Implementation of LoadUserDataAsync method
    }

    private async Task InitializeNavigationAsync()
    {
        // Временная заглушка для компиляции
        await Task.CompletedTask;
    }

    private async Task OpenUserMenuAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                await _navigationService.NavigateToAsync("profile");
            }
            else
            {
                await _navigationService.NavigateToAsync("auth");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка открытия меню пользователя");
            ShowError("Ошибка открытия меню пользователя");
        }
    }

    private async Task ExecuteQuickActionAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                // Быстрое действие - переход к созданию нового элемента
                await _navigationService.NavigateToAsync("students");
            }
            else
            {
                ShowError("Необходимо войти в систему для выполнения действий");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка выполнения быстрого действия");
            ShowError("Ошибка выполнения быстрого действия");
        }
    }

    #endregion

    public override void Dispose()
    {
        _disposables?.Dispose();
        StatusBar?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// Группа элементов меню
/// </summary>
public class MenuGroup
{
    /// <summary>
    /// Название группы (например, "Основное", "Образование", "Система")
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Элементы меню в этой группе
    /// </summary>
    public ObservableCollection<NavigationRoute> Items { get; set; } = new();

    /// <summary>
    /// Порядок отображения группы
    /// </summary>
    public int Order { get; set; }
}

