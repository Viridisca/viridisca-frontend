using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using DomainValidationResult = ViridiscaUi.Domain.Models.Base.ValidationResult;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Универсальная реализация CRUD операций для доменных моделей
/// Обеспечивает единообразный подход к работе с данными через Entity Framework Core
/// </summary>
/// <typeparam name="TEntity">Тип доменной модели</typeparam>
public class GenericCrudService<TEntity> : IGenericCrudService<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger<GenericCrudService<TEntity>> _logger;
    protected readonly string _entityName;

    public GenericCrudService(ApplicationDbContext dbContext, ILogger<GenericCrudService<TEntity>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = _dbContext.Set<TEntity>();
        _entityName = typeof(TEntity).Name;
    }

    #region Основные CRUD операции

    /// <summary>
    /// Получает сущность по идентификатору
    /// </summary>
    public virtual async Task<TEntity?> GetByUidAsync(Guid uid)
    {
        try
        {
            _logger.LogDebug("Getting {EntityName} by UID: {Uid}", _entityName, uid);
            
            var entity = await _dbSet.FindAsync(uid);
            
            if (entity == null)
            {
                _logger.LogWarning("{EntityName} with UID {Uid} not found", _entityName, uid);
            }
            else
            {
                _logger.LogDebug("Successfully retrieved {EntityName} with UID: {Uid}", _entityName, uid);
            }
            
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityName} by UID: {Uid}", _entityName, uid);
            throw;
        }
    }

    /// <summary>
    /// Получает все сущности
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting all {EntityName} entities", _entityName);
            
            var entities = await _dbSet.ToListAsync();
            
            _logger.LogDebug("Successfully retrieved {Count} {EntityName} entities", entities.Count, _entityName);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityName} entities", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Получает сущности с пагинацией
    /// </summary>
    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        try
        {
            _logger.LogDebug("Getting paged {EntityName} entities: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}", 
                _entityName, page, pageSize, searchTerm);

            var query = _dbSet.AsQueryable();

            // Применяем поиск если указан
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogDebug("Successfully retrieved {Count} of {TotalCount} {EntityName} entities for page {Page}", 
                items.Count, totalCount, _entityName, page);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged {EntityName} entities", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Создает новую сущность
    /// </summary>
    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        try
        {
            _logger.LogDebug("Creating new {EntityName} entity", _entityName);

            // Валидация перед созданием
            var validationResult = await ValidateAsync(entity);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                throw new InvalidOperationException($"Validation failed for {_entityName}: {errors}");
            }

            // Устанавливаем Uid если он не задан
            SetUidIfEmpty(entity);

            // Устанавливаем даты создания и изменения
            SetAuditFields(entity, isCreate: true);

            _dbSet.Add(entity);
            await _dbContext.SaveChangesAsync();

            var uid = GetEntityUid(entity);
            _logger.LogInformation("Successfully created {EntityName} with UID: {Uid}", _entityName, uid);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityName} entity", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующую сущность
    /// </summary>
    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        try
        {
            var uid = GetEntityUid(entity);
            _logger.LogDebug("Updating {EntityName} entity with UID: {Uid}", _entityName, uid);

            // Валидация перед обновлением
            var validationResult = await ValidateForUpdateAsync(entity);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors);
                throw new InvalidOperationException($"Validation failed for {_entityName}: {errors}");
            }

            // Устанавливаем дату изменения
            SetAuditFields(entity, isCreate: false);

            _dbSet.Update(entity);
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Successfully updated {EntityName} with UID: {Uid}", _entityName, uid);
            }
            else
            {
                _logger.LogWarning("No changes were made to {EntityName} with UID: {Uid}", _entityName, uid);
            }

            return result;
        }
        catch (Exception ex)
        {
            var uid = GetEntityUid(entity);
            _logger.LogError(ex, "Error updating {EntityName} entity with UID: {Uid}", _entityName, uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет сущность по идентификатору
    /// </summary>
    public virtual async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            _logger.LogDebug("Deleting {EntityName} entity with UID: {Uid}", _entityName, uid);

            var entity = await _dbSet.FindAsync(uid);
            if (entity == null)
            {
                _logger.LogWarning("{EntityName} with UID {Uid} not found for deletion", _entityName, uid);
                return false;
            }

            _dbSet.Remove(entity);
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Successfully deleted {EntityName} with UID: {Uid}", _entityName, uid);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityName} entity with UID: {Uid}", _entityName, uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет сущность
    /// </summary>
    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        try
        {
            var uid = GetEntityUid(entity);
            _logger.LogDebug("Deleting {EntityName} entity with UID: {Uid}", _entityName, uid);

            _dbSet.Remove(entity);
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Successfully deleted {EntityName} with UID: {Uid}", _entityName, uid);
            }

            return result;
        }
        catch (Exception ex)
        {
            var uid = GetEntityUid(entity);
            _logger.LogError(ex, "Error deleting {EntityName} entity with UID: {Uid}", _entityName, uid);
            throw;
        }
    }

    #endregion

    #region Расширенные операции поиска

    /// <summary>
    /// Находит сущности по предикату
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("Finding {EntityName} entities by predicate", _entityName);

            var entities = await _dbSet.Where(predicate).ToListAsync();

            _logger.LogDebug("Found {Count} {EntityName} entities by predicate", entities.Count, _entityName);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding {EntityName} entities by predicate", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Находит первую сущность по предикату
    /// </summary>
    public virtual async Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("Finding first {EntityName} entity by predicate", _entityName);

            var entity = await _dbSet.FirstOrDefaultAsync(predicate);

            if (entity != null)
            {
                var uid = GetEntityUid(entity);
                _logger.LogDebug("Found first {EntityName} entity with UID: {Uid}", _entityName, uid);
            }
            else
            {
                _logger.LogDebug("No {EntityName} entity found by predicate", _entityName);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding first {EntityName} entity by predicate", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование сущности по предикату
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("Checking existence of {EntityName} entity by predicate", _entityName);

            var exists = await _dbSet.AnyAsync(predicate);

            _logger.LogDebug("{EntityName} entity exists: {Exists}", _entityName, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of {EntityName} entity by predicate", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Получает количество сущностей по предикату
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        try
        {
            _logger.LogDebug("Counting {EntityName} entities", _entityName);

            var count = predicate != null 
                ? await _dbSet.CountAsync(predicate)
                : await _dbSet.CountAsync();

            _logger.LogDebug("Counted {Count} {EntityName} entities", count, _entityName);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityName} entities", _entityName);
            throw;
        }
    }

    #endregion

    #region Массовые операции

    /// <summary>
    /// Создает несколько сущностей
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> CreateManyAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            var entitiesList = entities.ToList();
            _logger.LogDebug("Creating {Count} {EntityName} entities", entitiesList.Count, _entityName);

            foreach (var entity in entitiesList)
            {
                // Валидация каждой сущности
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    throw new InvalidOperationException($"Validation failed for {_entityName}: {errors}");
                }

                SetUidIfEmpty(entity);
                SetAuditFields(entity, isCreate: true);
            }

            _dbSet.AddRange(entitiesList);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully created {Count} {EntityName} entities", entitiesList.Count, _entityName);
            return entitiesList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multiple {EntityName} entities", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Обновляет несколько сущностей
    /// </summary>
    public virtual async Task<bool> UpdateManyAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            var entitiesList = entities.ToList();
            _logger.LogDebug("Updating {Count} {EntityName} entities", entitiesList.Count, _entityName);

            foreach (var entity in entitiesList)
            {
                // Валидация каждой сущности
                var validationResult = await ValidateForUpdateAsync(entity);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    throw new InvalidOperationException($"Validation failed for {_entityName}: {errors}");
                }

                SetAuditFields(entity, isCreate: false);
            }

            _dbSet.UpdateRange(entitiesList);
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Successfully updated {Count} {EntityName} entities", entitiesList.Count, _entityName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating multiple {EntityName} entities", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Удаляет несколько сущностей по идентификаторам
    /// </summary>
    public virtual async Task<bool> DeleteManyAsync(IEnumerable<Guid> uids)
    {
        try
        {
            var uidsList = uids.ToList();
            _logger.LogDebug("Deleting {Count} {EntityName} entities by UIDs", uidsList.Count, _entityName);

            var entities = await _dbSet.Where(e => uidsList.Contains(GetEntityUid(e))).ToListAsync();
            
            if (entities.Count != uidsList.Count)
            {
                _logger.LogWarning("Found {FoundCount} of {RequestedCount} {EntityName} entities for deletion", 
                    entities.Count, uidsList.Count, _entityName);
            }

            _dbSet.RemoveRange(entities);
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Successfully deleted {Count} {EntityName} entities", entities.Count, _entityName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple {EntityName} entities by UIDs", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Удаляет несколько сущностей
    /// </summary>
    public virtual async Task<bool> DeleteManyAsync(IEnumerable<TEntity> entities)
    {
        try
        {
            var entitiesList = entities.ToList();
            _logger.LogDebug("Deleting {Count} {EntityName} entities", entitiesList.Count, _entityName);

            _dbSet.RemoveRange(entitiesList);
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (result)
            {
                _logger.LogInformation("Successfully deleted {Count} {EntityName} entities", entitiesList.Count, _entityName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple {EntityName} entities", _entityName);
            throw;
        }
    }

    #endregion

    #region Операции с включениями (Include)

    /// <summary>
    /// Получает сущность по идентификатору с включением связанных данных
    /// </summary>
    public virtual async Task<TEntity?> GetByUidWithIncludesAsync(Guid uid, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        try
        {
            _logger.LogDebug("Getting {EntityName} by UID with includes: {Uid}", _entityName, uid);

            var query = _dbSet.AsQueryable();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            var entity = await query.FirstOrDefaultAsync(e => GetEntityUid(e) == uid);

            if (entity == null)
            {
                _logger.LogWarning("{EntityName} with UID {Uid} not found", _entityName, uid);
            }
            else
            {
                _logger.LogDebug("Successfully retrieved {EntityName} with UID and includes: {Uid}", _entityName, uid);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityName} by UID with includes: {Uid}", _entityName, uid);
            throw;
        }
    }

    /// <summary>
    /// Получает все сущности с включением связанных данных
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includeProperties)
    {
        try
        {
            _logger.LogDebug("Getting all {EntityName} entities with includes", _entityName);

            var query = _dbSet.AsQueryable();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            var entities = await query.ToListAsync();

            _logger.LogDebug("Successfully retrieved {Count} {EntityName} entities with includes", entities.Count, _entityName);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityName} entities with includes", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Находит сущности по предикату с включением связанных данных
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> FindWithIncludesAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        try
        {
            _logger.LogDebug("Finding {EntityName} entities by predicate with includes", _entityName);

            var query = _dbSet.AsQueryable();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            var entities = await query.Where(predicate).ToListAsync();

            _logger.LogDebug("Found {Count} {EntityName} entities by predicate with includes", entities.Count, _entityName);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding {EntityName} entities by predicate with includes", _entityName);
            throw;
        }
    }

    #endregion

    #region Операции с сортировкой

    /// <summary>
    /// Получает все сущности с сортировкой
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllOrderedAsync<TKey>(Expression<Func<TEntity, TKey>> orderBy, bool ascending = true)
    {
        try
        {
            _logger.LogDebug("Getting all {EntityName} entities ordered", _entityName);

            var query = ascending 
                ? _dbSet.OrderBy(orderBy)
                : _dbSet.OrderByDescending(orderBy);

            var entities = await query.ToListAsync();

            _logger.LogDebug("Successfully retrieved {Count} ordered {EntityName} entities", entities.Count, _entityName);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all ordered {EntityName} entities", _entityName);
            throw;
        }
    }

    /// <summary>
    /// Получает сущности с пагинацией и сортировкой
    /// </summary>
    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedOrderedAsync<TKey>(
        int page, 
        int pageSize, 
        Expression<Func<TEntity, TKey>> orderBy, 
        bool ascending = true, 
        string? searchTerm = null)
    {
        try
        {
            _logger.LogDebug("Getting paged ordered {EntityName} entities: Page={Page}, PageSize={PageSize}", 
                _entityName, page, pageSize);

            var query = _dbSet.AsQueryable();

            // Применяем поиск если указан
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            // Применяем сортировку
            query = ascending 
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);

            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogDebug("Successfully retrieved {Count} of {TotalCount} ordered {EntityName} entities for page {Page}", 
                items.Count, totalCount, _entityName, page);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged ordered {EntityName} entities", _entityName);
            throw;
        }
    }

    #endregion

    #region Операции валидации

    /// <summary>
    /// Валидирует сущность перед сохранением
    /// </summary>
    public virtual async Task<DomainValidationResult> ValidateAsync(TEntity entity)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация через DataAnnotations
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(entity);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        
        if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(entity, validationContext, validationResults, true))
        {
            errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "Ошибка валидации"));
        }

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Валидирует сущность для обновления
    /// </summary>
    public virtual async Task<DomainValidationResult> ValidateForUpdateAsync(TEntity entity)
    {
        // По умолчанию используем ту же валидацию, что и для создания
        return await ValidateAsync(entity);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Получает Uid сущности через рефлексию
    /// </summary>
    protected virtual Guid GetEntityUid(TEntity entity)
    {
        var uidProperty = typeof(TEntity).GetProperty("Uid");
        if (uidProperty != null && uidProperty.PropertyType == typeof(Guid))
        {
            return (Guid)(uidProperty.GetValue(entity) ?? Guid.Empty);
        }
        
        throw new InvalidOperationException($"Entity {_entityName} does not have a Uid property of type Guid");
    }

    /// <summary>
    /// Устанавливает Uid если он пустой
    /// </summary>
    protected virtual void SetUidIfEmpty(TEntity entity)
    {
        var uidProperty = typeof(TEntity).GetProperty("Uid");
        if (uidProperty != null && uidProperty.CanWrite)
        {
            var currentUid = (Guid)(uidProperty.GetValue(entity) ?? Guid.Empty);
            if (currentUid == Guid.Empty)
            {
                uidProperty.SetValue(entity, Guid.NewGuid());
            }
        }
    }

    /// <summary>
    /// Устанавливает поля аудита (CreatedAt, LastModifiedAt)
    /// </summary>
    protected virtual void SetAuditFields(TEntity entity, bool isCreate)
    {
        var now = DateTime.UtcNow;

        if (isCreate)
        {
            var createdAtProperty = typeof(TEntity).GetProperty("CreatedAt");
            if (createdAtProperty != null && createdAtProperty.CanWrite)
            {
                createdAtProperty.SetValue(entity, now);
            }
        }

        var lastModifiedAtProperty = typeof(TEntity).GetProperty("LastModifiedAt");
        if (lastModifiedAtProperty != null && lastModifiedAtProperty.CanWrite)
        {
            lastModifiedAtProperty.SetValue(entity, now);
        }
    }

    /// <summary>
    /// Применяет фильтр поиска к запросу
    /// Может быть переопределен в наследниках для специфичного поиска
    /// </summary>
    protected virtual IQueryable<TEntity> ApplySearchFilter(IQueryable<TEntity> query, string searchTerm)
    {
        // Базовая реализация - поиск по строковым свойствам
        var stringProperties = typeof(TEntity)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .ToList();

        if (!stringProperties.Any())
        {
            return query;
        }

        // Создаем выражение для поиска по всем строковым свойствам
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression? searchExpression = null;

        foreach (var property in stringProperties)
        {
            var propertyAccess = Expression.Property(parameter, property);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var searchValue = Expression.Constant(searchTerm, typeof(string));
            
            var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
            var containsCall = Expression.Call(propertyAccess, containsMethod!, searchValue);
            var condition = Expression.AndAlso(nullCheck, containsCall);

            searchExpression = searchExpression == null 
                ? condition 
                : Expression.OrElse(searchExpression, condition);
        }

        if (searchExpression != null)
        {
            var lambda = Expression.Lambda<Func<TEntity, bool>>(searchExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Валидирует специфичные для сущности правила
    /// Может быть переопределен в наследниках
    /// </summary>
    protected virtual async Task ValidateEntitySpecificRulesAsync(TEntity entity, List<string> errors, List<string> warnings, bool isCreate)
    {
        // Базовая реализация - без дополнительных правил
        await Task.CompletedTask;
    }

    #endregion
} 