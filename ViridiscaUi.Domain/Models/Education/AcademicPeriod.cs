using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education.Enums;
using System.Collections.ObjectModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Академический период (семестр, четверть, триместр)
/// </summary>
public class AcademicPeriod : ViewModelBase
{
    private string _name = string.Empty;
    private string _code = string.Empty;
    private AcademicPeriodType _type;
    private DateTime _startDate;
    private DateTime _endDate;
    private bool _isActive;
    private bool _isCurrent;
    private int _academicYear;

    private ObservableCollection<CourseInstance> _courseInstances = [];
    private ObservableCollection<Exam> _exams = [];

    /// <summary>
    /// Название периода
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Код периода
    /// </summary>
    public string Code
    {
        get => _code;
        set => this.RaiseAndSetIfChanged(ref _code, value);
    }

    /// <summary>
    /// Тип периода
    /// </summary>
    public AcademicPeriodType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Дата начала
    /// </summary>
    public DateTime StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    /// <summary>
    /// Дата окончания
    /// </summary>
    public DateTime EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    /// <summary>
    /// Активен ли период
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Текущий ли период
    /// </summary>
    public bool IsCurrent
    {
        get => _isCurrent;
        set => this.RaiseAndSetIfChanged(ref _isCurrent, value);
    }

    /// <summary>
    /// Учебный год
    /// </summary>
    public int AcademicYear
    {
        get => _academicYear;
        set => this.RaiseAndSetIfChanged(ref _academicYear, value);
    }

    /// <summary>
    /// Экземпляры курсов в этом периоде
    /// </summary>
    public ObservableCollection<CourseInstance> CourseInstances
    {
        get => _courseInstances;
        set => this.RaiseAndSetIfChanged(ref _courseInstances, value);
    }

    /// <summary>
    /// Экзамены в этом периоде
    /// </summary>
    public ObservableCollection<Exam> Exams
    {
        get => _exams;
        set => this.RaiseAndSetIfChanged(ref _exams, value);
    }

    /// <summary>
    /// Продолжительность в днях
    /// </summary>
    public int DurationInDays => (EndDate - StartDate).Days;

    /// <summary>
    /// Завершенный ли период
    /// </summary>
    public bool IsCompleted => DateTime.Now > EndDate;

    public AcademicPeriod()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        IsActive = true;
    }
} 