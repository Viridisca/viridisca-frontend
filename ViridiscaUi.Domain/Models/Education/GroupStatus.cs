using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Статусы групп
/// </summary>
public enum GroupStatus
{
    /// <summary>
    /// Активная группа
    /// </summary>
    [Description("Активная")]
    Active = 1,

    /// <summary>
    /// Группа в процессе формирования
    /// </summary>
    [Description("Формирование")]
    Forming = 2,

    /// <summary>
    /// Приостановленная группа
    /// </summary>
    [Description("Приостановлена")]
    Suspended = 3,

    /// <summary>
    /// Завершившая обучение группа
    /// </summary>
    [Description("Завершена")]
    Completed = 4,

    /// <summary>
    /// Архивная группа
    /// </summary>
    [Description("Архивная")]
    Archived = 5
}

/// <summary>
/// Расширения для статусов групп
/// </summary>
public static class GroupStatusExtensions
{
    /// <summary>
    /// Получает отображаемое имя статуса группы
    /// </summary>
    public static string GetDisplayName(this GroupStatus status)
    {
        var fieldInfo = status.GetType().GetField(status.ToString());
        if (fieldInfo == null) return status.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : status.ToString();
    }
} 