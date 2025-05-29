using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Оценка студента
/// </summary>
public class Grade : ViewModelBase
{
    private Guid _studentUid;
    private Guid _subjectUid;
    private Guid _teacherUid;
    private Guid? _lessonUid;
    private Guid? _assignmentUid;
    private Guid? _gradedByUid;

    private decimal _value;
    
    private string _description = string.Empty;
    private string _comment = string.Empty;
    
    private GradeType _type;
    
    private DateTime _issuedAt;
    private DateTime _gradedAt;
    private DateTime? _publishedAt;
    
    private bool _isPublished;

    private ObservableCollection<GradeComment> _comments = [];
    private ObservableCollection<GradeRevision> _revisions = [];

    private Student? _student;
    private Subject? _subject;
    private Teacher? _teacher;
    private Teacher? _gradedBy;
    private Assignment? _assignment;

    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
    }

    /// <summary>
    /// Студент, получивший оценку
    /// </summary>
    public Student? Student
    {
        get => _student;
        set => this.RaiseAndSetIfChanged(ref _student, value);
    }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
    }

    /// <summary>
    /// Предмет, по которому выставлена оценка
    /// </summary>
    public Subject? Subject
    {
        get => _subject;
        set => this.RaiseAndSetIfChanged(ref _subject, value);
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
    /// Преподаватель, выставивший оценку
    /// </summary>
    public Teacher? Teacher
    {
        get => _teacher;
        set => this.RaiseAndSetIfChanged(ref _teacher, value);
    }

    /// <summary>
    /// Идентификатор урока (если оценка связана с конкретным уроком)
    /// </summary>
    public Guid? LessonUid
    {
        get => _lessonUid;
        set => this.RaiseAndSetIfChanged(ref _lessonUid, value);
    }

    /// <summary>
    /// Идентификатор задания (если оценка связана с конкретным заданием)
    /// </summary>
    public Guid? AssignmentUid
    {
        get => _assignmentUid;
        set => this.RaiseAndSetIfChanged(ref _assignmentUid, value);
    }

    /// <summary>
    /// Идентификатор преподавателя, выставившего оценку
    /// </summary>
    public Guid? GradedByUid
    {
        get => _gradedByUid;
        set => this.RaiseAndSetIfChanged(ref _gradedByUid, value);
    }

    /// <summary>
    /// Значение оценки
    /// </summary>
    public decimal Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    /// <summary>
    /// Описание/комментарий к оценке
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Комментарий к оценке
    /// </summary>
    public string Comment
    {
        get => _comment;
        set => this.RaiseAndSetIfChanged(ref _comment, value);
    }

    /// <summary>
    /// Тип оценки
    /// </summary>
    public GradeType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Дата выставления оценки
    /// </summary>
    public DateTime IssuedAt
    {
        get => _issuedAt;
        set => this.RaiseAndSetIfChanged(ref _issuedAt, value);
    }

    /// <summary>
    /// Дата выставления оценки
    /// </summary>
    public DateTime GradedAt
    {
        get => _gradedAt;
        set => this.RaiseAndSetIfChanged(ref _gradedAt, value);
    }

    /// <summary>
    /// Флаг публикации оценки (видна ли студенту/родителю)
    /// </summary>
    public bool IsPublished
    {
        get => _isPublished;
        set => this.RaiseAndSetIfChanged(ref _isPublished, value);
    }

    /// <summary>
    /// Дата публикации оценки
    /// </summary>
    public DateTime? PublishedAt
    {
        get => _publishedAt;
        set => this.RaiseAndSetIfChanged(ref _publishedAt, value);
    }

    /// <summary>
    /// Отображаемый тип оценки
    /// </summary>
    public string TypeDisplayName => Type.GetDisplayName();

    /// <summary>
    /// Имя студента
    /// </summary>
    public string StudentName => Student?.FullName ?? "Неизвестный студент";

    /// <summary>
    /// Название предмета
    /// </summary>
    public string SubjectName => Subject?.Name ?? "Неизвестный предмет";

    /// <summary>
    /// Комментарии к оценке
    /// </summary>
    public ObservableCollection<GradeComment> Comments
    {
        get => _comments;
        set => this.RaiseAndSetIfChanged(ref _comments, value);
    }

    /// <summary>
    /// История изменений оценки
    /// </summary>
    public ObservableCollection<GradeRevision> Revisions
    {
        get => _revisions;
        set => this.RaiseAndSetIfChanged(ref _revisions, value);
    }

    /// <summary>
    /// Задание, за которое выставлена оценка (если применимо)
    /// </summary>
    public Assignment? Assignment
    {
        get => _assignment;
        set => this.RaiseAndSetIfChanged(ref _assignment, value);
    }

    /// <summary>
    /// Преподаватель, выставивший оценку
    /// </summary>
    public Teacher? GradedBy
    {
        get => _gradedBy;
        set => this.RaiseAndSetIfChanged(ref _gradedBy, value);
    }

    /// <summary>
    /// Создает новый экземпляр оценки
    /// </summary>
    public Grade()
    {
        _issuedAt = DateTime.UtcNow;
        _gradedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр оценки с указанными параметрами
    /// </summary>
    public Grade(
        Guid studentUid,
        Guid subjectUid,
        Guid teacherUid,
        decimal value,
        GradeType type,
        string? description = null,
        Guid? lessonUid = null)
    {
        Uid = Guid.NewGuid();
        _studentUid = studentUid;
        _subjectUid = subjectUid;
        _teacherUid = teacherUid;
        _value = value;
        _type = type;
        _description = description ?? string.Empty;
        _lessonUid = lessonUid;
        _issuedAt = DateTime.UtcNow;
        _gradedAt = DateTime.UtcNow;
        _isPublished = false;
    }

    /// <summary>
    /// Обновляет значение оценки
    /// </summary>
    public void UpdateValue(decimal value, string? reason = null)
    {
        if (value == Value)
            return;

        var revision = new GradeRevision(
            Guid.NewGuid(),
            Uid,
            TeacherUid,
            Value,
            value,
            Description,
            Description,
            reason ?? "Изменение значения оценки");

        Revisions.Add(revision);
        Value = value;
        LastModifiedAt = DateTime.UtcNow;
        this.RaisePropertyChanged(nameof(Revisions));
    }

    /// <summary>
    /// Обновляет описание оценки
    /// </summary>
    public void UpdateDescription(string description)
    {
        if (description == Description)
            return;

        var revision = new GradeRevision(
            Guid.NewGuid(),
            Uid,
            TeacherUid,
            Value,
            Value,
            Description,
            description,
            "Изменение описания оценки");

        Revisions.Add(revision);
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
        this.RaisePropertyChanged(nameof(Revisions));
    }

    /// <summary>
    /// Публикует оценку (делает видимой для студента/родителя)
    /// </summary>
    public void Publish()
    {
        if (IsPublished)
            return;

        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        
        var comment = new GradeComment(
            Guid.NewGuid(),
            Uid,
            TeacherUid,
            GradeCommentType.Comment,
            "Оценка опубликована");
                
        Comments.Add(comment);
        this.RaisePropertyChanged(nameof(Comments));
    }

    /// <summary>
    /// Скрывает оценку от студента/родителя
    /// </summary>
    public void Unpublish()
    {
        if (!IsPublished)
            return;

        IsPublished = false;
        PublishedAt = null;
        LastModifiedAt = DateTime.UtcNow;
        
        var comment = new GradeComment(
            Guid.NewGuid(),
            Uid,
            TeacherUid,
            GradeCommentType.Comment,
            "Оценка скрыта");
                
        Comments.Add(comment);
        this.RaisePropertyChanged(nameof(Comments));
    }

    /// <summary>
    /// Добавляет комментарий к оценке
    /// </summary>
    public void AddComment(GradeComment comment)
    {
        if (comment != null && !Comments.Any(c => c.Uid == comment.Uid))
        {
            Comments.Add(comment);
            LastModifiedAt = DateTime.UtcNow;
            this.RaisePropertyChanged(nameof(Comments));
        }
    }

    /// <summary>
    /// Удаляет комментарий к оценке
    /// </summary>
    public void RemoveComment(GradeComment comment)
    {
        if (comment != null && Comments.Contains(comment))
        {
            Comments.Remove(comment);
            LastModifiedAt = DateTime.UtcNow;
            this.RaisePropertyChanged(nameof(Comments));
        }
    }
} 