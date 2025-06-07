using System.ComponentModel;

namespace ViridiscaUi.Domain.Models.Education.Enums;

/// <summary>
/// Статус записи на курс
/// </summary>
public enum EnrollmentStatus
{
    [Description("Активная")]
    Active = 1,

    [Description("Завершена")]
    Completed = 2,

    [Description("Отменена")]
    Cancelled = 3,

    [Description("Приостановлена")]
    Suspended = 4,

    [Description("Провалена")]
    Failed = 5,

    [Description("Отчислена")]
    Dropped = 6,

    [Description("В ожидании")]
    Pending = 7
} 