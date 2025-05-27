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
using System.Collections.ObjectModel;
using System.Linq;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel приложения, управляющая навигацией и состоянием пользователя
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IAuthService _authService;
    private readonly IUserSessionService _userSessionService;
    private readonly IStatusService _statusService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IScreen _screen;
    private readonly CompositeDisposable _disposables = new();

    #region Properties

    /// <summary>
    /// RoutingState для управления навигацией
    /// </summary>
    public RoutingState Router => _screen.Router;
    
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
    /// Флаг, указывающий на то, что пользователь авторизован
    /// </summary>
    [Reactive] public bool IsLoggedIn { get; private set; }
    
    /// <summary>
    /// Роль текущего пользователя
    /// </summary>
    [Reactive] public string? UserRole { get; private set; }
    
    /// <summary>
    /// Флаг возможности навигации назад
    /// </summary>
    [Reactive] public bool CanGoBack { get; private set; }
    
    /// <summary>
    /// Элементы меню для навигации
    /// </summary>
    [Reactive] public ObservableCollection<NavigationRoute> MenuItems { get; private set; } = new();

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

    #endregion

    public MainViewModel(
        IAuthService authService, 
        IUserSessionService userSessionService,
        IStatusService statusService,
        IUnifiedNavigationService navigationService,
        IScreen screen,
        IViewLocator viewLocator)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        ViewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));

        // Инициализация StatusBar
        StatusBar = new StatusBarViewModel(_statusService);

        // Инициализация NavigationService
        _navigationService.Initialize(_screen);
        _navigationService.ScanAndRegisterRoutes();

        // Инициализация команд
        InitializeCommands();

        // Подписка на изменения пользователя
        SubscribeToUserChanges();

        // Начальная навигация
        InitializeNavigation();

        ShowInfo("Главное окно приложения инициализировано");
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

        // Подписка на изменения навигации для обновления CanGoBack
        this.WhenAnyValue(x => x.Router.NavigationStack.Count)
            .Subscribe(count => CanGoBack = count > 1)
            .DisposeWith(_disposables);
    }

    private void SubscribeToUserChanges()
    {
        _authService.CurrentUserObservable
            .Subscribe(user => HandleUserChanged(user))
            .DisposeWith(_disposables);
    }

    private void HandleUserChanged(User? user)
    {
        // Обновляем состояние авторизации
        IsLoggedIn = user != null;
        CurrentUser = user?.Email;
        UserRole = user?.Role?.Name;
        
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

    private void UpdateMenuItems(User? user)
    {
        var userRoles = user?.Role?.Name != null ? new[] { user.Role.Name } : null;
        var menuRoutes = _navigationService.GetMenuRoutes(userRoles);
        
        MenuItems.Clear();
        foreach (var route in menuRoutes)
        {
            // Создаем команду навигации для каждого маршрута
            route.NavigateCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await _navigationService.NavigateToAsync(route.Path);
            });
            
            MenuItems.Add(route);
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

    #endregion

    public override void Dispose()
    {
        _disposables?.Dispose();
        StatusBar?.Dispose();
        base.Dispose();
    }
}
