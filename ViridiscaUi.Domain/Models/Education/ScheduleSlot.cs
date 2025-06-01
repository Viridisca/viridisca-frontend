using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Слот расписания - конкретное время проведения урока
/// </summary>
public class ScheduleSlot : ViewModelBase
{
    private Guid _courseInstanceUid;
    private DayOfWeek _dayOfWeek;
    private TimeSpan _startTime;
    private TimeSpan _endTime;
    private string _classroom = string.Empty;
    private bool _isActive = true;
    private DateTime _validFrom;
    private DateTime? _validTo;

    private CourseInstance? _courseInstance;

    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid
    {
        get => _courseInstanceUid;
        set => this.RaiseAndSetIfChanged(ref _courseInstanceUid, value);
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
    /// Время начала
    /// </summary>
    public TimeSpan StartTime
    {
        get => _startTime;
        set => this.RaiseAndSetIfChanged(ref _startTime, value);
    }

    /// <summary>
    /// Время окончания
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
    /// Активен ли слот
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Действует с
    /// </summary>
    public DateTime ValidFrom
    {
        get => _validFrom;
        set => this.RaiseAndSetIfChanged(ref _validFrom, value);
    }

    /// <summary>
    /// Действует до
    /// </summary>
    public DateTime? ValidTo
    {
        get => _validTo;
        set => this.RaiseAndSetIfChanged(ref _validTo, value);
    }

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance
    {
        get => _courseInstance;
        set => this.RaiseAndSetIfChanged(ref _courseInstance, value);
    }

    /// <summary>
    /// Продолжительность в минутах
    /// </summary>
    public int DurationInMinutes => (int)(EndTime - StartTime).TotalMinutes;

    /// <summary>
    /// Название дня недели
    /// </summary>
    public string DayName => DayOfWeek switch
    {
        DayOfWeek.Monday => "Понедельник",
        DayOfWeek.Tuesday => "Вторник", 
        DayOfWeek.Wednesday => "Среда",
        DayOfWeek.Thursday => "Четверг",
        DayOfWeek.Friday => "Пятница",
        DayOfWeek.Saturday => "Суббота",
        DayOfWeek.Sunday => "Воскресенье",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Действует ли слот в указанную дату
    /// </summary>
    public bool IsValidForDate(DateTime date)
    {
        return date >= ValidFrom && (!ValidTo.HasValue || date <= ValidTo.Value);
    }

    public ScheduleSlot()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        ValidFrom = DateTime.UtcNow;
    }
} 