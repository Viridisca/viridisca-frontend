using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// История изменений оценки
/// </summary>
public class GradeRevision : ViewModelBase
{
    private Guid _gradeUid;
    private Guid _teacherUid;
    private decimal _previousValue;
    private decimal _newValue;
    private string _previousDescription = string.Empty;
    private string _newDescription = string.Empty;
    private string _revisionReason = string.Empty;
    private DateTime _createdAt;

    /// <summary>
    /// Идентификатор оценки
    /// </summary>
    public Guid GradeUid
    {
        get => _gradeUid;
        set => this.RaiseAndSetIfChanged(ref _gradeUid, value);
    }

    /// <summary>
    /// Идентификатор преподавателя, изменившего оценку
    /// </summary>
    public Guid TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
    }

    /// <summary>
    /// Предыдущее значение оценки
    /// </summary>
    public decimal PreviousValue
    {
        get => _previousValue;
        set => this.RaiseAndSetIfChanged(ref _previousValue, value);
    }

    /// <summary>
    /// Новое значение оценки
    /// </summary>
    public decimal NewValue
    {
        get => _newValue;
        set => this.RaiseAndSetIfChanged(ref _newValue, value);
    }

    /// <summary>
    /// Предыдущее описание/комментарий к оценке
    /// </summary>
    public string PreviousDescription
    {
        get => _previousDescription;
        set => this.RaiseAndSetIfChanged(ref _previousDescription, value);
    }

    /// <summary>
    /// Новое описание/комментарий к оценке
    /// </summary>
    public string NewDescription
    {
        get => _newDescription;
        set => this.RaiseAndSetIfChanged(ref _newDescription, value);
    }

    /// <summary>
    /// Причина изменения оценки
    /// </summary>
    public string RevisionReason
    {
        get => _revisionReason;
        set => this.RaiseAndSetIfChanged(ref _revisionReason, value);
    }

    /// <summary>
    /// Дата создания ревизии
    /// </summary>
    public new DateTime CreatedAt
    {
        get => _createdAt;
        set => this.RaiseAndSetIfChanged(ref _createdAt, value);
    }

    /// <summary>
    /// Создает новый экземпляр записи истории изменений
    /// </summary>
    public GradeRevision()
    {
        _createdAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр записи истории изменений с указанными параметрами
    /// </summary>
    public GradeRevision(
        Guid uid,
        Guid gradeUid,
        Guid teacherUid,
        decimal previousValue,
        decimal newValue,
        string previousDescription,
        string newDescription,
        string revisionReason)
    {
        Uid = uid;
        _gradeUid = gradeUid;
        _teacherUid = teacherUid;
        _previousValue = previousValue;
        _newValue = newValue;
        _previousDescription = previousDescription;
        _newDescription = newDescription;
        _revisionReason = revisionReason;
        _createdAt = DateTime.UtcNow;
    }
} 