using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с группами
/// Наследуется от IGenericCrudService для получения универсальных CRUD операций
/// </summary>
public interface IGroupService : IGenericCrudService<Group>
{
    /// <summary>
    /// Получает группу по идентификатору
    /// </summary>
    Task<Group?> GetGroupAsync(Guid uid);
    
    /// <summary>
    /// Получает все группы
    /// </summary>
    Task<IEnumerable<Group>> GetAllGroupsAsync();
    
    /// <summary>
    /// Получает активные группы
    /// </summary>
    Task<IEnumerable<Group>> GetActiveGroupsAsync();
    
    /// <summary>
    /// Получает группы по департаменту
    /// </summary>
    Task<IEnumerable<Group>> GetGroupsByDepartmentAsync(Guid departmentUid);
    
    /// <summary>
    /// Создает новую группу
    /// </summary>
    Task<Group> CreateGroupAsync(Group group);
    
    /// <summary>
    /// Добавляет новую группу
    /// </summary>
    Task AddGroupAsync(Group group);
    
    /// <summary>
    /// Обновляет существующую группу
    /// </summary>
    Task<bool> UpdateGroupAsync(Group group);
    
    /// <summary>
    /// Удаляет группу
    /// </summary>
    Task<bool> DeleteGroupAsync(Guid uid);
    
    /// <summary>
    /// Поиск групп по названию
    /// </summary>
    Task<IEnumerable<Group>> SearchGroupsAsync(string searchTerm);
    
    /// <summary>
    /// Получает группы с пагинацией
    /// </summary>
    Task<(IEnumerable<Group> Groups, int TotalCount)> GetGroupsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Guid? departmentUid = null);
    
    /// <summary>
    /// Проверяет существование группы с указанным названием
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeUid = null);
    
    /// <summary>
    /// Получает статистику по группе
    /// </summary>
    Task<GroupStatistics> GetGroupStatisticsAsync(Guid groupUid);
    
    /// <summary>
    /// Назначает куратора группы
    /// </summary>
    Task<bool> AssignCuratorAsync(Guid groupUid, Guid? curatorUid);
    
    /// <summary>
    /// Получает группы по куратору
    /// </summary>
    Task<IEnumerable<Group>> GetGroupsByCuratorAsync(Guid curatorUid);
    
    /// <summary>
    /// Получает все группы (алиас для GetAllGroupsAsync)
    /// </summary>
    Task<IEnumerable<Group>> GetGroupsAsync();
}

/// <summary>
/// Статистика по группе
/// </summary>
public class GroupStatistics
{
    public int StudentsCount { get; set; }
    public int ActiveStudentsCount { get; set; }
    public int GraduatedStudentsCount { get; set; }
    public int ExpelledStudentsCount { get; set; }
    public decimal AverageGrade { get; set; }
    public int TotalEnrollments { get; set; }
}
