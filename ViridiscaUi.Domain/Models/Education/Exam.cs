using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education.Enums;
using System.Collections.ObjectModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Экзамен
/// </summary>
public class Exam : ViewModelBase
{
    private Guid _courseInstanceUid;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private DateTime _examDate;
    private TimeSpan _duration;
    private string _location = string.Empty;
    private ExamType _type = ExamType.Written;
    private decimal _maxScore = 100;
    private bool _isPublished;
    private string _instructions = string.Empty;

    private CourseInstance? _courseInstance;
    private ObservableCollection<ExamResult> _results = [];

    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid
    {
        get => _courseInstanceUid;
        set => this.RaiseAndSetIfChanged(ref _courseInstanceUid, value);
    }

    /// <summary>
    /// Название экзамена
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
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
    /// Дата и время экзамена
    /// </summary>
    public DateTime ExamDate
    {
        get => _examDate;
        set => this.RaiseAndSetIfChanged(ref _examDate, value);
    }

    /// <summary>
    /// Продолжительность экзамена
    /// </summary>
    public TimeSpan Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    /// <summary>
    /// Место проведения
    /// </summary>
    public string Location
    {
        get => _location;
        set => this.RaiseAndSetIfChanged(ref _location, value);
    }

    /// <summary>
    /// Тип экзамена
    /// </summary>
    public ExamType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Максимальный балл
    /// </summary>
    public decimal MaxScore
    {
        get => _maxScore;
        set => this.RaiseAndSetIfChanged(ref _maxScore, value);
    }

    /// <summary>
    /// Опубликован ли экзамен
    /// </summary>
    public bool IsPublished
    {
        get => _isPublished;
        set => this.RaiseAndSetIfChanged(ref _isPublished, value);
    }

    /// <summary>
    /// Инструкции для экзамена
    /// </summary>
    public string Instructions
    {
        get => _instructions;
        set => this.RaiseAndSetIfChanged(ref _instructions, value);
    }

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance
    {
        get => _courseInstance;
        set => this.RaiseAndSetIfChanged(ref _courseInstance, value);
    }

    /// <summary>
    /// Результаты экзамена
    /// </summary>
    public ObservableCollection<ExamResult> Results
    {
        get => _results;
        set => this.RaiseAndSetIfChanged(ref _results, value);
    }

    public Exam()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        Duration = TimeSpan.FromHours(2); // По умолчанию 2 часа
    }
} 