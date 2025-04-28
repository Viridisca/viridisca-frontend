using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Статусы комментариев к оценкам
/// </summary>
public enum GradeCommentStatus
{
    /// <summary>
    /// Ожидает проверки
    /// </summary>
    [Description("Ожидает проверки")]
    PendingReview = 0,
    
    /// <summary>
    /// Одобрен
    /// </summary>
    [Description("Одобрен")]
    Approved = 1,
    
    /// <summary>
    /// Отклонен
    /// </summary>
    [Description("Отклонен")]
    Rejected = 2,
    
    /// <summary>
    /// Архивирован
    /// </summary>
    [Description("Архивирован")]
    Archived = 3
}

/// <summary>
/// Расширения для статусов комментариев к оценкам
/// </summary>
public static class GradeCommentStatusExtensions
{
    /// <summary>
    /// Получает отображаемое имя статуса комментария
    /// </summary>
    public static string GetDisplayName(this GradeCommentStatus status)
    {
        var fieldInfo = status.GetType().GetField(status.ToString());
        if (fieldInfo == null) return status.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : status.ToString();
    }
} 