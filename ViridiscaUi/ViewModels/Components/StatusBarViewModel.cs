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
using static ViridiscaUi.Services.Interfaces.StatusMessageType;

namespace ViridiscaUi.ViewModels.Components;

/// <summary>
/// ViewModel для StatusBar компонента
/// </summary>
public class StatusBarViewModel : ViewModelBase
{
    private readonly IStatusService _statusService;
    private StatusHistoryWindow? _historyWindow;

    public ReadOnlyObservableCollection<StatusMessage> Messages => _statusService.Messages;

    [Reactive] public StatusMessage? CurrentMessage { get; private set; }
    [Reactive] public bool IsStatusMessageVisible { get; private set; }
    [Reactive] public bool IsHistoryVisible { get; private set; }
    [Reactive] public int TotalMessagesCount { get; private set; }
    [Reactive] public int ErrorsCount { get; private set; }
    [Reactive] public int WarningsCount { get; private set; }
    [Reactive] public int InfoCount { get; private set; }

    public ReactiveCommand<Unit, Unit> ToggleHistoryCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ClearHistoryCommand { get; private set; }
    public ReactiveCommand<StatusMessage, Unit> CopyMessageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CopyAllMessagesCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CloseHistoryCommand { get; private set; }
    
    // Новые команды для копирования по типу сообщений
    public ReactiveCommand<Unit, Unit> CopyErrorMessagesCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CopyWarningMessagesCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CopyInfoMessagesCommand { get; private set; }

    public StatusBarViewModel(IStatusService statusService)
    {
        _statusService = statusService;

        InitializeCommands();
        SetupSubscriptions();

        // Инициализация начальных значений
        CurrentMessage = _statusService.CurrentMessage;
        IsStatusMessageVisible = CurrentMessage != null;
        UpdateCounts();
    }

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        ToggleHistoryCommand = CreateSyncCommand(ToggleHistory, null, "Ошибка переключения истории");
        CloseHistoryCommand = CreateSyncCommand(CloseHistory, null, "Ошибка закрытия истории");

        ClearHistoryCommand = CreateSyncCommand(() =>
        {
            _statusService.Clear();
            UpdateCounts();
        }, null, "Ошибка очистки истории");

        CopyMessageCommand = CreateCommand<StatusMessage>(CopyMessageAsync, null, "Ошибка копирования сообщения");
        CopyAllMessagesCommand = CreateCommand(CopyAllMessagesAsync, null, "Ошибка копирования всех сообщений");

        // Новые команды для копирования по типу сообщений
        CopyErrorMessagesCommand = CreateCommand(() => CopyMessagesOfTypeAsync(Error), null, "Ошибка копирования всех ошибок");
        CopyWarningMessagesCommand = CreateCommand(() => CopyMessagesOfTypeAsync(Warning), null, "Ошибка копирования всех предупреждений");
        CopyInfoMessagesCommand = CreateCommand(() => CopyMessagesOfTypeAsync(Info), null, "Ошибка копирования всех информационных сообщений");
    }

    /// <summary>
    /// Настраивает подписки на изменения
    /// </summary>
    private void SetupSubscriptions()
    {
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

        currentMessageSubscription.DisposeWith(Disposables);
        messageAddedSubscription.DisposeWith(Disposables);
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
                DataContext = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // Подписываемся на закрытие окна
            _historyWindow.Closed += (s, e) =>
            {
                IsHistoryVisible = false;
                _historyWindow = null;
            };

            // Подписываемся на закрытие главного окна, чтобы закрыть окно истории
            void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
            {
                if (_historyWindow != null && _historyWindow.IsVisible)
                {
                    _historyWindow.Close();
                }
                mainWindow.Closing -= OnMainWindowClosing;
            }
            
            mainWindow.Closing += OnMainWindowClosing;

            // Для истории используем немодальное окно (Show), но с правильным управлением
            _historyWindow.Show(mainWindow);
            IsHistoryVisible = true;
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка открытия окна истории: {ex.Message}", "StatusBar");
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
        TotalMessagesCount = _statusService?.TotalMessagesCount ?? 0;
        ErrorsCount = _statusService?.ErrorsCount ?? 0;
        WarningsCount = _statusService?.WarningsCount ?? 0;
        InfoCount = _statusService?.InfoCount ?? 0;
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
                    _statusService?.ShowSuccess("Сообщение скопировано в буфер обмена", "StatusBar");
                }
            }
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка копирования: {ex.Message}", "StatusBar");
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
                    _statusService?.ShowSuccess($"Скопировано {Messages.Count} сообщений в буфер обмена", "StatusBar");
                }
            }
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка копирования всех сообщений: {ex.Message}", "StatusBar");
        }
    }

    private async Task CopyMessagesOfTypeAsync(StatusMessageType type)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel?.Clipboard != null && Messages.Count > 0)
                {
                    var filteredMessages = Messages.Where(m => m.Type == type).Select(m => m.CopyableText);
                    var allMessages = string.Join(Environment.NewLine, filteredMessages);
                    await topLevel.Clipboard.SetTextAsync(allMessages);
                    _statusService?.ShowSuccess($"Скопировано {filteredMessages.Count()} сообщений в буфер обмена", "StatusBar");
                }
            }
        }
        catch (Exception ex)
        {
            _statusService?.ShowError($"Ошибка копирования всех сообщений типа {type}: {ex.Message}", "StatusBar");
        }
    }
} 