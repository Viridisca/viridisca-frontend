using System;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Base;

/// <summary>
/// Опции для массового редактирования студентов
/// </summary>
public class BulkEditOptions
{
    /// <summary>
    /// Можно ли изменять группу
    /// </summary>
    public bool CanChangeGroup { get; set; }
    
    /// <summary>
    /// Можно ли изменять статус
    /// </summary>
    public bool CanChangeStatus { get; set; }
    
    /// <summary>
    /// Можно ли изменять академический год
    /// </summary>
    public bool CanChangeAcademicYear { get; set; }
}

/// <summary>
/// Результат массового редактирования
/// </summary>
public class BulkEditResult
{
    /// <summary>
    /// Новый UID группы (если изменяется)
    /// </summary>
    public Guid? NewGroupUid { get; set; }
    
    /// <summary>
    /// Новый статус (если изменяется)
    /// </summary>
    public StudentStatus? NewStatus { get; set; }
    
    /// <summary>
    /// Новый академический год (если изменяется)
    /// </summary>
    public int? NewAcademicYear { get; set; }
} 