using System;

namespace ViridiscaUi.Domain.Models.Base;

/// <summary>
/// Базовая сущность с полями аудита
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    public Guid Uid { get; set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
} 