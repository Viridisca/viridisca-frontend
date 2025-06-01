using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Domain.Models.System.Enums;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с уведомлениями
/// </summary>
public class NotificationService(ApplicationDbContext dbContext) : INotificationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Создает новое уведомление
    /// </summary>
    public async Task<Notification> CreateNotificationAsync(
        Guid recipientUid, 
        string title, 
        string message, 
        NotificationType type = NotificationType.Info, 
        NotificationPriority priority = NotificationPriority.Normal, 
        string? category = null, 
        string? actionUrl = null)
    {
        var notification = new Notification
        {
            Uid = Guid.NewGuid(),
            RecipientUid = recipientUid,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Category = category, // ParseCategoryToUid(category),
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        return notification;
    }

    /// <summary>
    /// Отправляет уведомление пользователю
    /// </summary>
    public async Task<Notification> SendNotificationAsync(
        Guid recipientUid, 
        string title, 
        string message, 
        NotificationType type = NotificationType.Info, 
        NotificationPriority priority = NotificationPriority.Normal, 
        string? category = null, 
        string? actionUrl = null, 
        Dictionary<string, object>? metadata = null, 
        DateTime? expiresAt = null)
    {
        var notification = new Notification
        {
            Uid = Guid.NewGuid(),
            RecipientUid = recipientUid,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Category = category, // ParseCategoryToUid(category),
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            ExpiresAt = expiresAt
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        return notification;
    }

    /// <summary>
    /// Получает уведомления пользователя с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsPagedAsync(
        Guid userUid, 
        int page = 1, 
        int pageSize = 20, 
        bool includeRead = true, 
        NotificationType? typeFilter = null, 
        NotificationPriority? priorityFilter = null, 
        string? categoryFilter = null)
    {
        var query = _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid)
            .AsQueryable();

        if (!includeRead)
            query = query.Where(n => !n.IsRead);

        if (typeFilter.HasValue)
            query = query.Where(n => n.Type == typeFilter.Value);

        if (priorityFilter.HasValue)
            query = query.Where(n => n.Priority == priorityFilter.Value);

        //if (!string.IsNullOrEmpty(categoryFilter) && Guid.TryParse(categoryFilter, out var categoryUid))
        //    query = query.Where(n => n.Category == categoryUid);

        // Исключаем истекшие уведомления
        query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

        var totalCount = await query.CountAsync();
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (notifications, totalCount);
    }

    /// <summary>
    /// Получает все уведомления пользователя
    /// </summary>
    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        Guid userUid, 
        bool includeRead = true, 
        int? limit = null)
    {
        var query = _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .AsQueryable();

        if (!includeRead)
            query = query.Where(n => !n.IsRead);

        query = query.OrderByDescending(n => n.CreatedAt);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Получает количество непрочитанных уведомлений
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userUid)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .CountAsync();
    }

    /// <summary>
    /// Отмечает уведомление как прочитанное
    /// </summary>
    public async Task<bool> MarkAsReadAsync(Guid notificationUid)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationUid);
        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Отмечает несколько уведомлений как прочитанные
    /// </summary>
    public async Task<int> MarkMultipleAsReadAsync(IEnumerable<Guid> notificationUids)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => notificationUids.Contains(n.Uid))
            .ToListAsync();

        var count = 0;
        foreach (var notification in notifications)
        {
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                count++;
            }
        }

        await _dbContext.SaveChangesAsync();
        return count;
    }

    /// <summary>
    /// Отмечает все уведомления пользователя как прочитанные
    /// </summary>
    public async Task<int> MarkAllAsReadAsync(Guid userUid)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return notifications.Count;
    }

    /// <summary>
    /// Удаляет уведомление
    /// </summary>
    public async Task<bool> DeleteNotificationAsync(Guid notificationUid)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationUid);
        if (notification == null)
            return false;

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Удаляет несколько уведомлений
    /// </summary>
    public async Task<int> DeleteMultipleNotificationsAsync(IEnumerable<Guid> notificationUids)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => notificationUids.Contains(n.Uid))
            .ToListAsync();

        _dbContext.Notifications.RemoveRange(notifications);
        await _dbContext.SaveChangesAsync();
        return notifications.Count;
    }

    /// <summary>
    /// Массовая отправка уведомлений
    /// </summary>
    public async Task<BulkNotificationResult> SendBulkNotificationAsync(
        IEnumerable<Guid> recipientUids, 
        string title, 
        string message, 
        NotificationType type = NotificationType.Info, 
        NotificationPriority priority = NotificationPriority.Normal, 
        string? category = null, 
        string? actionUrl = null, 
        Dictionary<string, object>? metadata = null, 
        DateTime? expiresAt = null)
    {
        var notifications = new List<Notification>();
        var errors = new List<string>();
        var successCount = 0;

        foreach (var recipientUid in recipientUids)
        {
            try
            {
                var notification = new Notification
                {
                    Uid = Guid.NewGuid(),
                    RecipientUid = recipientUid,
                    Title = title,
                    Message = message,
                    Type = type,
                    Priority = priority,
                    Category = category, // ParseCategoryToUid(category),
                    ActionUrl = actionUrl,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    ExpiresAt = expiresAt
                };

                notifications.Add(notification);
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to create notification for user {recipientUid}: {ex.Message}");
            }
        }

        if (notifications.Any())
        {
            _dbContext.Notifications.AddRange(notifications);
            await _dbContext.SaveChangesAsync();
        }

        return new BulkNotificationResult
        {
            SuccessfulSends = successCount,
            FailedSends = errors.Count,
            Errors = errors
        };
    }

    /// <summary>
    /// Отправляет общее уведомление
    /// </summary>
    public async Task SendNotificationAsync(string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal)
    {
        try
        {
            // В реальной реализации здесь будет отправка системного уведомления всем пользователям
            // Пока просто логируем
            System.Diagnostics.Debug.WriteLine($"General notification sent: {title} - {message}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending general notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Отправляет уведомление пользователям с определенной ролью
    /// </summary>
    public async Task SendNotificationToRoleAsync(string role, string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal)
    {
        try
        {
            // Получаем всех пользователей с указанными ролями
            var targetPersons = await _dbContext.PersonRoles
                .Where(pr => pr.Role.Name == role)
                .Include(pr => pr.Person)
                .Select(pr => pr.Person)
                .Distinct()
                .ToListAsync();

            await SendBulkNotificationAsync(targetPersons.Select(p => p.Uid), title, message, type, priority);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending notification to role: {ex.Message}");
        }
    }

    /// <summary>
    /// Отправляет уведомление всем студентам курса
    /// </summary>
    public async Task SendNotificationToCourseAsync(Guid courseInstanceUid, string title, string message, NotificationType type)
    {
        // Получаем студентов через записи на курс (Enrollments)
        var enrolledStudents = await _dbContext.Enrollments
            .Where(e => e.CourseInstanceUid == courseInstanceUid)
            .Select(e => e.StudentUid)
            .ToListAsync();

        await SendBulkNotificationAsync(enrolledStudents, title, message, type);
    }

    /// <summary>
    /// Получает статистику уведомлений пользователя
    /// </summary>
    public async Task<NotificationStatistics> GetUserNotificationStatisticsAsync(Guid userUid)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);

        var totalNotifications = await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid)
            .CountAsync();

        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .CountAsync();

        var todayNotifications = await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid && n.CreatedAt >= today)
            .CountAsync();

        var weekNotifications = await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid && n.CreatedAt >= weekAgo)
            .CountAsync();

        var monthNotifications = await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid && n.CreatedAt >= monthAgo)
            .CountAsync();

        var importantNotifications = await _dbContext.Notifications
            .Where(n => n.RecipientUid == userUid && n.Priority == NotificationPriority.High)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .CountAsync();

        return new NotificationStatistics
        {
            UserUid = userUid,
            TotalNotifications = totalNotifications,
            UnreadNotifications = unreadNotifications,
            TodayNotifications = todayNotifications,
            WeekNotifications = weekNotifications,
            ImportantNotifications = importantNotifications
        };
    }

    /// <summary>
    /// Получает системную статистику уведомлений
    /// </summary>
    public async Task<SystemNotificationStatistics> GetSystemNotificationStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        var totalNotifications = await _dbContext.Notifications.CountAsync();
        var unreadNotifications = await _dbContext.Notifications
            .Where(n => !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .CountAsync();
        var readNotifications = totalNotifications - unreadNotifications;
        var errorNotifications = 0; // В будущем можно добавить логику для отслеживания ошибок

        return new SystemNotificationStatistics
        {
            TotalNotifications = totalNotifications,
            TotalUsers = 100, // Заглушка
            ActiveUsers = 80, // Заглушка
            AverageNotificationsPerUser = totalNotifications / 100.0,
            ReadRate = totalNotifications > 0 ? (double)(totalNotifications - unreadNotifications) / totalNotifications * 100 : 0
        };
    }

    /// <summary>
    /// Очищает старые уведомления
    /// </summary>
    public async Task CleanupOldNotificationsAsync(TimeSpan maxAge)
    {
        var cutoffDate = DateTime.UtcNow - maxAge;
        
        var oldNotifications = await _dbContext.Notifications
            .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
            .ToListAsync();

        _dbContext.Notifications.RemoveRange(oldNotifications);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Архивирует старые уведомления
    /// </summary>
    public async Task<int> ArchiveOldNotificationsAsync(DateTime olderThan)
    {
        var notificationsToArchive = await _dbContext.Notifications
            .Where(n => n.CreatedAt < olderThan && n.IsRead)
            .ToListAsync();

        // В будущем можно добавить архивную таблицу
        // Пока просто удаляем старые прочитанные уведомления
        _dbContext.Notifications.RemoveRange(notificationsToArchive);
        await _dbContext.SaveChangesAsync();

        return notificationsToArchive.Count;
    }

    /// <summary>
    /// Отправляет уведомления о просроченных заданиях
    /// </summary>
    public async Task SendOverdueAssignmentNotificationsAsync()
    {
        var overdueAssignments = await _dbContext.Assignments
            .Where(a => a.DueDate.HasValue && a.DueDate < DateTime.UtcNow)
            .Where(a => a.Status == AssignmentStatus.Published)
            .Include(a => a.CourseInstance)
            .ToListAsync();

        foreach (var assignment in overdueAssignments)
        {
            await SendNotificationToCourseAsync(
                assignment.CourseInstanceUid,
                "Просроченное задание",
                $"Задание '{assignment.Title}' просрочено. Срок сдачи был: {assignment.DueDate:dd.MM.yyyy}",
                NotificationType.Warning);
        }
    }

    /// <summary>
    /// Отправляет уведомления о приближающихся дедлайнах
    /// </summary>
    public async Task SendUpcomingDeadlineNotificationsAsync()
    {
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var dayAfterTomorrow = DateTime.UtcNow.AddDays(2);

        var upcomingAssignments = await _dbContext.Assignments
            .Where(a => a.DueDate.HasValue && a.DueDate >= tomorrow && a.DueDate <= dayAfterTomorrow)
            .Where(a => a.Status == AssignmentStatus.Published)
            .Include(a => a.CourseInstance)
            .ToListAsync();

        foreach (var assignment in upcomingAssignments)
        {
            await SendNotificationToCourseAsync(
                assignment.CourseInstanceUid,
                "Приближается дедлайн",
                $"Задание '{assignment.Title}' нужно сдать до {assignment.DueDate:dd.MM.yyyy HH:mm}",
                NotificationType.Info);
        }
    }

    /// <summary>
    /// Уведомляет родителей об оценках
    /// </summary>
    public async Task NotifyParentsAboutGradesAsync(IEnumerable<Grade> grades)
    {
        foreach (var grade in grades)
        {
            // В будущем можно добавить связь студент-родитель
            // Пока отправляем уведомление самому студенту
            await CreateNotificationAsync(
                grade.StudentUid,
                "Новая оценка",
                $"Вы получили оценку {grade.Value} за задание",
                NotificationType.Info);
        }
    }

    // Методы-заглушки для совместимости с интерфейсом
    public async Task<NotificationStatistics> GetUserStatisticsAsync(Guid userUid) => 
        await GetUserNotificationStatisticsAsync(userUid);

    public async Task<SystemNotificationStatistics> GetSystemStatisticsAsync() => 
        await GetSystemNotificationStatisticsAsync();

    public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetNotificationsAdvancedAsync(
        Guid userUid, int page, int pageSize, NotificationFilter filter) =>
        await GetUserNotificationsPagedAsync(userUid, page, pageSize);

    public async Task<bool> MarkAsImportantAsync(Guid notificationUid, Guid userUid)
    {
        // Можно добавить поле IsImportant в модель Notification
        return true;
    }

    public async Task<bool> UnmarkAsImportantAsync(Guid notificationUid, Guid userUid)
    {
        // Можно добавить поле IsImportant в модель Notification
        return true;
    }

    public async Task<Notification> ScheduleNotificationAsync(
        Guid recipientUid, string title, string message, DateTime scheduledFor, 
        NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal, 
        string? category = null, string? actionUrl = null, Dictionary<string, object>? metadata = null, 
        TimeSpan? repeatInterval = null)
    {
        // В будущем можно добавить планировщик задач
        return await CreateNotificationAsync(recipientUid, title, message, type, priority, category, actionUrl);
    }

    public async Task<bool> CancelScheduledNotificationAsync(Guid notificationUid) 
    {
        await Task.CompletedTask;
        return true;
    }

    public async Task ProcessScheduledNotificationsAsync() 
    {
        await Task.CompletedTask;
    }

    public async Task<Notification> CreateReminderAsync(
        Guid userUid, string title, string message, DateTime reminderTime, 
        TimeSpan? repeatInterval = null, Dictionary<string, object>? metadata = null) =>
        await CreateNotificationAsync(userUid, title, message, NotificationType.Reminder);

    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync() 
    {
        await Task.CompletedTask;
        return new List<NotificationTemplate>();
    }

    public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template) => template;

    public async Task<Notification> SendFromTemplateAsync(
        Guid templateUid, Guid recipientUid, Dictionary<string, object>? parameters = null) =>
        await CreateNotificationAsync(recipientUid, "Template", "Message");

    public async Task<BulkNotificationResult> SendBulkFromTemplateAsync(
        Guid templateUid, IEnumerable<Guid> recipientUids, Dictionary<string, object>? parameters = null) =>
        await SendBulkNotificationAsync(recipientUids, "Template", "Message");

    public async Task<Interfaces.NotificationSettings> GetUserSettingsAsync(Guid userUid) 
    {
        await Task.CompletedTask;
        return new Interfaces.NotificationSettings
        {
            EmailNotifications = true,
            PushNotifications = true,
            SmsNotifications = false
        };
    }

    public async Task<bool> UpdateUserSettingsAsync(Guid userUid, Interfaces.NotificationSettings settings) 
    {
        await Task.CompletedTask;
        return true;
    }

    public async Task<Domain.Models.System.NotificationSettings> GetNotificationSettingsAsync(Guid userUid) =>
        new Domain.Models.System.NotificationSettings
        {
            UserUid = userUid,
            EmailNotifications = true,
            PushNotifications = true,
            SmsNotifications = false,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

    public async Task<NotificationTemplate> GetNotificationTemplateAsync(string templateName) =>
        new NotificationTemplate
        {
            Uid = Guid.NewGuid(),
            Name = templateName,
            TitleTemplate = $"Шаблон {templateName}",
            MessageTemplate = $"Содержимое шаблона {templateName}",
            CreatedAt = DateTime.UtcNow
        };

    public async Task<NotificationStatistics> GetNotificationStatisticsAsync(Guid userUid) =>
        await GetUserNotificationStatisticsAsync(userUid);

    public async Task<BulkNotificationResult> SendBulkNotificationAsync(
        IEnumerable<Guid> userUids, string title, string message, NotificationType type) =>
        await SendBulkNotificationAsync(userUids, title, message, type);

    /// <summary>
    /// Показывает уведомление об успехе
    /// </summary>
    public void ShowSuccess(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"SUCCESS: {message}");
    }

    /// <summary>
    /// Показывает уведомление об ошибке
    /// </summary>
    public void ShowError(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"ERROR: {message}");
    }

    /// <summary>
    /// Показывает информационное уведомление
    /// </summary>
    public void ShowInfo(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"INFO: {message}");
    }

    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    public void ShowWarning(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"WARNING: {message}");
    } 
}
