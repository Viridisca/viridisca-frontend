using System;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для управления сессией пользователя (Singleton)
/// </summary>
public interface IUserSessionService
{
    /// <summary>
    /// Наблюдаемый объект, отражающий текущего пользователя
    /// </summary>
    IObservable<User?> CurrentUserObservable { get; }
    
    /// <summary>
    /// Текущий пользователь
    /// </summary>
    User? CurrentUser { get; }
    
    /// <summary>
    /// Устанавливает текущего пользователя
    /// </summary>
    void SetCurrentUser(User? user);
    
    /// <summary>
    /// Очищает сессию пользователя
    /// </summary>
    void ClearSession();
} 