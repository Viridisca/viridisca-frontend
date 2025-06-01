using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Экземпляр курса (предмет для конкретной группы в конкретном периоде)
/// </summary>
public class CourseInstance : ViewModelBase
{
    private Guid _subjectUid;
    private Guid _groupUid;
    private Guid _academicPeriodUid;
    private Guid _teacherUid;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private int _maxStudents;
    private bool _isActive = true;

    // Backing fields для совместимости
    private string? _codeOverride;
    private string? _categoryOverride;
    private DateTime? _startDateOverride;
    private DateTime? _endDateOverride;
    private int? _creditsOverride;
    private string? _statusOverride;
    private string? _prerequisitesOverride;
    private string? _learningOutcomesOverride;

    private Subject? _subject;
    private Group? _group;
    private AcademicPeriod? _academicPeriod;
    private Teacher? _teacher;

    private ObservableCollection<Enrollment> _enrollments = [];
    private ObservableCollection<Assignment> _assignments = [];
    private ObservableCollection<Lesson> _lessons = [];
    private ObservableCollection<ScheduleSlot> _scheduleSlots = [];

    /// <summary>
    /// ID предмета
    /// </summary>
    public Guid SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
    }

    /// <summary>
    /// ID группы
    /// </summary>
    public Guid GroupUid
    {
        get => _groupUid;
        set => this.RaiseAndSetIfChanged(ref _groupUid, value);
    }

    /// <summary>
    /// ID академического периода
    /// </summary>
    public Guid AcademicPeriodUid
    {
        get => _academicPeriodUid;
        set => this.RaiseAndSetIfChanged(ref _academicPeriodUid, value);
    }

    /// <summary>
    /// ID преподавателя
    /// </summary>
    public Guid TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
    }

    /// <summary>
    /// Название экземпляра курса
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Максимальное количество студентов
    /// </summary>
    public int MaxStudents
    {
        get => _maxStudents;
        set => this.RaiseAndSetIfChanged(ref _maxStudents, value);
    }

    /// <summary>
    /// Активен ли курс
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
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
    /// Группа
    /// </summary>
    public Group? Group
    {
        get => _group;
        set => this.RaiseAndSetIfChanged(ref _group, value);
    }

    /// <summary>
    /// Академический период
    /// </summary>
    public AcademicPeriod? AcademicPeriod
    {
        get => _academicPeriod;
        set => this.RaiseAndSetIfChanged(ref _academicPeriod, value);
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
    /// Записи студентов
    /// </summary>
    public ObservableCollection<Enrollment> Enrollments
    {
        get => _enrollments;
        set => this.RaiseAndSetIfChanged(ref _enrollments, value);
    }

    /// <summary>
    /// Задания
    /// </summary>
    public ObservableCollection<Assignment> Assignments
    {
        get => _assignments;
        set => this.RaiseAndSetIfChanged(ref _assignments, value);
    }

    /// <summary>
    /// Занятия
    /// </summary>
    public ObservableCollection<Lesson> Lessons
    {
        get => _lessons;
        set => this.RaiseAndSetIfChanged(ref _lessons, value);
    }

    /// <summary>
    /// Слоты расписания
    /// </summary>
    public ObservableCollection<ScheduleSlot> ScheduleSlots
    {
        get => _scheduleSlots;
        set => this.RaiseAndSetIfChanged(ref _scheduleSlots, value);
    }

    /// <summary>
    /// Количество записанных студентов
    /// </summary>
    public int EnrolledStudentsCount => Enrollments.Count;

    /// <summary>
    /// Есть ли свободные места
    /// </summary>
    public bool HasAvailableSlots => EnrolledStudentsCount < MaxStudents;

    /// <summary>
    /// Полное название курса
    /// </summary>
    public string FullName => $"{Subject?.Name} - {Group?.Name} ({AcademicPeriod?.Name})";

    // === СВОЙСТВА ДЛЯ СОВМЕСТИМОСТИ СО СТАРЫМ КОДОМ ===
    
    /// <summary>
    /// Код курса (получается из предмета или переопределяется)
    /// </summary>
    public string Code
    {
        get => _codeOverride ?? Subject?.Code ?? string.Empty;
        set => this.RaiseAndSetIfChanged(ref _codeOverride, value);
    }
    
    /// <summary>
    /// Категория курса (получается из предмета или переопределяется)
    /// </summary>
    public string Category
    {
        get => _categoryOverride ?? Subject?.Type.ToString() ?? string.Empty;
        set => this.RaiseAndSetIfChanged(ref _categoryOverride, value);
    }
    
    /// <summary>
    /// Дата начала (получается из академического периода или переопределяется)
    /// </summary>
    public DateTime? StartDate
    {
        get => _startDateOverride ?? AcademicPeriod?.StartDate;
        set => this.RaiseAndSetIfChanged(ref _startDateOverride, value);
    }
    
    /// <summary>
    /// Дата окончания (получается из академического периода или переопределяется)
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDateOverride ?? AcademicPeriod?.EndDate;
        set => this.RaiseAndSetIfChanged(ref _endDateOverride, value);
    }
    
    /// <summary>
    /// Кредиты (получается из предмета или переопределяется)
    /// </summary>
    public int Credits
    {
        get => _creditsOverride ?? Subject?.Credits ?? 0;
        set => this.RaiseAndSetIfChanged(ref _creditsOverride, value);
    }
    
    /// <summary>
    /// Статус курса (переопределяется или вычисляется)
    /// </summary>
    public string Status
    {
        get
        {
            if (!string.IsNullOrEmpty(_statusOverride)) return _statusOverride;
            if (!IsActive) return "Неактивен";
            if (StartDate.HasValue && DateTime.Now < StartDate.Value) return "Запланирован";
            if (EndDate.HasValue && DateTime.Now > EndDate.Value) return "Завершен";
            return "Активен";
        }
        set => this.RaiseAndSetIfChanged(ref _statusOverride, value);
    }
    
    /// <summary>
    /// Пререквизиты (получается из предмета или переопределяется)
    /// </summary>
    public string Prerequisites
    {
        get => _prerequisitesOverride ?? Subject?.Prerequisites ?? string.Empty;
        set => this.RaiseAndSetIfChanged(ref _prerequisitesOverride, value);
    }
    
    /// <summary>
    /// Результаты обучения (получается из предмета или переопределяется)
    /// </summary>
    public string LearningOutcomes
    {
        get => _learningOutcomesOverride ?? Subject?.LearningOutcomes ?? string.Empty;
        set => this.RaiseAndSetIfChanged(ref _learningOutcomesOverride, value);
    }
    
    /// <summary>
    /// Максимальное количество записей (алиас для MaxStudents)
    /// </summary>
    public int MaxEnrollments
    {
        get => MaxStudents;
        set => MaxStudents = value;
    }

    public CourseInstance()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 