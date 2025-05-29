using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Зачисление студента на курс
/// </summary>
public class Enrollment : ViewModelBase
{
    private Guid _studentUid;
    private Student? _student;

    private Guid _courseUid;
    private Course? _course;

    private DateTime _enrollmentDate = DateTime.UtcNow;
    private DateTime? _completedAt;

    private EnrollmentStatus _status = EnrollmentStatus.Active;

    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
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
    /// Идентификатор курса
    /// </summary>
    public Guid CourseUid
    {
        get => _courseUid;
        set => this.RaiseAndSetIfChanged(ref _courseUid, value);
    }

    /// <summary>
    /// Курс
    /// </summary>
    public Course? Course
    {
        get => _course;
        set => this.RaiseAndSetIfChanged(ref _course, value);
    }

    /// <summary>
    /// Дата зачисления
    /// </summary>
    public DateTime EnrollmentDate
    {
        get => _enrollmentDate;
        set => this.RaiseAndSetIfChanged(ref _enrollmentDate, value);
    }

    /// <summary>
    /// Дата завершения курса
    /// </summary>
    public DateTime? CompletedAt
    {
        get => _completedAt;
        set => this.RaiseAndSetIfChanged(ref _completedAt, value);
    }

    /// <summary>
    /// Статус зачисления
    /// </summary>
    public EnrollmentStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Проверяет, завершен ли курс
    /// </summary>
    public bool IsCompleted => CompletedAt.HasValue;

    /// <summary>
    /// Создает новый экземпляр зачисления
    /// </summary>
    public Enrollment()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр зачисления с указанными параметрами
    /// </summary>
    public Enrollment(Guid studentUid, Guid courseUid, EnrollmentStatus status = EnrollmentStatus.Active)
    {
        Uid = Guid.NewGuid();
        _studentUid = studentUid;
        _courseUid = courseUid;
        _status = status;
        _enrollmentDate = DateTime.UtcNow;
    }
}
