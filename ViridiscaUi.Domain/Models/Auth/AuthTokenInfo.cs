using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель с информацией о токене авторизации
/// </summary>
public class AuthTokenInfo : ViewModelBase
{
    private string _refreshToken = string.Empty;
    private string _token = string.Empty;

    private DateTime _expiresAt;
    private bool _isValid;

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
    /// Флаг действительности токена
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        set => this.RaiseAndSetIfChanged(ref _isValid, value);
    }

    /// <summary>
    /// Создает новый экземпляр информации о токене
    /// </summary>
    public AuthTokenInfo()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Проверяет, истек ли срок действия токена
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// Создает новый экземпляр информации о токене
    /// </summary>
    public static AuthTokenInfo Create(string token, string refreshToken, DateTime expiresAt)
    {
        var tokenInfo = new AuthTokenInfo
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            IsValid = true
        };
        return tokenInfo;
    }

    /// <summary>
    /// Инвалидирует токен
    /// </summary>
    public void Invalidate()
    {
        IsValid = false;
        Token = string.Empty;
        RefreshToken = string.Empty;
    }
} 