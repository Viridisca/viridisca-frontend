namespace ViridiscaUi.Domain.Models.Education.Enums;

/// <summary>
/// Статус курса
/// </summary>
public enum CourseStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    Draft,
    
    /// <summary>
    /// Активный
    /// </summary>
    Active,
    
    /// <summary>
    /// Опубликованный
    /// </summary>
    Published,
    
    /// <summary>
    /// Завершенный
    /// </summary>
    Completed,
    
    /// <summary>
    /// Архивированный
    /// </summary>
    Archived,
    
    /// <summary>
    /// Приостановленный
    /// </summary>
    Suspended
}
