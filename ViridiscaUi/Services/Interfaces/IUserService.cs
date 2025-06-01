using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с людьми (Person)
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Получает человека по идентификатору
    /// </summary>
    Task<Person?> GetPersonAsync(Guid uid);
    
    /// <summary>
    /// Получает человека по электронной почте
    /// </summary>
    Task<Person?> GetPersonByEmailAsync(string email);
    
    /// <summary>
    /// Получает всех людей
    /// </summary>
    Task<IEnumerable<Person>> GetAllPersonsAsync();
     
    /// <summary>
    /// Добавляет нового человека
    /// </summary>
    Task<Person> AddPersonAsync(Person person);
    
    /// <summary>
    /// Обновляет существующего человека
    /// </summary>
    Task<bool> UpdatePersonAsync(Person person);
    
    /// <summary>
    /// Удаляет человека (мягкое удаление)
    /// </summary>
    Task<bool> DeletePersonAsync(Guid uid);
    
    /// <summary>
    /// Получает людей по роли
    /// </summary>
    Task<IEnumerable<Person>> GetPersonsByRoleAsync(string roleName);
    
    /// <summary>
    /// Получает людей по роли с контекстом
    /// </summary>
    Task<IEnumerable<Person>> GetPersonsByRoleAndContextAsync(string roleName, string? context = null);
    
    /// <summary>
    /// Назначает роль человеку
    /// </summary>
    Task<bool> AssignRoleAsync(Guid personUid, Guid roleUid, string? context = null, DateTime? validUntil = null, string? assignedBy = null);
    
    /// <summary>
    /// Отзывает роль у человека
    /// </summary>
    Task<bool> RevokeRoleAsync(Guid personUid, Guid roleUid, string? context = null);
    
    /// <summary>
    /// Получает роли человека
    /// </summary>
    Task<IEnumerable<PersonRole>> GetPersonRolesAsync(Guid personUid);
    
    /// <summary>
    /// Обновляет профиль человека
    /// </summary>
    Task<bool> UpdateProfileAsync(Guid uid, string firstName, string lastName, string? middleName, string? phoneNumber, string? address);
    
    /// <summary>
    /// Обновляет фото профиля человека
    /// </summary>
    Task<bool> UpdateProfileImageAsync(Guid uid, string imageUrl);
    
    /// <summary>
    /// Поиск людей по имени или email
    /// </summary>
    Task<IEnumerable<Person>> SearchPersonsAsync(string searchTerm);
}
