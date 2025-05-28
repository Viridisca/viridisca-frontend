using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        // Подписываемся на ThrownExceptions для предотвращения разрыва observable pipeline
        this.ThrownExceptions
            .Subscribe(ex => 
            {
                SetError("Произошла ошибка в реактивной команде", ex);
                Logger?.LogError(ex, "Unhandled exception in reactive pipeline for {ViewModelType}", GetType().Name);
            })
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
    /// Очищает все ошибки
    /// </summary>
    protected void ClearErrors()
    {
        ClearError();
    }

    /// <summary>
    /// Устанавливает ошибку
    /// </summary>
    protected void SetError(string message, Exception? exception = null)
    {
        ErrorMessage = message;
        Logger?.LogError(exception, "Error in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
    }

    /// <summary>
    /// Устанавливает состояние загрузки
    /// </summary>
    protected void SetLoading(bool isLoading, string? message = null)
    {
        IsBusy = isLoading;
        if (isLoading && !string.IsNullOrEmpty(message))
        {
            Logger?.LogDebug("Loading started in {ViewModelType}: {Message}", GetType().Name, message);
        }
        else if (!isLoading)
        {
            Logger?.LogDebug("Loading finished in {ViewModelType}", GetType().Name);
        }
    }

    /// <summary>
    /// Показывает информационное сообщение
    /// </summary>
    protected void ShowInfo(string message)
    {
        StatusLogger.LogInfo(message, GetType().Name);
    }

    /// <summary>
    /// Показывает сообщение об успехе
    /// </summary>
    protected void ShowSuccess(string message)
    {
        StatusLogger.LogSuccess(message, GetType().Name);
    }

    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    protected void ShowWarning(string message)
    {
        StatusLogger.LogWarning(message, GetType().Name);
    }

    /// <summary>
    /// Показывает ошибку
    /// </summary>
    protected void ShowError(string message)
    {
        StatusLogger.LogError(message, GetType().Name);
    }

    /// <summary>
    /// Логирует информацию
    /// </summary>
    protected void LogInfo(string message, params object[] args)
    {
        Logger?.LogInformation(message, args);
    }

    /// <summary>
    /// Логирует отладочную информацию
    /// </summary>
    protected void LogDebug(string message, params object[] args)
    {
        Logger?.LogDebug(message, args);
    }

    /// <summary>
    /// Логирует ошибку
    /// </summary>
    protected void LogError(Exception exception, string message, params object[] args)
    {
        Logger?.LogError(exception, message, args);
    }

    /// <summary>
    /// Логирует предупреждение
    /// </summary>
    protected void LogWarning(string message, params object[] args)
    {
        Logger?.LogWarning(message, args);
    }

    /// <summary>
    /// Выполняет асинхронную операцию с обработкой ошибок
    /// </summary>
    protected async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            await operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            Logger?.LogError(ex, "Error executing operation in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
        }
    }

    /// <summary>
    /// Выполняет асинхронную операцию с обработкой ошибок и возвращает результат
    /// </summary>
    protected async Task<T?> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            return await operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            Logger?.LogError(ex, "Error executing operation in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
            return default;
        }
    }

    /// <summary>
    /// Выполняет синхронную операцию с обработкой ошибок
    /// </summary>
    protected void ExecuteWithErrorHandling(Action operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            Logger?.LogError(ex, "Error executing operation in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
        }
    }

    /// <summary>
    /// Выполняет синхронную операцию с обработкой ошибок и возвращает результат
    /// </summary>
    protected T? ExecuteWithErrorHandling<T>(Func<T> operation, string? errorMessage = null)
    {
        try
        {
            ClearError();
            return operation();
        }
        catch (Exception ex)
        {
            var message = errorMessage ?? "Произошла ошибка при выполнении операции";
            SetError(message, ex);
            Logger?.LogError(ex, "Error executing operation in {ViewModelType}: {ErrorMessage}", GetType().Name, message);
            return default;
        }
    }

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

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex => 
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                Logger?.LogError(ex, "Command execution error in {ViewModelType}: {ErrorMessage}", GetType().Name, errorMessage);
            })
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

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex => 
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                Logger?.LogError(ex, "Command execution error in {ViewModelType}: {ErrorMessage}", GetType().Name, errorMessage);
            })
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

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex => 
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                Logger?.LogError(ex, "Sync command execution error in {ViewModelType}: {ErrorMessage}", GetType().Name, errorMessage);
            })
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

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex => 
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                Logger?.LogError(ex, "Sync command execution error in {ViewModelType}: {ErrorMessage}", GetType().Name, errorMessage);
            })
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций с возвращаемым значением
    /// </summary>
    protected ReactiveCommand<Unit, TResult> CreateCommand<TResult>(
        Func<Task<TResult>> execute, 
        IObservable<bool>? canExecute = null, 
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask(
            async () => await ExecuteWithErrorHandlingAsync(execute, errorMessage) ?? default(TResult)!,
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex => 
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                Logger?.LogError(ex, "Command execution error in {ViewModelType}: {ErrorMessage}", GetType().Name, errorMessage);
            })
            .DisposeWith(Disposables);

        return command;
    }

    /// <summary>
    /// Создает ReactiveCommand с обработкой ошибок для асинхронных операций с параметром и возвращаемым значением
    /// </summary>
    protected ReactiveCommand<TParam, TResult> CreateCommand<TParam, TResult>(
        Func<TParam, Task<TResult>> execute, 
        IObservable<bool>? canExecute = null, 
        string? errorMessage = null)
    {
        var command = ReactiveCommand.CreateFromTask<TParam, TResult>(
            async param => await ExecuteWithErrorHandlingAsync(() => execute(param), errorMessage) ?? default(TResult)!,
            canExecute,
            RxApp.MainThreadScheduler);

        // Подписываемся на ThrownExceptions для предотвращения разрыва pipeline
        command.ThrownExceptions
            .Subscribe(ex => 
            {
                SetError(errorMessage ?? "Произошла ошибка при выполнении команды", ex);
                Logger?.LogError(ex, "Command execution error in {ViewModelType}: {ErrorMessage}", GetType().Name, errorMessage);
            })
            .DisposeWith(Disposables);

        return command;
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Disposables?.Dispose();
                Logger?.LogDebug("Disposed {ViewModelType}", GetType().Name);
            }
            _disposed = true;
        }
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
