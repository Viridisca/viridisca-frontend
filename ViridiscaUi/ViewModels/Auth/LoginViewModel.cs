using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.ViewModels.Auth;

public class LoginViewModel : ViewModelBase
{
    private LoginRequest _loginRequest;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private bool _rememberMe;

    public LoginRequest LoginRequest 
    { 
        get => _loginRequest;
        set => this.RaiseAndSetIfChanged(ref _loginRequest, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => this.RaiseAndSetIfChanged(ref _rememberMe, value);
    }

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToRegisterCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToForgotPasswordCommand { get; }

    public LoginViewModel()
    {
        _loginRequest = new LoginRequest();

        var canLogin = this.WhenAnyValue(
            x => x.LoginRequest.Username,
            x => x.LoginRequest.Password,
            x => x.IsLoading,
            (username, password, isLoading) => 
                !string.IsNullOrWhiteSpace(username) && 
                !string.IsNullOrWhiteSpace(password) && 
                !isLoading
        );

        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);
        NavigateToRegisterCommand = ReactiveCommand.Create(NavigateToRegister);
        NavigateToForgotPasswordCommand = ReactiveCommand.Create(NavigateToForgotPassword);
    }

    private async Task LoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Здесь будет реализация запроса к API для авторизации
            await Task.Delay(1000); // Имитация запроса

            // Временная заглушка для демонстрации
            if (LoginRequest.Username == "admin" && LoginRequest.Password == "password")
            {
                // Успешная авторизация
                // Здесь будет навигация на главную страницу
            }
            else
            {
                ErrorMessage = "Неверное имя пользователя или пароль";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при авторизации: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NavigateToRegister()
    {
        // Навигация на страницу регистрации
    }

    private void NavigateToForgotPassword()
    {
        // Навигация на страницу восстановления пароля
    }
} 