using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Статистика преподавателя
/// </summary>
public class TeacherStatistics : ViewModelBase
{
    private int _totalCourses;
    private int _activeCourses;
    private int _totalStudents;

    private double _averageGrade;
    private double _completionRate;
    
    private int _totalGrades;
    private int _totalAssignments;
    private int _pendingGrades;

    /// <summary>
    /// Общее количество курсов
    /// </summary>
    public int TotalCourses
    {
        get => _totalCourses;
        set => this.RaiseAndSetIfChanged(ref _totalCourses, value);
    }

    /// <summary>
    /// Количество активных курсов
    /// </summary>
    public int ActiveCourses
    {
        get => _activeCourses;
        set => this.RaiseAndSetIfChanged(ref _activeCourses, value);
    }

    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents
    {
        get => _totalStudents;
        set => this.RaiseAndSetIfChanged(ref _totalStudents, value);
    }

    /// <summary>
    /// Средняя оценка
    /// </summary>
    public double AverageGrade
    {
        get => _averageGrade;
        set => this.RaiseAndSetIfChanged(ref _averageGrade, value);
    }

    /// <summary>
    /// Общее количество выставленных оценок
    /// </summary>
    public int TotalGrades
    {
        get => _totalGrades;
        set => this.RaiseAndSetIfChanged(ref _totalGrades, value);
    }

    /// <summary>
    /// Общее количество заданий
    /// </summary>
    public int TotalAssignments
    {
        get => _totalAssignments;
        set => this.RaiseAndSetIfChanged(ref _totalAssignments, value);
    }

    /// <summary>
    /// Количество оценок, ожидающих проверки
    /// </summary>
    public int PendingGrades
    {
        get => _pendingGrades;
        set => this.RaiseAndSetIfChanged(ref _pendingGrades, value);
    }

    /// <summary>
    /// Процент завершенности курсов
    /// </summary>
    public double CompletionRate
    {
        get => _completionRate;
        set => this.RaiseAndSetIfChanged(ref _completionRate, value);
    }

    /// <summary>
    /// Отформатированная средняя оценка
    /// </summary>
    public string FormattedAverageGrade => AverageGrade.ToString("F2");

    /// <summary>
    /// Отформатированный процент завершенности
    /// </summary>
    public string FormattedCompletionRate => $"{CompletionRate:F1}%";

    /// <summary>
    /// Создает новый экземпляр статистики преподавателя
    /// </summary>
    public TeacherStatistics()
    {
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр статистики преподавателя с указанными параметрами
    /// </summary>
    public TeacherStatistics(
        int totalCourses,
        int activeCourses,
        int totalStudents,
        double averageGrade,
        int totalGrades = 0,
        int totalAssignments = 0,
        int pendingGrades = 0,
        double completionRate = 0.0)
    {
        _totalCourses = totalCourses;
        _activeCourses = activeCourses;
        _totalStudents = totalStudents;
        _averageGrade = averageGrade;
        _totalGrades = totalGrades;
        _totalAssignments = totalAssignments;
        _pendingGrades = pendingGrades;
        _completionRate = completionRate;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 