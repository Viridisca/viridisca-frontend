using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModels с общей функциональностью
/// </summary>
public abstract class ViewModelBase : ReactiveObject, IDisposable
{
    protected readonly CompositeDisposable Disposables = [];

    protected ILogger? Logger { get; private set; }

    [Reactive] public bool IsBusy { get; protected set; }
    [Reactive] public string? ErrorMessage { get; protected set; }
    [Reactive] public bool HasError { get; protected set; }

    protected ViewModelBase()
    {
        // Автоматическое обновление HasError при изменении ErrorMessage
        this.WhenAnyValue(x => x.ErrorMessage)
            .Subscribe(error => HasError = !string.IsNullOrEmpty(error))
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Устанавливает логгер для ViewModel
    /// </summary>
    public void SetLogger(ILogger logger)
    {
        Logger = logger;
        Logger?.LogDebug("Logger set for {ViewModelType}", GetType().Name);
    }

    /// <summary>
    /// Очищает ошибку
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = null;
        Logger?.LogDebug("Error cleared in {ViewModelType}", GetType().Name);
    }

    /// <summary>
    /// Устанавливает ошибку
    /// </summary>
    protected void SetError(string message, Exception? exception = null)
    {
        ErrorMessage = message;
        Logger?.LogError(exception, "ViewModel error in {ViewModelType}: {Message}", GetType().Name, message);
        
        // Показываем ошибку пользователю через StatusLogger
        ShowError(message);
    }

    /// <summary>
    /// Выполняет операцию с обработкой ошибок и индикацией загрузки
    /// </summary>
    protected async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string? errorMessage = null)
    {
        try
        {
            IsBusy = true;
            ClearError();
            Logger?.LogDebug("Starting operation in {ViewModelType}", GetType().Name);
            await operation();
            Logger?.LogDebug("Operation completed successfully in {ViewModelType}", GetType().Name);
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Выполняет операцию с возвращаемым значением с обработкой ошибок
    /// </summary>
    protected async Task<T?> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, string? errorMessage = null)
    {
        try
        {
            IsBusy = true;
            ClearError();
            Logger?.LogDebug("Starting operation with return value in {ViewModelType}", GetType().Name);
            var result = await operation();
            Logger?.LogDebug("Operation with return value completed successfully in {ViewModelType}", GetType().Name);
            return result;
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Выполняет операцию синхронно с обработкой ошибок
    /// </summary>
    protected void ExecuteWithErrorHandling(Action operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            Logger?.LogDebug("Starting synchronous operation in {ViewModelType}", GetType().Name);
            operation();
            Logger?.LogDebug("Synchronous operation completed successfully in {ViewModelType}", GetType().Name);
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
        }
    }

    /// <summary>
    /// Выполняет операцию синхронно с возвращаемым значением и обработкой ошибок
    /// </summary>
    protected T? ExecuteWithErrorHandling<T>(Func<T> operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            Logger?.LogDebug("Starting synchronous operation with return value in {ViewModelType}", GetType().Name);
            var result = operation();
            Logger?.LogDebug("Synchronous operation with return value completed successfully in {ViewModelType}", GetType().Name);
            return result;
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            return default;
        }
    }

    #region StatusLogger Methods - Для отображения сообщений пользователю

    /// <summary>
    /// Показывает информационное сообщение пользователю через StatusLogger
    /// </summary>
    protected void ShowInfo(string message)
    {
        StatusLogger.LogInfo(message, GetType().Name);
        Logger?.LogInformation(message);
    }

    /// <summary>
    /// Показывает предупреждение пользователю через StatusLogger
    /// </summary>
    protected void ShowWarning(string message)
    {
        StatusLogger.LogWarning(message, GetType().Name);
        Logger?.LogWarning(message);
    }

    /// <summary>
    /// Показывает сообщение об успехе пользователю через StatusLogger
    /// </summary>
    protected void ShowSuccess(string message)
    {
        StatusLogger.LogSuccess(message, GetType().Name);
        Logger?.LogInformation("Success: {Message}", message);
    }

    /// <summary>
    /// Показывает ошибку пользователю через StatusLogger
    /// </summary>
    protected void ShowError(string message)
    {
        StatusLogger.LogError(message, GetType().Name);
        Logger?.LogError(message);
    }

    #endregion

    #region Logger Methods - Только для технического логирования

    /// <summary>
    /// Логирует информационное сообщение (только в лог, не показывает пользователю)
    /// </summary>
    protected void LogInfo(string message, params object[] args)
    {
        Logger?.LogInformation(message, args);
    }

    /// <summary>
    /// Логирует предупреждение (только в лог, не показывает пользователю)
    /// </summary>
    protected void LogWarning(string message, params object[] args)
    {
        Logger?.LogWarning(message, args);
    }

    /// <summary>
    /// Логирует отладочное сообщение (только в лог, не показывает пользователю)
    /// </summary>
    protected void LogDebug(string message, params object[] args)
    {
        Logger?.LogDebug(message, args);
    }

    /// <summary>
    /// Логирует ошибку (только в лог, не показывает пользователю)
    /// </summary>
    protected void LogError(Exception? exception, string message, params object[] args)
    {
        Logger?.LogError(exception, message, args);
    }

    #endregion

    #region Command Creation Helpers

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций без параметров
    /// </summary>
    protected ReactiveCommand<Unit, Unit> CreateCommand(
        Func<Task> execute, 
        IObservable<bool>? canExecute = null, 
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask(
            async () => await ExecuteWithErrorHandlingAsync(execute, errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        command.ThrownExceptions
            .Subscribe(ex => SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex))
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций с параметром
    /// </summary>
    protected ReactiveCommand<T, Unit> CreateCommand<T>(
        Func<T, Task> execute, 
        IObservable<bool>? canExecute = null, 
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask<T>(
            async param => await ExecuteWithErrorHandlingAsync(() => execute(param), errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        command.ThrownExceptions
            .Subscribe(ex => SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex))
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для синхронных операций без параметров
    /// </summary>
    protected ReactiveCommand<Unit, Unit> CreateSyncCommand(
        Action execute, 
        IObservable<bool>? canExecute = null, 
        string? errorMessage = null)
    {
        var command = ReactiveCommand.Create(
            () => ExecuteWithErrorHandling(execute, errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        command.ThrownExceptions
            .Subscribe(ex => SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex))
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для синхронных операций с параметром
    /// </summary>
    protected ReactiveCommand<T, Unit> CreateSyncCommand<T>(
        Action<T> execute, 
        IObservable<bool>? canExecute = null, 
        string? errorMessage = null)
    {
        var command = ReactiveCommand.Create<T>(
            param => ExecuteWithErrorHandling(() => execute(param), errorMessage),
            canExecute,
            RxApp.MainThreadScheduler);

        command.ThrownExceptions
            .Subscribe(ex => SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex))
            .DisposeWith(Disposables);

        return command;
    }

    #endregion

    public virtual void Dispose()
    {
        Logger?.LogDebug("Disposing {ViewModelType}", GetType().Name);
        Disposables?.Dispose();
    }
}
