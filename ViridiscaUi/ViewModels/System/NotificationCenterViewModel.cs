using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Domain.Models.System;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// Простая ViewModel для центра уведомлений
/// </summary>
public class NotificationCenterViewModel : ReactiveObject
{
    [Reactive] public string Title { get; set; } = "Центр уведомлений";
    [Reactive] public int UnreadCount { get; set; } = 3;
    
    public ObservableCollection<NotificationItem> Notifications { get; } = new();

    public NotificationCenterViewModel(INotificationService? notificationService = null)
    {
        // Добавляем тестовые уведомления
        Notifications.Add(new NotificationItem 
        { 
            Title = "Новое задание", 
            Message = "Добавлено новое задание по математике",
            CreatedAt = DateTime.Now.AddHours(-1),
            IsRead = false
        });
        Notifications.Add(new NotificationItem 
        { 
            Title = "Оценка выставлена", 
            Message = "Получена оценка за лабораторную работу",
            CreatedAt = DateTime.Now.AddHours(-3),
            IsRead = false
        });
        Notifications.Add(new NotificationItem 
        { 
            Title = "Напоминание", 
            Message = "Завтра срок сдачи проекта",
            CreatedAt = DateTime.Now.AddDays(-1),
            IsRead = true
        });
    }
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string TimeAgo => GetTimeAgo(CreatedAt);
    
    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        
        if (timeSpan.TotalMinutes < 1)
            return "только что";
        if (timeSpan.TotalHours < 1)
            return $"{(int)timeSpan.TotalMinutes} мин назад";
        if (timeSpan.TotalDays < 1)
            return $"{(int)timeSpan.TotalHours} ч назад";
        
        return $"{(int)timeSpan.TotalDays} дн назад";
    }
} 