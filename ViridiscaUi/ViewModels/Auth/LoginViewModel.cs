using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.ViewModels.Auth;

/// <summary>
/// ViewModel для страницы входа в систему
/// </summary>
public class LoginViewModel : RoutableViewModelBase
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// URL-сегмент для навигации
    /// </summary>
    public override string UrlPathSegment => "login";

    /// <summary>
    /// Имя пользователя (логин)
    /// </summary>
    [Reactive]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Reactive]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    [Reactive]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Флаг, указывающий на процесс входа
    /// </summary>
    [Reactive]
    public bool IsLoggingIn { get; set; } = false;

    /// <summary>
    /// Команда для входа в систему
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    
    /// <summary>
    /// Команда для перехода на страницу регистрации
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoToRegisterCommand { get; }

    /// <summary>
    /// Создает новый экземпляр ViewModel для входа в систему
    /// </summary>
    /// <param name="authService">Сервис аутентификации</param>
    /// <param name="navigationService">Сервис навигации</param>
    /// <param name="hostScreen">Родительский экран</param>
    public LoginViewModel(IAuthService authService, INavigationService navigationService, IScreen hostScreen) 
        : base(hostScreen)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        // Проверка возможности выполнения команды входа
        var canLogin = this.WhenAnyValue(
            x => x.Username,
            x => x.Password,
            x => x.IsLoggingIn,
            (username, password, isLoggingIn) =>
                !string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password) &&
                !isLoggingIn
        );

        // Создание команды входа
        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);
        
        // Команда для перехода на страницу регистрации
        GoToRegisterCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await _navigationService.NavigateToAsync("register");
        });
    }

    /// <summary>
    /// Асинхронный метод для выполнения входа в систему
    /// </summary>
    private async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            var result = await _authService.AuthenticateAsync(Username, Password);

            if (result.Success)
            {
                // Если вход успешный, перейти на домашнюю страницу или по умолчанию
                await _navigationService.NavigateToAsync("home");
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка входа: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }
}
