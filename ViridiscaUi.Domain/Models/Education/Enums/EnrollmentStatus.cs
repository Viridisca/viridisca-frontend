namespace ViridiscaUi.Domain.Models.Education.Enums;

/// <summary>
/// Статус зачисления
/// </summary>
public enum EnrollmentStatus
{
    /// <summary>
    /// Активное зачисление
    /// </summary>
    Active,
    
    /// <summary>
    /// Завершено
    /// </summary>
    Completed,
    
    /// <summary>
    /// Отменено
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Приостановлено
    /// </summary>
    Suspended,
    
    /// <summary>
    /// В ожидании
    /// </summary>
    Pending
} 