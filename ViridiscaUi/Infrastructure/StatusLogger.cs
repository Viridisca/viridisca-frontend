using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Статический класс для логирования сообщений в StatusService
/// Заменяет Console.WriteLine для централизованного отображения сообщений в UI
/// 
/// Примечание: Это удобный фасад для IStatusService. Можно использовать как:
/// 1. StatusLogger.LogInfo() - статический метод (требует инициализации)
/// 2. IStatusService напрямую через DI - для сложных сценариев
/// </summary>
public static class StatusLogger
{
    private static IStatusService? _statusService;

    /// <summary>
    /// Инициализация StatusLogger с StatusService
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _statusService = serviceProvider.GetService<IStatusService>();
    }

    /// <summary>
    /// Логирует информационное сообщение
    /// </summary>
    public static void LogInfo(string message, string? source = null)
    {
        if (_statusService != null)
        {
            _statusService.ShowInfo(message, source);
        }
        else
        {
            // Fallback на Console.WriteLine если сервис не инициализирован
            Console.WriteLine($"[INFO] {source}: {message}");
        }
    }

    /// <summary>
    /// Логирует предупреждение
    /// </summary>
    public static void LogWarning(string message, string? source = null)
    {
        if (_statusService != null)
        {
            _statusService.ShowWarning(message, source);
        }
        else
        {
            Console.WriteLine($"[WARNING] {source}: {message}");
        }
    }

    /// <summary>
    /// Логирует ошибку
    /// </summary>
    public static void LogError(string message, string? source = null)
    {
        if (_statusService != null)
        {
            _statusService.ShowError(message, source);
        }
        else
        {
            Console.WriteLine($"[ERROR] {source}: {message}");
        }
    }

    /// <summary>
    /// Логирует успешное сообщение
    /// </summary>
    public static void LogSuccess(string message, string? source = null)
    {
        if (_statusService != null)
        {
            _statusService.ShowSuccess(message, source);
        }
        else
        {
            Console.WriteLine($"[SUCCESS] {source}: {message}");
        }
    }

    /// <summary>
    /// Логирует отладочное сообщение
    /// </summary>
    public static void LogDebug(string message, string? source = null)
    {
        if (_statusService != null)
        {
            _statusService.AddMessage(LogLevel.Debug, message, source);
        }
        else
        {
            Console.WriteLine($"[DEBUG] {source}: {message}");
        }
    }

    /// <summary>
    /// Универсальный метод логирования с указанием уровня
    /// </summary>
    public static void Log(LogLevel level, string message, string? source = null)
    {
        if (_statusService != null)
        {
            _statusService.AddMessage(level, message, source);
        }
        else
        {
            Console.WriteLine($"[{level.ToString().ToUpper()}] {source}: {message}");
        }
    }
} 