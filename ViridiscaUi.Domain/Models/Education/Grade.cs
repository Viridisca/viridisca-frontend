using System;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Оценка студента
/// </summary>
public class Grade : AuditableEntity
{
    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid SubjectUid { get; set; }

    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Идентификатор преподавателя, выставившего оценку
    /// </summary>
    public Guid TeacherUid { get; set; }

    /// <summary>
    /// Идентификатор задания (опционально)
    /// </summary>
    public Guid? AssignmentUid { get; set; }

    /// <summary>
    /// Идентификатор экзамена (опционально)
    /// </summary>
    public Guid? ExamUid { get; set; }
    
    /// <summary>
    /// Значение оценки
    /// </summary>
    public decimal Value { get; set; }
    
    /// <summary>
    /// Комментарий к оценке
    /// </summary>
    public string? Comment { get; set; }
    
    /// <summary>
    /// Тип оценки
    /// </summary>
    public GradeType Type { get; set; }
    
    /// <summary>
    /// Описание оценки
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Дата выставления оценки
    /// </summary>
    public DateTime IssuedAt { get; set; }
    
    /// <summary>
    /// Опубликована ли оценка для студента
    /// </summary>
    public bool IsPublished { get; set; }
    
    /// <summary>
    /// Дата публикации оценки
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Максимальное значение для данной оценки
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Вес оценки при расчете итоговой оценки
    /// </summary>
    public decimal Weight { get; set; } = 1.0m;

    /// <summary>
    /// Студент, получивший оценку
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Предмет, по которому выставлена оценка
    /// </summary>
    public Subject? Subject { get; set; }

    /// <summary>
    /// Экземпляр курса, по которому выставлена оценка
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }
    
    /// <summary>
    /// Задание, за которое выставлена оценка (если применимо)
    /// </summary>
    public Assignment? Assignment { get; set; }

    /// <summary>
    /// Преподаватель, выставивший оценку
    /// </summary>
    public Teacher? Teacher { get; set; }

    /// <summary>
    /// Связанный экзамен (если применимо)
    /// </summary>
    public Exam? Exam { get; set; }
} 