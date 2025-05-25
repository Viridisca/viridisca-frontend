using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Windows;

namespace ViridiscaUi.ViewModels.Components;

/// <summary>
/// ViewModel для StatusBar компонента
/// </summary>
public class StatusBarViewModel : ReactiveObject, IDisposable
{
    private readonly IStatusService _statusService;
    private readonly IDisposable _subscriptions;
    private StatusHistoryWindow? _historyWindow;

    public ReadOnlyObservableCollection<StatusMessage> Messages => _statusService.Messages;

    [Reactive] public StatusMessage? CurrentMessage { get; private set; }
    [Reactive] public bool IsStatusMessageVisible { get; private set; }
    [Reactive] public bool IsHistoryVisible { get; private set; }
    [Reactive] public int TotalMessagesCount { get; private set; }
    [Reactive] public int ErrorsCount { get; private set; }
    [Reactive] public int WarningsCount { get; private set; }
    [Reactive] public int InfoCount { get; private set; }

    public ReactiveCommand<Unit, Unit> ToggleHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearHistoryCommand { get; }
    public ReactiveCommand<StatusMessage, Unit> CopyMessageCommand { get; }
    public ReactiveCommand<Unit, Unit> CopyAllMessagesCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseHistoryCommand { get; }

    public StatusBarViewModel(IStatusService statusService)
    {
        _statusService = statusService;

        // Подписка на изменения текущего сообщения через событие
        var currentMessageSubscription = Observable.FromEventPattern<EventHandler<StatusMessage?>, StatusMessage?>(
                h => _statusService.CurrentMessageChanged += h,
                h => _statusService.CurrentMessageChanged -= h)
            .Select(ep => ep.EventArgs)
            .Subscribe(message => 
            {
                CurrentMessage = message;
                IsStatusMessageVisible = message != null;
            });

        // Подписка на добавление новых сообщений для обновления счетчиков
        var messageAddedSubscription = Observable.FromEventPattern<EventHandler<StatusMessage>, StatusMessage>(
                h => _statusService.MessageAdded += h,
                h => _statusService.MessageAdded -= h)
            .Subscribe(_ => UpdateCounts());

        _subscriptions = new CompositeDisposable(
            currentMessageSubscription,
            messageAddedSubscription
        );

        // Команды
        ToggleHistoryCommand = ReactiveCommand.Create(ToggleHistory);
        CloseHistoryCommand = ReactiveCommand.Create(CloseHistory);

        ClearHistoryCommand = ReactiveCommand.Create(() =>
        {
            _statusService.Clear();
            UpdateCounts();
        });

        CopyMessageCommand = ReactiveCommand.CreateFromTask<StatusMessage>(CopyMessageAsync);
        CopyAllMessagesCommand = ReactiveCommand.CreateFromTask(CopyAllMessagesAsync);

        // Инициализация начальных значений
        CurrentMessage = _statusService.CurrentMessage;
        IsStatusMessageVisible = CurrentMessage != null;
        UpdateCounts();
    }

    private void ToggleHistory()
    {
        if (_historyWindow == null || !_historyWindow.IsVisible)
        {
            ShowHistory();
        }
        else
        {
            CloseHistory();
        }
    }

    private void ShowHistory()
    {
        if (_historyWindow != null && _historyWindow.IsVisible)
            return;

        try
        {
            // Получаем главное окно
            var mainWindow = GetMainWindow();
            if (mainWindow == null) return;

            // Создаем новое окно истории
            _historyWindow = new StatusHistoryWindow
            {
                DataContext = this
            };

            // Устанавливаем владельца окна
            if (mainWindow != null)
            {
                // Owner может быть установлен только после создания окна
                _historyWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            // Подписываемся на закрытие окна
            _historyWindow.Closed += (s, e) =>
            {
                IsHistoryVisible = false;
                _historyWindow = null;
            };

            _historyWindow.Show();
            IsHistoryVisible = true;
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка открытия окна истории: {ex.Message}", "StatusBar");
        }
    }

    private void CloseHistory()
    {
        if (_historyWindow != null)
        {
            _historyWindow.Close();
            _historyWindow = null;
        }
        IsHistoryVisible = false;
    }

    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    private void UpdateCounts()
    {
        TotalMessagesCount = _statusService.TotalMessagesCount;
        ErrorsCount = _statusService.ErrorsCount;
        WarningsCount = _statusService.WarningsCount;
        InfoCount = _statusService.InfoCount;
    }

    private async Task CopyMessageAsync(StatusMessage message)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel?.Clipboard != null)
                {
                    await topLevel.Clipboard.SetTextAsync(message.CopyableText);
                    _statusService.ShowSuccess("Сообщение скопировано в буфер обмена", "StatusBar");
                }
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка копирования: {ex.Message}", "StatusBar");
        }
    }

    private async Task CopyAllMessagesAsync()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel?.Clipboard != null && Messages.Count > 0)
                {
                    var allMessages = string.Join(Environment.NewLine, Messages.Select(m => m.CopyableText));
                    await topLevel.Clipboard.SetTextAsync(allMessages);
                    _statusService.ShowSuccess($"Скопировано {Messages.Count} сообщений в буфер обмена", "StatusBar");
                }
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка копирования всех сообщений: {ex.Message}", "StatusBar");
        }
    }

    public void Dispose()
    {
        CloseHistory();
        _subscriptions?.Dispose();
    }
} 