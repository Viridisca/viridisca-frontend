using System;
using System.ComponentModel.DataAnnotations;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель запроса аутентификации пользователя
/// </summary>
public class LoginRequest : ViewModelBase
{
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;

    /// <summary>
    /// Имя пользователя или email
    /// </summary>
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    /// <summary>
    /// Флаг "Запомнить меня"
    /// </summary>
    public bool RememberMe
    {
        get => _rememberMe;
        set => this.RaiseAndSetIfChanged(ref _rememberMe, value);
    }

    /// <summary>
    /// Создает новый экземпляр запроса авторизации
    /// </summary>
    public LoginRequest()
    {
        Uid = Guid.NewGuid();
    }
} 