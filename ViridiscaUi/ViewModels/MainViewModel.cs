using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Pages;
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.ViewModels.Profile;
using ViridiscaUi.ViewModels.Components;
using System.Threading.Tasks;
using Avalonia.Controls;
using ViridiscaUi.Infrastructure;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel для управления приложением
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IAuthService _authService;
    private readonly IUserSessionService _userSessionService;
    private readonly IStatusService _statusService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IScreen _screen;
    private readonly CompositeDisposable _disposables = new();
    private bool _isLoggedIn;
    private string? _currentUser;
    private string? _userRole;
    private bool _canGoBack;
    
    /// <summary>
    /// RoutingState для управления навигацией
    /// </summary>
    public RoutingState Router => _screen.Router;
    
    /// <summary>
    /// StatusBar ViewModel
    /// </summary>
    public StatusBarViewModel StatusBar { get; }
    
    /// <summary>
    /// Текущий аутентифицированный пользователь
    /// </summary>
    public string? CurrentUser
    {
        get => _currentUser;
        set => this.RaiseAndSetIfChanged(ref _currentUser, value);
    }
    
    /// <summary>
    /// Флаг, указывающий на то, что пользователь авторизован
    /// </summary>
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
    }
    
    /// <summary>
    /// Роль текущего пользователя
    /// </summary>
    public string? UserRole
    {
        get => _userRole;
        set => this.RaiseAndSetIfChanged(ref _userRole, value);
    }
    
    /// <summary>
    /// Флаг возможности навигации назад
    /// </summary>
    public bool CanGoBack
    {
        get => _canGoBack;
        set => this.RaiseAndSetIfChanged(ref _canGoBack, value);
    }
    
    /// <summary>
    /// Элементы меню для навигации
    /// </summary>
    private ObservableCollection<NavigationItemViewModel> _menuItems = new();
    public ObservableCollection<NavigationItemViewModel> MenuItems
    {
        get => _menuItems;
        private set => this.RaiseAndSetIfChanged(ref _menuItems, value);
    }
    
    /// <summary>
    /// Команда для выхода из системы
    /// </summary>
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
    
    /// <summary>
    /// Команда для перехода на главную страницу
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToHomeCommand { get; }
    
    /// <summary>
    /// Команда для перехода на страницу курсов
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToCoursesCommand { get; }
    
    /// <summary>
    /// Команда для перехода на страницу пользователей
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToUsersCommand { get; }
    
    /// <summary>
    /// Команда для перехода на страницу профиля
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToProfileCommand { get; }
    
    /// <summary>
    /// Команда для перехода на страницу студентов
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToStudentsCommand { get; }
    
    /// <summary>
    /// Команда для возврата назад
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
    
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public MainViewModel(
        IAuthService authService, 
        IUserSessionService userSessionService,
        IStatusService statusService,
        IServiceProvider serviceProvider,
        IScreen screen)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));

        // Команды
        GoBackCommand = ReactiveCommand.Create(() => { Router.NavigateBack.Execute(); });

        // Подписка на изменения навигации для обновления CanGoBack
        this.WhenAnyValue(x => x.Router.NavigationStack.Count)
            .Subscribe(count => CanGoBack = count > 1)
            .DisposeWith(_disposables);

        // Инициализация StatusBar
        StatusBar = new StatusBarViewModel(_statusService);

        // Инициализация команд навигации
        LogoutCommand = ReactiveCommand.CreateFromTask(Logout);
        NavigateToHomeCommand = ReactiveCommand.CreateFromObservable(() => {
            var homeViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
            return Router.Navigate.Execute(homeViewModel);
        });
        NavigateToCoursesCommand = ReactiveCommand.CreateFromObservable(() => {
            var coursesViewModel = _serviceProvider.GetRequiredService<ViridiscaUi.ViewModels.Education.CoursesViewModel>();
            return Router.Navigate.Execute(coursesViewModel);
        });
        NavigateToUsersCommand = ReactiveCommand.CreateFromObservable(() => {
            var usersViewModel = _serviceProvider.GetRequiredService<UsersViewModel>();
            return Router.Navigate.Execute(usersViewModel);
        });
        NavigateToStudentsCommand = ReactiveCommand.CreateFromObservable(() => {
            var studentsViewModel = _serviceProvider.GetRequiredService<StudentsViewModel>();
            return Router.Navigate.Execute(studentsViewModel);
        });
        NavigateToProfileCommand = ReactiveCommand.CreateFromObservable(() => {
            var profileViewModel = _serviceProvider.GetRequiredService<ProfileViewModel>();
            return Router.Navigate.Execute(profileViewModel);
        });

        // Инициализация меню
        InitializeMenuItems();
        
        // ВАЖНО: Подписка на изменения текущего пользователя для автоматической навигации
        _authService.CurrentUserObservable
            .Subscribe(user => {
                StatusLogger.LogInfo($"Изменение текущего пользователя: {user?.Email ?? "null"}", "MainViewModel");
                
                // Обновляем состояние авторизации
                IsLoggedIn = user != null;
                CurrentUser = user?.Email;
                UserRole = user?.Role?.Name;
                
                if (user != null)
                {
                    // Пользователь авторизован - обновляем меню и выполняем навигацию
                    UpdateMenuItems(user);
                    
                    // Проверяем, находимся ли мы на странице авторизации
                    var currentViewModel = Router.GetCurrentViewModel();
                    if (currentViewModel is AuthenticationViewModel)
                    {
                        StatusLogger.LogInfo("Пользователь авторизован, выполняем навигацию с страницы авторизации", "MainViewModel");
                        NavigateToDefaultPage(user);
                    }
                    else if (Router.NavigationStack.Count == 0)
                    {
                        // Если стек навигации пуст (первый запуск), выполняем начальную навигацию
                        StatusLogger.LogInfo("Стек навигации пуст, выполняем начальную навигацию", "MainViewModel");
                        NavigateToDefaultPage(user);
                    }
                }
                else
                {
                    // Пользователь не авторизован - очищаем меню и переходим на авторизацию
                    MenuItems.Clear();
                    
                    var currentViewModel = Router.GetCurrentViewModel();
                    if (!(currentViewModel is AuthenticationViewModel))
                    {
                        StatusLogger.LogInfo("Пользователь не авторизован, переходим на страницу авторизации", "MainViewModel");
                        Router.NavigationStack.Clear();
                        var authViewModel = _serviceProvider.GetRequiredService<AuthenticationViewModel>();
                        Router.Navigate.Execute(authViewModel).Subscribe();
                    }
                }
                
                UpdateCanGoBack();
            })
            .DisposeWith(_disposables);
            
        // Начальная навигация: если пользователь не авторизован и стек пуст, переходим на авторизацию
        if (_userSessionService.CurrentUser == null && Router.NavigationStack.Count == 0)
        {
            StatusLogger.LogInfo("Начальная навигация: переходим на страницу авторизации", "MainViewModel");
            var authViewModel = _serviceProvider.GetRequiredService<AuthenticationViewModel>();
            Router.Navigate.Execute(authViewModel).Subscribe();
        }
    }
    
    /// <summary>
    /// Обновляет элементы меню в зависимости от роли пользователя
    /// </summary>
    private void UpdateMenuItems(User? user)
    {
        var menuItems = new List<NavigationItemViewModel>();
        
        if (user == null)
        {
            return;
        }

        // Общие пункты меню для всех пользователей
        menuItems.Add(new NavigationItemViewModel("Главная", "🏠", NavigateToHomeCommand));
        menuItems.Add(new NavigationItemViewModel("Курсы", "📚", NavigateToCoursesCommand));
        menuItems.Add(new NavigationItemViewModel("Мой профиль", "👤", NavigateToProfileCommand));
        
        // Пункты меню для Администраторов и Преподавателей
        if (user.Role?.Name == "Administrator" || user.Role?.Name == "Teacher")
        {
            menuItems.Add(new NavigationItemViewModel("Студенты", "🎓", NavigateToStudentsCommand));
        }
        
        // Пункты меню только для администраторов
        if (user.Role?.Name == "Administrator")
        {
            menuItems.Add(new NavigationItemViewModel("Пользователи", "👥", NavigateToUsersCommand));
        }
        
        MenuItems = new ObservableCollection<NavigationItemViewModel>(menuItems);
    }
    
    /// <summary>
    /// Выполняет навигацию на страницу по умолчанию в зависимости от роли пользователя
    /// </summary>
    private void NavigateToDefaultPage(User? user)
    {
        if (user == null)
            return;
        
        StatusLogger.LogInfo($"Выполняем начальную навигацию для {user.Role?.Name}", "MainViewModel");
        
        // ВАЖНО: Очищаем стек навигации после авторизации
        // чтобы страница авторизации не оставалась в истории
        Router.NavigationStack.Clear();
        
        // Навигация в зависимости от роли
        if (user.Role?.Name == "Administrator")
        {
            var homeViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
            Router.Navigate.Execute(homeViewModel).Subscribe();
        }
        else if (user.Role?.Name == "Teacher")
        {
            var coursesViewModel = _serviceProvider.GetRequiredService<ViridiscaUi.ViewModels.Education.CoursesViewModel>();
            Router.Navigate.Execute(coursesViewModel).Subscribe();
        }
        else // Student или другие роли
        {
            var coursesViewModel = _serviceProvider.GetRequiredService<ViridiscaUi.ViewModels.Education.CoursesViewModel>();
            Router.Navigate.Execute(coursesViewModel).Subscribe();
        }
        
        // Обновляем состояние кнопки "Назад"
        UpdateCanGoBack();
    }
    
    private async Task Logout()
    {
        await _authService.LogoutAsync();
        IsLoggedIn = false;
        CurrentUser = null;
        UserRole = null;
        MenuItems.Clear();
        
        // Очищаем стек и переходим на авторизацию
        Router.NavigationStack.Clear();
        var authViewModel = _serviceProvider.GetRequiredService<AuthenticationViewModel>();
        Router.Navigate.Execute(authViewModel).Subscribe();
        UpdateCanGoBack();
    }

    private void GoBack()
    {
        if (Router.NavigationStack.Count > 1)
        {
            var currentViewModel = Router.GetCurrentViewModel();
            var previousViewModel = Router.NavigationStack.Count > 1 ? Router.NavigationStack[Router.NavigationStack.Count - 2] : null;
            
            // Проверяем чтобы не возвращаться на страницу авторизации если пользователь авторизован
            if (previousViewModel is AuthenticationViewModel && IsLoggedIn)
            {
                StatusLogger.LogInfo("Блокируем возврат на страницу авторизации для авторизованного пользователя", "MainViewModel");
                return;
            }
            
            StatusLogger.LogInfo($"Возвращаемся назад от {currentViewModel?.GetType().Name} к {previousViewModel?.GetType().Name}", "MainViewModel");
            Router.NavigateBack.Execute().Subscribe(_ => UpdateCanGoBack());
        }
    }

    private void UpdateCanGoBack()
    {
        // Проверяем что есть куда вернуться И что предыдущая страница не AuthenticationView для авторизованного пользователя
        var canGoBack = Router.NavigationStack.Count > 1;
        
        if (canGoBack && IsLoggedIn && Router.NavigationStack.Count > 1)
        {
            var previousViewModel = Router.NavigationStack[Router.NavigationStack.Count - 2];
            if (previousViewModel is AuthenticationViewModel)
            {
                canGoBack = false;
            }
        }
        
        CanGoBack = canGoBack;
        StatusLogger.LogInfo($"CanGoBack={CanGoBack}, стек размер={Router.NavigationStack.Count}, авторизован={IsLoggedIn}", "MainViewModel");
    }

    /// <summary>
    /// Инициализирует элементы меню
    /// </summary>
    private void InitializeMenuItems()
    {
        MenuItems = new ObservableCollection<NavigationItemViewModel>();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
