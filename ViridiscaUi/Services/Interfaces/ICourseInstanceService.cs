using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с экземплярами курсов
/// </summary>
public interface ICourseInstanceService : IGenericCrudService<CourseInstance>
{
    /// <summary>
    /// Получает экземпляр курса по UID с включением связанных данных
    /// </summary>
    Task<CourseInstance?> GetCourseInstanceAsync(Guid uid);

    /// <summary>
    /// Получает все экземпляры курсов с включением связанных данных
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync();

    /// <summary>
    /// Получает экземпляры курсов по преподавателю
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByTeacherAsync(Guid teacherUid);

    /// <summary>
    /// Получает неназначенные экземпляры курсов
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetUnassignedAsync();

    /// <summary>
    /// Получает экземпляры курсов по UID преподавателя
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetByTeacherUidAsync(Guid teacherUid);

    /// <summary>
    /// Добавляет новый экземпляр курса
    /// </summary>
    Task AddCourseInstanceAsync(CourseInstance courseInstance);

    /// <summary>
    /// Обновляет экземпляр курса
    /// </summary>
    Task<bool> UpdateCourseInstanceAsync(CourseInstance courseInstance);

    /// <summary>
    /// Удаляет экземпляр курса
    /// </summary>
    Task<bool> DeleteCourseInstanceAsync(Guid uid);

    /// <summary>
    /// Активирует экземпляр курса
    /// </summary>
    Task<bool> ActivateCourseInstanceAsync(Guid uid);

    /// <summary>
    /// Завершает экземпляр курса
    /// </summary>
    Task<bool> CompleteCourseInstanceAsync(Guid uid);

    /// <summary>
    /// Получает экземпляры курсов по студенту
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByStudentAsync(Guid studentUid);

    /// <summary>
    /// Получает экземпляры курсов по группе
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByGroupAsync(Guid groupUid);

    /// <summary>
    /// Получает экземпляры курсов по академическому периоду
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByAcademicPeriodAsync(Guid academicPeriodUid);

    /// <summary>
    /// Получает экземпляры курсов по предмету
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetCourseInstancesBySubjectAsync(Guid subjectUid);

    /// <summary>
    /// Записывает студента на курс
    /// </summary>
    Task<bool> EnrollStudentAsync(Guid courseInstanceUid, Guid studentUid);

    /// <summary>
    /// Отписывает студента от курса
    /// </summary>
    Task<bool> UnenrollStudentAsync(Guid courseInstanceUid, Guid studentUid);

    /// <summary>
    /// Получает прогресс студента по курсу
    /// </summary>
    Task<CourseInstanceProgress> GetCourseInstanceProgressAsync(Guid courseInstanceUid, Guid studentUid);

    /// <summary>
    /// Получает статистику по экземпляру курса
    /// </summary>
    Task<CourseInstanceStatistics> GetCourseInstanceStatisticsAsync(Guid courseInstanceUid);

    /// <summary>
    /// Получает экземпляры курсов с пагинацией
    /// </summary>
    Task<(IEnumerable<CourseInstance> CourseInstances, int TotalCount)> GetCourseInstancesPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? subjectFilter = null,
        Guid? teacherFilter = null,
        Guid? groupFilter = null,
        Guid? academicPeriodFilter = null);

    /// <summary>
    /// Получает все экземпляры курсов с фильтрами
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync(
        Guid? subjectFilter = null,
        Guid? teacherFilter = null,
        Guid? groupFilter = null,
        Guid? academicPeriodFilter = null);

    /// <summary>
    /// Клонирует экземпляр курса
    /// </summary>
    Task<CourseInstance?> CloneCourseInstanceAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid);

    /// <summary>
    /// Получает доступные экземпляры курсов для студента
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetAvailableCourseInstancesForStudentAsync(Guid studentUid);

    /// <summary>
    /// Массовая запись группы на курс
    /// </summary>
    Task<BulkEnrollmentResult> BulkEnrollGroupAsync(Guid courseInstanceUid, Guid groupUid);

    /// <summary>
    /// Получает рекомендованные курсы для студента
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetRecommendedCourseInstancesAsync(Guid studentUid);

    /// <summary>
    /// Получает управляемые экземпляры курсов
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetManagedCourseInstancesAsync();

    /// <summary>
    /// Получает все курсы
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetAllCoursesAsync();

    /// <summary>
    /// Получает курсы с пагинацией
    /// </summary>
    Task<(IEnumerable<CourseInstance> Courses, int TotalCount)> GetCoursesPagedAsync(
        int page, int pageSize, string? searchTerm = null);

    /// <summary>
    /// Получает статистику курса
    /// </summary>
    Task<object> GetCourseStatisticsAsync(Guid courseInstanceUid);

    /// <summary>
    /// Получает курсы по студенту
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetCoursesByStudentAsync(Guid studentUid);

    /// <summary>
    /// Получает курс по UID
    /// </summary>
    Task<CourseInstance?> GetCourseAsync(Guid courseInstanceUid);

    /// <summary>
    /// Клонирует курс
    /// </summary>
    Task<CourseInstance?> CloneCourseAsync(Guid sourceUid, Guid? newGroupUid = null, Guid? newAcademicPeriodUid = null);

    /// <summary>
    /// Получает экземпляры курсов по фильтрам
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByFiltersAsync(
        Guid? subjectUid = null,
        Guid? teacherUid = null,
        Guid? groupUid = null,
        Guid? academicPeriodUid = null);
} 