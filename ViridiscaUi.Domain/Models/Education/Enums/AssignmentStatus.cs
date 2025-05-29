namespace ViridiscaUi.Domain.Models.Education.Enums;

/// <summary>
/// Статус задания
/// </summary>
public enum AssignmentStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    Draft,
    
    /// <summary>
    /// Опубликовано
    /// </summary>
    Published,
    
    /// <summary>
    /// Активно (доступно для выполнения)
    /// </summary>
    Active,
    
    /// <summary>
    /// Завершено
    /// </summary>
    Completed,
    
    /// <summary>
    /// Просрочено
    /// </summary>
    Overdue,
    
    /// <summary>
    /// Закрыто для сдачи
    /// </summary>
    Closed,
    
    /// <summary>
    /// Архивировано
    /// </summary>
    Archived
} 