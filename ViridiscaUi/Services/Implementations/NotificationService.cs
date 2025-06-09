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
using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с уведомлениями
/// </summary>
public class NotificationService(ApplicationDbContext dbContext, ILogger<NotificationService> logger) : INotificationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<NotificationService> _logger = logger;

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
            PersonUid = recipientUid,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Category = category,
            ActionUrl = actionUrl,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Notification created for user {UserId}: {Title}", recipientUid, title);
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
            PersonUid = recipientUid,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Category = category,
            ActionUrl = actionUrl,
            SentAt = DateTime.UtcNow,
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
            .Where(n => n.PersonUid == userUid)
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
            .OrderByDescending(n => n.SentAt)
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
            .Where(n => n.PersonUid == userUid)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .AsQueryable();

        if (!includeRead)
            query = query.Where(n => !n.IsRead);

        query = query.OrderByDescending(n => n.SentAt);

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
            .Where(n => n.PersonUid == userUid && !n.IsRead)
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
            .Where(n => notificationUids.Contains(n.PersonUid))
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
            .Where(n => n.PersonUid == userUid && !n.IsRead)
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
            .Where(n => notificationUids.Contains(n.PersonUid))
            .ToListAsync();

        _dbContext.Notifications.RemoveRange(notifications);
        await _dbContext.SaveChangesAsync();
        return notifications.Count;
    }

    /// <summary>
    /// Массовая отправка уведомлений
    /// </summary>
    public async Task<BulkNotificationResult> SendBulkNotificationAsync(
        IEnumerable<Guid> personUids,
        string title,
        string message,
        NotificationType type,
        NotificationPriority priority,
        string? category = null,
        string? actionUrl = null,
        Dictionary<string, object>? metadata = null,
        DateTime? expiresAt = null)
    {
        var successCount = 0;
        var failureCount = 0;
        var errors = new List<string>();

        foreach (var personUid in personUids)
        {
            try
            {
                await SendNotificationAsync(personUid, title, message, type, priority, category, actionUrl, metadata, expiresAt);
                successCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                errors.Add($"Failed to send to {personUid}: {ex.Message}");
            }
        }

        return new BulkNotificationResult
        {
            SuccessfulSends = successCount,
            FailedSends = failureCount,
            Errors = errors,
            SentNotificationUids = new List<Guid>()
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

        await SendBulkNotificationAsync(enrolledStudents, title, message, type, NotificationPriority.Normal);
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
            .Where(n => n.PersonUid == userUid)
            .CountAsync();

        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .CountAsync();

        var todayNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.SentAt >= today)
            .CountAsync();

        var weekNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.SentAt >= weekAgo)
            .CountAsync();

        var monthNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.SentAt >= monthAgo)
            .CountAsync();

        var importantNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.Priority == NotificationPriority.High)
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
    /// Получает статистику уведомлений пользователя
    /// </summary>
    public async Task<NotificationStatistics> GetNotificationStatisticsAsync(Guid userUid) =>
        await GetUserNotificationStatisticsAsync(userUid);

    /// <summary>
    /// Очищает старые уведомления
    /// </summary>
    public async Task CleanupOldNotificationsAsync(TimeSpan maxAge)
    {
        var cutoffDate = DateTime.UtcNow - maxAge;
        
        var oldNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt < cutoffDate && n.IsRead)
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

    /// <summary>
    /// Получает статистику уведомлений пользователя (алиас для совместимости)
    /// </summary>
    public async Task<NotificationStatistics> GetUserStatisticsAsync(Guid userUid) => 
        await GetUserNotificationStatisticsAsync(userUid);

    /// <summary>
    /// Получает системную статистику уведомлений (алиас для совместимости)
    /// </summary>
    public async Task<SystemNotificationStatistics> GetSystemStatisticsAsync() => 
        await GetSystemNotificationStatisticsAsync();

    /// <summary>
    /// Получает системную статистику уведомлений
    /// </summary>
    public async Task<SystemNotificationStatistics> GetSystemNotificationStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);

        var totalNotifications = await _dbContext.Notifications.CountAsync();
        var unreadNotifications = await _dbContext.Notifications
            .Where(n => !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .CountAsync();

        var todayNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt >= today)
            .CountAsync();

        var weekNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt >= weekAgo)
            .CountAsync();

        var monthNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt >= monthAgo)
            .CountAsync();

        return new SystemNotificationStatistics
        {
            TotalNotifications = totalNotifications,
            UnreadNotifications = unreadNotifications,
            TodayNotifications = todayNotifications,
            WeekNotifications = weekNotifications,
            MonthNotifications = monthNotifications
        };
    }

    // Методы-заглушки для совместимости с интерфейсом
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
        await SendBulkNotificationAsync(recipientUids, "Template", "Message", NotificationType.Info, NotificationPriority.Normal);

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

    // === ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ИНТЕРФЕЙСА ===

    /// <summary>
    /// Получает все уведомления
    /// </summary>
    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _dbContext.Notifications
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    /// <summary>
    /// Получает уведомление по идентификатору
    /// </summary>
    public async Task<Notification?> GetByIdAsync(Guid uid)
    {
        return await _dbContext.Notifications.FindAsync(uid);
    }

    /// <summary>
    /// Получает уведомления пользователя
    /// </summary>
    public async Task<IEnumerable<Notification>> GetByPersonAsync(Guid personUid)
    {
        return await GetUserNotificationsAsync(personUid);
    }

    /// <summary>
    /// Создает новое уведомление
    /// </summary>
    public async Task<Notification> CreateAsync(Notification notification)
    {
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
        return notification;
    }

    /// <summary>
    /// Обновляет уведомление
    /// </summary>
    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _dbContext.Notifications.Update(notification);
        await _dbContext.SaveChangesAsync();
        return notification;
    }

    /// <summary>
    /// Удаляет уведомление (возвращает bool для совместимости с интерфейсом)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        return await DeleteNotificationAsync(uid);
    }

    /// <summary>
    /// Проверяет существование уведомления
    /// </summary>
    public async Task<bool> ExistsAsync(Guid uid)
    {
        return await _dbContext.Notifications.AnyAsync(n => n.Uid == uid);
    }

    /// <summary>
    /// Получает количество уведомлений
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await _dbContext.Notifications.CountAsync();
    }

    /// <summary>
    /// Отправляет уведомление пользователю (простая версия)
    /// </summary>
    public async Task SendNotificationAsync(Guid recipientUid, string title, string message)
    {
        await SendNotificationAsync(recipientUid, title, message, NotificationType.Info);
    }

    /// <summary>
    /// Отправляет уведомление пользователю с категорией
    /// </summary>
    public async Task SendNotificationAsync(Guid recipientUid, string title, string message, string category)
    {
        await SendNotificationAsync(recipientUid, title, message, NotificationType.Info, NotificationPriority.Normal, category);
    }

    /// <summary>
    /// Массовая отправка уведомлений (простая версия)
    /// </summary>
    public async Task SendBulkNotificationAsync(IEnumerable<Guid> recipientUids, string title, string message)
    {
        await SendBulkNotificationAsync(recipientUids, title, message, NotificationType.Info, NotificationPriority.Normal);
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
    /// Отправляет массовые уведомления
    /// </summary>
    public async Task<BulkNotificationResult> SendBulkNotificationsAsync(IEnumerable<Notification> notifications)
    {
        try
        {
            var notificationsList = notifications.ToList();
            var successCount = 0;
            var failureCount = 0;

            foreach (var notification in notificationsList)
            {
                try
                {
                    await SendNotificationAsync(notification.PersonUid, notification.Title, notification.Message, notification.Type, notification.Priority, notification.Category, notification.ActionUrl, null, notification.ExpiresAt);
                    successCount++;
                }
                catch
                {
                    failureCount++;
                }
            }

            return new BulkNotificationResult
            {
                SuccessfulSends = successCount,
                FailedSends = failureCount,
                Errors = new List<string>(),
                SentNotificationUids = notificationsList.Select(n => n.Uid).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке массовых уведомлений");
            var notificationsList = notifications.ToList();
            return new BulkNotificationResult
            {
                SuccessfulSends = 0,
                FailedSends = notificationsList.Count,
                Errors = new List<string> { ex.Message },
                SentNotificationUids = new List<Guid>()
            };
        }
    }

    /// <summary>
    /// Получает последние уведомления
    /// </summary>
    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(int count = 10)
    {
        try
        {
            return await _dbContext.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent notifications");
            return [];
        }
    }
}
