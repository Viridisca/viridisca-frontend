using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Результат экзамена студента
/// </summary>
public class ExamResult : ViewModelBase
{
    private Guid _examUid;
    private Guid _studentUid;
    private decimal _score;
    private string _feedback = string.Empty;
    private DateTime? _submittedAt;
    private DateTime? _gradedAt;
    private bool _isAbsent;
    private string _notes = string.Empty;

    private Exam? _exam;
    private Student? _student;

    /// <summary>
    /// ID экзамена
    /// </summary>
    public Guid ExamUid
    {
        get => _examUid;
        set => this.RaiseAndSetIfChanged(ref _examUid, value);
    }

    /// <summary>
    /// ID студента
    /// </summary>
    public Guid StudentUid
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
    }

    /// <summary>
    /// Полученный балл
    /// </summary>
    public decimal Score
    {
        get => _score;
        set => this.RaiseAndSetIfChanged(ref _score, value);
    }

    /// <summary>
    /// Обратная связь от преподавателя
    /// </summary>
    public string Feedback
    {
        get => _feedback;
        set => this.RaiseAndSetIfChanged(ref _feedback, value);
    }

    /// <summary>
    /// Время сдачи экзамена
    /// </summary>
    public DateTime? SubmittedAt
    {
        get => _submittedAt;
        set => this.RaiseAndSetIfChanged(ref _submittedAt, value);
    }

    /// <summary>
    /// Время выставления оценки
    /// </summary>
    public DateTime? GradedAt
    {
        get => _gradedAt;
        set => this.RaiseAndSetIfChanged(ref _gradedAt, value);
    }

    /// <summary>
    /// Отсутствовал ли студент
    /// </summary>
    public bool IsAbsent
    {
        get => _isAbsent;
        set => this.RaiseAndSetIfChanged(ref _isAbsent, value);
    }

    /// <summary>
    /// Дополнительные заметки
    /// </summary>
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    /// <summary>
    /// Экзамен
    /// </summary>
    public Exam? Exam
    {
        get => _exam;
        set => this.RaiseAndSetIfChanged(ref _exam, value);
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
    /// Процент от максимального балла
    /// </summary>
    public decimal Percentage => Exam?.MaxScore > 0 ? (Score / Exam.MaxScore) * 100 : 0;

    public ExamResult()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 