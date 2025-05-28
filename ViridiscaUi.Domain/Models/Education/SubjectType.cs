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
/// Категории предметов
/// </summary>
public enum SubjectCategory
{
    /// <summary>
    /// Математика
    /// </summary>
    [Description("Математика")]
    Mathematics = 1,

    /// <summary>
    /// Естественные науки
    /// </summary>
    [Description("Естественные науки")]
    NaturalSciences = 2,

    /// <summary>
    /// Гуманитарные науки
    /// </summary>
    [Description("Гуманитарные науки")]
    Humanities = 3,

    /// <summary>
    /// Информатика
    /// </summary>
    [Description("Информатика")]
    ComputerScience = 4,

    /// <summary>
    /// Языки
    /// </summary>
    [Description("Языки")]
    Languages = 5,

    /// <summary>
    /// Искусство
    /// </summary>
    [Description("Искусство")]
    Arts = 6,

    /// <summary>
    /// Спорт
    /// </summary>
    [Description("Спорт")]
    Sports = 7,

    /// <summary>
    /// Экономика
    /// </summary>
    [Description("Экономика")]
    Economics = 8,

    /// <summary>
    /// Инженерия
    /// </summary>
    [Description("Инженерия")]
    Engineering = 9,

    /// <summary>
    /// Другое
    /// </summary>
    [Description("Другое")]
    Other = 10
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
        if (fieldInfo == null) return subjectType.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : subjectType.ToString();
    }
} 