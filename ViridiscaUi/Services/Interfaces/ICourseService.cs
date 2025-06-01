using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с экземплярами курсов (CourseInstance)
    /// Наследуется от IGenericCrudService для получения универсальных CRUD операций
    /// </summary>
    public interface ICourseInstanceService : IGenericCrudService<CourseInstance>
    {
        /// <summary>
        /// Получает экземпляр курса по идентификатору
        /// </summary>
        Task<CourseInstance?> GetCourseInstanceAsync(Guid uid);
        
        /// <summary>
        /// Получает все экземпляры курсов
        /// </summary>
        Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync();
        
        /// <summary>
        /// Получает экземпляры курсов преподавателя
        /// </summary>
        Task<IEnumerable<CourseInstance>> GetCourseInstancesByTeacherAsync(Guid teacherUid);
        
        /// <summary>
        /// Добавляет новый экземпляр курса
        /// </summary>
        Task AddCourseInstanceAsync(CourseInstance courseInstance);
        
        /// <summary>
        /// Обновляет существующий экземпляр курса
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

        // === РАСШИРЕНИЯ ЭТАПА 2 ===
        
        /// <summary>
        /// Получает экземпляры курсов студента
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
        /// Регистрирует студента на экземпляр курса
        /// </summary>
        Task<bool> EnrollStudentAsync(Guid courseInstanceUid, Guid studentUid);
        
        /// <summary>
        /// Отменяет регистрацию студента на экземпляр курса
        /// </summary>
        Task<bool> UnenrollStudentAsync(Guid courseInstanceUid, Guid studentUid);
        
        /// <summary>
        /// Получает прогресс студента по экземпляру курса
        /// </summary>
        Task<CourseInstanceProgress> GetCourseInstanceProgressAsync(Guid courseInstanceUid, Guid studentUid);
        
        /// <summary>
        /// Получает статистику экземпляра курса
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
        /// Доступные экземпляры курсов для регистрации студента
        /// </summary>
        Task<IEnumerable<CourseInstance>> GetAvailableCourseInstancesForStudentAsync(Guid studentUid);
        
        /// <summary>
        /// Массовая регистрация студентов группы на экземпляр курса
        /// </summary>
        Task<BulkEnrollmentResult> BulkEnrollGroupAsync(Guid courseInstanceUid, Guid groupUid);
        
        /// <summary>
        /// Получает рекомендованные экземпляры курсов для студента
        /// </summary>
        Task<IEnumerable<CourseInstance>> GetRecommendedCourseInstancesAsync(Guid studentUid);

        /// <summary>
        /// Получает управляемые экземпляры курсов (для академических руководителей)
        /// </summary>
        Task<IEnumerable<CourseInstance>> GetManagedCourseInstancesAsync();

        // === ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ДЛЯ СОВМЕСТИМОСТИ ===
        
        /// <summary>
        /// Получает все экземпляры курсов (алиас)
        /// </summary>
        Task<IEnumerable<CourseInstance>> GetAllCoursesAsync();

        /// <summary>
        /// Получает экземпляры курсов с пагинацией (алиас)
        /// </summary>
        Task<(IEnumerable<CourseInstance> Courses, int TotalCount)> GetCoursesPagedAsync(
            int page, int pageSize, string? searchTerm = null);

        /// <summary>
        /// Получает статистику курса (алиас)
        /// </summary>
        Task<object> GetCourseStatisticsAsync(Guid courseInstanceUid);

        /// <summary>
        /// Получает курсы студента (алиас)
        /// </summary>
        Task<IEnumerable<CourseInstance>> GetCoursesByStudentAsync(Guid studentUid);

        /// <summary>
        /// Получает экземпляр курса по ID (алиас)
        /// </summary>
        Task<CourseInstance?> GetCourseAsync(Guid courseInstanceUid);

        /// <summary>
        /// Клонирует экземпляр курса (алиас)
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

    /// <summary>
    /// Прогресс студента по экземпляру курса
    /// </summary>
    public class CourseInstanceProgress
    {
        public Guid CourseInstanceUid { get; set; }
        public Guid StudentUid { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedAssignments { get; set; }
        public int TotalAssignments { get; set; }
        public double AverageGrade { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime EnrolledAt { get; set; }
        public TimeSpan TotalTimeSpent { get; set; }
    }

    /// <summary>
    /// Статистика экземпляра курса
    /// </summary>
    public class CourseInstanceStatistics
    {
        public Guid CourseInstanceUid { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int CompletedStudents { get; set; }
        public double AverageGrade { get; set; }
        public double AverageCompletionRate { get; set; }
        public int TotalLessons { get; set; }
        public int TotalAssignments { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public TimeSpan AverageTimeToComplete { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new();
    }

    /// <summary>
    /// Результат массовой регистрации
    /// </summary>
    public class BulkEnrollmentResult
    {
        public int SuccessfulEnrollments { get; set; }
        public int FailedEnrollments { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<Guid> EnrolledStudentUids { get; set; } = new();
    }
} 