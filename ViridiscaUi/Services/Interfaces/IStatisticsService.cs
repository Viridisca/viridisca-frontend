using System;
using System.Threading.Tasks;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для получения общей статистики системы
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Получает общую статистику системы
    /// </summary>
    Task<SystemStatistics> GetSystemStatisticsAsync();
    
    /// <summary>
    /// Получает статистику для конкретного пользователя
    /// </summary>
    Task<UserStatistics> GetUserStatisticsAsync(Guid userUid);
    
    /// <summary>
    /// Получает статистику активности
    /// </summary>
    Task<ActivityStatistics> GetActivityStatisticsAsync();
}

/// <summary>
/// Общая статистика системы
/// </summary>
public class SystemStatistics
{
    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents { get; set; }
    
    /// <summary>
    /// Общее количество курсов
    /// </summary>
    public int TotalCourses { get; set; }
    
    /// <summary>
    /// Общее количество преподавателей
    /// </summary>
    public int TotalTeachers { get; set; }
    
    /// <summary>
    /// Общее количество заданий
    /// </summary>
    public int TotalAssignments { get; set; }
    
    /// <summary>
    /// Количество активных курсов
    /// </summary>
    public int ActiveCourses { get; set; }
    
    /// <summary>
    /// Количество активных студентов
    /// </summary>
    public int ActiveStudents { get; set; }
    
    /// <summary>
    /// Количество групп
    /// </summary>
    public int TotalGroups { get; set; }
    
    /// <summary>
    /// Количество предметов
    /// </summary>
    public int TotalSubjects { get; set; }
    
    /// <summary>
    /// Количество департаментов
    /// </summary>
    public int TotalDepartments { get; set; }
    
    /// <summary>
    /// Дата последнего обновления статистики
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Статистика пользователя
/// </summary>
public class UserStatistics
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserUid { get; set; }
    
    /// <summary>
    /// Количество непрочитанных уведомлений
    /// </summary>
    public int UnreadNotifications { get; set; }
    
    /// <summary>
    /// Количество активных заданий (для студентов)
    /// </summary>
    public int ActiveAssignments { get; set; }
    
    /// <summary>
    /// Количество просроченных заданий (для студентов)
    /// </summary>
    public int OverdueAssignments { get; set; }
    
    /// <summary>
    /// Количество курсов (для студентов/преподавателей)
    /// </summary>
    public int TotalCourses { get; set; }
    
    /// <summary>
    /// Средняя оценка (для студентов)
    /// </summary>
    public double? AverageGrade { get; set; }
}

/// <summary>
/// Статистика активности
/// </summary>
public class ActivityStatistics
{
    /// <summary>
    /// Количество пользователей онлайн
    /// </summary>
    public int OnlineUsersCount { get; set; }
    
    /// <summary>
    /// Количество активных сессий
    /// </summary>
    public int ActiveSessions { get; set; }
    
    /// <summary>
    /// Количество действий за сегодня
    /// </summary>
    public int TodayActions { get; set; }
    
    /// <summary>
    /// Количество действий за неделю
    /// </summary>
    public int WeekActions { get; set; }
    
    /// <summary>
    /// Пиковое время активности
    /// </summary>
    public TimeSpan? PeakActivityTime { get; set; }
} 