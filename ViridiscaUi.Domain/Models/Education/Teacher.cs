using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Преподаватель
/// </summary>
public class Teacher : AuditableEntity
{
    /// <summary>
    /// ID связанного человека
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Код сотрудника
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// Дата найма
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Дата увольнения
    /// </summary>
    public DateTime? TerminationDate { get; set; }

    /// <summary>
    /// Квалификация
    /// </summary>
    public string Qualification { get; set; } = string.Empty;

    /// <summary>
    /// Специализация
    /// </summary>
    public string? Specialization { get; set; }

    /// <summary>
    /// Зарплата
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Активен ли преподаватель
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Расположение офиса
    /// </summary>
    public string? OfficeLocation { get; set; }
    
    /// <summary>
    /// Рабочие часы
    /// </summary>
    public string? WorkingHours { get; set; }

    /// <summary>
    /// ID департамента
    /// </summary>
    public Guid? DepartmentUid { get; set; }

    /// <summary>
    /// Связанный человек
    /// </summary>
    public Person? Person { get; set; }

    /// <summary>
    /// Имя (алиас для совместимости)
    /// </summary>
    public string? FirstName => Person?.FirstName;

    /// <summary>
    /// Фамилия (алиас для совместимости)
    /// </summary>
    public string? LastName => Person?.LastName;

    /// <summary>
    /// Отчество (алиас для совместимости)
    /// </summary>
    public string? MiddleName => Person?.MiddleName;

    /// <summary>
    /// Email (алиас для совместимости)
    /// </summary>
    public string? Email => Person?.Email;

    /// <summary>
    /// Полное имя (делегирует к Person.FullName)
    /// </summary>
    public string? FullName => Person?.FullName;

    /// <summary>
    /// Ученая степень
    /// </summary>
    public string? AcademicDegree { get; set; }

    /// <summary>
    /// Ученое звание
    /// </summary>
    public string? AcademicTitle { get; set; }

    /// <summary>
    /// Почасовая ставка
    /// </summary>
    public decimal? HourlyRate { get; set; }

    /// <summary>
    /// Департамент
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Экземпляры курсов, которые ведет преподаватель
    /// </summary>
    public ICollection<CourseInstance> CourseInstances { get; set; } = new List<CourseInstance>();
    
    /// <summary>
    /// Оценки, выставленные преподавателем
    /// </summary>
    public ICollection<Grade> GradedMarks { get; set; } = new List<Grade>();

    /// <summary>
    /// Обновляет детали преподавателя
    /// </summary>
    public void UpdateDetails(string qualification, string? specialization, string? academicDegree, string? academicTitle, decimal? hourlyRate)
    {
        Qualification = qualification;
        Specialization = specialization;
        AcademicDegree = academicDegree;
        AcademicTitle = academicTitle;
        HourlyRate = hourlyRate;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет статус преподавателя
    /// </summary>
    public void UpdateStatus(bool isActive, DateTime? terminationDate = null)
    {
        IsActive = isActive;
        if (!isActive && terminationDate.HasValue)
        {
            TerminationDate = terminationDate;
        }
        LastModifiedAt = DateTime.UtcNow;
    }
}