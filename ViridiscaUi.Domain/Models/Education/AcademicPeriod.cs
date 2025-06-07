using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Академический период (семестр, четверть, триместр)
/// </summary>
public class AcademicPeriod : AuditableEntity
{
    /// <summary>
    /// Название периода
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Код периода
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Описание периода
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Тип периода
    /// </summary>
    public AcademicPeriodType Type { get; set; }

    /// <summary>
    /// Дата начала
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Активен ли период
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Текущий ли период
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Учебный год
    /// </summary>
    public int AcademicYear { get; set; }

    /// <summary>
    /// Экземпляры курсов в этом периоде
    /// </summary>
    public ICollection<CourseInstance> CourseInstances { get; set; } = new List<CourseInstance>();

    /// <summary>
    /// Экзамены в этом периоде
    /// </summary>
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();

    /// <summary>
    /// Продолжительность в днях
    /// </summary>
    public int DurationInDays => (EndDate - StartDate).Days;

    /// <summary>
    /// Завершенный ли период
    /// </summary>
    public bool IsCompleted => DateTime.Now > EndDate;
} 