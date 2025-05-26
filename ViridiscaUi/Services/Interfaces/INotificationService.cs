using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.System;
using NotificationPriority = ViridiscaUi.Domain.Models.System.NotificationPriority;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса уведомлений
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Отправляет уведомление пользователю
        /// </summary>
        Task<Notification> SendNotificationAsync(
            Guid recipientUid,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string? category = null,
            string? actionUrl = null,
            Dictionary<string, object>? metadata = null,
            DateTime? expiresAt = null);

        /// <summary>
        /// Отправляет уведомления нескольким пользователям
        /// </summary>
        Task<BulkNotificationResult> SendBulkNotificationAsync(
            IEnumerable<Guid> recipientUids,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string? category = null,
            string? actionUrl = null,
            Dictionary<string, object>? metadata = null,
            DateTime? expiresAt = null);

        /// <summary>
        /// Получает уведомления пользователя
        /// </summary>
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            Guid userUid,
            bool includeRead = true,
            int? limit = null);

        /// <summary>
        /// Получает уведомления пользователя с пагинацией
        /// </summary>
        Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsPagedAsync(
            Guid userUid,
            int page = 1,
            int pageSize = 20,
            bool includeRead = true,
            NotificationType? typeFilter = null,
            NotificationPriority? priorityFilter = null,
            string? categoryFilter = null);

        /// <summary>
        /// Отмечает уведомление как прочитанное
        /// </summary>
        Task<bool> MarkAsReadAsync(Guid notificationUid);

        /// <summary>
        /// Отмечает несколько уведомлений как прочитанные
        /// </summary>
        Task<int> MarkMultipleAsReadAsync(IEnumerable<Guid> notificationUids);

        /// <summary>
        /// Отмечает все уведомления пользователя как прочитанные
        /// </summary>
        Task<int> MarkAllAsReadAsync(Guid userUid);

        /// <summary>
        /// Удаляет уведомление
        /// </summary>
        Task<bool> DeleteNotificationAsync(Guid notificationUid);

        /// <summary>
        /// Удаляет несколько уведомлений
        /// </summary>
        Task<int> DeleteMultipleNotificationsAsync(IEnumerable<Guid> notificationUids);

        /// <summary>
        /// Получает количество непрочитанных уведомлений пользователя
        /// </summary>
        Task<int> GetUnreadCountAsync(Guid userUid);

        /// <summary>
        /// Получает статистику уведомлений пользователя
        /// </summary>
        Task<NotificationStatistics> GetUserStatisticsAsync(Guid userUid);

        /// <summary>
        /// Получает системную статистику уведомлений
        /// </summary>
        Task<SystemNotificationStatistics> GetSystemStatisticsAsync();

        /// <summary>
        /// Планирует отправку уведомления
        /// </summary>
        Task<Notification> ScheduleNotificationAsync(
            Guid recipientUid,
            string title,
            string message,
            DateTime scheduledFor,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string? category = null,
            string? actionUrl = null,
            Dictionary<string, object>? metadata = null,
            TimeSpan? repeatInterval = null);

        /// <summary>
        /// Отменяет запланированное уведомление
        /// </summary>
        Task<bool> CancelScheduledNotificationAsync(Guid notificationUid);

        /// <summary>
        /// Обрабатывает запланированные уведомления
        /// </summary>
        Task ProcessScheduledNotificationsAsync();

        /// <summary>
        /// Создает шаблон уведомления
        /// </summary>
        Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template);
        
        /// <summary>
        /// Получает все шаблоны уведомлений
        /// </summary>
        Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync();
        
        /// <summary>
        /// Отправляет уведомление по шаблону
        /// </summary>
        Task<Notification> SendFromTemplateAsync(
            Guid templateUid,
            Guid recipientUid,
            Dictionary<string, object>? parameters = null);
        
        /// <summary>
        /// Массовая отправка по шаблону
        /// </summary>
        Task<BulkNotificationResult> SendBulkFromTemplateAsync(
            Guid templateUid,
            IEnumerable<Guid> recipientUids,
            Dictionary<string, object>? parameters = null);
        
        /// <summary>
        /// Настройки уведомлений пользователя
        /// </summary>
        Task<NotificationSettings> GetUserSettingsAsync(Guid userUid);
        
        /// <summary>
        /// Обновляет настройки уведомлений пользователя
        /// </summary>
        Task<bool> UpdateUserSettingsAsync(Guid userUid, NotificationSettings settings);

        /// <summary>
        /// Создает напоминание
        /// </summary>
        Task<Notification> CreateReminderAsync(
            Guid userUid,
            string title,
            string message,
            DateTime reminderTime,
            TimeSpan? repeatInterval = null,
            Dictionary<string, object>? metadata = null);

        /// <summary>
        /// Отправляет уведомления о просроченных заданиях
        /// </summary>
        Task SendOverdueAssignmentNotificationsAsync();

        /// <summary>
        /// Отправляет уведомления о приближающихся дедлайнах
        /// </summary>
        Task SendUpcomingDeadlineNotificationsAsync();

        /// <summary>
        /// Очищает старые уведомления
        /// </summary>
        Task CleanupOldNotificationsAsync(TimeSpan maxAge);

        /// <summary>
        /// Создает уведомление
        /// </summary>
        Task<Notification> CreateNotificationAsync(
            Guid recipientUid,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string? category = null,
            string? actionUrl = null);
        
        /// <summary>
        /// Получает расширенные уведомления с фильтрацией
        /// </summary>
        Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetNotificationsAdvancedAsync(
            Guid userUid,
            int page,
            int pageSize,
            NotificationFilter filter);
        
        /// <summary>
        /// Отмечает уведомление как важное
        /// </summary>
        Task<bool> MarkAsImportantAsync(Guid notificationUid, Guid userUid);
        
        /// <summary>
        /// Снимает отметку важности с уведомления
        /// </summary>
        Task<bool> UnmarkAsImportantAsync(Guid notificationUid, Guid userUid);
        
        /// <summary>
        /// Получает статистику уведомлений пользователя
        /// </summary>
        Task<NotificationStatistics> GetUserNotificationStatisticsAsync(Guid userUid);
        
        /// <summary>
        /// Получает системную статистику уведомлений
        /// </summary>
        Task<SystemNotificationStatistics> GetSystemNotificationStatisticsAsync();
        
        /// <summary>
        /// Архивирует старые уведомления
        /// </summary>
        Task<int> ArchiveOldNotificationsAsync(DateTime olderThan);
        
        /// <summary>
        /// Отправляет уведомление пользователям с определенной ролью
        /// </summary>
        Task SendNotificationToRoleAsync(
            string role,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal);
    }

    /// <summary>
    /// Фильтр для уведомлений
    /// </summary>
    public class NotificationFilter
    {
        public bool? IsRead { get; set; }
        public NotificationType? Type { get; set; }
        public NotificationPriority? Priority { get; set; }
        public string? Category { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsImportant { get; set; }
        public bool? IsExpired { get; set; }
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// Статистика уведомлений пользователя
    /// </summary>
    public class NotificationStatistics
    {
        public Guid UserUid { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int ImportantNotifications { get; set; }
        public int TodayNotifications { get; set; }
        public int WeekNotifications { get; set; }
        public Dictionary<NotificationType, int> ByType { get; set; } = new();
        public Dictionary<string, int> ByCategory { get; set; } = new();
        public Dictionary<NotificationPriority, int> ByPriority { get; set; } = new();
        public DateTime? LastNotificationDate { get; set; }
        public double AverageReadTime { get; set; } // в часах
    }

    /// <summary>
    /// Системная статистика уведомлений
    /// </summary>
    public class SystemNotificationStatistics
    {
        public int TotalNotifications { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public double AverageNotificationsPerUser { get; set; }
        public double ReadRate { get; set; }
        public Dictionary<NotificationType, int> TypeDistribution { get; set; } = new();
        public Dictionary<string, int> CategoryDistribution { get; set; } = new();
        public Dictionary<NotificationPriority, int> PriorityDistribution { get; set; } = new();
        public List<DailyNotificationStats> DailyStats { get; set; } = new();
        public DateTime? PeakHour { get; set; }
        public int PeakHourCount { get; set; }
    }

    /// <summary>
    /// Ежедневная статистика уведомлений
    /// </summary>
    public class DailyNotificationStats
    {
        public DateTime Date { get; set; }
        public int TotalSent { get; set; }
        public int TotalRead { get; set; }
        public double ReadRate { get; set; }
    }

    /// <summary>
    /// Результат массовой отправки уведомлений
    /// </summary>
    public class BulkNotificationResult
    {
        public int SuccessfulSends { get; set; }
        public int FailedSends { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<Guid> SentNotificationUids { get; set; } = new();
    }

    /// <summary>
    /// Настройки уведомлений пользователя
    /// </summary>
    public class NotificationSettings
    {
        public Guid UserUid { get; set; }
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public TimeSpan QuietHoursStart { get; set; } = TimeSpan.FromHours(22);
        public TimeSpan QuietHoursEnd { get; set; } = TimeSpan.FromHours(8);
        public bool WeekendNotifications { get; set; } = false;
        public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;
        public Dictionary<NotificationType, bool> TypeSettings { get; set; } = new();
        public Dictionary<string, bool> CategorySettings { get; set; } = new();
    }
}
