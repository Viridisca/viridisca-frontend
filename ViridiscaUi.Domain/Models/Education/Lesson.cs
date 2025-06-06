using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Урок в рамках экземпляра курса
/// </summary>
public class Lesson : ViewModelBase
{
    private string _title = string.Empty;
    private string _description = string.Empty;

    private string _content = string.Empty;
    
    private Guid _courseInstanceUid;
    private CourseInstance? _courseInstance;
    
    private int _orderIndex;
    private TimeSpan? _duration;
    
    private LessonType _type = LessonType.Lecture;
    
    private bool _isPublished;
    private string _topic = string.Empty;
    
    private Guid? _subjectUid;
    private Guid? _teacherUid;
    private Guid? _groupUid;

    /// <summary>
    /// Название урока
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Тема урока (синоним для Title)
    /// </summary>
    public string Topic
    {
        get => string.IsNullOrEmpty(_topic) ? _title : _topic;
        set => this.RaiseAndSetIfChanged(ref _topic, value);
    }

    /// <summary>
    /// Описание урока
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Содержимое урока
    /// </summary>
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid
    {
        get => _courseInstanceUid;
        set => this.RaiseAndSetIfChanged(ref _courseInstanceUid, value);
    }

    /// <summary>
    /// Экземпляр курса, к которому принадлежит урок
    /// </summary>
    public CourseInstance? CourseInstance
    {
        get => _courseInstance;
        set => this.RaiseAndSetIfChanged(ref _courseInstance, value);
    }

    /// <summary>
    /// Порядковый номер урока в курсе
    /// </summary>
    public int OrderIndex
    {
        get => _orderIndex;
        set => this.RaiseAndSetIfChanged(ref _orderIndex, value);
    }

    /// <summary>
    /// Продолжительность урока
    /// </summary>
    public TimeSpan? Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    /// <summary>
    /// Тип урока
    /// </summary>
    public LessonType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid? SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
    }

    /// <summary>
    /// Идентификатор преподавателя
    /// </summary>
    public Guid? TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
    }

    /// <summary>
    /// Идентификатор группы
    /// </summary>
    public Guid? GroupUid
    {
        get => _groupUid;
        set => this.RaiseAndSetIfChanged(ref _groupUid, value);
    }

    /// <summary>
    /// Флаг публикации урока
    /// </summary>
    public bool IsPublished
    {
        get => _isPublished;
        set => this.RaiseAndSetIfChanged(ref _isPublished, value);
    }

    /// <summary>
    /// Прогресс студентов по уроку
    /// </summary>
    public ICollection<LessonProgress> LessonProgress { get; set; } = new List<LessonProgress>();

    /// <summary>
    /// Создает новый экземпляр урока
    /// </summary>
    public Lesson()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр урока с указанными параметрами
    /// </summary>
    public Lesson(string title, string description, Guid courseInstanceUid)
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        _title = title.Trim();
        _description = description;
        _courseInstanceUid = courseInstanceUid;
    }
}
