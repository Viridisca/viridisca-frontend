using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Типы родственных отношений
/// </summary>
public enum ParentRelationType
{
    /// <summary>
    /// Мать
    /// </summary>
    [Description("Мать")]
    Mother,

    /// <summary>
    /// Отец
    /// </summary>
    [Description("Отец")]
    Father,

    /// <summary>
    /// Бабушка
    /// </summary>
    [Description("Бабушка")]
    Grandmother,

    /// <summary>
    /// Дедушка
    /// </summary>
    [Description("Дедушка")]
    Grandfather,

    /// <summary>
    /// Опекун
    /// </summary>
    [Description("Опекун")]
    Guardian,

    /// <summary>
    /// Другое
    /// </summary>
    [Description("Другое")]
    Other
}

/// <summary>
/// Расширения для типов родственных отношений
/// </summary>
public static class ParentRelationTypeExtensions
{
    /// <summary>
    /// Получает отображаемое имя типа родственного отношения
    /// </summary>
    public static string GetDisplayName(this ParentRelationType relationType)
    {
        var fieldInfo = relationType.GetType().GetField(relationType.ToString());
        if (fieldInfo == null) return relationType.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : relationType.ToString();
    }
} 