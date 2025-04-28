using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Комментарий к оценке
/// </summary>
public class GradeComment : ViewModelBase
{
    private Guid _gradeUid;
    private Guid _authorUid;
    private GradeCommentType _type;
    private string _content = string.Empty;
    private bool _isDeleted;
    private DateTime? _deletedAt;
    private GradeCommentStatus _status;

    /// <summary>
    /// Идентификатор оценки
    /// </summary>
    public Guid GradeUid
    {
        get => _gradeUid;
        set => this.RaiseAndSetIfChanged(ref _gradeUid, value);
    }

    /// <summary>
    /// Идентификатор автора комментария
    /// </summary>
    public Guid AuthorUid
    {
        get => _authorUid;
        set => this.RaiseAndSetIfChanged(ref _authorUid, value);
    }

    /// <summary>
    /// Тип комментария
    /// </summary>
    public GradeCommentType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Содержание комментария
    /// </summary>
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    /// <summary>
    /// Флаг удаления комментария
    /// </summary>
    public bool IsDeleted
    {
        get => _isDeleted;
        set => this.RaiseAndSetIfChanged(ref _isDeleted, value);
    }

    /// <summary>
    /// Дата удаления комментария
    /// </summary>
    public DateTime? DeletedAt
    {
        get => _deletedAt;
        set => this.RaiseAndSetIfChanged(ref _deletedAt, value);
    }

    /// <summary>
    /// Статус комментария
    /// </summary>
    public GradeCommentStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Отображаемый тип комментария
    /// </summary>
    public string TypeDisplayName => Type.GetDisplayName();

    /// <summary>
    /// Отображаемый статус комментария
    /// </summary>
    public string StatusDisplayName => Status.GetDisplayName();

    /// <summary>
    /// Создает новый экземпляр комментария к оценке
    /// </summary>
    public GradeComment()
    {
        _status = GradeCommentStatus.PendingReview;
    }

    /// <summary>
    /// Создает новый экземпляр комментария к оценке с указанными параметрами
    /// </summary>
    public GradeComment(Guid uid, Guid gradeUid, Guid authorUid, GradeCommentType type, string content, GradeCommentStatus status = GradeCommentStatus.PendingReview)
    {
        Uid = uid;
        _gradeUid = gradeUid;
        _authorUid = authorUid;
        _type = type;
        _content = content;
        _isDeleted = false;
        _status = status;
    }

    /// <summary>
    /// Обновляет содержание комментария
    /// </summary>
    public void UpdateContent(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            return;

        Content = newContent;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Удаляет комментарий
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет статус комментария
    /// </summary>
    public void UpdateStatus(GradeCommentStatus newStatus)
    {
        Status = newStatus;
        LastModifiedAt = DateTime.UtcNow;
    }
} 