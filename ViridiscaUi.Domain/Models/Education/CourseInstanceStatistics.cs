using System;
using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Статистика экземпляра курса
/// </summary>
public class CourseInstanceStatistics
{
    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Количество активных студентов
    /// </summary>
    public int ActiveStudents { get; set; }

    /// <summary>
    /// Количество студентов, завершивших курс
    /// </summary>
    public int CompletedStudents { get; set; }

    /// <summary>
    /// Средняя оценка по курсу
    /// </summary>
    public double AverageGrade { get; set; }

    /// <summary>
    /// Средний процент завершения
    /// </summary>
    public double AverageCompletionRate { get; set; }

    /// <summary>
    /// Общее количество занятий
    /// </summary>
    public int TotalLessons { get; set; }

    /// <summary>
    /// Общее количество заданий
    /// </summary>
    public int TotalAssignments { get; set; }

    /// <summary>
    /// Дата последней активности
    /// </summary>
    public DateTime? LastActivityDate { get; set; }

    /// <summary>
    /// Среднее время завершения курса
    /// </summary>
    public TimeSpan AverageTimeToComplete { get; set; }

    /// <summary>
    /// Распределение оценок
    /// </summary>
    public Dictionary<string, int> GradeDistribution { get; set; } = new();
} 