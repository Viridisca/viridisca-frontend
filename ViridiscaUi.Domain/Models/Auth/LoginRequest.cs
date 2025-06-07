using System.ComponentModel.DataAnnotations;
using System;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель запроса аутентификации пользователя
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Имя пользователя или email
    /// </summary>
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Флаг "Запомнить меня"
    /// </summary>
    public bool RememberMe { get; set; }

    /// <summary>
    /// Создает новый экземпляр запроса авторизации
    /// </summary>
    public LoginRequest()
    {
    }
} 