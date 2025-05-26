using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using System.Text.RegularExpressions;
using NotificationPriority = ViridiscaUi.Domain.Models.System.NotificationPriority;
using DomainNotificationTemplate = ViridiscaUi.Domain.Models.System.NotificationTemplate;
using DomainNotificationSettings = ViridiscaUi.Domain.Models.System.NotificationSettings;
using InterfaceNotificationSettings = ViridiscaUi.Services.Interfaces.NotificationSettings;
using static ViridiscaUi.Services.Interfaces.INotificationService;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с уведомлениями
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> CreateNotificationAsync(
            Guid recipientUid, 
            string title, 
            string message, 
            NotificationType type = NotificationType.Info,
            DateTime? scheduledFor = null)
        {
            var notification = new Notification
            {
                Uid = Guid.NewGuid(),
                RecipientUid = recipientUid,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                ScheduledFor = scheduledFor,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            return await CreateNotificationAsync(notification);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationUid, Guid userUid)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Uid == notificationUid && n.RecipientUid == userUid);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            Guid userUid, 
            bool includeRead = true, 
            int? limit = null)
        {
            var query = _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid);

            if (!includeRead)
            {
                query = query.Where(n => !n.IsRead);
            }

            query = query.OrderByDescending(n => n.CreatedAt);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userUid)
        {
            return await _dbContext.Notifications
                .CountAsync(n => n.RecipientUid == userUid && !n.IsRead);
        }

        public async Task SendNotificationToRoleAsync(
            string roleName, 
            string title, 
            string message, 
            NotificationType type = NotificationType.Info)
        {
            // Получаем всех пользователей с указанной ролью
            var userUids = await _dbContext.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.Name == roleName)
                .Select(ur => ur.UserUid)
                .ToListAsync();

            if (userUids.Any())
            {
                await SendBulkNotificationAsync(userUids, title, message, type, NotificationPriority.Normal);
            }
        }

        public async Task SendNotificationToGroupAsync(
            Guid groupUid, 
            string title, 
            string message, 
            NotificationType type = NotificationType.Info)
        {
            // Получаем всех студентов группы
            var studentUids = await _dbContext.Students
                .Where(s => s.GroupUid == groupUid)
                .Include(s => s.User)
                .Select(s => s.UserUid)
                .ToListAsync();

            if (studentUids.Any())
            {
                await SendBulkNotificationAsync(studentUids, title, message, type, NotificationPriority.Normal);
            }
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationUid, Guid userUid)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Uid == notificationUid && n.RecipientUid == userUid);

            if (notification == null)
                return false;

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetNotificationsPagedAsync(
            Guid userUid, 
            int page, 
            int pageSize, 
            bool includeRead = true,
            NotificationType? filterByType = null)
        {
            var query = _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid);

            if (!includeRead)
            {
                query = query.Where(n => !n.IsRead);
            }

            if (filterByType.HasValue)
            {
                query = query.Where(n => n.Type == filterByType.Value);
            }

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public async Task<Notification> ScheduleNotificationAsync(
            Guid recipientUid,
            string title,
            string message,
            DateTime scheduledFor,
            NotificationType type = NotificationType.Info)
        {
            var notification = new Notification
            {
                Uid = Guid.NewGuid(),
                RecipientUid = recipientUid,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                ScheduledFor = scheduledFor,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            return await CreateNotificationAsync(notification);
        }

        public async Task<bool> CancelScheduledNotificationAsync(Guid notificationUid)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Uid == notificationUid && n.ScheduledFor.HasValue && n.ScheduledFor > DateTime.UtcNow);

            if (notification == null)
                return false;

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Notification>> GetScheduledNotificationsAsync()
        {
            return await _dbContext.Notifications
                .Where(n => n.ScheduledFor.HasValue && n.ScheduledFor <= DateTime.UtcNow && !n.IsRead)
                .OrderBy(n => n.ScheduledFor)
                .ToListAsync();
        }

        // === РАСШИРЕНИЯ ЭТАПА 4: ЦЕНТР УВЕДОМЛЕНИЙ ===

        public async Task<Notification> CreateAdvancedNotificationAsync(
            Guid recipientUid,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string? category = null,
            string? actionUrl = null,
            Dictionary<string, object>? metadata = null,
            DateTime? expiresAt = null,
            DateTime? scheduledFor = null)
        {
            var notification = new Notification
            {
                Uid = Guid.NewGuid(),
                RecipientUid = recipientUid,
                Title = title,
                Message = message,
                Type = type,
                Priority = priority,
                Category = category,
                ActionUrl = actionUrl,
                MetadataJson = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null,
                ExpiresAt = expiresAt,
                ScheduledFor = scheduledFor,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            return await CreateNotificationAsync(notification);
        }

        public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetNotificationsAdvancedAsync(
            Guid userUid,
            int page,
            int pageSize,
            NotificationFilter filter)
        {
            var query = _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid);

            // Применяем фильтры
            if (filter.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == filter.IsRead.Value);
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(n => n.Type == filter.Type.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(n => n.Priority == filter.Priority.Value);
            }

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(n => n.Category == filter.Category);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);
            }

            if (filter.IsImportant.HasValue)
            {
                query = query.Where(n => n.IsImportant == filter.IsImportant.Value);
            }

            if (filter.IsExpired.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsExpired.Value)
                {
                    query = query.Where(n => n.ExpiresAt.HasValue && n.ExpiresAt.Value < now);
                }
                else
                {
                    query = query.Where(n => !n.ExpiresAt.HasValue || n.ExpiresAt.Value >= now);
                }
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(n => n.Title.Contains(filter.SearchTerm) || n.Message.Contains(filter.SearchTerm));
            }

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public async Task<NotificationStatistics> GetUserNotificationStatisticsAsync(Guid userUid)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid)
                .ToListAsync();

            var today = DateTime.Today;
            var weekAgo = today.AddDays(-7);

            var stats = new NotificationStatistics
            {
                UserUid = userUid,
                TotalNotifications = notifications.Count,
                UnreadNotifications = notifications.Count(n => !n.IsRead),
                ImportantNotifications = notifications.Count(n => n.IsImportant),
                TodayNotifications = notifications.Count(n => n.CreatedAt.Date == today),
                WeekNotifications = notifications.Count(n => n.CreatedAt >= weekAgo),
                LastNotificationDate = notifications.Any() ? notifications.Max(n => n.CreatedAt) : null
            };

            // Группировка по типам
            stats.ByType = notifications
                .GroupBy(n => n.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            // Группировка по категориям
            stats.ByCategory = notifications
                .Where(n => !string.IsNullOrEmpty(n.Category))
                .GroupBy(n => n.Category!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Группировка по приоритетам
            stats.ByPriority = notifications
                .GroupBy(n => n.Priority)
                .ToDictionary(g => g.Key, g => g.Count());

            // Среднее время прочтения
            var readNotifications = notifications.Where(n => n.IsRead && n.ReadAt.HasValue).ToList();
            if (readNotifications.Any())
            {
                var averageTicks = readNotifications.Average(n => (n.ReadAt!.Value - n.CreatedAt).TotalHours);
                stats.AverageReadTime = averageTicks;
            }

            return stats;
        }

        public async Task<SystemNotificationStatistics> GetSystemNotificationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var totalNotifications = await _dbContext.Notifications.CountAsync();
            var totalUsers = await _dbContext.Users.CountAsync();
            var activeUsers = await _dbContext.Notifications
                .Where(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .Select(n => n.RecipientUid)
                .Distinct()
                .CountAsync();

            var readNotifications = await _dbContext.Notifications
                .Where(n => n.IsRead)
                .CountAsync();

            var readRate = totalNotifications > 0 ? (double)readNotifications / totalNotifications * 100 : 0;

            var typeDistribution = await _dbContext.Notifications
                .GroupBy(n => n.Type)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var categoryDistribution = await _dbContext.Notifications
                .Where(n => !string.IsNullOrEmpty(n.Category))
                .GroupBy(n => n.Category!)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var priorityDistribution = await _dbContext.Notifications
                .GroupBy(n => n.Priority)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            // Дневная статистика за последние 30 дней
            var dailyStats = new List<DailyNotificationStats>();
            for (int i = 29; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var dayStart = date;
                var dayEnd = date.AddDays(1);

                var totalSent = await _dbContext.Notifications
                    .Where(n => n.CreatedAt >= dayStart && n.CreatedAt < dayEnd)
                    .CountAsync();

                var totalRead = await _dbContext.Notifications
                    .Where(n => n.CreatedAt >= dayStart && n.CreatedAt < dayEnd && n.IsRead)
                    .CountAsync();

                var dayReadRate = totalSent > 0 ? (double)totalRead / totalSent * 100 : 0;

                dailyStats.Add(new DailyNotificationStats
                {
                    Date = date,
                    TotalSent = totalSent,
                    TotalRead = totalRead,
                    ReadRate = dayReadRate
                });
            }

            // Пиковый час
            var hourlyStats = await _dbContext.Notifications
                .Where(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(n => n.CreatedAt.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            var stats = new SystemNotificationStatistics
            {
                TotalNotifications = totalNotifications,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                AverageNotificationsPerUser = totalUsers > 0 ? (double)totalNotifications / totalUsers : 0,
                ReadRate = readRate,
                TypeDistribution = typeDistribution,
                CategoryDistribution = categoryDistribution,
                PriorityDistribution = priorityDistribution,
                DailyStats = dailyStats,
                PeakHour = hourlyStats != null ? DateTime.Today.AddHours(hourlyStats.Hour) : null,
                PeakHourCount = hourlyStats?.Count ?? 0
            };

            return stats;
        }

        // Перегрузка без параметров для интерфейса
        public async Task<SystemNotificationStatistics> GetSystemNotificationStatisticsAsync()
        {
            return await GetSystemNotificationStatisticsAsync(null, null);
        }

        public async Task<Notification> SendFromTemplateAsync(
            Guid templateUid,
            Guid recipientUid,
            Dictionary<string, object>? parameters = null)
        {
            var template = await _dbContext.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Uid == templateUid && t.IsActive);

            if (template == null)
                throw new ArgumentException($"Template with ID {templateUid} not found or inactive");

            var title = ProcessTemplate(template.TitleTemplate, parameters);
            var message = ProcessTemplate(template.MessageTemplate, parameters);

            return await CreateAdvancedNotificationAsync(
                recipientUid,
                title,
                message,
                template.Type,
                template.Priority,
                template.Category);
        }

        public async Task<BulkNotificationResult> SendBulkFromTemplateAsync(
            Guid templateUid,
            IEnumerable<Guid> recipientUids,
            Dictionary<string, object>? parameters = null)
        {
            var result = new BulkNotificationResult();

            foreach (var recipientUid in recipientUids)
            {
                try
                {
                    var notification = await SendFromTemplateAsync(templateUid, recipientUid, parameters);
                    result.SuccessfulSends++;
                    result.SentNotificationUids.Add(notification.Uid);
                }
                catch (Exception ex)
                {
                    result.FailedSends++;
                    result.Errors.Add($"Failed to send to {recipientUid}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<bool> UpdateUserSettingsAsync(Guid userUid, DomainNotificationSettings settings)
        {
            var existingSettings = await _dbContext.NotificationSettings
                .FirstOrDefaultAsync(s => s.UserUid == userUid);

            if (existingSettings == null)
            {
                settings.UserUid = userUid;
                settings.LastModifiedAt = DateTime.UtcNow;
                await _dbContext.NotificationSettings.AddAsync(settings);
            }
            else
            {
                existingSettings.EmailNotifications = settings.EmailNotifications;
                existingSettings.PushNotifications = settings.PushNotifications;
                existingSettings.SmsNotifications = settings.SmsNotifications;
                existingSettings.TypeSettings = settings.TypeSettings;
                existingSettings.CategorySettings = settings.CategorySettings;
                existingSettings.QuietHoursStart = settings.QuietHoursStart;
                existingSettings.QuietHoursEnd = settings.QuietHoursEnd;
                existingSettings.WeekendNotifications = settings.WeekendNotifications;
                existingSettings.MinimumPriority = settings.MinimumPriority;
                existingSettings.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> ArchiveOldNotificationsAsync(DateTime olderThan)
        {
            var oldNotifications = await _dbContext.Notifications
                .Where(n => n.CreatedAt < olderThan && n.IsRead)
                .ToListAsync();

            _dbContext.Notifications.RemoveRange(oldNotifications);
            await _dbContext.SaveChangesAsync();

            return oldNotifications.Count;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByCategoryAsync(
            Guid userUid, 
            string category, 
            int? limit = null)
        {
            IQueryable<Notification> query = _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid && n.Category == category)
                .OrderByDescending(n => n.CreatedAt);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetHighPriorityNotificationsAsync(Guid userUid)
        {
            return await _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid && 
                           (n.Priority == NotificationPriority.High || n.Priority == NotificationPriority.Critical))
                .OrderByDescending(n => n.Priority)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkAsImportantAsync(Guid notificationUid, Guid userUid)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Uid == notificationUid && n.RecipientUid == userUid);

            if (notification == null)
                return false;

            notification.IsImportant = true;
            notification.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnmarkAsImportantAsync(Guid notificationUid, Guid userUid)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Uid == notificationUid && n.RecipientUid == userUid);

            if (notification == null)
                return false;

            notification.IsImportant = false;
            notification.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Notification>> GetImportantNotificationsAsync(Guid userUid)
        {
            return await _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid && n.IsImportant)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification> CreateReminderAsync(
            Guid userUid,
            string title,
            string message,
            DateTime remindAt,
            TimeSpan? repeatInterval = null)
        {
            var reminder = new Notification
            {
                Uid = Guid.NewGuid(),
                RecipientUid = userUid,
                Title = title,
                Message = message,
                Type = NotificationType.Reminder,
                Priority = NotificationPriority.Normal,
                Category = "Reminder",
                ScheduledFor = remindAt,
                RepeatInterval = repeatInterval,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            return await CreateNotificationAsync(reminder);
        }

        public async Task<IEnumerable<Notification>> GetActiveRemindersAsync(Guid userUid)
        {
            return await _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid && 
                           n.Type == NotificationType.Reminder &&
                           n.ScheduledFor.HasValue &&
                           n.ScheduledFor.Value > DateTime.UtcNow)
                .OrderBy(n => n.ScheduledFor)
                .ToListAsync();
        }

        public async Task ProcessScheduledNotificationsAsync()
        {
            var scheduledNotifications = await _dbContext.Notifications
                .Where(n => n.ScheduledFor.HasValue && 
                           n.ScheduledFor.Value <= DateTime.UtcNow && 
                           !n.IsRead)
                .ToListAsync();

            foreach (var notification in scheduledNotifications)
            {
                // Обрабатываем уведомление (отправляем)
                // В реальной системе здесь была бы логика отправки email/push/sms

                // Если это повторяющееся напоминание
                if (notification.RepeatInterval.HasValue)
                {
                    var nextReminder = new Notification
                    {
                        Uid = Guid.NewGuid(),
                        RecipientUid = notification.RecipientUid,
                        Title = notification.Title,
                        Message = notification.Message,
                        Type = notification.Type,
                        Priority = notification.Priority,
                        Category = notification.Category,
                        ScheduledFor = notification.ScheduledFor.Value.Add(notification.RepeatInterval.Value),
                        RepeatInterval = notification.RepeatInterval,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        LastModifiedAt = DateTime.UtcNow
                    };

                    await _dbContext.Notifications.AddAsync(nextReminder);
                }

                // Помечаем текущее уведомление как обработанное
                notification.ScheduledFor = null;
                notification.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        // Вспомогательный метод для обработки шаблонов
        private string ProcessTemplate(string template, Dictionary<string, object>? parameters)
        {
            if (parameters == null || !parameters.Any())
                return template;

            var result = template;
            foreach (var parameter in parameters)
            {
                var placeholder = $"{{{parameter.Key}}}";
                result = result.Replace(placeholder, parameter.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        // === НОВЫЕ МЕТОДЫ ДЛЯ СООТВЕТСТВИЯ ИНТЕРФЕЙСУ ===

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
            return await CreateAdvancedNotificationAsync(
                recipientUid, title, message, type, priority, category, actionUrl, metadata, expiresAt);
        }

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
            var result = new BulkNotificationResult();

            foreach (var recipientUid in recipientUids)
            {
                try
                {
                    var notification = await SendNotificationAsync(
                        recipientUid, title, message, type, priority, category, actionUrl, metadata, expiresAt);
                    result.SuccessfulSends++;
                    result.SentNotificationUids.Add(notification.Uid);
                }
                catch (Exception ex)
                {
                    result.FailedSends++;
                    result.Errors.Add($"Failed to send to {recipientUid}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsPagedAsync(
            Guid userUid,
            int page = 1,
            int pageSize = 20,
            bool includeRead = true,
            NotificationType? typeFilter = null,
            NotificationPriority? priorityFilter = null,
            string? categoryFilter = null)
        {
            var filter = new NotificationFilter
            {
                IsRead = includeRead ? null : false,
                Type = typeFilter,
                Priority = priorityFilter,
                Category = categoryFilter
            };

            return await GetNotificationsAdvancedAsync(userUid, page, pageSize, filter);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationUid)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Uid == notificationUid);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkMultipleAsReadAsync(IEnumerable<Guid> notificationUids)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => notificationUids.Contains(n.Uid) && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return notifications.Count;
        }

        public async Task<int> MarkAllAsReadAsync(Guid userUid)
        {
            var unreadNotifications = await _dbContext.Notifications
                .Where(n => n.RecipientUid == userUid && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return unreadNotifications.Count;
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationUid)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Uid == notificationUid);

            if (notification == null)
                return false;

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteMultipleNotificationsAsync(IEnumerable<Guid> notificationUids)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => notificationUids.Contains(n.Uid))
                .ToListAsync();

            _dbContext.Notifications.RemoveRange(notifications);
            await _dbContext.SaveChangesAsync();
            return notifications.Count;
        }

        public async Task<NotificationStatistics> GetUserStatisticsAsync(Guid userUid)
        {
            return await GetUserNotificationStatisticsAsync(userUid);
        }

        public async Task<SystemNotificationStatistics> GetSystemStatisticsAsync()
        {
            return await GetSystemNotificationStatisticsAsync();
        }

        public async Task<Notification> ScheduleNotificationAsync(
            Guid recipientUid,
            string title,
            string message,
            DateTime scheduledFor,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string? category = null,
            string? actionUrl = null,
            Dictionary<string, object>? metadata = null,
            TimeSpan? repeatInterval = null)
        {
            var notification = new Notification
            {
                Uid = Guid.NewGuid(),
                RecipientUid = recipientUid,
                Title = title,
                Message = message,
                Type = type,
                Priority = priority,
                Category = category,
                ActionUrl = actionUrl,
                MetadataJson = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null,
                ScheduledFor = scheduledFor,
                RepeatInterval = repeatInterval,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            return await CreateNotificationAsync(notification);
        }

        public async Task<Notification> CreateReminderAsync(
            Guid userUid,
            string title,
            string message,
            DateTime reminderTime,
            TimeSpan? repeatInterval = null,
            Dictionary<string, object>? metadata = null)
        {
            return await ScheduleNotificationAsync(
                userUid, title, message, reminderTime, NotificationType.Reminder, 
                NotificationPriority.Normal, "Reminder", null, metadata, repeatInterval);
        }

        public async Task SendOverdueAssignmentNotificationsAsync()
        {
            try
            {
                var overdueAssignments = await _dbContext.Assignments
                    .Where(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.UtcNow && a.Status != AssignmentStatus.Closed)
                    .Include(a => a.Course)
                    .ToListAsync();

                foreach (var assignment in overdueAssignments)
                {
                    var enrolledStudents = await _dbContext.Enrollments
                        .Where(e => e.CourseUid == assignment.CourseUid)
                        .Include(e => e.Student)
                        .Select(e => e.Student.UserUid)
                        .ToListAsync();

                    await SendBulkNotificationAsync(
                        enrolledStudents,
                        "Просроченное задание",
                        $"Задание '{assignment.Title}' просрочено. Срок сдачи: {assignment.DueDate:dd.MM.yyyy HH:mm}",
                        NotificationType.Warning,
                        NotificationPriority.High,
                        "Assignment");
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Error sending overdue assignment notifications: {ex.Message}");
            }
        }

        public async Task SendUpcomingDeadlineNotificationsAsync()
        {
            try
            {
                var upcomingDeadline = DateTime.UtcNow.AddDays(1); // За день до дедлайна
                var upcomingAssignments = await _dbContext.Assignments
                    .Where(a => a.DueDate.HasValue && 
                               a.DueDate.Value > DateTime.UtcNow && 
                               a.DueDate.Value <= upcomingDeadline &&
                               a.Status != AssignmentStatus.Closed)
                    .Include(a => a.Course)
                    .ToListAsync();

                foreach (var assignment in upcomingAssignments)
                {
                    var enrolledStudents = await _dbContext.Enrollments
                        .Where(e => e.CourseUid == assignment.CourseUid)
                        .Include(e => e.Student)
                        .Select(e => e.Student.UserUid)
                        .ToListAsync();

                    await SendBulkNotificationAsync(
                        enrolledStudents,
                        "Приближается дедлайн",
                        $"Задание '{assignment.Title}' необходимо сдать до {assignment.DueDate:dd.MM.yyyy HH:mm}",
                        NotificationType.Warning,
                        NotificationPriority.Normal,
                        "Assignment");
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Error sending upcoming deadline notifications: {ex.Message}");
            }
        }

        public async Task CleanupOldNotificationsAsync(TimeSpan maxAge)
        {
            var cutoffDate = DateTime.UtcNow.Subtract(maxAge);
            var oldNotifications = await _dbContext.Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ToListAsync();

            _dbContext.Notifications.RemoveRange(oldNotifications);
            await _dbContext.SaveChangesAsync();
        }

        // Метод для интерфейса с NotificationSettings
        public async Task<InterfaceNotificationSettings> GetUserSettingsAsync(Guid userUid)
        {
            var domainSettings = await GetDomainUserSettingsAsync(userUid);
            
            return new InterfaceNotificationSettings
            {
                UserUid = domainSettings.UserUid,
                EmailNotifications = domainSettings.EmailNotifications,
                PushNotifications = domainSettings.PushNotifications,
                SmsNotifications = domainSettings.SmsNotifications,
                QuietHoursStart = domainSettings.QuietHoursStart,
                QuietHoursEnd = domainSettings.QuietHoursEnd,
                WeekendNotifications = domainSettings.WeekendNotifications,
                MinimumPriority = domainSettings.MinimumPriority,
                TypeSettings = domainSettings.TypeSettings,
                CategorySettings = domainSettings.CategorySettings
            };
        }

        // Внутренний метод для работы с Domain моделями
        private async Task<DomainNotificationSettings> GetDomainUserSettingsAsync(Guid userUid)
        {
            var settings = await _dbContext.NotificationSettings
                .FirstOrDefaultAsync(s => s.UserUid == userUid);

            if (settings == null)
            {
                // Создаем настройки по умолчанию
                settings = new DomainNotificationSettings
                {
                    UserUid = userUid,
                    EmailNotifications = true,
                    PushNotifications = true,
                    SmsNotifications = false,
                    WeekendNotifications = false,
                    MinimumPriority = NotificationPriority.Low,
                    LastModifiedAt = DateTime.UtcNow
                };

                await _dbContext.NotificationSettings.AddAsync(settings);
                await _dbContext.SaveChangesAsync();
            }

            return settings;
        }

        // Метод для интерфейса с NotificationSettings
        public async Task<bool> UpdateUserSettingsAsync(Guid userUid, InterfaceNotificationSettings settings)
        {
            // Конвертируем NotificationSettings в DomainNotificationSettings
            var domainSettings = new DomainNotificationSettings
            {
                UserUid = userUid,
                EmailNotifications = settings.EmailNotifications,
                PushNotifications = settings.PushNotifications,
                SmsNotifications = settings.SmsNotifications,
                QuietHoursStart = settings.QuietHoursStart,
                QuietHoursEnd = settings.QuietHoursEnd,
                WeekendNotifications = settings.WeekendNotifications,
                MinimumPriority = settings.MinimumPriority,
                TypeSettings = settings.TypeSettings,
                CategorySettings = settings.CategorySettings
            };

            return await UpdateUserSettingsAsync(userUid, domainSettings);
        }

        // === НЕДОСТАЮЩИЕ МЕТОДЫ ИНТЕРФЕЙСА ===
        
        public async Task<Notification> CreateNotificationAsync(
            Guid recipientUid,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string? category = null,
            string? actionUrl = null)
        {
            return await SendNotificationAsync(recipientUid, title, message, type, priority, category, actionUrl);
        }
        
        public async Task SendNotificationToRoleAsync(
            string role,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            await SendNotificationToRoleAsync(role, title, message, type);
        }

        public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template)
        {
            var domainTemplate = new DomainNotificationTemplate
            {
                Uid = template.Uid,
                Name = template.Name,
                Description = template.Description,
                TitleTemplate = template.TitleTemplate,
                MessageTemplate = template.MessageTemplate,
                Type = template.Type,
                Priority = template.Priority,
                Category = template.Category,
                IsActive = template.IsActive,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            await _dbContext.NotificationTemplates.AddAsync(domainTemplate);
            await _dbContext.SaveChangesAsync();
            
            return new NotificationTemplate
            {
                Uid = domainTemplate.Uid,
                Name = domainTemplate.Name,
                Description = domainTemplate.Description,
                TitleTemplate = domainTemplate.TitleTemplate,
                MessageTemplate = domainTemplate.MessageTemplate,
                Type = domainTemplate.Type,
                Priority = domainTemplate.Priority,
                Category = domainTemplate.Category,
                IsActive = domainTemplate.IsActive,
                CreatedAt = domainTemplate.CreatedAt,
                LastModifiedAt = domainTemplate.LastModifiedAt
            };
        }
        
        public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync()
        {
            var domainTemplates = await _dbContext.NotificationTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
            
            return domainTemplates.Select(t => new NotificationTemplate
            {
                Uid = t.Uid,
                Name = t.Name,
                Description = t.Description,
                TitleTemplate = t.TitleTemplate,
                MessageTemplate = t.MessageTemplate,
                Type = t.Type,
                Priority = t.Priority,
                Category = t.Category,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                LastModifiedAt = t.LastModifiedAt
            });
        }
    }
} 