using System;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель ответа на запрос аутентификации
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Флаг успешной авторизации
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// JWT токен доступа
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Токен обновления
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Время истечения токена
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Сообщение об ошибке (если авторизация не удалась)
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public Person? Person { get; set; }

    /// <summary>
    /// Создает новый экземпляр ответа на запрос авторизации
    /// </summary>
    public LoginResponse()
    {
    }

    /// <summary>
    /// Создает успешный ответ авторизации
    /// </summary>
    public static LoginResponse CreateSuccess(string token, string refreshToken, DateTime expiresAt, Person person)
    {
        var response = new LoginResponse
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            Person = person
        };
        return response;
    }

    /// <summary>
    /// Создает ответ с ошибкой авторизации
    /// </summary>
    public static LoginResponse Failure(string errorMessage)
    {
        var response = new LoginResponse
        {
            Success = false,
            ErrorMessage = errorMessage
        };
        return response;
    }
} 