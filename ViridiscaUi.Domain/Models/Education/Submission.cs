using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Сданная студентом работа по заданию
/// </summary>
public class Submission : ViewModelBase
{
    private Guid _studentUid;
    private Guid _assignmentUid;
    private DateTime _submissionDate = DateTime.UtcNow;
    private string _content = string.Empty;
    private double? _score;
    private string _feedback = string.Empty;
    private Guid? _gradedByUid;
    private DateTime? _gradedDate;
    private SubmissionStatus _status = SubmissionStatus.Submitted;
    private Student? _student;
    private Assignment? _assignment;
    private Teacher? _gradedBy;

    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
    }

    /// <summary>
    /// Идентификатор студента (синоним для StudentUid)
    /// </summary>
    public Guid StudentId
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
    }

    /// <summary>
    /// Идентификатор задания
    /// </summary>
    public Guid AssignmentUid
    {
        get => _assignmentUid;
        set => this.RaiseAndSetIfChanged(ref _assignmentUid, value);
    }

    /// <summary>
    /// Идентификатор задания (синоним для AssignmentUid)
    /// </summary>
    public Guid AssignmentId
    {
        get => _assignmentUid;
        set => this.RaiseAndSetIfChanged(ref _assignmentUid, value);
    }

    /// <summary>
    /// Дата сдачи работы
    /// </summary>
    public DateTime SubmissionDate
    {
        get => _submissionDate;
        set => this.RaiseAndSetIfChanged(ref _submissionDate, value);
    }

    /// <summary>
    /// Содержимое работы (текст ответа или ссылка на файл)
    /// </summary>
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    /// <summary>
    /// Путь к файлу (синоним для Content)
    /// </summary>
    public string FilePath
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    /// <summary>
    /// Оценка за работу
    /// </summary>
    public double? Score
    {
        get => _score;
        set => this.RaiseAndSetIfChanged(ref _score, value);
    }

    /// <summary>
    /// Комментарий преподавателя
    /// </summary>
    public string Feedback
    {
        get => _feedback;
        set => this.RaiseAndSetIfChanged(ref _feedback, value);
    }

    /// <summary>
    /// Идентификатор преподавателя, поставившего оценку
    /// </summary>
    public Guid? GradedByUid
    {
        get => _gradedByUid;
        set => this.RaiseAndSetIfChanged(ref _gradedByUid, value);
    }

    /// <summary>
    /// Дата выставления оценки
    /// </summary>
    public DateTime? GradedDate
    {
        get => _gradedDate;
        set => this.RaiseAndSetIfChanged(ref _gradedDate, value);
    }

    /// <summary>
    /// Статус работы
    /// </summary>
    public SubmissionStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Студент, сдавший работу
    /// </summary>
    public Student? Student
    {
        get => _student;
        set => this.RaiseAndSetIfChanged(ref _student, value);
    }

    /// <summary>
    /// Задание, по которому сдана работа
    /// </summary>
    public Assignment? Assignment
    {
        get => _assignment;
        set => this.RaiseAndSetIfChanged(ref _assignment, value);
    }

    /// <summary>
    /// Преподаватель, поставивший оценку
    /// </summary>
    public Teacher? GradedBy
    {
        get => _gradedBy;
        set => this.RaiseAndSetIfChanged(ref _gradedBy, value);
    }

    /// <summary>
    /// Создает новый экземпляр сданной работы
    /// </summary>
    public Submission()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр сданной работы с указанными параметрами
    /// </summary>
    public Submission(Guid studentUid, Guid assignmentUid, string content)
    {
        Uid = Guid.NewGuid();
        _studentUid = studentUid;
        _assignmentUid = assignmentUid;
        _content = content;
        _submissionDate = DateTime.UtcNow;
        _status = SubmissionStatus.Submitted;
    }

    /// <summary>
    /// Отображаемый статус работы
    /// </summary>
    public string StatusDisplayName => Status switch
    {
        SubmissionStatus.Draft => "Черновик",
        SubmissionStatus.Submitted => "Сдано",
        SubmissionStatus.Late => "Сдано с опозданием",
        SubmissionStatus.UnderReview => "На проверке",
        SubmissionStatus.Graded => "Оценено",
        SubmissionStatus.Returned => "Возвращено на доработку",
        _ => "Неизвестный статус"
    };

    /// <summary>
    /// Проверяет, оценена ли работа
    /// </summary>
    public bool IsGraded => Status == SubmissionStatus.Graded && Score.HasValue;

    /// <summary>
    /// Проверяет, сдана ли работа с опозданием
    /// </summary>
    public bool IsLate => Status == SubmissionStatus.Late || 
                         (Assignment?.DueDate.HasValue == true && SubmissionDate > Assignment.DueDate.Value);
}

/// <summary>
/// Статус сданной работы
/// </summary>
public enum SubmissionStatus
{
    /// <summary>
    /// Черновик (не сдано)
    /// </summary>
    Draft,
    
    /// <summary>
    /// Сдано
    /// </summary>
    Submitted,
    
    /// <summary>
    /// Сдано с опозданием
    /// </summary>
    Late,
    
    /// <summary>
    /// На проверке
    /// </summary>
    UnderReview,
    
    /// <summary>
    /// Оценено
    /// </summary>
    Graded,
    
    /// <summary>
    /// Возвращено на доработку
    /// </summary>
    Returned
} 