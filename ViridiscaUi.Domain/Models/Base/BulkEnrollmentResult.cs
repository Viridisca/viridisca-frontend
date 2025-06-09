using System;
using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Base;

/// <summary>
/// Результат массовой записи студентов на курсы
/// </summary>
public class BulkEnrollmentResult
{
    /// <summary>
    /// Общее количество обработанных записей
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Количество успешных записей
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Количество неудачных записей
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Количество пропущенных записей (уже существуют)
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// Список успешно записанных студентов
    /// </summary>
    public List<Guid> SuccessfulEnrollments { get; set; } = new();

    /// <summary>
    /// Список ошибок с деталями
    /// </summary>
    public List<BulkEnrollmentError> Errors { get; set; } = new();

    /// <summary>
    /// Список предупреждений
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Время выполнения операции
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }

    /// <summary>
    /// Успешна ли операция в целом
    /// </summary>
    public bool IsSuccess => FailureCount == 0;

    /// <summary>
    /// Есть ли частичный успех
    /// </summary>
    public bool IsPartialSuccess => SuccessCount > 0 && FailureCount > 0;
}

/// <summary>
/// Ошибка массовой записи
/// </summary>
public class BulkEnrollmentError
{
    /// <summary>
    /// ID студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// ID курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Код ошибки
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительные данные об ошибке
    /// </summary>
    public Dictionary<string, object> ErrorData { get; set; } = new();
} 