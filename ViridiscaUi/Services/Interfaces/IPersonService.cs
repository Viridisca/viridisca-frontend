using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с людьми (Person)
/// </summary>
public interface IPersonService : IGenericCrudService<Person>
{
    /// <summary>
    /// Получает человека по UID
    /// </summary>
    Task<Person?> GetPersonAsync(Guid uid);

    /// <summary>
    /// Получает всех людей
    /// </summary>
    Task<IEnumerable<Person>> GetAllPersonsAsync();

    /// <summary>
    /// Получает людей с пагинацией
    /// </summary>
    new Task<(IEnumerable<Person> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null);

    /// <summary>
    /// Создает нового человека
    /// </summary>
    Task<Person> CreatePersonAsync(Person person);

    /// <summary>
    /// Обновляет человека
    /// </summary>
    Task<bool> UpdatePersonAsync(Person person);

    /// <summary>
    /// Удаляет человека
    /// </summary>
    Task<bool> DeletePersonAsync(Guid uid);

    /// <summary>
    /// Ищет людей по имени
    /// </summary>
    Task<IEnumerable<Person>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Получает человека по email
    /// </summary>
    Task<Person?> GetByEmailAsync(string email);

    /// <summary>
    /// Получает человека по телефону
    /// </summary>
    Task<Person?> GetByPhoneAsync(string phone);

    /// <summary>
    /// Проверяет существование человека по email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Проверяет существование человека по телефону
    /// </summary>
    Task<bool> ExistsByPhoneAsync(string phone);

    /// <summary>
    /// Валидирует данные человека
    /// </summary>
    Task<ValidationResult> ValidatePersonAsync(Person person);

    /// <summary>
    /// Получает аккаунт человека
    /// </summary>
    Task<Account?> GetPersonAccountAsync(Guid personUid);

    /// <summary>
    /// Получает человека по электронной почте
    /// </summary>
    Task<Person?> GetPersonByEmailAsync(string email);
} 