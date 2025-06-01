using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Запись студента на курс
/// </summary>
public class Enrollment : ViewModelBase
{
    private Guid _studentUid;
    private Guid _courseInstanceUid;
    private DateTime _enrollmentDate;
    private DateTime? _completionDate;
    private EnrollmentStatus _status = EnrollmentStatus.Active;
    private decimal? _finalGrade;
    private string _notes = string.Empty;

    private Student? _student;
    private CourseInstance? _courseInstance;

    /// <summary>
    /// ID студента
    /// </summary>
    public Guid StudentUid
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
    }

    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid
    {
        get => _courseInstanceUid;
        set => this.RaiseAndSetIfChanged(ref _courseInstanceUid, value);
    }

    /// <summary>
    /// Дата записи
    /// </summary>
    public DateTime EnrollmentDate
    {
        get => _enrollmentDate;
        set => this.RaiseAndSetIfChanged(ref _enrollmentDate, value);
    }

    /// <summary>
    /// Дата завершения
    /// </summary>
    public DateTime? CompletionDate
    {
        get => _completionDate;
        set => this.RaiseAndSetIfChanged(ref _completionDate, value);
    }

    /// <summary>
    /// Алиас для CompletionDate (для совместимости со старым кодом)
    /// </summary>
    public DateTime? CompletedAt
    {
        get => _completionDate;
        set => this.RaiseAndSetIfChanged(ref _completionDate, value);
    }

    /// <summary>
    /// Дата записи (алиас для EnrollmentDate)
    /// </summary>
    public DateTime EnrolledAt
    {
        get => _enrollmentDate;
        set => this.RaiseAndSetIfChanged(ref _enrollmentDate, value);
    }

    /// <summary>
    /// Статус записи
    /// </summary>
    public EnrollmentStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Итоговая оценка
    /// </summary>
    public decimal? FinalGrade
    {
        get => _finalGrade;
        set => this.RaiseAndSetIfChanged(ref _finalGrade, value);
    }

    /// <summary>
    /// Заметки
    /// </summary>
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student
    {
        get => _student;
        set => this.RaiseAndSetIfChanged(ref _student, value);
    }

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance
    {
        get => _courseInstance;
        set => this.RaiseAndSetIfChanged(ref _courseInstance, value);
    }

    public Enrollment()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        EnrollmentDate = DateTime.UtcNow;
    }
}
