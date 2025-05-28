using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Components;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Infrastructure;
using System.Collections.ObjectModel;
using System.Linq;
using ViridiscaUi.ViewModels;
using System.Collections.Generic; 

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel приложения, управляющая навигацией и состоянием пользователя
/// </summary>
public class MainViewModel : ViewModelBase, IScreen, IDisposable
{
    private readonly IAuthService _authService;
    private readonly IUserSessionService _userSessionService;
    private readonly IStatusService _statusService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IStatisticsService _statisticsService;
    private readonly CompositeDisposable _disposables = new();

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
        IUserSessionService userSessionService,
        IStatusService statusService,
        IUnifiedNavigationService navigationService,
        IStatisticsService statisticsService,
        IViewLocator viewLocator)
    {
        StatusLogger.LogInfo("Инициализация главной модели представления", "MainViewModel");
        
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        ViewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));

        // Initialize collections
        GroupedMenuItems = new ObservableCollection<MenuGroup>();

        // Initialize router
        Router = new RoutingState();

        // Initialize status bar
        StatusBar = new StatusBarViewModel(_statusService);

        // Subscribe to user changes
        SubscribeToUserChanges();

        // Load initial statistics
        LoadStatisticsAsync().ConfigureAwait(false);

        StatusLogger.LogInfo($"MainViewModel инициализирована: Пользователь={_userSessionService.CurrentUser?.Email ?? "не авторизован"}", "MainViewModel");
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
            // TODO: Реализовать открытие меню пользователя
            ShowInfo("Меню пользователя");
        }, null, "Ошибка открытия меню пользователя");
        
        // Команда быстрого действия
        QuickActionCommand = CreateCommand(async () =>
        {
            // TODO: Реализовать быстрое действие
            ShowInfo("Быстрое действие");
        }, null, "Ошибка быстрого действия");

        // Команда обновления статистики
        RefreshStatisticsCommand = CreateCommand(LoadStatisticsAsync, null, "Ошибка обновления статистики");

        // Подписка на изменения навигации для обновления CanGoBack
        this.WhenAnyValue(x => x.Router.NavigationStack.Count)
            .Subscribe(count => CanGoBack = count > 1)
            .DisposeWith(_disposables);
    }

    private void SubscribeToUserChanges()
    {
        _userSessionService.CurrentUserObservable
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(user =>
            {
                HandleUserChanged(user);
            })
            .DisposeWith(_disposables);
    }

    private void HandleUserChanged(User? user)
    {
        // Обновляем состояние авторизации
        IsLoggedIn = user != null;
        CurrentUser = user?.Email;
        UserRole = user?.Role?.Name;
        
        if (user != null)
        {
            StatusLogger.LogInfo($"Пользователь авторизован: {user.Email} ({user.Role?.Name ?? "без роли"})", "MainViewModel");
        }
        
        // Обновляем полную информацию о пользователе
        if (user != null)
        {
            CurrentUserInfo = new CurrentUserInfo
            {
                Id = user.Uid,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = user.Role?.Name != null ? new[] { user.Role.Name } : Array.Empty<string>(),
                PrimaryRole = user.Role?.Name ?? "Пользователь",
                LastLoginAt = user.LastLoginAt ?? DateTime.Now
            };
        }
        else
        {
            CurrentUserInfo = null;
        }
        
        // Обновляем инициалы пользователя
        if (user != null)
        {
            UserInitials = GetUserInitials(user);
        }
        else
        {
            UserInitials = "U";
        }
        
        // Обновляем меню
        UpdateMenuItems(user);
        
        if (user != null)
        {
            HandleUserLoggedIn(user);
        }
        else
        {
            HandleUserLoggedOut();
        }
    }

    private string GetUserInitials(User user)
    {
        var firstInitial = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName[0].ToString().ToUpper() : "";
        var lastInitial = !string.IsNullOrEmpty(user.LastName) ? user.LastName[0].ToString().ToUpper() : "";
        var initials = firstInitial + lastInitial;
        if (string.IsNullOrEmpty(initials))
        {
            initials = user.Email?.Length > 0 ? user.Email[0].ToString().ToUpper() : "U";
        }
        return initials;
    }

    private void UpdateMenuItems(User? user)
    {
        var userRoles = user?.Role?.Name != null ? new[] { user.Role.Name } : null;
        var menuRoutes = _navigationService.GetMenuRoutes(userRoles);
        
        // Очищаем коллекцию
        GroupedMenuItems.Clear();
        
        // Создаем сгруппированное меню одним LINQ-запросом
        var groupedMenuItems = menuRoutes
            .GroupBy(route => route.Group ?? "Основное")
            .OrderBy(group => group.Min(r => r.Order)) // Сортируем группы по минимальному Order в группе
            .Select(group => new MenuGroup
            {
                GroupName = group.Key,
                Order = group.Min(r => r.Order), // Используем минимальный Order из группы
                Items = new ObservableCollection<NavigationRoute>(
                    group.OrderBy(r => r.Order)
                         .ThenBy(r => r.DisplayName)
                         .Select(r =>
                         {
                             // Создаем команду навигации для каждого маршрута
                             r.NavigateCommand = ReactiveCommand.CreateFromTask(async () =>
                             {
                                 await _navigationService.NavigateToAsync(r.Path);
                             });
                             return r;
                         }))
            })
            .OrderBy(g => g.Order);

        // Добавляем группы в коллекцию
        foreach (var group in groupedMenuItems)
        {
            GroupedMenuItems.Add(group);
        }
        
        if (user != null)
        {
            StatusLogger.LogInfo($"Меню обновлено: {GroupedMenuItems.Count} групп, {menuRoutes.Count()} пунктов", "MainViewModel");
        }
    }

    private void HandleUserLoggedIn(User user)
    {
        ShowSuccess($"Добро пожаловать, {user.Email}!");
        
        var currentViewModel = Router.GetCurrentViewModel();
        if (currentViewModel is AuthenticationViewModel)
        {
            NavigateToDefaultPage(user);
        }
        else if (Router.NavigationStack.Count == 0)
        {
            NavigateToDefaultPage(user);
        }
    }

    private void NavigateToDefaultPage(User user)
    {
        // Навигация к домашней странице или странице по умолчанию для роли
        var defaultRoute = GetDefaultRouteForUser(user);
        if (!string.IsNullOrEmpty(defaultRoute))
        {
            _navigationService.NavigateToAsync(defaultRoute);
        }
        else
        {
            _navigationService.NavigateToAsync("home");
        }
    }

    private string? GetDefaultRouteForUser(User user)
    {
        // Определяем маршрут по умолчанию в зависимости от роли
        return user.Role?.Name?.ToLowerInvariant() switch
        {
            "student" => "student-dashboard",
            "teacher" => "teacher-dashboard", 
            "admin" => "admin-dashboard",
            "systemadmin" => "system-admin-dashboard",
            _ => "home"
        };
    }

    private void HandleUserLoggedOut()
    {
        ShowInfo("Вы вышли из системы");
        
        var currentViewModel = Router.GetCurrentViewModel();
        if (!(currentViewModel is AuthenticationViewModel))
        {
            _navigationService.NavigateAndResetAsync("auth");
        }
    }

    private void InitializeNavigation()
    {
        // Начальная навигация: если пользователь не авторизован и стек пуст, переходим на авторизацию
        if (_userSessionService.CurrentUser == null && Router.NavigationStack.Count == 0)
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
 
 