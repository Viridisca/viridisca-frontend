using System;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Системная статистика
/// </summary>
public class SystemStatistics : AuditableEntity
{
    /// <summary>
    /// Общее количество пользователей
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Количество активных пользователей
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Общее количество преподавателей
    /// </summary>
    public int TotalTeachers { get; set; }

    /// <summary>
    /// Общее количество курсов
    /// </summary>
    public int TotalCourses { get; set; }

    /// <summary>
    /// Общее количество предметов
    /// </summary>
    public int TotalSubjects { get; set; }

    /// <summary>
    /// Размер базы данных в байтах
    /// </summary>
    public long DatabaseSize { get; set; }

    /// <summary>
    /// Дата последнего резервного копирования
    /// </summary>
    public DateTime? LastBackupDate { get; set; }

    /// <summary>
    /// Загрузка системы в процентах
    /// </summary>
    public double SystemLoad { get; set; }

    /// <summary>
    /// Использование памяти в процентах
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public SystemStatistics()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 