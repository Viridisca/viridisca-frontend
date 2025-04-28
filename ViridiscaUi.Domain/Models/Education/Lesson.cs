using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Урок (занятие)
/// </summary>
public class Lesson : ViewModelBase
{
    private Guid _subjectUid;
    private Guid _teacherUid;
    private Guid _groupUid;
    private string _topic = string.Empty;
    private string _description = string.Empty;
    private DateTime _startTime;
    private DateTime _endTime;
    private bool _isCancelled;
    private string _cancellationReason = string.Empty;
    private bool _isCompleted;
    private ObservableCollection<Grade> _grades = new();
    private Subject? _subject;
    private Teacher? _teacher;
    private Group? _group;

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
    }

    /// <summary>
    /// Идентификатор преподавателя
    /// </summary>
    public Guid TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
    }

    /// <summary>
    /// Идентификатор группы
    /// </summary>
    public Guid GroupUid
    {
        get => _groupUid;
        set => this.RaiseAndSetIfChanged(ref _groupUid, value);
    }

    /// <summary>
    /// Тема урока
    /// </summary>
    public string Topic
    {
        get => _topic;
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
    /// Время начала урока
    /// </summary>
    public DateTime StartTime
    {
        get => _startTime;
        set => this.RaiseAndSetIfChanged(ref _startTime, value);
    }

    /// <summary>
    /// Время окончания урока
    /// </summary>
    public DateTime EndTime
    {
        get => _endTime;
        set => this.RaiseAndSetIfChanged(ref _endTime, value);
    }

    /// <summary>
    /// Признак отмены урока
    /// </summary>
    public bool IsCancelled
    {
        get => _isCancelled;
        set => this.RaiseAndSetIfChanged(ref _isCancelled, value);
    }

    /// <summary>
    /// Причина отмены урока
    /// </summary>
    public string CancellationReason
    {
        get => _cancellationReason;
        set => this.RaiseAndSetIfChanged(ref _cancellationReason, value);
    }

    /// <summary>
    /// Признак завершения урока
    /// </summary>
    public bool IsCompleted
    {
        get => _isCompleted;
        set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
    }

    /// <summary>
    /// Оценки, выставленные на уроке
    /// </summary>
    public ObservableCollection<Grade> Grades
    {
        get => _grades;
        set => this.RaiseAndSetIfChanged(ref _grades, value);
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
    /// Преподаватель
    /// </summary>
    public Teacher? Teacher
    {
        get => _teacher;
        set => this.RaiseAndSetIfChanged(ref _teacher, value);
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
    /// Создает новый экземпляр урока
    /// </summary>
    public Lesson()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр урока с указанными параметрами
    /// </summary>
    public Lesson(
        Guid subjectUid,
        Guid teacherUid,
        Guid groupUid,
        string topic,
        DateTime startTime,
        DateTime endTime,
        string? description = null)
    {
        Uid = Guid.NewGuid();
        _subjectUid = subjectUid;
        _teacherUid = teacherUid;
        _groupUid = groupUid;
        _topic = topic;
        _description = description ?? string.Empty;
        _startTime = startTime;
        _endTime = endTime;
        _isCancelled = false;
        _isCompleted = false;
    }

    /// <summary>
    /// Обновляет информацию об уроке
    /// </summary>
    public void UpdateDetails(
        string topic,
        DateTime startTime,
        DateTime endTime,
        string? description = null)
    {
        if (IsCancelled || IsCompleted)
            return;

        Topic = topic;
        StartTime = startTime;
        EndTime = endTime;
        Description = description ?? string.Empty;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отменяет урок
    /// </summary>
    public void Cancel(string reason)
    {
        if (IsCompleted)
            return;

        IsCancelled = true;
        CancellationReason = reason;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Восстанавливает отмененный урок
    /// </summary>
    public void Resume()
    {
        if (!IsCancelled || IsCompleted)
            return;

        IsCancelled = false;
        CancellationReason = string.Empty;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Завершает урок
    /// </summary>
    public void Complete()
    {
        if (IsCancelled || IsCompleted)
            return;

        IsCompleted = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавляет оценку к уроку
    /// </summary>
    public void AddGrade(Grade grade)
    {
        if (IsCancelled || !IsCompleted || grade == null)
            return;

        if (!Grades.Any(g => g.StudentUid == grade.StudentUid))
        {
            Grades.Add(grade);
            this.RaisePropertyChanged(nameof(Grades));
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Удаляет оценку из урока
    /// </summary>
    public void RemoveGrade(Grade grade)
    {
        if (grade != null && Grades.Contains(grade))
        {
            Grades.Remove(grade);
            this.RaisePropertyChanged(nameof(Grades));
            LastModifiedAt = DateTime.UtcNow;
        }
    }
} 