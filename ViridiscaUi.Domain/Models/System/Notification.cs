using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Уведомление пользователя
/// </summary>
public class Notification : ViewModelBase
{
    private Guid _recipientUid;
    private string _title = string.Empty;
    private string _message = string.Empty;
    private NotificationType _type;
    private NotificationPriority _priority;
    private bool _isRead;
    private DateTime _sentAt;
    private DateTime? _readAt;
    private User? _recipient;

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

/// <summary>
/// Тип уведомления
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Информационное
    /// </summary>
    Info = 0,

    /// <summary>
    /// Предупреждение
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Ошибка
    /// </summary>
    Error = 2,

    /// <summary>
    /// Успех
    /// </summary>
    Success = 3,

    /// <summary>
    /// Системное
    /// </summary>
    System = 4,

    /// <summary>
    /// Оценка
    /// </summary>
    Grade = 5,

    /// <summary>
    /// Посещаемость
    /// </summary>
    Attendance = 6,

    /// <summary>
    /// Задание
    /// </summary>
    Assignment = 7
}

/// <summary>
/// Приоритет уведомления
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Низкий
    /// </summary>
    Low = 0,

    /// <summary>
    /// Обычный
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Высокий
    /// </summary>
    High = 2,

    /// <summary>
    /// Критический
    /// </summary>
    Critical = 3
} 