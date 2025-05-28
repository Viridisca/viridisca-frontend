using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Типы статус-сообщений для удобной фильтрации
/// </summary>
public enum StatusMessageType
{
    Info,
    Warning,
    Error,
    Success,
    Debug,
    Trace
}

/// <summary>
/// Сервис для управления статус-сообщениями и интеграции с логированием
/// </summary>
public interface IStatusService
{
    /// <summary>
    /// Коллекция статус-сообщений для отображения в UI
    /// </summary>
    ReadOnlyObservableCollection<StatusMessage> Messages { get; }

    /// <summary>
    /// Текущее статус-сообщение
    /// </summary>
    StatusMessage? CurrentMessage { get; }

    /// <summary>
    /// Общее количество сообщений
    /// </summary>
    int TotalMessagesCount { get; }

    /// <summary>
    /// Количество сообщений ошибок
    /// </summary>
    int ErrorsCount { get; }

    /// <summary>
    /// Количество предупреждений
    /// </summary>
    int WarningsCount { get; }

    /// <summary>
    /// Количество информационных сообщений
    /// </summary>
    int InfoCount { get; }

    /// <summary>
    /// Максимальное количество сообщений в истории
    /// </summary>
    int MaxMessagesCount { get; set; }

    /// <summary>
    /// Показать информационное сообщение
    /// </summary>
    void ShowInfo(string message, string? source = null);

    /// <summary>
    /// Показать предупреждение
    /// </summary>
    void ShowWarning(string message, string? source = null);

    /// <summary>
    /// Показать ошибку
    /// </summary>
    void ShowError(string message, string? source = null);

    /// <summary>
    /// Показать успешное сообщение
    /// </summary>
    void ShowSuccess(string message, string? source = null);

    /// <summary>
    /// Добавить сообщение с определенным уровнем логирования
    /// </summary>
    void AddMessage(LogLevel level, string message, string? source = null);

    /// <summary>
    /// Очистить все сообщения
    /// </summary>
    void Clear();

    /// <summary>
    /// Событие изменения текущего сообщения
    /// </summary>
    event EventHandler<StatusMessage?> CurrentMessageChanged;

    /// <summary>
    /// Событие добавления нового сообщения
    /// </summary>
    event EventHandler<StatusMessage> MessageAdded;
}

/// <summary>
/// Статус-сообщение
/// </summary>
public class StatusMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Source { get; init; }
    public string? Category { get; init; }

    /// <summary>
    /// Тип сообщения для удобной фильтрации
    /// </summary>
    public StatusMessageType Type => Level switch
    {
        LogLevel.Error or LogLevel.Critical => StatusMessageType.Error,
        LogLevel.Warning => StatusMessageType.Warning,
        LogLevel.Information => StatusMessageType.Info,
        LogLevel.Debug => StatusMessageType.Debug,
        LogLevel.Trace => StatusMessageType.Trace,
        _ => StatusMessageType.Info
    };

    /// <summary>
    /// Иконка для отображения в UI
    /// </summary>
    public string Icon => Level switch
    {
        LogLevel.Error or LogLevel.Critical => "❌",
        LogLevel.Warning => "⚠️",
        LogLevel.Information => "ℹ️",
        LogLevel.Debug => "🔍",
        LogLevel.Trace => "📍",
        _ => "📝"
    };

    /// <summary>
    /// Цвет для отображения в UI
    /// </summary>
    public string Color => Level switch
    {
        LogLevel.Error or LogLevel.Critical => "Red",
        LogLevel.Warning => "Orange",
        LogLevel.Information => "Blue",
        LogLevel.Debug => "Gray",
        LogLevel.Trace => "LightGray",
        _ => "Black"
    };

    /// <summary>
    /// Форматированный текст сообщения
    /// </summary>
    public string FormattedMessage => $"[{Timestamp:HH:mm:ss}] {(string.IsNullOrEmpty(Source) ? "" : $"[{Source}] ")}{Message}";

    /// <summary>
    /// Копируемый текст сообщения
    /// </summary>
    public string CopyableText => $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {(string.IsNullOrEmpty(Source) ? "" : $"[{Source}] ")}{Message}";
} 