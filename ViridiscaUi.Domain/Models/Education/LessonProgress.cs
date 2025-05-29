using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Прогресс студента по уроку
/// </summary>
public class LessonProgress : ViewModelBase
{
    private Guid _studentUid;
    private Guid _lessonUid;

    private bool _isCompleted;
    
    private DateTime? _completedAt;
    private TimeSpan? _timeSpent;
    
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
    /// Флаг завершения урока
    /// </summary>
    public bool IsCompleted
    {
        get => _isCompleted;
        set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
    }

    /// <summary>
    /// Дата завершения урока
    /// </summary>
    public DateTime? CompletedAt
    {
        get => _completedAt;
        set => this.RaiseAndSetIfChanged(ref _completedAt, value);
    }

    /// <summary>
    /// Время, потраченное на урок
    /// </summary>
    public TimeSpan? TimeSpent
    {
        get => _timeSpent;
        set => this.RaiseAndSetIfChanged(ref _timeSpent, value);
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
    /// Создает новый экземпляр прогресса урока
    /// </summary>
    public LessonProgress()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр прогресса урока с указанными параметрами
    /// </summary>
    public LessonProgress(Guid studentUid, Guid lessonUid)
    {
        Uid = Guid.NewGuid();
        _studentUid = studentUid;
        _lessonUid = lessonUid;
    }
} 