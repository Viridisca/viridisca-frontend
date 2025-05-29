using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Типы предметов
/// </summary>
public enum SubjectType
{
    /// <summary>
    /// Обязательный предмет
    /// </summary>
    [Description("Обязательный")]
    Required = 1,

    /// <summary>
    /// Факультативный предмет
    /// </summary>
    [Description("Факультативный")]
    Elective = 2,

    /// <summary>
    /// Специализированный предмет
    /// </summary>
    [Description("Специализированный")]
    Specialized = 3,

    /// <summary>
    /// Практикум
    /// </summary>
    [Description("Практикум")]
    Practicum = 4,

    /// <summary>
    /// Семинар
    /// </summary>
    [Description("Семинар")]
    Seminar = 5,
    Laboratory = 6,
    Lecture = 7
}

/// <summary>
/// Расширения для типов предметов
/// </summary>
public static class SubjectTypeExtensions
{
    /// <summary>
    /// Получает отображаемое имя типа предмета
    /// </summary>
    public static string GetDisplayName(this SubjectType subjectType)
    {
        var fieldInfo = subjectType.GetType().GetField(subjectType.ToString());

        if (fieldInfo == null)
            return subjectType.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : subjectType.ToString();
    }
}