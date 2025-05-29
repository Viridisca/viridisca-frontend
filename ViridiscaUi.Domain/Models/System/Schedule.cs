using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.System.Enums;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Расписание занятий
/// </summary>
public class Schedule : ViewModelBase
{
    private Guid _groupUid;
    private Group? _group;
    
    private Guid _subjectUid;
    private Subject? _subject;
    
    private Guid _teacherUid;
    private Teacher? _teacher;

    private DayOfWeek _dayOfWeek;
    private ScheduleType _type;
    
    private string _classroom = string.Empty;
    
    private TimeSpan _startTime;
    private TimeSpan _endTime;
    private DateTime _validFrom;
    private DateTime? _validTo;
    
    private bool _isActive;
     
    /// <summary>
    /// Идентификатор группы
    /// </summary>
    public Guid GroupUid
    {
        get => _groupUid;
        set => this.RaiseAndSetIfChanged(ref _groupUid, value);
    }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
    }

    /// <summary>
    /// Идентификатор преподавателя
    /// </summary>
    public Guid TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
    }

    /// <summary>
    /// День недели
    /// </summary>
    public DayOfWeek DayOfWeek
    {
        get => _dayOfWeek;
        set => this.RaiseAndSetIfChanged(ref _dayOfWeek, value);
    }

    /// <summary>
    /// Время начала занятия
    /// </summary>
    public TimeSpan StartTime
    {
        get => _startTime;
        set => this.RaiseAndSetIfChanged(ref _startTime, value);
    }

    /// <summary>
    /// Время окончания занятия
    /// </summary>
    public TimeSpan EndTime
    {
        get => _endTime;
        set => this.RaiseAndSetIfChanged(ref _endTime, value);
    }

    /// <summary>
    /// Аудитория
    /// </summary>
    public string Classroom
    {
        get => _classroom;
        set => this.RaiseAndSetIfChanged(ref _classroom, value);
    }

    /// <summary>
    /// Тип занятия
    /// </summary>
    public ScheduleType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Дата начала действия расписания
    /// </summary>
    public DateTime ValidFrom
    {
        get => _validFrom;
        set => this.RaiseAndSetIfChanged(ref _validFrom, value);
    }

    /// <summary>
    /// Дата окончания действия расписания
    /// </summary>
    public DateTime? ValidTo
    {
        get => _validTo;
        set => this.RaiseAndSetIfChanged(ref _validTo, value);
    }

    /// <summary>
    /// Флаг активности записи расписания
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Группа
    /// </summary>
    public Group? Group
    {
        get => _group;
        set => this.RaiseAndSetIfChanged(ref _group, value);
    }

    /// <summary>
    /// Предмет
    /// </summary>
    public Subject? Subject
    {
        get => _subject;
        set => this.RaiseAndSetIfChanged(ref _subject, value);
    }

    /// <summary>
    /// Преподаватель
    /// </summary>
    public Teacher? Teacher
    {
        get => _teacher;
        set => this.RaiseAndSetIfChanged(ref _teacher, value);
    }

    /// <summary>
    /// Продолжительность занятия в минутах
    /// </summary>
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

    /// <summary>
    /// Создает новый экземпляр расписания
    /// </summary>
    public Schedule()
    {
        _validFrom = DateTime.Today;
        _isActive = true;
        _type = ScheduleType.Lecture;
    }

    /// <summary>
    /// Создает новый экземпляр расписания с указанными параметрами
    /// </summary>
    public Schedule(
        Guid groupUid,
        Guid subjectUid,
        Guid teacherUid,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        string classroom,
        ScheduleType type = ScheduleType.Lecture)
    {
        Uid = Guid.NewGuid();
        _groupUid = groupUid;
        _subjectUid = subjectUid;
        _teacherUid = teacherUid;
        _dayOfWeek = dayOfWeek;
        _startTime = startTime;
        _endTime = endTime;
        _classroom = classroom;
        _type = type;
        _validFrom = DateTime.Today;
        _isActive = true;
    }
}
