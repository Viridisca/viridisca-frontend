using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы со студентами
    /// </summary>
    public interface IStudentService
    {
        /// <summary>
        /// Получает студента по идентификатору
        /// </summary>
        Task<Student?> GetStudentAsync(Guid uid);
        
        /// <summary>
        /// Получает всех студентов
        /// </summary>
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        
        /// <summary>
        /// Получает всех студентов (алиас для GetAllStudentsAsync)
        /// </summary>
        Task<IEnumerable<Student>> GetStudentsAsync();
        
        /// <summary>
        /// Получает студентов по идентификатору группы
        /// </summary>
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid);
        
        /// <summary>
        /// Добавляет нового студента
        /// </summary>
        Task AddStudentAsync(Student student);
        
        /// <summary>
        /// Обновляет существующего студента
        /// </summary>
        Task<bool> UpdateStudentAsync(Student student);
        
        /// <summary>
        /// Удаляет студента
        /// </summary>
        Task<bool> DeleteStudentAsync(Guid uid);

        // === РАСШИРЕНИЯ ЭТАПА 1 ===
        
        /// <summary>
        /// Получает успеваемость студента
        /// </summary>
        Task<StudentPerformance> GetStudentPerformanceAsync(Guid studentUid);
        
        /// <summary>
        /// Переводит студента в другую группу
        /// </summary>
        Task<bool> TransferStudentToGroupAsync(Guid studentUid, Guid newGroupUid);
        
        /// <summary>
        /// Получает курсы студента
        /// </summary>
        Task<IEnumerable<Course>> GetStudentCoursesAsync(Guid studentUid);
        
        /// <summary>
        /// Получает оценки студента
        /// </summary>
        Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid);
        
        /// <summary>
        /// Получает студентов с пагинацией
        /// </summary>
        Task<(IEnumerable<Student> Students, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, string? searchTerm = null);
        
        /// <summary>
        /// Получает статистику студента
        /// </summary>
        Task<StudentStatistics> GetStudentStatisticsAsync(Guid studentUid);
        
        /// <summary>
        /// Получает активные задания студента
        /// </summary>
        Task<IEnumerable<Assignment>> GetStudentActiveAssignmentsAsync(Guid studentUid);
        
        /// <summary>
        /// Получает просроченные задания студента
        /// </summary>
        Task<IEnumerable<Assignment>> GetStudentOverdueAssignmentsAsync(Guid studentUid);
    }

    /// <summary>
    /// Успеваемость студента
    /// </summary>
    public class StudentPerformance
    {
        public Guid StudentUid { get; set; }
        public double AverageGrade { get; set; }
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int OverdueAssignments { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public Dictionary<string, double> GradesBySubject { get; set; } = new();
    }

    /// <summary>
    /// Статистика студента
    /// </summary>
    public class StudentStatistics
    {
        public Guid StudentUid { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public double AverageScore { get; set; }
        public TimeSpan TotalStudyTime { get; set; }
        public int AttendanceRate { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public Dictionary<string, int> AssignmentsByType { get; set; } = new();
        public List<MonthlyProgress> MonthlyProgress { get; set; } = new();
    }

    /// <summary>
    /// Месячный прогресс студента
    /// </summary>
    public class MonthlyProgress
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double AverageGrade { get; set; }
        public int CompletedAssignments { get; set; }
        public int TotalAssignments { get; set; }
    }
} 