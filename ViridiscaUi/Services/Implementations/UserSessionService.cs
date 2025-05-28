using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Singleton сервис для управления сессией пользователя
/// </summary>
public class UserSessionService : IUserSessionService, IDisposable
{
    private readonly BehaviorSubject<User?> _currentUserSubject = new(null);
    private bool _disposed;

    /// <summary>
    /// Конструктор UserSessionService
    /// </summary>
    public UserSessionService()
    {
        _currentUserSubject = new BehaviorSubject<User?>(null);
        StatusLogger.LogInfo("Сервис пользовательской сессии инициализирован", "UserSessionService");
    }

    /// <summary>
    /// Observable для отслеживания изменений текущего пользователя
    /// </summary>
    public IObservable<User?> CurrentUserObservable
    {
        get
        {
            return _currentUserSubject.AsObservable();
        }
    }

    /// <summary>
    /// Текущий авторизованный пользователь
    /// </summary>
    public User? CurrentUser => _currentUserSubject.Value;

    /// <summary>
    /// Устанавливает текущего пользователя и уведомляет всех подписчиков
    /// </summary>
    /// <param name="user">Пользователь для установки (null для выхода из системы)</param>
    public void SetCurrentUser(User? user)
    {
        if (user != null)
        {
            StatusLogger.LogInfo($"Установлен текущий пользователь: {user.Email}", "UserSessionService");
        }
        
        _currentUserSubject.OnNext(user);
    }

    /// <summary>
    /// Очищает сессию пользователя
    /// </summary>
    public void ClearSession()
    {
        _currentUserSubject.OnNext(null);
    }

    /// <summary>
    /// Освобождает ресурсы
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _currentUserSubject?.Dispose();
            _disposed = true;
        }
    }
} 