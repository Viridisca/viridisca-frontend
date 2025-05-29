using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education.Enums;

/// <summary>
/// Статусы студентов
/// </summary>
public enum StudentStatus
{
    /// <summary>
    /// Активный студент
    /// </summary>
    [Description("Активный")]
    Active = 1,

    /// <summary>
    /// Неактивный студент
    /// </summary>
    [Description("Неактивный")]
    Inactive = 2,

    /// <summary>
    /// Студент в академическом отпуске
    /// </summary>
    [Description("Академический отпуск")]
    AcademicLeave = 3,

    /// <summary>
    /// Отчисленный студент
    /// </summary>
    [Description("Отчислен")]
    Expelled = 4,

    /// <summary>
    /// Выпущенный студент
    /// </summary>
    [Description("Выпущен")]
    Graduated = 5,

    /// <summary>
    /// Переведенный студент
    /// </summary>
    [Description("Переведен")]
    Transferred = 6,

    /// <summary>
    /// Студент с приостановленным обучением
    /// </summary>
    [Description("Приостановлен")]
    Suspended = 7
}

/// <summary>
/// Расширения для статусов студентов
/// </summary>
public static class StudentStatusExtensions
{
    /// <summary>
    /// Получает отображаемое имя статуса студента
    /// </summary>
    public static string GetDisplayName(this StudentStatus status)
    {
        var fieldInfo = status.GetType().GetField(status.ToString());

        if (fieldInfo == null) 
            return status.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : status.ToString();
    }
}
