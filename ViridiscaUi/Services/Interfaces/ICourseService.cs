using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using CourseStatus = ViridiscaUi.Domain.Models.Education.CourseStatus;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с курсами
    /// Наследуется от IGenericCrudService для получения универсальных CRUD операций
    /// </summary>
    public interface ICourseService : IGenericCrudService<Course>
    {
        /// <summary>
        /// Получает курс по идентификатору
        /// </summary>
        Task<Course?> GetCourseAsync(Guid uid);
        
        /// <summary>
        /// Получает все курсы
        /// </summary>
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        
        /// <summary>
        /// Получает курсы преподавателя
        /// </summary>
        Task<IEnumerable<Course>> GetCoursesByTeacherAsync(Guid teacherUid);
        
        /// <summary>
        /// Добавляет новый курс
        /// </summary>
        Task AddCourseAsync(Course course);
        
        /// <summary>
        /// Обновляет существующий курс
        /// </summary>
        Task<bool> UpdateCourseAsync(Course course);
        
        /// <summary>
        /// Удаляет курс
        /// </summary>
        Task<bool> DeleteCourseAsync(Guid uid);
        
        /// <summary>
        /// Публикует курс (изменяет статус)
        /// </summary>
        Task<bool> PublishCourseAsync(Guid uid);
        
        /// <summary>
        /// Архивирует курс (изменяет статус)
        /// </summary>
        Task<bool> ArchiveCourseAsync(Guid uid);

        // === РАСШИРЕНИЯ ЭТАПА 2 ===
        
        /// <summary>
        /// Получает курсы студента
        /// </summary>
        Task<IEnumerable<Course>> GetCoursesByStudentAsync(Guid studentUid);
        
        /// <summary>
        /// Регистрирует студента на курс
        /// </summary>
        Task<bool> EnrollStudentAsync(Guid courseUid, Guid studentUid);
        
        /// <summary>
        /// Отменяет регистрацию студента на курс
        /// </summary>
        Task<bool> UnenrollStudentAsync(Guid courseUid, Guid studentUid);
        
        /// <summary>
        /// Получает прогресс студента по курсу
        /// </summary>
        Task<CourseProgress> GetCourseProgressAsync(Guid courseUid, Guid studentUid);
        
        /// <summary>
        /// Получает статистику курса
        /// </summary>
        Task<CourseStatistics> GetCourseStatisticsAsync(Guid courseUid);
        
        /// <summary>
        /// Получает курсы с пагинацией
        /// </summary>
        Task<(IEnumerable<Course> Courses, int TotalCount)> GetCoursesPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null,
            string? categoryFilter = null,
            string? statusFilter = null,
            string? difficultyFilter = null,
            Guid? teacherFilter = null);
        
        /// <summary>
        /// Получает все курсы с фильтрами
        /// </summary>
        Task<IEnumerable<Course>> GetAllCoursesAsync(
            string? categoryFilter = null,
            string? statusFilter = null,
            string? difficultyFilter = null);
        
        /// <summary>
        /// Клонирует курс
        /// </summary>
        Task<Course?> CloneCourseAsync(Guid courseUid, string newName);
        
        /// <summary>
        /// Доступные курсы для регистрации студента
        /// </summary>
        Task<IEnumerable<Course>> GetAvailableCoursesForStudentAsync(Guid studentUid);
        
        /// <summary>
        /// Массовая регистрация студентов группы на курс
        /// </summary>
        Task<BulkEnrollmentResult> BulkEnrollGroupAsync(Guid courseUid, Guid groupUid);
        
        /// <summary>
        /// Получает рекомендованные курсы для студента
        /// </summary>
        Task<IEnumerable<Course>> GetRecommendedCoursesAsync(Guid studentUid);

        /// <summary>
        /// Получает управляемые курсы (для академических руководителей)
        /// </summary>
        Task<IEnumerable<Course>> GetManagedCoursesAsync();
    }

    /// <summary>
    /// Прогресс студента по курсу
    /// </summary>
    public class CourseProgress
    {
        public Guid CourseUid { get; set; }
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
    /// Статистика курса
    /// </summary>
    public class CourseStatistics
    {
        public Guid CourseUid { get; set; }
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