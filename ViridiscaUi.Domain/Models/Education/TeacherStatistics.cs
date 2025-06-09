namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Статистика преподавателя
/// </summary>
public class TeacherStatistics
{
    /// <summary>
    /// Общее количество курсов
    /// </summary>
    public int TotalCourses { get; set; }

    /// <summary>
    /// Количество активных курсов
    /// </summary>
    public int ActiveCourses { get; set; }

    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Средняя оценка
    /// </summary>
    public double AverageGrade { get; set; }

    /// <summary>
    /// Общее количество выставленных оценок
    /// </summary>
    public int TotalGrades { get; set; }

    /// <summary>
    /// Общее количество заданий
    /// </summary>
    public int TotalAssignments { get; set; }

    /// <summary>
    /// Количество оценок, ожидающих проверки
    /// </summary>
    public int PendingGrades { get; set; }

    /// <summary>
    /// Процент завершенности курсов
    /// </summary>
    public double CompletionRate { get; set; }
    
    /// <summary>
    /// Количество курируемых групп
    /// </summary>
    public int CuratedGroups { get; set; }
    
    /// <summary>
    /// Количество завершенных курсов
    /// </summary>
    public int CompletedCourses { get; set; }
    
    /// <summary>
    /// Количество заданий, ожидающих проверки
    /// </summary>
    public int PendingAssignments { get; set; }
} 