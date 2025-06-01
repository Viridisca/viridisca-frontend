using System;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для управления сессией пользователя (Singleton)
/// </summary>
public interface IPersonSessionService
{
    /// <summary>
    /// Наблюдаемый объект, отражающий текущего пользователя
    /// </summary>
    IObservable<Person?> CurrentPersonObservable { get; }
    
    /// <summary>
    /// Текущий пользователь
    /// </summary>
    Person? CurrentPerson { get; }
    
    /// <summary>
    /// Устанавливает текущего пользователя
    /// </summary>
    void SetCurrentPerson(Person? person);
    
    /// <summary>
    /// Очищает сессию пользователя
    /// </summary>
    void ClearSession();
    
    /// <summary>
    /// Получает аккаунт текущего пользователя
    /// </summary>
    Account? CurrentAccount { get; }
    
    /// <summary>
    /// Устанавливает аккаунт текущего пользователя
    /// </summary>
    void SetCurrentAccount(Account? account);
} 