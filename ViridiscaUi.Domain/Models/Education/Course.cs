using System;
using System.Collections.Generic;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Курс обучения
/// </summary>
public class Course : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private Guid? _teacherUid;
    private Teacher? _teacher;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private int _credits;
    private CourseStatus _status = CourseStatus.Draft;

    /// <summary>
    /// Название курса
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание курса
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Идентификатор преподавателя
    /// </summary>
    public Guid? TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
    }

    /// <summary>
    /// Преподаватель курса
    /// </summary>
    public Teacher? Teacher
    {
        get => _teacher;
        set => this.RaiseAndSetIfChanged(ref _teacher, value);
    }

    /// <summary>
    /// Дата начала курса
    /// </summary>
    public DateTime? StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    /// <summary>
    /// Дата окончания курса
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    /// <summary>
    /// Количество кредитов
    /// </summary>
    public int Credits
    {
        get => _credits;
        set => this.RaiseAndSetIfChanged(ref _credits, value);
    }

    /// <summary>
    /// Статус курса
    /// </summary>
    public CourseStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Модули курса
    /// </summary>
    public ICollection<Module> Modules { get; set; } = new List<Module>();

    /// <summary>
    /// Записи на курс
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    /// <summary>
    /// Задания курса
    /// </summary>
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    /// <summary>
    /// Создает новый экземпляр курса
    /// </summary>
    public Course()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр курса с указанными параметрами
    /// </summary>
    public Course(string name, string description, Guid? teacherUid = null)
    {
        Uid = Guid.NewGuid();
        _name = name.Trim();
        _description = description;
        _teacherUid = teacherUid;
    }
}

/// <summary>
/// Статус курса
/// </summary>
public enum CourseStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    Draft,
    
    /// <summary>
    /// Активный
    /// </summary>
    Active,
    
    /// <summary>
    /// Архивированный
    /// </summary>
    Archived,
    
    /// <summary>
    /// Приостановленный
    /// </summary>
    Suspended
} 