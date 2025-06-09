using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Base;

/// <summary>
/// Результат операции импорта
/// </summary>
/// <typeparam name="T">Тип импортируемых объектов</typeparam>
public class ImportResult<T>
{
    /// <summary>
    /// Количество успешно импортированных элементов
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Количество элементов с ошибками
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Общее количество обработанных элементов
    /// </summary>
    public int TotalCount => SuccessCount + FailureCount;

    /// <summary>
    /// Успешно импортированные элементы
    /// </summary>
    public IList<T> ImportedItems { get; set; } = new List<T>();

    /// <summary>
    /// Список ошибок
    /// </summary>
    public IList<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public IList<string> Warnings { get; set; } = new List<string>();

    /// <summary>
    /// Успешность операции импорта
    /// </summary>
    public bool IsSuccess => FailureCount == 0;

    /// <summary>
    /// Процент успешности
    /// </summary>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;

    /// <summary>
    /// Сводка результата импорта
    /// </summary>
    public string Summary => $"Импортировано: {SuccessCount}, Ошибок: {FailureCount}, Всего: {TotalCount}";
} 