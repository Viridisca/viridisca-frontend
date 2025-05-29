namespace ViridiscaUi.Domain.Models.Education.Enums;

/// <summary>
/// Статус сданной работы
/// </summary>
public enum SubmissionStatus
{
    /// <summary>
    /// Черновик (не сдано)
    /// </summary>
    Draft,
    
    /// <summary>
    /// Сдано
    /// </summary>
    Submitted,
    
    /// <summary>
    /// Сдано с опозданием
    /// </summary>
    Late,
    
    /// <summary>
    /// На проверке
    /// </summary>
    UnderReview,
    
    /// <summary>
    /// Оценено
    /// </summary>
    Graded,
    
    /// <summary>
    /// Возвращено на доработку
    /// </summary>
    Returned
} 