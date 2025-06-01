using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Предмет в учебном плане
/// </summary>
public class CurriculumSubject : ViewModelBase
{
    private Guid _curriculumUid;
    private Guid _subjectUid;
    private int _semester;
    private int _credits;
    private bool _isRequired = true;
    private bool _isActive = true;

    private Curriculum? _curriculum;
    private Subject? _subject;

    /// <summary>
    /// ID учебного плана
    /// </summary>
    public Guid CurriculumUid
    {
        get => _curriculumUid;
        set => this.RaiseAndSetIfChanged(ref _curriculumUid, value);
    }

    /// <summary>
    /// ID предмета
    /// </summary>
    public Guid SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
    }

    /// <summary>
    /// Семестр изучения
    /// </summary>
    public int Semester
    {
        get => _semester;
        set => this.RaiseAndSetIfChanged(ref _semester, value);
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
    /// Обязательный ли предмет
    /// </summary>
    public bool IsRequired
    {
        get => _isRequired;
        set => this.RaiseAndSetIfChanged(ref _isRequired, value);
    }

    /// <summary>
    /// Активен ли предмет в плане
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Учебный план
    /// </summary>
    public Curriculum? Curriculum
    {
        get => _curriculum;
        set => this.RaiseAndSetIfChanged(ref _curriculum, value);
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
    /// Тип предмета (обязательный/элективный)
    /// </summary>
    public string SubjectType => IsRequired ? "Обязательный" : "Элективный";

    public CurriculumSubject()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 