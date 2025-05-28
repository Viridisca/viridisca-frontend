using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Статусы преподавателей
/// </summary>
public enum TeacherStatus
{
    /// <summary>
    /// Активный преподаватель
    /// </summary>
    [Description("Активный")]
    Active = 1,

    /// <summary>
    /// Преподаватель в отпуске
    /// </summary>
    [Description("В отпуске")]
    OnLeave = 2,

    /// <summary>
    /// Уволенный преподаватель
    /// </summary>
    [Description("Уволен")]
    Terminated = 3,

    /// <summary>
    /// Временно неактивный преподаватель
    /// </summary>
    [Description("Временно неактивен")]
    Inactive = 4,

    /// <summary>
    /// Преподаватель на пенсии
    /// </summary>
    [Description("На пенсии")]
    Retired = 5,

    /// <summary>
    /// Отстраненный преподаватель
    /// </summary>
    [Description("Отстранен")]
    Suspended = 6
}

/// <summary>
/// Расширения для статусов преподавателей
/// </summary>
public static class TeacherStatusExtensions
{
    /// <summary>
    /// Получает отображаемое имя статуса преподавателя
    /// </summary>
    public static string GetDisplayName(this TeacherStatus status)
    {
        var fieldInfo = status.GetType().GetField(status.ToString());
        if (fieldInfo == null) return status.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : status.ToString();
    }
} 