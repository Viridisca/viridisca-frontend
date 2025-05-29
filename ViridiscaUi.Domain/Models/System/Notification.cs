using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Auth;
using ReactiveUI;
using ViridiscaUi.Domain.Models.System.Enums;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Уведомление пользователя
/// </summary>
public class Notification : ViewModelBase
{
    private Guid _recipientUid;
    private User? _recipient;
    
    private string _title = string.Empty;
    private string _message = string.Empty;

    private NotificationType _type;
    private NotificationPriority _priority;
    
    private bool _isRead;
    private bool _isImportant;
    
    private DateTime _sentAt;
    private DateTime? _readAt;
    
    private string? _category;
    private string? _actionUrl;
    private string? _metadataJson;
    
    private DateTime? _expiresAt;
    private DateTime? _scheduledFor;
    private TimeSpan? _repeatInterval;

    private Dictionary<string, object>? _metadata;

    /// <summary>
    /// Идентификатор получателя
    /// </summary>
    public Guid RecipientUid
    {
        get => _recipientUid;
        set => this.RaiseAndSetIfChanged(ref _recipientUid, value);
    }

    /// <summary>
    /// Заголовок уведомления
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Текст уведомления
    /// </summary>
    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    /// <summary>
    /// Тип уведомления
    /// </summary>
    public NotificationType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Приоритет уведомления
    /// </summary>
    public NotificationPriority Priority
    {
        get => _priority;
        set => this.RaiseAndSetIfChanged(ref _priority, value);
    }

    /// <summary>
    /// Флаг прочитанности
    /// </summary>
    public bool IsRead
    {
        get => _isRead;
        set => this.RaiseAndSetIfChanged(ref _isRead, value);
    }

    /// <summary>
    /// Время отправки
    /// </summary>
    public DateTime SentAt
    {
        get => _sentAt;
        set => this.RaiseAndSetIfChanged(ref _sentAt, value);
    }

    /// <summary>
    /// Время прочтения
    /// </summary>
    public DateTime? ReadAt
    {
        get => _readAt;
        set => this.RaiseAndSetIfChanged(ref _readAt, value);
    }

    /// <summary>
    /// Получатель уведомления
    /// </summary>
    public User? Recipient
    {
        get => _recipient;
        set => this.RaiseAndSetIfChanged(ref _recipient, value);
    }

    /// <summary>
    /// Категория уведомления
    /// </summary>
    public string? Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    /// <summary>
    /// URL для действия
    /// </summary>
    public string? ActionUrl
    {
        get => _actionUrl;
        set => this.RaiseAndSetIfChanged(ref _actionUrl, value);
    }

    /// <summary>
    /// Флаг важности
    /// </summary>
    public bool IsImportant
    {
        get => _isImportant;
        set => this.RaiseAndSetIfChanged(ref _isImportant, value);
    }

    /// <summary>
    /// Время истечения
    /// </summary>
    public DateTime? ExpiresAt
    {
        get => _expiresAt;
        set => this.RaiseAndSetIfChanged(ref _expiresAt, value);
    }

    /// <summary>
    /// Запланированное время отправки
    /// </summary>
    public DateTime? ScheduledFor
    {
        get => _scheduledFor;
        set => this.RaiseAndSetIfChanged(ref _scheduledFor, value);
    }

    /// <summary>
    /// Интервал повтора для напоминаний
    /// </summary>
    public TimeSpan? RepeatInterval
    {
        get => _repeatInterval;
        set => this.RaiseAndSetIfChanged(ref _repeatInterval, value);
    }

    /// <summary>
    /// Дополнительные метаданные
    /// </summary>
    public Dictionary<string, object>? Metadata
    {
        get => _metadata;
        set => this.RaiseAndSetIfChanged(ref _metadata, value);
    }

    /// <summary>
    /// Дополнительные метаданные в формате JSON
    /// </summary>
    public string? MetadataJson
    {
        get => _metadataJson;
        set => this.RaiseAndSetIfChanged(ref _metadataJson, value);
    }

    /// <summary>
    /// Создает новый экземпляр уведомления
    /// </summary>
    public Notification()
    {
        _sentAt = DateTime.UtcNow;
        _priority = NotificationPriority.Normal;
    }

    /// <summary>
    /// Создает новый экземпляр уведомления с указанными параметрами
    /// </summary>
    public Notification(
        Guid recipientUid,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationPriority priority = NotificationPriority.Normal)
    {
        Uid = Guid.NewGuid();
        _recipientUid = recipientUid;
        _title = title;
        _message = message;
        _type = type;
        _priority = priority;
        _sentAt = DateTime.UtcNow;
        _isRead = false;
    }

    /// <summary>
    /// Отмечает уведомление как прочитанное
    /// </summary>
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}
