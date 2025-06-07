using System;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Комментарий к оценке
/// </summary>
public class GradeComment : AuditableEntity
{
    /// <summary>
    /// Идентификатор оценки
    /// </summary>
    public Guid GradeUid { get; set; }

    /// <summary>
    /// Идентификатор автора комментария
    /// </summary>
    public Guid AuthorUid { get; set; }

    /// <summary>
    /// Тип комментария
    /// </summary>
    public GradeCommentType Type { get; set; }

    /// <summary>
    /// Содержание комментария
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Флаг удаления комментария
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Дата удаления комментария
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Статус комментария
    /// </summary>
    public GradeCommentStatus Status { get; set; }

    /// <summary>
    /// Оценка
    /// </summary>
    public Grade? Grade { get; set; }

    /// <summary>
    /// Автор комментария
    /// </summary>
    public Person? Author { get; set; }

    /// <summary>
    /// Создает новый экземпляр комментария к оценке
    /// </summary>
    public GradeComment()
    {
        Status = GradeCommentStatus.PendingReview;
    }

    /// <summary>
    /// Создает новый экземпляр комментария к оценке с указанными параметрами
    /// </summary>
    public GradeComment(Guid uid, Guid gradeUid, Guid authorUid, GradeCommentType type, string content, GradeCommentStatus status = GradeCommentStatus.PendingReview)
    {
        Uid = uid;
        GradeUid = gradeUid;
        AuthorUid = authorUid;
        Type = type;
        Content = content;
        IsDeleted = false;
        Status = status;
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