using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Типы комментариев к оценкам
/// </summary>
public enum GradeCommentType
{
    /// <summary>
    /// Комментарий
    /// </summary>
    Comment = 0,
    
    /// <summary>
    /// Причина изменения оценки
    /// </summary>
    ChangeReason = 1
}

/// <summary>
/// Расширения для типов комментариев к оценкам
/// </summary>
public static class GradeCommentTypeExtensions
{
    /// <summary>
    /// Получает отображаемое имя типа комментария
    /// </summary>
    public static string GetDisplayName(this GradeCommentType commentType)
    {
        var fieldInfo = commentType.GetType().GetField(commentType.ToString());
        if (fieldInfo == null) return commentType.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : commentType.ToString();
    }
} 