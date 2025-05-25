using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Посещаемость студентов
/// </summary>
public class Attendance : ViewModelBase
{
    private Guid _studentUid;
    private Guid _lessonUid;
    private AttendanceStatus _status;
    private string _notes = string.Empty;
    private DateTime _checkedAt;
    private Guid _checkedByUid;
    private Student? _student;
    private Lesson? _lesson;

    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
    }

    /// <summary>
    /// Идентификатор урока
    /// </summary>
    public Guid LessonUid
    {
        get => _lessonUid;
        set => this.RaiseAndSetIfChanged(ref _lessonUid, value);
    }

    /// <summary>
    /// Статус посещения
    /// </summary>
    public AttendanceStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Заметки о посещении
    /// </summary>
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    /// <summary>
    /// Время отметки посещения
    /// </summary>
    public DateTime CheckedAt
    {
        get => _checkedAt;
        set => this.RaiseAndSetIfChanged(ref _checkedAt, value);
    }

    /// <summary>
    /// Идентификатор преподавателя, отметившего посещение
    /// </summary>
    public Guid CheckedByUid
    {
        get => _checkedByUid;
        set => this.RaiseAndSetIfChanged(ref _checkedByUid, value);
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
    /// Урок
    /// </summary>
    public Lesson? Lesson
    {
        get => _lesson;
        set => this.RaiseAndSetIfChanged(ref _lesson, value);
    }

    /// <summary>
    /// Создает новый экземпляр записи посещаемости
    /// </summary>
    public Attendance()
    {
        _checkedAt = DateTime.UtcNow;
        _status = AttendanceStatus.Present;
    }

    /// <summary>
    /// Создает новый экземпляр записи посещаемости с указанными параметрами
    /// </summary>
    public Attendance(Guid studentUid, Guid lessonUid, AttendanceStatus status, Guid checkedByUid)
    {
        Uid = Guid.NewGuid();
        _studentUid = studentUid;
        _lessonUid = lessonUid;
        _status = status;
        _checkedByUid = checkedByUid;
        _checkedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Статус посещения
/// </summary>
public enum AttendanceStatus
{
    /// <summary>
    /// Присутствовал
    /// </summary>
    Present = 0,

    /// <summary>
    /// Отсутствовал
    /// </summary>
    Absent = 1,

    /// <summary>
    /// Опоздал
    /// </summary>
    Late = 2,

    /// <summary>
    /// Ушел раньше
    /// </summary>
    LeftEarly = 3,

    /// <summary>
    /// Уважительная причина
    /// </summary>
    Excused = 4
} 