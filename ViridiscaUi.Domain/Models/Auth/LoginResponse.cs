using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель ответа на запрос аутентификации
/// </summary>
public class LoginResponse : ViewModelBase
{
    private string _refreshToken = string.Empty;
    private string _token = string.Empty;

    private string _errorMessage = string.Empty;

    private DateTime _expiresAt;
    private bool _success;
    
    private Person? _person;

    /// <summary>
    /// Флаг успешной авторизации
    /// </summary>
    public bool Success
    {
        get => _success;
        set => this.RaiseAndSetIfChanged(ref _success, value);
    }

    /// <summary>
    /// JWT токен доступа
    /// </summary>
    public string Token
    {
        get => _token;
        set => this.RaiseAndSetIfChanged(ref _token, value);
    }

    /// <summary>
    /// Токен обновления
    /// </summary>
    public string RefreshToken
    {
        get => _refreshToken;
        set => this.RaiseAndSetIfChanged(ref _refreshToken, value);
    }

    /// <summary>
    /// Время истечения токена
    /// </summary>
    public DateTime ExpiresAt
    {
        get => _expiresAt;
        set => this.RaiseAndSetIfChanged(ref _expiresAt, value);
    }

    /// <summary>
    /// Сообщение об ошибке (если авторизация не удалась)
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public Person? Person
    {
        get => _person;
        set => this.RaiseAndSetIfChanged(ref _person, value);
    }

    /// <summary>
    /// Создает новый экземпляр ответа на запрос авторизации
    /// </summary>
    public LoginResponse()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает успешный ответ авторизации
    /// </summary>
    public static LoginResponse CreateSuccess(string token, string refreshToken, DateTime expiresAt, Person person)
    {
        var response = new LoginResponse
        {
            Uid = Guid.NewGuid(),
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
            Uid = Guid.NewGuid(),
            Success = false,
            ErrorMessage = errorMessage
        };
        return response;
    }
} 