using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.Bases.Navigation_New;

/// <summary>
/// Базовый класс для ViewModels, которые могут принимать параметры
/// </summary>
public abstract class ParameterizedViewModelBase : RoutableViewModelBase, IParameterizedViewModel
{
    private object? _parameter;
    private object? _result;
    private bool _isInitialized;

    /// <summary>
    /// Результат выполнения операции
    /// </summary>
    public object? Result
    {
        get => _result;
        protected set => this.RaiseAndSetIfChanged(ref _result, value);
    }

    /// <summary>
    /// Параметр, переданный в ViewModel
    /// </summary>
    public object? Parameter
    {
        get => _parameter;
        private set => this.RaiseAndSetIfChanged(ref _parameter, value);
    }

    /// <summary>
    /// Параметры (для обратной совместимости)
    /// </summary>
    public object? Parameters => Parameter;

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public new string? ErrorMessage
    {
        get => base.ErrorMessage;
        protected set => base.ErrorMessage = value;
    }

    /// <summary>
    /// Есть ли ошибка
    /// </summary>
    public new bool HasError
    {
        get => base.HasError;
        protected set => base.HasError = value;
    }

    /// <summary>
    /// Инициализирован ли ViewModel
    /// </summary>
    public bool IsInitialized
    {
        get => _isInitialized;
        private set => this.RaiseAndSetIfChanged(ref _isInitialized, value);
    }

    /// <summary>
    /// Типизированные параметры для удобства использования
    /// </summary>
    protected Dictionary<string, object> ParametersDict { get; private set; } = new();

    /// <summary>
    /// Команда для возврата результата (для модальных окон)
    /// </summary>
    public ReactiveCommand<object?, Unit> ReturnResultCommand { get; protected set; }

    /// <summary>
    /// Статус успеха
    /// </summary>
    [Reactive] public string? SuccessMessage { get; protected set; }

    /// <summary>
    /// Есть ли сообщение об успехе
    /// </summary>
    [Reactive] public bool HasSuccess { get; protected set; }

    protected ParameterizedViewModelBase(IScreen hostScreen) 
        : base(hostScreen)
    {
        ReturnResultCommand = ReactiveCommand.Create<object?>(SetResult);
    }

    /// <summary>
    /// Инициализация с параметром
    /// </summary>
    /// <param name="parameter">Параметр для инициализации</param>
    public virtual void Initialize(object parameter)
    {
        try
        {
            Parameter = parameter;
            OnParameterReceived(parameter);
            IsInitialized = true;
            
            LogInfo($"ViewModel {GetType().Name} инициализирован с параметром: {parameter?.GetType().Name ?? "null"}");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка инициализации: {ex.Message}", ex);
            LogError(ex, "Ошибка инициализации ViewModel {ViewModelName}", GetType().Name);
        }
    }

    /// <summary>
    /// Обработка полученного параметра
    /// </summary>
    /// <param name="parameter">Полученный параметр</param>
    protected virtual void OnParameterReceived(object parameter)
    {
        // Переопределяется в наследниках для обработки конкретных параметров
    }

    /// <summary>
    /// Установка ошибки
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="exception">Исключение</param>
    protected new void SetError(string message, Exception? exception = null)
    {
        ErrorMessage = message;
        HasError = true;
        
        if (exception != null)
        {
            LogError(exception, "Ошибка в {ViewModelName}: {ErrorMessage}", GetType().Name, message);
        }
        else
        {
            LogWarning("Ошибка в {ViewModelName}: {ErrorMessage}", GetType().Name, message);
        }
    }

    /// <summary>
    /// Очистка ошибки
    /// </summary>
    protected new void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    /// <summary>
    /// Показ сообщения об успехе
    /// </summary>
    /// <param name="message">Сообщение</param>
    protected new void ShowSuccess(string message)
    {
        ClearError();
        LogInfo("Успех в {ViewModelName}: {Message}", GetType().Name, message);
        
        // Можно добавить уведомление пользователю
        // NotificationService?.ShowSuccess(message);
    }

    /// <summary>
    /// Валидация параметра
    /// </summary>
    /// <typeparam name="T">Тип ожидаемого параметра</typeparam>
    /// <returns>Валидный параметр или null</returns>
    protected T? ValidateParameter<T>() where T : class
    {
        if (Parameter is T validParameter)
        {
            return validParameter;
        }

        var expectedType = typeof(T).Name;
        var actualType = Parameter?.GetType().Name ?? "null";
        
        SetError($"Неверный тип параметра. Ожидался: {expectedType}, получен: {actualType}");
        return null;
    }

    /// <summary>
    /// Валидация параметра для value types
    /// </summary>
    /// <typeparam name="T">Тип ожидаемого параметра</typeparam>
    /// <returns>Валидный параметр или default</returns>
    protected T ValidateValueParameter<T>() where T : struct
    {
        if (Parameter is T validParameter)
        {
            return validParameter;
        }

        var expectedType = typeof(T).Name;
        var actualType = Parameter?.GetType().Name ?? "null";
        
        SetError($"Неверный тип параметра. Ожидался: {expectedType}, получен: {actualType}");
        return default;
    }

    /// <summary>
    /// Логирование ошибки
    /// </summary>
    /// <param name="exception">Исключение</param>
    /// <param name="messageTemplate">Шаблон сообщения</param>
    /// <param name="args">Аргументы</param>
    protected new void LogError(Exception exception, string messageTemplate, params object[] args)
    {
        base.LogError(exception, messageTemplate, args);
    }

    /// <summary>
    /// Логирование информации
    /// </summary>
    /// <param name="messageTemplate">Шаблон сообщения</param>
    /// <param name="args">Аргументы</param>
    protected new void LogInfo(string messageTemplate, params object[] args)
    {
        base.LogInfo(messageTemplate, args);
    }

    /// <summary>
    /// Логирование предупреждения
    /// </summary>
    /// <param name="messageTemplate">Шаблон сообщения</param>
    /// <param name="args">Аргументы</param>
    protected new void LogWarning(string messageTemplate, params object[] args)
    {
        base.LogWarning(messageTemplate, args);
    }

    /// <summary>
    /// Асинхронная инициализация
    /// </summary>
    /// <param name="parameter">Параметр</param>
    /// <returns>Task</returns>
    public virtual async Task InitializeAsync(object parameter)
    {
        try
        {
            Initialize(parameter);
            await OnParameterReceivedAsync(parameter);
        }
        catch (Exception ex)
        {
            SetError($"Ошибка асинхронной инициализации: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Асинхронная обработка параметра
    /// </summary>
    /// <param name="parameter">Параметр</param>
    /// <returns>Task</returns>
    protected virtual async Task OnParameterReceivedAsync(object parameter)
    {
        await Task.CompletedTask;
        // Переопределяется в наследниках для асинхронной обработки
    }

    /// <summary>
    /// Установка результата
    /// </summary>
    /// <param name="result">Результат</param>
    protected void SetResult(object? result)
    {
        Result = result;
        LogInfo("Установлен результат в {ViewModelName}: {ResultType}", 
            GetType().Name, 
            result?.GetType().Name ?? "null");
    }

    /// <summary>
    /// Завершение с результатом
    /// </summary>
    /// <param name="result">Результат</param>
    /// <returns>Task</returns>
    protected async Task CompleteWithResultAsync(object? result)
    {
        try
        {
            SetResult(result);
            await OnCompletedAsync(result);
        }
        catch (Exception ex)
        {
            SetError($"Ошибка завершения операции: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Обработка завершения операции
    /// </summary>
    /// <param name="result">Результат</param>
    /// <returns>Task</returns>
    protected virtual async Task OnCompletedAsync(object? result)
    {
        await Task.CompletedTask;
        // Переопределяется в наследниках
    }

    /// <summary>
    /// Отмена операции
    /// </summary>
    /// <returns>Task</returns>
    protected async Task CancelAsync()
    {
        try
        {
            await OnCancelledAsync();
            SetResult(null);
        }
        catch (Exception ex)
        {
            SetError($"Ошибка отмены операции: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Обработка отмены операции
    /// </summary>
    /// <returns>Task</returns>
    protected virtual async Task OnCancelledAsync()
    {
        await Task.CompletedTask;
        // Переопределяется в наследниках
    }

    /// <summary>
    /// Очистка ресурсов при деактивации
    /// </summary>
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        
        // Очищаем параметры и результат при деактивации
        if (!IsInitialized)
        {
            Parameter = null;
            Result = null;
            ClearError();
        }
    }

    /// <summary>
    /// Преобразование параметров в словарь
    /// </summary>
    private Dictionary<string, object> ConvertToParametersDictionary(object parameters)
    {
        var dict = new Dictionary<string, object>();
        
        if (parameters == null)
            return dict;

        // Если уже словарь
        if (parameters is Dictionary<string, object> existingDict)
        {
            return new Dictionary<string, object>(existingDict);
        }

        // Если анонимный объект или обычный объект - используем рефлексию
        var type = parameters.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            try
            {
                var value = property.GetValue(parameters);
                if (value != null)
                {
                    dict[property.Name] = value;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error converting property {PropertyName} to dictionary", property.Name);
            }
        }

        return dict;
    }

    /// <summary>
    /// Создает объект параметров из пар ключ-значение
    /// </summary>
    protected static object CreateParameters(params (string key, object value)[] parameters)
    {
        var dict = new Dictionary<string, object>();
        foreach (var (key, value) in parameters)
        {
            dict[key] = value;
        }
        return dict;
    }

    /// <summary>
    /// Обратная совместимость - вызывается при получении параметров навигации
    /// </summary>
    protected virtual async Task OnParametersReceivedAsync(object? parameters)
    {
        if (parameters != null)
        {
            await OnParameterReceivedAsync(parameters);
        }
        else
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Проверяет наличие свойства в параметрах
    /// </summary>
    protected bool HasProperty(object? parameters, string propertyName)
    {
        if (parameters == null) return false;

        try
        {
            var type = parameters.GetType();
            return type.GetProperty(propertyName) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Получает значение свойства из параметров
    /// </summary>
    protected T? GetProperty<T>(object? parameters, string propertyName)
    {
        if (parameters == null) return default;

        try
        {
            var type = parameters.GetType();
            var property = type.GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(parameters);
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                // Попытка конвертации
                if (value != null)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Error getting property {PropertyName} from parameters", propertyName);
        }

        return default;
    }

    /// <summary>
    /// Получает значение свойства из параметров с значением по умолчанию
    /// </summary>
    protected T GetProperty<T>(object? parameters, string propertyName, T defaultValue)
    {
        var result = GetProperty<T>(parameters, propertyName);
        return result ?? defaultValue;
    }
} 
