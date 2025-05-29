using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System.Enums;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Посещаемость студентов
/// </summary>
public class Attendance : ViewModelBase
{
    private Guid _studentUid;
    private Student? _student;

    private Guid _lessonUid;
    private Lesson? _lesson;

    private AttendanceStatus _status;
    
    private string _notes = string.Empty;
    
    private DateTime _checkedAt;
    private Guid _checkedByUid;

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
    /// Присутствовал ли студент (вычисляемое свойство)
    /// </summary>
    public bool IsPresent => Status == AttendanceStatus.Present || Status == AttendanceStatus.Late;

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
