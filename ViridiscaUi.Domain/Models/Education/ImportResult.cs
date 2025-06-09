using System;
using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Результат операции импорта
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Успешность операции импорта
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Количество импортированных записей
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// Количество записей с ошибками
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Список ошибок
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Общее количество обработанных записей
    /// </summary>
    public int TotalProcessed => ImportedCount + ErrorCount;

    /// <summary>
    /// Процент успешности
    /// </summary>
    public double SuccessRate => TotalProcessed > 0 ? (double)ImportedCount / TotalProcessed * 100 : 0;

    /// <summary>
    /// Есть ли ошибки
    /// </summary>
    public bool HasErrors => Errors.Count > 0 || ErrorCount > 0;

    /// <summary>
    /// Есть ли предупреждения
    /// </summary>
    public bool HasWarnings => Warnings.Count > 0;

    /// <summary>
    /// Создает успешный результат
    /// </summary>
    public static ImportResult Success(int importedCount)
    {
        return new ImportResult
        {
            IsSuccess = true,
            ImportedCount = importedCount
        };
    }

    /// <summary>
    /// Создает результат с ошибками
    /// </summary>
    public static ImportResult Failure(List<string> errors)
    {
        return new ImportResult
        {
            IsSuccess = false,
            Errors = errors
        };
    }

    /// <summary>
    /// Создает частично успешный результат
    /// </summary>
    public static ImportResult Partial(int importedCount, int errorCount, List<string> errors)
    {
        return new ImportResult
        {
            IsSuccess = importedCount > 0,
            ImportedCount = importedCount,
            ErrorCount = errorCount,
            Errors = errors
        };
    }
} 