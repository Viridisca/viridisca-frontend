using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Типы ролей в системе
/// </summary>
public enum RoleType
{
    /// <summary>
    /// Системный администратор
    /// </summary>
    [Description("Системный администратор")]
    SystemAdmin = 1,

    /// <summary>
    /// Директор учебного заведения
    /// </summary>
    [Description("Директор школы")]
    SchoolDirector = 2,

    /// <summary>
    /// Руководитель учебного отдела
    /// </summary>
    [Description("Руководитель учебного отдела")]
    AcademicAffairsHead = 3,

    /// <summary>
    /// Преподаватель
    /// </summary>
    [Description("Преподаватель")]
    Teacher = 4,

    /// <summary>
    /// Куратор группы
    /// </summary>
    [Description("Куратор группы")]
    GroupCurator = 5,

    /// <summary>
    /// Ученик
    /// </summary>
    [Description("Ученик")]
    Student = 6,

    /// <summary>
    /// Родитель
    /// </summary>
    [Description("Родитель")]
    Parent = 7,

    /// <summary>
    /// Методист
    /// </summary>
    [Description("Методист")]
    EducationMethodist = 8,

    /// <summary>
    /// Финансовый менеджер
    /// </summary>
    [Description("Финансовый менеджер")]
    FinancialManager = 9,

    /// <summary>
    /// Специалист по контролю качества
    /// </summary>
    [Description("Менеджер контроля качества")]
    QualityAssuranceManager = 10,

    /// <summary>
    /// Менеджер учебного контента
    /// </summary>
    [Description("Менеджер контента")]
    ContentManager = 11,

    /// <summary>
    /// Аналитик данных
    /// </summary>
    [Description("Аналитик данных")]
    DataAnalyst = 12,

    /// <summary>
    /// Библиотекарь
    /// </summary>
    [Description("Библиотекарь")]
    Librarian = 13,

    /// <summary>
    /// Психолог
    /// </summary>
    [Description("Психолог")]
    Psychologist = 14,

    /// <summary>
    /// Специалист по здравоохранению
    /// </summary>
    [Description("Медицинский специалист")]
    HealthcareSpecialist = 15,

    /// <summary>
    /// Техническая поддержка
    /// </summary>
    [Description("Техническая поддержка")]
    TechnicalSupport = 16,

    /// <summary>
    /// Гость
    /// </summary>
    [Description("Гость")]
    Guest = 17
}

/// <summary>
/// Расширения для типов ролей
/// </summary>
public static class RoleTypeExtensions
{
    /// <summary>
    /// Получает отображаемое имя роли
    /// </summary>
    public static string GetDisplayName(this RoleType roleType)
    {
        var fieldInfo = roleType.GetType().GetField(roleType.ToString());

        if (fieldInfo is null) 
            return roleType.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0 ? attributes[0].Description : roleType.ToString();
    }
} 