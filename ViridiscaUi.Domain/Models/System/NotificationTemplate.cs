using System;
using System.Collections.Generic;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.System.Enums;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Шаблон уведомления
/// </summary>
public class NotificationTemplate : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _titleTemplate = string.Empty;
    private string _messageTemplate = string.Empty;
    private NotificationType _type = NotificationType.Info;
    private NotificationPriority _priority = NotificationPriority.Normal;
    private string? _category;
    private bool _isActive = true;

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание шаблона
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Шаблон заголовка
    /// </summary>
    public string TitleTemplate
    {
        get => _titleTemplate;
        set => this.RaiseAndSetIfChanged(ref _titleTemplate, value);
    }

    /// <summary>
    /// Шаблон сообщения
    /// </summary>
    public string MessageTemplate
    {
        get => _messageTemplate;
        set => this.RaiseAndSetIfChanged(ref _messageTemplate, value);
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
    /// Категория уведомления
    /// </summary>
    public string? Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    /// <summary>
    /// Флаг активности шаблона
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Параметры шаблона
    /// </summary>
    public List<string> Parameters { get; set; } = new();

    /// <summary>
    /// Создает новый экземпляр шаблона уведомления
    /// </summary>
    public NotificationTemplate()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр шаблона уведомления с указанными параметрами
    /// </summary>
    public NotificationTemplate(string name, string titleTemplate, string messageTemplate)
    {
        Uid = Guid.NewGuid();
        _name = name.Trim();
        _titleTemplate = titleTemplate;
        _messageTemplate = messageTemplate;
    }
} 