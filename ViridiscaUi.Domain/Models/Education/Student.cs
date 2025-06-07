using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Студент
/// </summary>
public class Student : AuditableEntity
{
    /// <summary>
    /// ID связанного человека
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Студенческий код
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;

    /// <summary>
    /// Дата поступления
    /// </summary>
    public DateTime EnrollmentDate { get; set; }

    /// <summary>
    /// Дата выпуска
    /// </summary>
    public DateTime? GraduationDate { get; set; }

    /// <summary>
    /// Статус студента
    /// </summary>
    public StudentStatus Status { get; set; } = StudentStatus.Active;

    /// <summary>
    /// Средний балл
    /// </summary>
    public decimal GPA { get; set; }

    /// <summary>
    /// ID группы
    /// </summary>
    public Guid? GroupUid { get; set; }

    /// <summary>
    /// ID учебного плана
    /// </summary>
    public Guid? CurriculumUid { get; set; }

    /// <summary>
    /// Связанный человек
    /// </summary>
    public Person? Person { get; set; }

    /// <summary>
    /// Полное имя (алиас для совместимости)
    /// </summary>
    public string? FullName => Person != null ? $"{Person.LastName} {Person.FirstName} {Person.MiddleName}".Trim() : null;

    /// <summary>
    /// Адрес (алиас для совместимости)
    /// </summary>
    public string? Address => Person?.Address;

    /// <summary>
    /// Группа
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Учебный план
    /// </summary>
    public Curriculum? Curriculum { get; set; }

    /// <summary>
    /// Записи на курсы
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    /// <summary>
    /// Оценки
    /// </summary>
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();

    /// <summary>
    /// Посещаемость
    /// </summary>
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    
    /// <summary>
    /// Результаты экзаменов
    /// </summary>
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    
    /// <summary>
    /// Сданные работы
    /// </summary>
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    
    /// <summary>
    /// Прогресс по занятиям
    /// </summary>
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    
    /// <summary>
    /// Является ли студент активным (не отчислен, не в академ. отпуске)
    /// </summary>
    public bool IsActive => Status == StudentStatus.Active;
}