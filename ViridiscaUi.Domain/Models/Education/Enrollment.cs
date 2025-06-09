using System;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Запись студента на курс
/// </summary>
public class Enrollment : AuditableEntity
{
    /// <summary>
    /// ID студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Дата записи
    /// </summary>
    public DateTime EnrollmentDate { get; set; }

    /// <summary>
    /// Дата завершения
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Дата завершения (алиас для совместимости)
    /// </summary>
    public DateTime? CompletedAt 
    { 
        get => CompletionDate; 
        set => CompletionDate = value; 
    }
    
    /// <summary>
    /// Статус записи
    /// </summary>
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    /// <summary>
    /// Итоговая оценка
    /// </summary>
    public decimal? FinalGrade { get; set; }

    /// <summary>
    /// Заметки
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Флаг удаления (мягкое удаление)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Дата удаления
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }
}
