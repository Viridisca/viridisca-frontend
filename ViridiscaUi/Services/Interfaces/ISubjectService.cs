using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с предметами
/// </summary>
public interface ISubjectService
{
    /// <summary>
    /// Получает предмет по идентификатору
    /// </summary>
    Task<Subject?> GetSubjectAsync(Guid uid);

    /// <summary>
    /// Получает все предметы
    /// </summary>
    Task<IEnumerable<Subject>> GetAllSubjectsAsync();

    /// <summary>
    /// Получает активные предметы
    /// </summary>
    Task<IEnumerable<Subject>> GetActiveSubjectsAsync();

    /// <summary>
    /// Получает предметы по департаменту
    /// </summary>
    Task<IEnumerable<Subject>> GetSubjectsByDepartmentAsync(Guid departmentUid);

    /// <summary>
    /// Создает новый предмет
    /// </summary>
    Task<Subject> CreateSubjectAsync(Subject subject);

    /// <summary>
    /// Добавляет новый предмет
    /// </summary>
    Task AddSubjectAsync(Subject subject);

    /// <summary>
    /// Обновляет существующий предмет
    /// </summary>
    Task<bool> UpdateSubjectAsync(Subject subject);

    /// <summary>
    /// Удаляет предмет
    /// </summary>
    Task<bool> DeleteSubjectAsync(Guid uid);

    /// <summary>
    /// Поиск предметов по названию или коду
    /// </summary>
    Task<IEnumerable<Subject>> SearchSubjectsAsync(string searchTerm);

    /// <summary>
    /// Получает предметы с пагинацией
    /// </summary>
    Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetSubjectsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        Guid? departmentUid = null);

    /// <summary>
    /// Проверяет существование предмета с указанным кодом
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null);

    /// <summary>
    /// Получает статистику по предмету
    /// </summary>
    Task<SubjectStatistics> GetSubjectStatisticsAsync(Guid subjectUid);

    /// <summary>
    /// Активирует/деактивирует предмет
    /// </summary>
    Task<bool> SetSubjectActiveStatusAsync(Guid uid, bool isActive);

    /// <summary>
    /// Получает предметы с пагинацией
    /// </summary>
    Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetPagedAsync(
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null, 
        Guid? departmentUid = null);

    /// <summary>
    /// Создает новый предмет
    /// </summary>
    Task<Subject> CreateAsync(Subject subject);

    /// <summary>
    /// Получает предмет по идентификатору
    /// </summary>
    Task<Subject?> GetByUidAsync(Guid uid);

    /// <summary>
    /// Обновляет предмет
    /// </summary>
    Task<Subject> UpdateAsync(Subject subject);

    /// <summary>
    /// Удаляет предмет
    /// </summary>
    Task<bool> DeleteAsync(Guid uid);

    /// <summary>
    /// Получает предмет по коду
    /// </summary>
    Task<Subject?> GetByCodeAsync(string code);

    /// <summary>
    /// Получает количество экземпляров курсов для предмета
    /// </summary>
    Task<int> GetCourseInstancesCountAsync(Guid subjectUid);

    /// <summary>
    /// Получает количество предметов в учебных планах
    /// </summary>
    Task<int> GetCurriculumSubjectsCountAsync(Guid subjectUid);

    /// <summary>
    /// Получает информацию о связанных данных предмета для безопасного удаления
    /// </summary>
    Task<SubjectRelatedDataInfo> GetSubjectRelatedDataInfoAsync(Guid subjectUid);

    /// <summary>
    /// Получает предмет по названию
    /// </summary>
    Task<Subject?> GetByNameAsync(string name);

    /// <summary>
    /// Получает общее количество предметов
    /// </summary>
    Task<int> GetTotalCountAsync();

    /// <summary>
    /// Получает курсы предмета
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetSubjectCoursesAsync(Guid subjectUid);

    /// <summary>
    /// Получает учебные планы предмета
    /// </summary>
    Task<IEnumerable<Curriculum>> GetSubjectCurriculaAsync(Guid subjectUid);

    /// <summary>
    /// Получает задания предмета
    /// </summary>
    Task<IEnumerable<Assignment>> GetSubjectAssignmentsAsync(Guid subjectUid);
}

/// <summary>
/// Статистика по предмету
/// </summary>
public class SubjectStatistics
{
    public int TeachersCount { get; set; }
    public int CoursesCount { get; set; }
    public int StudentsCount { get; set; }
    public int ActiveCoursesCount { get; set; }
    public decimal AverageGrade { get; set; }
} 