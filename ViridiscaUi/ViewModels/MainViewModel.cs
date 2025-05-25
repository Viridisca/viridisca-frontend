using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Pages;
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.ViewModels.Profile;
using System.Threading.Tasks;
using Avalonia.Controls;
using ViridiscaUi.Infrastructure;
using System.Collections.Generic;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel, реализующая IScreen для управления навигацией
/// </summary>
public class MainViewModel : ViewModelBase, IScreen
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IStudentService _studentService;
    private readonly IDialogService _dialogService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IUserSessionService _userSessionService;
    private bool _isLoggedIn;
    private string? _currentUser;
    private string? _userRole;
    private bool _canGoBack;
    
    /// <summary>
    /// RoutingState для управления навигацией
    /// </summary>
    public RoutingState Router { get; } = new RoutingState();
    
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
        INavigationService navigationService,
        IStudentService studentService,
        IDialogService dialogService,
        IUserService userService,
        IRoleService roleService,
        IUserSessionService userSessionService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        
        // Подписка на изменения текущего пользователя
        _authService.CurrentUserObservable.Subscribe(user =>
        {
            var wasLoggedIn = IsLoggedIn;
            IsLoggedIn = user != null;
            CurrentUser = user?.Email;
            UserRole = user?.Role?.Name;
            
            // Если пользователь только что вошел в систему
            if (!wasLoggedIn && user != null)
            {
                Console.WriteLine($"MainViewModel: Пользователь вошел в систему: {user.Email}");
                UpdateMenuItems(user);
                NavigateToDefaultPage(user);
            }
            // Если пользователь вышел из системы
            else if (wasLoggedIn && user == null)
            {
                Console.WriteLine("MainViewModel: Пользователь вышел из системы");
                MenuItems.Clear();
                Router.Navigate.Execute(new AuthenticationViewModel(_authService, _navigationService, _roleService, this)).Subscribe();
            }
        });
        
        // Инициализация команд навигации
        NavigateToHomeCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new HomeViewModel(this))
                    .Do(_ => UpdateCanGoBack())
        );
        NavigateToCoursesCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new CoursesViewModel(this))
                    .Do(_ => UpdateCanGoBack())
        );
        NavigateToUsersCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new UsersViewModel(this))
                    .Do(_ => UpdateCanGoBack())
        );
        NavigateToStudentsCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new StudentsViewModel(this, _studentService, _navigationService, _dialogService, _authService))
                    .Do(_ => UpdateCanGoBack())
        );
        NavigateToProfileCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new ProfileViewModel(this, _userService, _dialogService, _authService))
                    .Do(_ => UpdateCanGoBack())
        );
        LogoutCommand = ReactiveCommand.CreateFromTask(Logout);
        GoBackCommand = ReactiveCommand.Create(GoBack);
        
        // Проверяем текущего пользователя и выполняем навигацию
        CheckCurrentUserAndNavigate();
    }
    
    /// <summary>
    /// Проверяет текущего пользователя и выполняет соответствующую навигацию
    /// </summary>
    private async void CheckCurrentUserAndNavigate()
    {
        Console.WriteLine("MainViewModel: Проверяем текущего пользователя...");
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser == null)
        {
            Console.WriteLine("MainViewModel: Пользователь не авторизован, показываем экран авторизации");
            // Если пользователь не авторизован, показываем экран авторизации
            Router.Navigate.Execute(new AuthenticationViewModel(_authService, _navigationService, _roleService, this)).Subscribe();
        }
        else
        {
            Console.WriteLine($"MainViewModel: Пользователь уже авторизован: {currentUser.Email}");
            // Если пользователь авторизован, принудительно обновляем состояние через UserSessionService
            // чтобы сработала подписка на CurrentUserObservable
            _userSessionService.SetCurrentUser(currentUser);
        }
    }
    
    /// <summary>
    /// Обновляет элементы меню в зависимости от роли пользователя
    /// </summary>
    private void UpdateMenuItems(User? user)
    {
        Console.WriteLine($"MainViewModel.UpdateMenuItems: Начинаем обновление меню для пользователя: {user?.Email ?? "null"}");
        
        var menuItems = new List<NavigationItemViewModel>();
        
        if (user == null)
        {
            Console.WriteLine("MainViewModel.UpdateMenuItems: Пользователь null, меню не создается");
            return;
        }

        Console.WriteLine($"MainViewModel.UpdateMenuItems: Роль пользователя: {user.Role?.Name ?? "null"}");
        Console.WriteLine($"MainViewModel.UpdateMenuItems: RoleId: {user.RoleId}");
        
        // Общие пункты меню для всех пользователей
        menuItems.Add(new NavigationItemViewModel("Главная", "🏠", NavigateToHomeCommand));
        menuItems.Add(new NavigationItemViewModel("Курсы", "📚", NavigateToCoursesCommand));
        menuItems.Add(new NavigationItemViewModel("Мой профиль", "👤", NavigateToProfileCommand));
        
        Console.WriteLine("MainViewModel.UpdateMenuItems: Добавлены базовые пункты меню: Главная, Курсы, Профиль");
        
        // Пункты меню для Администраторов и Преподавателей
        if (user.Role?.Name == "Administrator" || user.Role?.Name == "Teacher")
        {
            menuItems.Add(new NavigationItemViewModel("Студенты", "🎓", NavigateToStudentsCommand));
            Console.WriteLine($"MainViewModel.UpdateMenuItems: Добавлен пункт 'Студенты' для роли: {user.Role.Name}");
        }
        
        // Пункты меню только для администраторов
        if (user.Role?.Name == "Administrator")
        {
            menuItems.Add(new NavigationItemViewModel("Пользователи", "👥", NavigateToUsersCommand));
            Console.WriteLine("MainViewModel.UpdateMenuItems: Добавлен пункт 'Пользователи' для Администратора");
        }
        
        Console.WriteLine($"MainViewModel.UpdateMenuItems: Итого создано пунктов меню: {menuItems.Count}");
        foreach (var item in menuItems)
        {
            Console.WriteLine($"  - {item.Label}");
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
        
        Console.WriteLine($"MainViewModel.NavigateToDefaultPage: Выполняем начальную навигацию для {user.Role?.Name}");
        
        // ВАЖНО: Очищаем стек навигации после авторизации
        // чтобы страница авторизации не оставалась в истории
        Router.NavigationStack.Clear();
        
        // Навигация в зависимости от роли
        if (user.Role?.Name == "Administrator")
        {
            Router.Navigate.Execute(new HomeViewModel(this)).Subscribe();
        }
        else if (user.Role?.Name == "Teacher")
        {
            Router.Navigate.Execute(new CoursesViewModel(this)).Subscribe();
        }
        else // Student или другие роли
        {
            Router.Navigate.Execute(new CoursesViewModel(this)).Subscribe();
        }
        
        // Обновляем состояние кнопки "Назад"
        UpdateCanGoBack();
        
        Console.WriteLine($"MainViewModel.NavigateToDefaultPage: Стек навигации очищен, размер: {Router.NavigationStack.Count}");
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
        Router.Navigate.Execute(new AuthenticationViewModel(_authService, _navigationService, _roleService, this)).Subscribe();
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
                Console.WriteLine("MainViewModel.GoBack: Блокируем возврат на страницу авторизации для авторизованного пользователя");
                return;
            }
            
            Console.WriteLine($"MainViewModel.GoBack: Возвращаемся назад от {currentViewModel?.GetType().Name} к {previousViewModel?.GetType().Name}");
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
        Console.WriteLine($"MainViewModel.UpdateCanGoBack: CanGoBack={CanGoBack}, стек размер={Router.NavigationStack.Count}, авторизован={IsLoggedIn}");
    }
}
