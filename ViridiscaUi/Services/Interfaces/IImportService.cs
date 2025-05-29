using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Интерфейс сервиса для импорта данных
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Импортирует студентов из Excel файла
    /// </summary>
    /// <param name="filePath">Путь к файлу Excel</param>
    /// <returns>Количество успешно импортированных записей</returns>
    Task<int> ImportStudentsFromExcelAsync(string filePath);
    
    /// <summary>
    /// Импортирует студентов из CSV файла
    /// </summary>
    /// <param name="filePath">Путь к CSV файлу</param>
    /// <returns>Количество успешно импортированных записей</returns>
    Task<int> ImportStudentsFromCsvAsync(string filePath);
    
    /// <summary>
    /// Импортирует студентов из файла (автоматически определяет формат)
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Количество успешно импортированных записей</returns>
    Task<int> ImportStudentsAsync(string filePath);
    
    /// <summary>
    /// Импортирует группы из Excel файла
    /// </summary>
    /// <param name="filePath">Путь к файлу Excel</param>
    /// <returns>Количество успешно импортированных записей</returns>
    Task<int> ImportGroupsFromExcelAsync(string filePath);
    
    /// <summary>
    /// Импортирует группы из CSV файла
    /// </summary>
    /// <param name="filePath">Путь к CSV файлу</param>
    /// <returns>Количество успешно импортированных записей</returns>
    Task<int> ImportGroupsFromCsvAsync(string filePath);
    
    /// <summary>
    /// Валидирует файл перед импортом
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Результат валидации с ошибками и предупреждениями</returns>
    Task<ImportValidationResult> ValidateImportFileAsync(string filePath);
    
    /// <summary>
    /// Получает предварительный просмотр данных для импорта
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="maxRows">Максимальное количество строк для предпросмотра</param>
    /// <returns>Предварительный просмотр данных</returns>
    Task<ImportPreviewResult> GetImportPreviewAsync(string filePath, int maxRows = 10);
    
    /// <summary>
    /// Получает поддерживаемые форматы импорта
    /// </summary>
    /// <returns>Список поддерживаемых форматов</returns>
    IEnumerable<string> GetSupportedImportFormats();
    
    /// <summary>
    /// Проверяет, поддерживается ли указанный формат для импорта
    /// </summary>
    /// <param name="format">Формат файла</param>
    /// <returns>True если формат поддерживается</returns>
    bool IsFormatSupported(string format);
    
    /// <summary>
    /// Получает шаблон файла для импорта студентов
    /// </summary>
    /// <param name="format">Формат шаблона (xlsx, csv)</param>
    /// <returns>Путь к созданному шаблону</returns>
    Task<string?> GetStudentImportTemplateAsync(string format = "xlsx");
    
    /// <summary>
    /// Получает шаблон файла для импорта групп
    /// </summary>
    /// <param name="format">Формат шаблона (xlsx, csv)</param>
    /// <returns>Путь к созданному шаблону</returns>
    Task<string?> GetGroupImportTemplateAsync(string format = "xlsx");
}

/// <summary>
/// Результат валидации файла импорта
/// </summary>
public class ImportValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public string? FileFormat { get; set; }
}

/// <summary>
/// Результат предварительного просмотра импорта
/// </summary>
public class ImportPreviewResult
{
    public bool IsValid { get; set; }
    public List<string> Headers { get; set; } = new();
    public List<Dictionary<string, object?>> SampleData { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public int TotalRows { get; set; }
    public string? FileFormat { get; set; }
}
