using System;
using System.Reactive.Subjects;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Singleton сервис для управления сессией пользователя
/// </summary>
public class UserSessionService : IUserSessionService, IDisposable
{
    private readonly BehaviorSubject<User?> _currentUserSubject = new(null);
    private bool _disposed;

    /// <summary>
    /// Наблюдаемый объект, отражающий текущего пользователя
    /// </summary>
    public IObservable<User?> CurrentUserObservable => _currentUserSubject;

    /// <summary>
    /// Текущий пользователь
    /// </summary>
    public User? CurrentUser => _currentUserSubject.Value;

    /// <summary>
    /// Устанавливает текущего пользователя
    /// </summary>
    public void SetCurrentUser(User? user)
    {
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