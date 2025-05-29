using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure;
using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с учебными группами
/// Наследуется от GenericCrudService для получения универсальных CRUD операций
/// </summary>
public class GroupService : GenericCrudService<Group>, IGroupService
{
    private new readonly ApplicationDbContext _dbContext;
    private new readonly ILogger<GroupService> _logger;

    public GroupService(ApplicationDbContext dbContext, ILogger<GroupService> logger)
        : base(dbContext, logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Переопределение базовых методов для специфичной логики

    /// <summary>
    /// Применяет специфичный для групп поиск
    /// </summary>
    protected override IQueryable<Group> ApplySearchFilter(IQueryable<Group> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.ToLower();

        return query.Where(g => 
            g.Name.ToLower().Contains(lowerSearchTerm) ||
            g.Code.ToLower().Contains(lowerSearchTerm) ||
            g.Description.ToLower().Contains(lowerSearchTerm) ||
            (g.Curator != null && (
                g.Curator.FirstName.ToLower().Contains(lowerSearchTerm) ||
                g.Curator.LastName.ToLower().Contains(lowerSearchTerm)
            ))
        );
    }

    /// <summary>
    /// Валидирует специфичные для группы правила
    /// </summary>
    protected override async Task ValidateEntitySpecificRulesAsync(Group entity, List<string> errors, List<string> warnings, bool isCreate)
    {
        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(entity.Name))
            errors.Add("Название группы обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(entity.Code))
            errors.Add("Код группы обязателен для заполнения");

        if (string.IsNullOrWhiteSpace(entity.Description))
            warnings.Add("Рекомендуется добавить описание группы");

        // Проверка уникальности кода группы
        if (!string.IsNullOrWhiteSpace(entity.Code))
        {
            var codeExists = await _dbSet
                .Where(g => g.Uid != entity.Uid && g.Code.ToLower() == entity.Code.ToLower())
                .AnyAsync();

            if (codeExists)
                errors.Add($"Группа с кодом '{entity.Code}' уже существует");
        }

        // Проверка уникальности названия группы
        if (!string.IsNullOrWhiteSpace(entity.Name))
        {
            var nameExists = await _dbSet
                .Where(g => g.Uid != entity.Uid && g.Name.ToLower() == entity.Name.ToLower())
                .AnyAsync();

            if (nameExists)
                warnings.Add($"Группа с названием '{entity.Name}' уже существует");
        }

        // Проверка максимального количества студентов
        if (entity.MaxStudents <= 0)
            errors.Add("Максимальное количество студентов должно быть больше нуля");

        if (entity.MaxStudents > 100)
            warnings.Add("Максимальное количество студентов больше 100 - это очень много");

        // Проверка года обучения
        if (entity.Year < DateTime.Now.Year - 10)
            warnings.Add("Год обучения более 10 лет назад");

        if (entity.Year > DateTime.Now.Year + 1)
            errors.Add("Год обучения не может быть в будущем");

        // Проверка года обучения
        if (entity.Year <= 0 || entity.Year > 6)
            errors.Add("Год обучения должен быть от 1 до 6");

        // Проверка куратора
        if (entity.CuratorUid.HasValue)
        {
            var curatorExists = await _dbContext.Teachers
                .Where(t => t.Uid == entity.CuratorUid.Value)
                .AnyAsync();

            if (!curatorExists)
                errors.Add($"Куратор с Uid {entity.CuratorUid.Value} не найден");
        }

        await base.ValidateEntitySpecificRulesAsync(entity, errors, warnings, isCreate);
    }

    /// <summary>
    /// Переопределяем создание для генерации кода группы
    /// </summary>
    public override async Task<Group> CreateAsync(Group entity)
    {
        // Генерируем код группы если не указан
        if (string.IsNullOrEmpty(entity.Code))
        {
            entity.Code = await GenerateGroupCodeAsync(entity.Year);
        }

        return await base.CreateAsync(entity);
    }

    #endregion

    #region Реализация интерфейса IGroupService (существующие методы)

    public async Task<Group?> GetGroupAsync(Guid uid)
    {
        return await GetByUidWithIncludesAsync(uid, 
            g => g.Students, 
            g => g.Curator);
    }

    public async Task<IEnumerable<Group>> GetAllGroupsAsync()
    {
        return await GetAllWithIncludesAsync(g => g.Students, g => g.Curator);
    }

    public async Task<IEnumerable<Group>> GetGroupsAsync()
    {
        return await GetAllGroupsAsync();
    }

    public async Task<Group> CreateGroupAsync(Group group)
    {
        return await CreateAsync(group);
    }

    public async Task AddGroupAsync(Group group)
    {
        await CreateAsync(group);
    }

    public async Task<bool> UpdateGroupAsync(Group group)
    {
        return await UpdateAsync(group);
    }

    public async Task<bool> DeleteGroupAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    public async Task<IEnumerable<Group>> GetGroupsByCourseAsync(int course)
    {
        return await FindWithIncludesAsync(g => g.Year == course, g => g.Students, g => g.Curator);
    }

    public async Task<IEnumerable<Group>> GetGroupsByYearAsync(int year)
    {
        return await FindWithIncludesAsync(g => g.Year == year, g => g.Students, g => g.Curator);
    }

    public async Task<IEnumerable<Group>> GetGroupsByCuratorAsync(Guid curatorUid)
    {
        return await FindWithIncludesAsync(g => g.CuratorUid == curatorUid, g => g.Students, g => g.Curator);
    }

    /// <summary>
    /// Удаляет куратора группы
    /// </summary>
    public async Task<bool> RemoveCuratorAsync(Guid groupUid)
    {
        try
        {
            var group = await GetByUidAsync(groupUid);
            if (group == null)
            {
                _logger.LogWarning("Group {GroupUid} not found for curator removal", groupUid);
                return false;
            }

            group.CuratorUid = null;
            group.LastModifiedAt = DateTime.UtcNow;

            var result = await UpdateAsync(group);
            
            if (result)
            {
                _logger.LogInformation("Curator removed from group {GroupUid}", groupUid);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing curator from group {GroupUid}", groupUid);
            return false;
        }
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Получает статистику группы
    /// </summary>
    public async Task<GroupStatistics> GetGroupStatisticsAsync(Guid groupUid)
    {
        try
        {
            var group = await _dbContext.Groups
                .Include(g => g.Students)
                .FirstOrDefaultAsync(g => g.Uid == groupUid);

            if (group == null)
                throw new ArgumentException($"Group with UID {groupUid} not found");

            var activeStudents = group.Students.Count(s => s.Status == StudentStatus.Active);
            var averageGrade = await _dbContext.Grades
                .Where(g => group.Students.Select(s => s.Uid).Contains(g.StudentUid))
                .AverageAsync(g => (double?)g.Value) ?? 0;

            return new GroupStatistics
            {
                StudentsCount = group.Students.Count,
                ActiveStudentsCount = activeStudents,
                GraduatedStudentsCount = 0, // Заглушка
                ExpelledStudentsCount = 0, // Заглушка
                AverageGrade = (decimal)averageGrade,
                TotalEnrollments = group.Students.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group statistics: {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает группы с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Group> Groups, int TotalCount)> GetGroupsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        int? courseFilter = null,
        int? yearFilter = null)
    {
        try
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(g => g.Year == courseFilter.Value);
            }

            if (yearFilter.HasValue)
            {
                query = query.Where(g => g.Year == yearFilter.Value);
            }

            var totalCount = await query.CountAsync();

            var groups = await query
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (groups, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged groups");
            throw;
        }
    }

    /// <summary>
    /// Переводит группу на следующий курс
    /// </summary>
    public async Task<bool> PromoteToNextCourseAsync(Guid groupUid)
    {
        try
        {
            var group = await GetByUidAsync(groupUid);
            if (group == null)
            {
                _logger.LogWarning("Group not found for promotion: {GroupUid}", groupUid);
                return false;
            }

            if (group.Year >= 6)
            {
                _logger.LogWarning("Group is already on the final course: {GroupUid}", groupUid);
                return false;
            }

            group.Year++;
            return await UpdateAsync(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting group to next course: {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает активные группы
    /// </summary>
    public async Task<IEnumerable<Group>> GetActiveGroupsAsync()
    {
        try
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .Where(g => g.Status == GroupStatus.Active)
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active groups");
            throw;
        }
    }

    /// <summary>
    /// Получает группы по департаменту
    /// </summary>
    public async Task<IEnumerable<Group>> GetGroupsByDepartmentAsync(Guid departmentUid)
    {
        try
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .Where(g => g.DepartmentUid == departmentUid)
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups by department {DepartmentUid}", departmentUid);
            throw;
        }
    }

    /// <summary>
    /// Поиск групп по названию
    /// </summary>
    public async Task<IEnumerable<Group>> SearchGroupsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllGroupsAsync();

            var query = _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .AsQueryable();

            query = ApplySearchFilter(query, searchTerm);

            return await query
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching groups with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает группы с пагинацией (правильная сигнатура интерфейса)
    /// </summary>
    public async Task<(IEnumerable<Group> Groups, int TotalCount)> GetGroupsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Guid? departmentUid = null)
    {
        try
        {
            var query = _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            if (departmentUid.HasValue)
            {
                query = query.Where(g => g.DepartmentUid == departmentUid.Value);
            }

            var totalCount = await query.CountAsync();

            var groups = await query
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (groups, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged groups");
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование группы с указанным названием
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeUid = null)
    {
        try
        {
            var query = _dbContext.Groups
                .Where(g => g.Name.ToLower() == name.ToLower());

            if (excludeUid.HasValue)
            {
                query = query.Where(g => g.Uid != excludeUid.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if group exists by name: {Name}", name);
            throw;
        }
    }

    /// <summary>
    /// Назначает куратора группы
    /// </summary>
    public async Task<bool> AssignCuratorAsync(Guid groupUid, Guid? curatorUid)
    {
        try
        {
            var group = await GetByUidAsync(groupUid);
            if (group == null)
            {
                _logger.LogWarning("Group {GroupUid} not found for curator assignment", groupUid);
                return false;
            }

            // Проверяем существование куратора если указан
            if (curatorUid.HasValue)
            {
                var curatorExists = await _dbContext.Teachers
                    .AnyAsync(t => t.Uid == curatorUid.Value);

                if (!curatorExists)
                {
                    _logger.LogWarning("Curator {CuratorUid} not found", curatorUid.Value);
                    return false;
                }
            }

            group.CuratorUid = curatorUid;
            group.LastModifiedAt = DateTime.UtcNow;

            return await UpdateAsync(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning curator {CuratorUid} to group {GroupUid}", curatorUid, groupUid);
            return false;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Генерирует уникальный код группы
    /// </summary>
    private async Task<string> GenerateGroupCodeAsync(int year)
    {
        var yearCode = year.ToString();
        
        var lastCode = await _dbContext.Groups
            .Where(g => g.Code.StartsWith($"ГР-{yearCode}"))
            .OrderByDescending(g => g.Code)
            .Select(g => g.Code)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastCode))
        {
            var parts = lastCode.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"ГР-{yearCode}-{nextNumber:D2}";
    }

    #endregion
}
