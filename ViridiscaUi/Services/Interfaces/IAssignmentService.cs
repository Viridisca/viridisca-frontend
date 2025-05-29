using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с заданиями
/// Наследуется от IGenericCrudService для получения универсальных CRUD операций
/// </summary>
public interface IAssignmentService : IGenericCrudService<Assignment>
{
    /// <summary>
    /// Получает задание по идентификатору
    /// </summary>
    Task<Assignment?> GetAssignmentAsync(Guid uid);
    
    /// <summary>
    /// Получает все задания
    /// </summary>
    Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
    
    /// <summary>
    /// Получает все задания (алиас для GetAllAssignmentsAsync)
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsAsync();
    
    /// <summary>
    /// Получает задания по курсу
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(Guid courseUid);
    
    /// <summary>
    /// Получает задания по статусу
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsByStatusAsync(AssignmentStatus status);
    
    /// <summary>
    /// Получает задания по типу
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsByTypeAsync(AssignmentType type);
    
    /// <summary>
    /// Создает новое задание
    /// </summary>
    Task<Assignment> CreateAssignmentAsync(Assignment assignment);
    
    /// <summary>
    /// Добавляет новое задание
    /// </summary>
    Task AddAssignmentAsync(Assignment assignment);
    
    /// <summary>
    /// Обновляет существующее задание
    /// </summary>
    Task<bool> UpdateAssignmentAsync(Assignment assignment);
    
    /// <summary>
    /// Удаляет задание
    /// </summary>
    Task<bool> DeleteAssignmentAsync(Guid uid);
    
    /// <summary>
    /// Публикует задание
    /// </summary>
    Task<bool> PublishAssignmentAsync(Guid uid);

    // === РАСШИРЕНИЯ ЭТАПА 3 ===
    
    /// <summary>
    /// Получает задания студента
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsByStudentAsync(Guid studentUid);
    
    /// <summary>
    /// Получает задания преподавателя
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherUid);
    
    /// <summary>
    /// Получает сдачи задания
    /// </summary>
    Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid);
    
    /// <summary>
    /// Получает сдачи студента
    /// </summary>
    Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(Guid studentUid);
    
    /// <summary>
    /// Создает сдачу задания
    /// </summary>
    Task<Submission> CreateSubmissionAsync(Submission submission);
    
    /// <summary>
    /// Обновляет сдачу задания
    /// </summary>
    Task<bool> UpdateSubmissionAsync(Submission submission);
    
    /// <summary>
    /// Оценивает сдачу задания
    /// </summary>
    Task<bool> GradeSubmissionAsync(Guid submissionUid, double score, string? feedback = null, Guid? gradedByUid = null);
    
    /// <summary>
    /// Получает статистику задания
    /// </summary>
    Task<AssignmentStatistics> GetAssignmentStatisticsAsync(Guid assignmentUid);
    
    /// <summary>
    /// Получает прогресс студента по заданию
    /// </summary>
    Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentUid, Guid studentUid);
    
    /// <summary>
    /// Получает задания с пагинацией
    /// </summary>
    Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetAssignmentsPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        AssignmentStatus? statusFilter = null,
        Guid? courseFilter = null,
        Guid? teacherFilter = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null);
    
    /// <summary>
    /// Получает просроченные задания
    /// </summary>
    Task<IEnumerable<Assignment>> GetOverdueAssignmentsAsync();
    
    /// <summary>
    /// Получает задания, требующие проверки
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsPendingGradingAsync(Guid? teacherUid = null);
    
    /// <summary>
    /// Массовое оценивание сдач
    /// </summary>
    Task<BulkGradingResult> BulkGradeSubmissionsAsync(IEnumerable<GradingRequest> gradingRequests);
    
    /// <summary>
    /// Отправляет напоминание о сроке сдачи
    /// </summary>
    Task SendDueDateReminderAsync(Guid assignmentUid);
    
    /// <summary>
    /// Получает аналитику по заданиям
    /// </summary>
    Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid? courseUid = null, DateTime? fromDate = null, DateTime? toDate = null);
}

/// <summary>
/// Статистика задания
/// </summary>
public class AssignmentStatistics
{
    public Guid AssignmentUid { get; set; }
    public int TotalStudents { get; set; }
    public int SubmittedCount { get; set; }
    public int GradedCount { get; set; }
    public int PendingCount { get; set; }
    public int OverdueCount { get; set; }
    public double AverageScore { get; set; }
    public double SubmissionRate { get; set; }
    public TimeSpan AverageSubmissionTime { get; set; }
    public DateTime? FirstSubmissionDate { get; set; }
    public DateTime? LastSubmissionDate { get; set; }
    public Dictionary<string, int> ScoreDistribution { get; set; } = new();
}

/// <summary>
/// Прогресс студента по заданию
/// </summary>
public class AssignmentProgress
{
    public Guid AssignmentUid { get; set; }
    public Guid StudentUid { get; set; }
    public AssignmentProgressStatus Status { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public double? Score { get; set; }
    public string? Feedback { get; set; }
    public TimeSpan? TimeSpent { get; set; }
    public int AttemptsCount { get; set; }
    public bool IsOverdue { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}

/// <summary>
/// Запрос на оценивание
/// </summary>
public class GradingRequest
{
    public Guid SubmissionUid { get; set; }
    public double Score { get; set; }
    public string? Feedback { get; set; }
    public Guid GradedByUid { get; set; }
}

/// <summary>
/// Результат массового оценивания
/// </summary>
public class BulkGradingResult
{
    public int SuccessfulGradings { get; set; }
    public int FailedGradings { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Guid> GradedSubmissionUids { get; set; } = new();
}

/// <summary>
/// Аналитика заданий
/// </summary>
public class AssignmentAnalytics
{
    public int TotalAssignments { get; set; }
    public int PublishedAssignments { get; set; }
    public int DraftAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double AverageScore { get; set; }
    public double AverageSubmissionRate { get; set; }
    public TimeSpan AverageCompletionTime { get; set; }
    public Dictionary<string, int> AssignmentsByType { get; set; } = new();
    public Dictionary<string, double> ScoresByDifficulty { get; set; } = new();
    public List<AssignmentPerformanceData> PerformanceData { get; set; } = new();
}

/// <summary>
/// Данные производительности задания
/// </summary>
public class AssignmentPerformanceData
{
    public Guid AssignmentUid { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public double SubmissionRate { get; set; }
    public TimeSpan AverageCompletionTime { get; set; }
    public int TotalSubmissions { get; set; }
}

/// <summary>
/// Статус прогресса задания
/// </summary>
public enum AssignmentProgressStatus
{
    NotStarted,     // Не начато
    InProgress,     // В процессе
    Submitted,      // Сдано
    Graded,         // Оценено
    Overdue,        // Просрочено
    Resubmitted     // Пересдано
}
