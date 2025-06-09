using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Base;
using DomainValidationResult = ViridiscaUi.Domain.Models.Base.ValidationResult;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Универсальный интерфейс для CRUD операций над доменными моделями
/// Обеспечивает единообразный подход к работе с данными
/// </summary>
/// <typeparam name="TEntity">Тип доменной модели</typeparam>
public interface IGenericCrudService<TEntity> where TEntity : class
{
    #region Основные CRUD операции

    /// <summary>
    /// Получает сущность по идентификатору
    /// </summary>
    /// <param name="uid">Идентификатор сущности</param>
    /// <returns>Сущность или null, если не найдена</returns>
    Task<TEntity?> GetByUidAsync(Guid uid);

    /// <summary>
    /// Получает все сущности
    /// </summary>
    /// <returns>Коллекция всех сущностей</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Получает сущности с пагинацией
    /// </summary>
    /// <param name="page">Номер страницы (начиная с 1)</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="searchTerm">Поисковый запрос (опционально)</param>
    /// <returns>Кортеж с сущностями и общим количеством</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null);

    /// <summary>
    /// Создает новую сущность
    /// </summary>
    /// <param name="entity">Сущность для создания</param>
    /// <returns>Созданная сущность</returns>
    Task<TEntity> CreateAsync(TEntity entity);

    /// <summary>
    /// Обновляет существующую сущность
    /// </summary>
    /// <param name="entity">Сущность для обновления</param>
    /// <returns>True, если обновление прошло успешно</returns>
    Task<bool> UpdateAsync(TEntity entity);

    /// <summary>
    /// Удаляет сущность по идентификатору
    /// </summary>
    /// <param name="uid">Идентификатор сущности</param>
    /// <returns>True, если удаление прошло успешно</returns>
    Task<bool> DeleteAsync(Guid uid);

    /// <summary>
    /// Удаляет сущность
    /// </summary>
    /// <param name="entity">Сущность для удаления</param>
    /// <returns>True, если удаление прошло успешно</returns>
    Task<bool> DeleteAsync(TEntity entity);

    #endregion

    #region Расширенные операции поиска

    /// <summary>
    /// Находит сущности по предикату
    /// </summary>
    /// <param name="predicate">Условие поиска</param>
    /// <returns>Коллекция найденных сущностей</returns>
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Находит первую сущность по предикату
    /// </summary>
    /// <param name="predicate">Условие поиска</param>
    /// <returns>Первая найденная сущность или null</returns>
    Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Проверяет существование сущности по предикату
    /// </summary>
    /// <param name="predicate">Условие проверки</param>
    /// <returns>True, если сущность существует</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Получает количество сущностей по предикату
    /// </summary>
    /// <param name="predicate">Условие подсчета (опционально)</param>
    /// <returns>Количество сущностей</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

    #endregion

    #region Массовые операции

    /// <summary>
    /// Создает несколько сущностей
    /// </summary>
    /// <param name="entities">Коллекция сущностей для создания</param>
    /// <returns>Коллекция созданных сущностей</returns>
    Task<IEnumerable<TEntity>> CreateManyAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Обновляет несколько сущностей
    /// </summary>
    /// <param name="entities">Коллекция сущностей для обновления</param>
    /// <returns>True, если все обновления прошли успешно</returns>
    Task<bool> UpdateManyAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Удаляет несколько сущностей по идентификаторам
    /// </summary>
    /// <param name="uids">Коллекция идентификаторов</param>
    /// <returns>True, если все удаления прошли успешно</returns>
    Task<bool> DeleteManyAsync(IEnumerable<Guid> uids);

    /// <summary>
    /// Удаляет несколько сущностей
    /// </summary>
    /// <param name="entities">Коллекция сущностей для удаления</param>
    /// <returns>True, если все удаления прошли успешно</returns>
    Task<bool> DeleteManyAsync(IEnumerable<TEntity> entities);

    #endregion

    #region Операции с включениями (Include)

    /// <summary>
    /// Получает сущность по идентификатору с включением связанных данных
    /// </summary>
    /// <param name="uid">Идентификатор сущности</param>
    /// <param name="includeProperties">Свойства для включения</param>
    /// <returns>Сущность с включенными данными или null</returns>
    Task<TEntity?> GetByUidWithIncludesAsync(Guid uid, params Expression<Func<TEntity, object>>[] includeProperties);

    /// <summary>
    /// Получает все сущности с включением связанных данных
    /// </summary>
    /// <param name="includeProperties">Свойства для включения</param>
    /// <returns>Коллекция сущностей с включенными данными</returns>
    Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includeProperties);

    /// <summary>
    /// Находит сущности по предикату с включением связанных данных
    /// </summary>
    /// <param name="predicate">Условие поиска</param>
    /// <param name="includeProperties">Свойства для включения</param>
    /// <returns>Коллекция найденных сущностей с включенными данными</returns>
    Task<IEnumerable<TEntity>> FindWithIncludesAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);

    #endregion

    #region Операции с сортировкой

    /// <summary>
    /// Получает все сущности с сортировкой
    /// </summary>
    /// <typeparam name="TKey">Тип ключа сортировки</typeparam>
    /// <param name="orderBy">Выражение для сортировки</param>
    /// <param name="ascending">Направление сортировки (по возрастанию)</param>
    /// <returns>Отсортированная коллекция сущностей</returns>
    Task<IEnumerable<TEntity>> GetAllOrderedAsync<TKey>(Expression<Func<TEntity, TKey>> orderBy, bool ascending = true);

    /// <summary>
    /// Получает сущности с пагинацией и сортировкой
    /// </summary>
    /// <typeparam name="TKey">Тип ключа сортировки</typeparam>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="orderBy">Выражение для сортировки</param>
    /// <param name="ascending">Направление сортировки</param>
    /// <param name="searchTerm">Поисковый запрос</param>
    /// <returns>Кортеж с отсортированными сущностями и общим количеством</returns>
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedOrderedAsync<TKey>(
        int page, 
        int pageSize, 
        Expression<Func<TEntity, TKey>> orderBy, 
        bool ascending = true, 
        string? searchTerm = null);

    #endregion

    #region Операции валидации

    /// <summary>
    /// Валидирует сущность перед сохранением
    /// </summary>
    /// <param name="entity">Сущность для валидации</param>
    /// <returns>Результат валидации</returns>
    Task<DomainValidationResult> ValidateAsync(TEntity entity);

    /// <summary>
    /// Валидирует сущность для обновления
    /// </summary>
    /// <param name="entity">Сущность для валидации</param>
    /// <returns>Результат валидации</returns>
    Task<DomainValidationResult> ValidateForUpdateAsync(TEntity entity);

    #endregion
} 