using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с преподавателями
/// Наследуется от IGenericCrudService для получения универсальных CRUD операций
/// </summary>
public interface ITeacherService : IGenericCrudService<Teacher>
{
    /// <summary>
    /// Получает преподавателя по идентификатору
    /// </summary>
    Task<Teacher?> GetTeacherAsync(Guid uid);

    /// <summary>
    /// Получает всех преподавателей
    /// </summary>
    Task<IEnumerable<Teacher>> GetAllTeachersAsync();

    /// <summary>
    /// Получает всех преподавателей (алиас для GetAllTeachersAsync)
    /// </summary>
    Task<IEnumerable<Teacher>> GetTeachersAsync();

    /// <summary>
    /// Создает нового преподавателя
    /// </summary>
    Task<Teacher> CreateTeacherAsync(Teacher teacher);

    /// <summary>
    /// Добавляет нового преподавателя
    /// </summary>
    Task AddTeacherAsync(Teacher teacher);

    /// <summary>
    /// Обновляет существующего преподавателя
    /// </summary>
    Task<bool> UpdateTeacherAsync(Teacher teacher);

    /// <summary>
    /// Удаляет преподавателя
    /// </summary>
    Task<bool> DeleteTeacherAsync(Guid uid);

    /// <summary>
    /// Назначает преподавателя на курс
    /// </summary>
    Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid);

    /// <summary>
    /// Получает преподавателей с пагинацией
    /// </summary>
    Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetTeachersPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? specializationFilter = null,
        string? statusFilter = null);

    /// <summary>
    /// Получает статистику преподавателя
    /// </summary>
    Task<TeacherStatistics> GetTeacherStatisticsAsync(Guid teacherUid);

    /// <summary>
    /// Получает курсы преподавателя
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetTeacherCoursesAsync(Guid teacherUid);

    /// <summary>
    /// Получает группы преподавателя
    /// </summary>
    Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid);

    /// <summary>
    /// Назначает преподавателя на группу
    /// </summary>
    Task<bool> AssignToGroupAsync(Guid teacherUid, Guid groupUid);

    /// <summary>
    /// Отменяет назначение преподавателя на группу
    /// </summary>
    Task<bool> UnassignFromGroupAsync(Guid teacherUid, Guid groupUid);

    /// <summary>
    /// Отменяет назначение преподавателя на курс
    /// </summary>
    Task<bool> UnassignFromCourseAsync(Guid teacherUid, Guid courseUid);

    /// <summary>
    /// Получает доступных кураторов
    /// </summary>
    Task<IEnumerable<Teacher>> GetAvailableCuratorsAsync();

    /// <summary>
    /// Получает доступных кураторов для назначения группе
    /// </summary>
    Task<IEnumerable<Teacher>> GetAvailableCuratorsForGroupAsync(Guid groupUid);

    /// <summary>
    /// Проверяет существование преподавателя по email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Проверяет существование преподавателя по email (с исключением определенного UID)
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, Guid excludeUid);

    /// <summary>
    /// Получает группы, которые курирует преподаватель
    /// </summary>
    Task<IEnumerable<Group>> GetCuratedGroupsAsync(Guid teacherUid);

    /// <summary>
    /// Экспортирует данные преподавателей
    /// </summary>
    Task<string> ExportTeachersAsync(IEnumerable<Teacher> teachers, string format = "xlsx");

    /// <summary>
    /// Получает информацию о связанных данных преподавателя для безопасного удаления
    /// </summary>
    Task<TeacherRelatedDataInfo> GetTeacherRelatedDataInfoAsync(Guid teacherUid);
}