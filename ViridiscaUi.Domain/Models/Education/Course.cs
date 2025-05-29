using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Курс обучения
/// </summary>
public class Course : ViewModelBase
{
    private string _name = string.Empty;
    private string _code = string.Empty;
    
    private string _description = string.Empty;
    private string _category = string.Empty;

    private Guid? _teacherUid; 
    private Teacher? _teacher;
    
    private DateTime? _startDate;
    private DateTime? _endDate;
    
    private int _credits;
    
    private CourseStatus _status = CourseStatus.Draft;
    
    private string _prerequisites = string.Empty;
    private string _learningOutcomes = string.Empty;
    
    private int _maxEnrollments = 100;

    private Guid? _subjectUid;

    /// <summary>
    /// Название курса
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Заголовок курса (синоним для Name)
    /// </summary>
    public string Title
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Код курса
    /// </summary>
    public string Code
    {
        get => _code;
        set => this.RaiseAndSetIfChanged(ref _code, value);
    }

    /// <summary>
    /// Описание курса
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Категория курса
    /// </summary>
    public string Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
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
    /// Преподаватель курса
    /// </summary>
    public Teacher? Teacher
    {
        get => _teacher;
        set => this.RaiseAndSetIfChanged(ref _teacher, value);
    }

    /// <summary>
    /// Дата начала курса
    /// </summary>
    public DateTime? StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    /// <summary>
    /// Дата окончания курса
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
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
    /// Статус курса
    /// </summary>
    public CourseStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Предварительные требования для курса
    /// </summary>
    public string Prerequisites
    {
        get => _prerequisites;
        set => this.RaiseAndSetIfChanged(ref _prerequisites, value);
    }

    /// <summary>
    /// Ожидаемые результаты обучения
    /// </summary>
    public string LearningOutcomes
    {
        get => _learningOutcomes;
        set => this.RaiseAndSetIfChanged(ref _learningOutcomes, value);
    }

    /// <summary>
    /// Максимальное количество записей на курс
    /// </summary>
    public int MaxEnrollments
    {
        get => _maxEnrollments;
        set => this.RaiseAndSetIfChanged(ref _maxEnrollments, value);
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
    /// Модули курса
    /// </summary>
    public ICollection<Module> Modules { get; set; } = [];

    /// <summary>
    /// Записи на курс
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = [];

    /// <summary>
    /// Задания курса
    /// </summary>
    public ICollection<Assignment> Assignments { get; set; } = [];

    /// <summary>
    /// Создает новый экземпляр курса
    /// </summary>
    public Course()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр курса с указанными параметрами
    /// </summary>
    public Course(string name, string description, Guid? teacherUid = null)
    {
        Uid = Guid.NewGuid();
        _name = name.Trim();
        _description = description;
        _teacherUid = teacherUid;
    }
}