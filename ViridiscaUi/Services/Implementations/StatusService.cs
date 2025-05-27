using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViridiscaUi.Services.Interfaces;
using Avalonia.Threading;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Сервис для управления статус-сообщениями и интеграции с логированием
/// </summary>
public class StatusService : ReactiveObject, IStatusService
{
    private readonly ObservableCollection<StatusMessage> _messages = [];
    private readonly ILogger<StatusService> _logger;
    private readonly Timer _messageTimer;
    private StatusMessage? _currentMessage;
    private int _maxMessagesCount = 500;

    public ReadOnlyObservableCollection<StatusMessage> Messages { get; }

    public StatusMessage? CurrentMessage
    {
        get => _currentMessage;
        private set
        {
            if (_currentMessage != value)
            {
                this.RaiseAndSetIfChanged(ref _currentMessage, value);
                CurrentMessageChanged?.Invoke(this, value);
            }
        }
    }

    public int TotalMessagesCount => _messages.Count;

    public int ErrorsCount => _messages.Count(m => m.Level == LogLevel.Error || m.Level == LogLevel.Critical);

    public int WarningsCount => _messages.Count(m => m.Level == LogLevel.Warning);

    public int InfoCount => _messages.Count(m => m.Level == LogLevel.Information);

    public int MaxMessagesCount
    {
        get => _maxMessagesCount;
        set => this.RaiseAndSetIfChanged(ref _maxMessagesCount, Math.Max(1, value));
    }

    public event EventHandler<StatusMessage?>? CurrentMessageChanged;
    public event EventHandler<StatusMessage>? MessageAdded;

    public StatusService(ILogger<StatusService> logger)
    {
        _logger = logger;
        Messages = new ReadOnlyObservableCollection<StatusMessage>(_messages);

        // Таймер для автоматического скрытия статус-сообщений через определенное время
        _messageTimer = new Timer(OnMessageTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

        // Подписка на изменения в коллекции сообщений для обновления счетчиков
        _messages.CollectionChanged += (_, _) =>
        {
            this.RaisePropertyChanged(nameof(TotalMessagesCount));
            this.RaisePropertyChanged(nameof(ErrorsCount));
            this.RaisePropertyChanged(nameof(WarningsCount));
            this.RaisePropertyChanged(nameof(InfoCount));
        };
    }

    public void ShowInfo(string message, string? source = null)
    {
        AddMessage(LogLevel.Information, message, source);
    }

    public void ShowWarning(string message, string? source = null)
    {
        AddMessage(LogLevel.Warning, message, source);
    }

    public void ShowError(string message, string? source = null)
    {
        AddMessage(LogLevel.Error, message, source);
    }

    public void ShowSuccess(string message, string? source = null)
    {
        // Используем Information уровень для успешных сообщений, но добавляем категорию
        var statusMessage = new StatusMessage
        {
            Level = LogLevel.Information,
            Message = message,
            Source = source,
            Category = "Success"
        };

        AddMessageInternal(statusMessage);
    }

    public void AddMessage(LogLevel level, string message, string? source = null)
    {
        var statusMessage = new StatusMessage
        {
            Level = level,
            Message = message,
            Source = source
        };

        AddMessageInternal(statusMessage);
    }

    private void AddMessageInternal(StatusMessage statusMessage)
    {
        try
        {
            // Логируем сообщение в стандартный логгер
            _logger.Log(statusMessage.Level, "{Source}: {Message}", statusMessage.Source ?? "StatusService", statusMessage.Message);

            // Добавляем в UI коллекцию (в UI потоке)
            if (Dispatcher.UIThread.CheckAccess())
            {
                AddToCollection(statusMessage);
                SetCurrentMessage(statusMessage);
                MessageAdded?.Invoke(this, statusMessage);
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AddToCollection(statusMessage);
                    SetCurrentMessage(statusMessage);
                    MessageAdded?.Invoke(this, statusMessage);
                });
            }
        }
        catch (Exception ex)
        {
            // Безопасное логирование ошибки добавления сообщения
            _logger.LogError(ex, "Ошибка при добавлении статус-сообщения: {Message}", statusMessage.Message);
        }
    }

    private void AddToCollection(StatusMessage statusMessage)
    {
        _messages.Add(statusMessage);

        // Ограничиваем количество сообщений
        while (_messages.Count > MaxMessagesCount)
        {
            _messages.RemoveAt(0);
        }
    }

    private void SetCurrentMessage(StatusMessage statusMessage)
    {
        CurrentMessage = statusMessage;

        // Устанавливаем таймер для автоматического скрытия сообщения
        var delay = GetMessageDisplayDelay(statusMessage.Level);
        if (delay > TimeSpan.Zero)
        {
            _messageTimer.Change(delay, Timeout.InfiniteTimeSpan);
        }
    }

    private TimeSpan GetMessageDisplayDelay(LogLevel level)
    {
        return level switch
        {
            LogLevel.Error or LogLevel.Critical => TimeSpan.FromSeconds(10), // Ошибки показываем дольше
            LogLevel.Warning => TimeSpan.FromSeconds(7),
            LogLevel.Information => TimeSpan.FromSeconds(5),
            LogLevel.Debug => TimeSpan.FromSeconds(3),
            LogLevel.Trace => TimeSpan.FromSeconds(2),
            _ => TimeSpan.FromSeconds(5)
        };
    }

    private void OnMessageTimerElapsed(object? state)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            CurrentMessage = null;
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => CurrentMessage = null);
        }
    }

    public void Clear()
    {
        try
        {
            _logger.LogInformation("Очистка всех статус-сообщений");

            if (Dispatcher.UIThread.CheckAccess())
            {
                _messages.Clear();
                CurrentMessage = null;
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _messages.Clear();
                    CurrentMessage = null;
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при очистке статус-сообщений");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageTimer?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
} 