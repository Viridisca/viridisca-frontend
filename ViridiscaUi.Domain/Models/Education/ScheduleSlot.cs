using System;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Слот расписания - конкретное время проведения урока
/// </summary>
public class ScheduleSlot : AuditableEntity
{
    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// День недели
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Время начала
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Время окончания
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Аудитория/комната
    /// </summary>
    public string? Room { get; set; }

    /// <summary>
    /// Активен ли слот
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Действует с (дата)
    /// </summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Действует до (дата)
    /// </summary>
    public DateTime EffectiveTo { get; set; }
    
    /// <summary>
    /// Период действия с (для совместимости)
    /// </summary>
    public DateTime ValidFrom { get; set; }
    
    /// <summary>
    /// Период действия до (для совместимости)
    /// </summary>
    public DateTime ValidTo { get; set; }
    
    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }
}
