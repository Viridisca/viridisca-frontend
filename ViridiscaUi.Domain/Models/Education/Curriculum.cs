using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Учебный план
/// </summary>
public class Curriculum : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private int _totalCredits;
    private int _durationInSemesters;
    private bool _isActive = true;
    private DateTime _validFrom;
    private DateTime? _validTo;

    private ObservableCollection<CurriculumSubject> _curriculumSubjects = [];
    private ObservableCollection<Student> _students = [];

    /// <summary>
    /// Название учебного плана
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Общее количество кредитов
    /// </summary>
    public int TotalCredits
    {
        get => _totalCredits;
        set => this.RaiseAndSetIfChanged(ref _totalCredits, value);
    }

    /// <summary>
    /// Продолжительность в семестрах
    /// </summary>
    public int DurationInSemesters
    {
        get => _durationInSemesters;
        set => this.RaiseAndSetIfChanged(ref _durationInSemesters, value);
    }

    /// <summary>
    /// Активен ли план
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
    /// Предметы в учебном плане
    /// </summary>
    public ObservableCollection<CurriculumSubject> CurriculumSubjects
    {
        get => _curriculumSubjects;
        set => this.RaiseAndSetIfChanged(ref _curriculumSubjects, value);
    }

    /// <summary>
    /// Студенты, обучающиеся по этому плану
    /// </summary>
    public ObservableCollection<Student> Students
    {
        get => _students;
        set => this.RaiseAndSetIfChanged(ref _students, value);
    }

    /// <summary>
    /// Количество предметов в плане
    /// </summary>
    public int SubjectsCount => CurriculumSubjects.Count;

    /// <summary>
    /// Действует ли план в указанную дату
    /// </summary>
    public bool IsValidForDate(DateTime date)
    {
        return date >= ValidFrom && (!ValidTo.HasValue || date <= ValidTo.Value);
    }

    public Curriculum()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        ValidFrom = DateTime.UtcNow;
    }
} 