using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Связь между преподавателем и предметом
/// </summary>
public class TeacherSubject : ViewModelBase
{
    private Guid _teacherUid;
    private Teacher? _teacher;

    private Guid _subjectUid;
    private Subject? _subject;

    private bool _isMainTeacher;
    private bool _isActive;
    
    private DateTime _assignedDate;
    private DateTime? _deactivatedDate;

    /// <summary>
    /// Идентификатор преподавателя
    /// </summary>
    public Guid TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
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
    /// Идентификатор предмета
    /// </summary>
    public Guid SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
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
    /// Является ли основным преподавателем предмета
    /// </summary>
    public bool IsMainTeacher
    {
        get => _isMainTeacher;
        set => this.RaiseAndSetIfChanged(ref _isMainTeacher, value);
    }

    /// <summary>
    /// Дата назначения предмета
    /// </summary>
    public DateTime AssignedDate
    {
        get => _assignedDate;
        set => this.RaiseAndSetIfChanged(ref _assignedDate, value);
    }

    /// <summary>
    /// Дата деактивации связи
    /// </summary>
    public DateTime? DeactivatedDate
    {
        get => _deactivatedDate;
        set => this.RaiseAndSetIfChanged(ref _deactivatedDate, value);
    }

    /// <summary>
    /// Активна ли связь
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Название предмета
    /// </summary>
    public string SubjectName => Subject?.Name ?? "Неизвестный предмет";

    /// <summary>
    /// Создает новый экземпляр связи между преподавателем и предметом
    /// </summary>
    public TeacherSubject()
    {
        _isActive = true;
        _assignedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр связи между преподавателем и предметом с указанными параметрами
    /// </summary>
    public TeacherSubject(Guid teacherUid, Guid subjectUid, bool isMainTeacher = false)
    {
        Uid = Guid.NewGuid();
        _teacherUid = teacherUid;
        _subjectUid = subjectUid;
        _isMainTeacher = isMainTeacher;
        _assignedDate = DateTime.UtcNow;
        _isActive = true;
    }

    /// <summary>
    /// Устанавливает статус основного преподавателя
    /// </summary>
    public void SetAsMainTeacher(bool isMain)
    {
        IsMainTeacher = isMain;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивирует связь преподавателя с предметом
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        DeactivatedDate = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активирует связь преподавателя с предметом
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        DeactivatedDate = null;
        LastModifiedAt = DateTime.UtcNow;
    }
} 