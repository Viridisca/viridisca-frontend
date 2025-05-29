using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.System;

namespace ViridiscaUi.ViewModels.System;

public class NotificationViewModel : ReactiveObject
{
    private readonly Notification _notification;
    
    public NotificationViewModel(Notification notification)
    {
        _notification = notification ?? throw new ArgumentNullException(nameof(notification));
    }
    
    public Guid Uid => _notification.Uid;
    public string Title => _notification.Title;
    public string Message => _notification.Message;
    public bool IsRead => _notification.IsRead;
    public DateTime CreatedAt => _notification.SentAt;
    public string SenderName => "Система"; // Default sender name since model doesn't have this property
    
    public string TypeDisplayName => _notification.Type switch
    {
        Domain.Models.System.Enums.NotificationType.Info => "Информация",
        Domain.Models.System.Enums.NotificationType.Warning => "Предупреждение", 
        Domain.Models.System.Enums.NotificationType.Error => "Ошибка",
        Domain.Models.System.Enums.NotificationType.Success => "Успех",
        Domain.Models.System.Enums.NotificationType.System => "Система",
        Domain.Models.System.Enums.NotificationType.Grade => "Оценка",
        Domain.Models.System.Enums.NotificationType.Attendance => "Посещаемость",
        Domain.Models.System.Enums.NotificationType.Assignment => "Задание",
        Domain.Models.System.Enums.NotificationType.Reminder => "Напоминание",
        _ => "Неизвестно"
    };
    
    public Domain.Models.System.Enums.NotificationPriority Priority => _notification.Priority;
    
    public string PriorityDisplayName => Priority switch
    {
        Domain.Models.System.Enums.NotificationPriority.Low => "Низкий",
        Domain.Models.System.Enums.NotificationPriority.Normal => "Обычный",
        Domain.Models.System.Enums.NotificationPriority.High => "Высокий", 
        Domain.Models.System.Enums.NotificationPriority.Critical => "Критический",
        _ => "Неизвестно"
    };
    
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - CreatedAt;
            
            if (timeSpan.TotalMinutes < 1)
                return "Только что";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} мин назад";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ч назад";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} дн назад";
            
            return CreatedAt.ToString("dd.MM.yyyy");
        }
    }
} 