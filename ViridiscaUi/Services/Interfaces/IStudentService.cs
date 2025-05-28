using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы со студентами
/// Наследуется от IGenericCrudService для получения универсальных CRUD операций
/// </summary>
public interface IStudentService : IGenericCrudService<Student>
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
    /// Получает студентов по статусу
    /// </summary>
    Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentStatus status);
    
    /// <summary>
    /// Создает нового студента
    /// </summary>
    Task<Student> CreateStudentAsync(Student student);
    
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

    /// <summary>
    /// Поиск студентов по имени или email
    /// </summary>
    Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm);

    /// <summary>
    /// Получает студентов с пагинацией
    /// </summary>
    Task<(IEnumerable<Student> Students, int TotalCount)> GetStudentsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        StudentStatus? status = null,
        Guid? groupUid = null);

    /// <summary>
    /// Проверяет существование студента с указанным email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeUid = null);

    /// <summary>
    /// Получает статистику по студенту
    /// </summary>
    Task<StudentStatistics> GetStudentStatisticsAsync(Guid studentUid);

    /// <summary>
    /// Изменяет статус студента
    /// </summary>
    Task<bool> ChangeStudentStatusAsync(Guid uid, StudentStatus status);

    /// <summary>
    /// Переводит студента в другую группу
    /// </summary>
    Task<bool> TransferStudentToGroupAsync(Guid studentUid, Guid? newGroupUid);

    // === РАСШИРЕНИЯ ЭТАПА 1 ===
    
    /// <summary>
    /// Получает успеваемость студента
    /// </summary>
    Task<StudentPerformance> GetStudentPerformanceAsync(Guid studentUid);
    
    /// <summary>
    /// Назначает студента в группу
    /// </summary>
    Task<bool> AssignToGroupAsync(Guid studentUid, Guid groupUid);
    
    /// <summary>
    /// Получает курсы студента
    /// </summary>
    Task<IEnumerable<Course>> GetStudentCoursesAsync(Guid studentUid);
    
    /// <summary>
    /// Получает оценки студента
    /// </summary>
    Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid);
    
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
    public int EnrollmentsCount { get; set; }
    public int CompletedCoursesCount { get; set; }
    public int ActiveCoursesCount { get; set; }
    public decimal AverageGrade { get; set; }
    public int TotalCredits { get; set; }
    public int CompletedCredits { get; set; }
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
