using System;
using System.Collections.Generic;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.System.Enums;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Настройки уведомлений пользователя
/// </summary>
public class NotificationSettings : ViewModelBase
{
    private Guid _userUid;
    private bool _emailNotifications = true;
    private bool _pushNotifications = true;
    private bool _smsNotifications = false;
    private TimeSpan _quietHoursStart = TimeSpan.FromHours(22);
    private TimeSpan _quietHoursEnd = TimeSpan.FromHours(8);
    private bool _weekendNotifications = false;
    private NotificationPriority _minimumPriority = NotificationPriority.Low;

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserUid
    {
        get => _userUid;
        set => this.RaiseAndSetIfChanged(ref _userUid, value);
    }

    /// <summary>
    /// Включены ли email уведомления
    /// </summary>
    public bool EmailNotifications
    {
        get => _emailNotifications;
        set => this.RaiseAndSetIfChanged(ref _emailNotifications, value);
    }

    /// <summary>
    /// Включены ли push уведомления
    /// </summary>
    public bool PushNotifications
    {
        get => _pushNotifications;
        set => this.RaiseAndSetIfChanged(ref _pushNotifications, value);
    }

    /// <summary>
    /// Включены ли SMS уведомления
    /// </summary>
    public bool SmsNotifications
    {
        get => _smsNotifications;
        set => this.RaiseAndSetIfChanged(ref _smsNotifications, value);
    }

    /// <summary>
    /// Начало тихих часов
    /// </summary>
    public TimeSpan QuietHoursStart
    {
        get => _quietHoursStart;
        set => this.RaiseAndSetIfChanged(ref _quietHoursStart, value);
    }

    /// <summary>
    /// Конец тихих часов
    /// </summary>
    public TimeSpan QuietHoursEnd
    {
        get => _quietHoursEnd;
        set => this.RaiseAndSetIfChanged(ref _quietHoursEnd, value);
    }

    /// <summary>
    /// Включены ли уведомления в выходные
    /// </summary>
    public bool WeekendNotifications
    {
        get => _weekendNotifications;
        set => this.RaiseAndSetIfChanged(ref _weekendNotifications, value);
    }

    /// <summary>
    /// Минимальный приоритет уведомлений
    /// </summary>
    public NotificationPriority MinimumPriority
    {
        get => _minimumPriority;
        set => this.RaiseAndSetIfChanged(ref _minimumPriority, value);
    }

    /// <summary>
    /// Настройки по типам уведомлений
    /// </summary>
    public Dictionary<NotificationType, bool> TypeSettings { get; set; } = new();

    /// <summary>
    /// Настройки по категориям уведомлений
    /// </summary>
    public Dictionary<string, bool> CategorySettings { get; set; } = new();

    /// <summary>
    /// Создает новый экземпляр настроек уведомлений
    /// </summary>
    public NotificationSettings()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр настроек уведомлений для пользователя
    /// </summary>
    public NotificationSettings(Guid userUid)
    {
        Uid = Guid.NewGuid();
        _userUid = userUid;
    }
} 