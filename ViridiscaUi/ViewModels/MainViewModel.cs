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

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel, реализующая IScreen для управления навигацией
/// </summary>
public class MainViewModel : ViewModelBase, IScreen
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    
    /// <summary>
    /// RoutingState для управления навигацией
    /// </summary>
    public RoutingState Router { get; } = new RoutingState();
    
    /// <summary>
    /// Текущий аутентифицированный пользователь
    /// </summary>
    private User? _currentUser;
    public User? CurrentUser
    {
        get => _currentUser;
        private set => this.RaiseAndSetIfChanged(ref _currentUser, value);
    }
    
    /// <summary>
    /// Флаг, указывающий на то, что пользователь авторизован
    /// </summary>
    private bool _isUserLoggedIn;
    public bool IsUserLoggedIn
    {
        get => _isUserLoggedIn;
        private set => this.RaiseAndSetIfChanged(ref _isUserLoggedIn, value);
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
    public ReactiveCommand<Unit, IRoutableViewModel> GoBackCommand { get; }
    
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public MainViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        // Подписка на изменения текущего пользователя
        _authService.CurrentUserObservable
            .Subscribe(user =>
            {
                CurrentUser = user;
                IsUserLoggedIn = user != null;
                
                // Обновляем меню и навигацию при смене пользователя
                UpdateMenuItems(user);
                NavigateToDefaultPage(user);
            });
        
        // Инициализация команд навигации
        NavigateToHomeCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new HomeViewModel(this))
        );
        
        NavigateToCoursesCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new CoursesViewModel(this))
        );
        
        NavigateToUsersCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new UsersViewModel(this))
        );
        
        NavigateToProfileCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new HomeViewModel(this)) // TODO: заменить на ProfileViewModel
        );
        
        NavigateToStudentsCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new UsersViewModel(this)) // TODO: заменить на StudentsViewModel
        );
        
        // Команда для возврата назад по стеку навигации
        GoBackCommand = Router.NavigateBack;
        
        // Команда выхода из системы
        LogoutCommand = ReactiveCommand.CreateFromTask(async _ =>
        {
            await _authService.LogoutAsync();
        });
        
        // Слушаем изменения в сервисе навигации
        // для реализации навигации через сервис, а не только через Router
        _navigationService.RouteChanged
            .Subscribe(route => HandleRouteChange(route));
        
        // Проверяем текущего пользователя и выполняем навигацию
        CheckCurrentUserAndNavigate();
    }
    
    /// <summary>
    /// Обрабатывает изменение маршрута в сервисе навигации
    /// </summary>
    private void HandleRouteChange(string route)
    {
        switch (route)
        {
            case "home":
                NavigateToHomeCommand.Execute().Subscribe();
                break;
            case "courses":
                NavigateToCoursesCommand.Execute().Subscribe();
                break;
            case "users":
                NavigateToUsersCommand.Execute().Subscribe();
                break;
            case "login":
                Router.Navigate.Execute(new LoginViewModel(_authService, _navigationService, this)).Subscribe();
                break;
            case "register":
                Router.Navigate.Execute(new RegisterViewModel(_authService, _navigationService, this)).Subscribe();
                break;
            case "back":
                GoBackCommand.Execute().Subscribe();
                break;
        }
    }
    
    /// <summary>
    /// Проверяет текущего пользователя и выполняет соответствующую навигацию
    /// </summary>
    private async void CheckCurrentUserAndNavigate()
    {
        var currentUser = await _authService.GetCurrentUserAsync();
        
        if (currentUser == null)
        {
            // Если пользователь не авторизован, показываем экран входа
            Router.Navigate.Execute(new LoginViewModel(_authService, _navigationService, this));
        }
        else
        {
            // Если пользователь авторизован, обновляем данные и выполняем навигацию
            CurrentUser = currentUser;
            IsUserLoggedIn = true;
            UpdateMenuItems(currentUser);
            NavigateToDefaultPage(currentUser);
        }
    }
    
    /// <summary>
    /// Обновляет элементы меню в зависимости от роли пользователя
    /// </summary>
    private void UpdateMenuItems(User? user)
    {
        MenuItems.Clear();
        
        if (user == null)
            return;
        
        // Общие пункты меню для всех пользователей
        MenuItems.Add(new NavigationItemViewModel("Главная", "Home", NavigateToHomeCommand));
        MenuItems.Add(new NavigationItemViewModel("Курсы", "Book", NavigateToCoursesCommand));
        MenuItems.Add(new NavigationItemViewModel("Мой профиль", "Person", NavigateToProfileCommand));
        
        // Пункты меню для администраторов и преподавателей
        if (user.Role?.Name == "Administrator" || user.Role?.Name == "Teacher")
        {
            MenuItems.Add(new NavigationItemViewModel("Студенты", "People", NavigateToStudentsCommand));
        }
        
        // Пункты меню только для администраторов
        if (user.Role?.Name == "Administrator")
        {
            MenuItems.Add(new NavigationItemViewModel("Пользователи", "People", NavigateToUsersCommand));
        }
    }
    
    /// <summary>
    /// Выполняет навигацию на страницу по умолчанию в зависимости от роли пользователя
    /// </summary>
    private void NavigateToDefaultPage(User? user)
    {
        if (user == null)
            return;
        
        // Навигация в зависимости от роли
        if (user.Role?.Name == "Administrator")
        {
            NavigateToHomeCommand.Execute().Subscribe();
        }
        else if (user.Role?.Name == "Teacher")
        {
            NavigateToCoursesCommand.Execute().Subscribe();
        }
        else // Student или другие роли
        {
            NavigateToCoursesCommand.Execute().Subscribe();
        }
    }
}
