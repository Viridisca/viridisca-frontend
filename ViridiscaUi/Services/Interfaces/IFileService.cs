using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с файлами
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Загружает файл
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType,
        Guid? entityUid = null,
        string? entityType = null);
    
    /// <summary>
    /// Загружает файл по пути
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(
        string filePath,
        Guid? entityUid = null,
        string? entityType = null);
    
    /// <summary>
    /// Загружает файл (массив байтов)
    /// </summary>
    Task<FileUploadResult> UploadFileAsync(
        byte[] fileData,
        string fileName,
        string contentType,
        Guid? entityUid = null,
        string? entityType = null);
    
    /// <summary>
    /// Скачивает файл
    /// </summary>
    Task<FileDownloadResult?> DownloadFileAsync(Guid fileUid);
    
    /// <summary>
    /// Скачивает файл по имени
    /// </summary>
    Task<FileDownloadResult?> DownloadFileAsync(string fileName, Guid? entityUid = null);
    
    /// <summary>
    /// Удаляет файл
    /// </summary>
    Task<bool> DeleteFileAsync(Guid fileUid);
    
    /// <summary>
    /// Получает файлы для указанной сущности
    /// </summary>
    Task<IEnumerable<FileInfo>> GetFilesByEntityAsync(Guid entityUid, string? entityType = null);
    
    /// <summary>
    /// Получает информацию о файле
    /// </summary>
    Task<FileInfo?> GetFileInfoAsync(Guid fileUid);
    
    /// <summary>
    /// Проверяет валидность файла
    /// </summary>
    Task<FileValidationResult> ValidateFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType,
        long? maxSizeBytes = null);
    
    /// <summary>
    /// Проверяет валидность файла
    /// </summary>
    Task<FileValidationResult> ValidateFileAsync(
        string filePath,
        long? maxSizeBytes = null);
    
    /// <summary>
    /// Получает список разрешенных типов файлов
    /// </summary>
    Task<IEnumerable<string>> GetAllowedFileTypesAsync();
    
    /// <summary>
    /// Получает максимальный размер файла
    /// </summary>
    Task<long> GetMaxFileSizeAsync();
    
    /// <summary>
    /// Получает путь к файлу
    /// </summary>
    Task<string?> GetFilePathAsync(Guid fileUid);
    
    /// <summary>
    /// Создает резервную копию файла
    /// </summary>
    Task<bool> BackupFileAsync(Guid fileUid);
    
    /// <summary>
    /// Очищает временные файлы
    /// </summary>
    Task CleanupTemporaryFilesAsync(TimeSpan olderThan);
    
    /// <summary>
    /// Получает статистику использования хранилища
    /// </summary>
    Task<StorageStatistics> GetStorageStatisticsAsync();
    
    /// <summary>
    /// Выбирает файл изображения через диалог
    /// </summary>
    Task<SelectedFile?> SelectImageFileAsync();
    
    /// <summary>
    /// Загружает изображение профиля
    /// </summary>
    Task<FileUploadResult> UploadProfileImageAsync(SelectedFile selectedFile, Guid personUid);
}

/// <summary>
/// Результат загрузки файла
/// </summary>
public class FileUploadResult
{
    public bool Success { get; set; }
    public Guid? FileUid { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Результат загрузки файла
/// </summary>
public class FileDownloadResult
{
    public bool Success { get; set; }
    public Stream? FileStream { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Информация о файле
/// </summary>
public class FileInfo
{
    public Guid Uid { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public Guid? EntityUid { get; set; }
    public string? EntityType { get; set; }
    public string? FilePath { get; set; }
}

/// <summary>
/// Результат валидации файла
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
}

/// <summary>
/// Статистика хранилища
/// </summary>
public class StorageStatistics
{
    public long TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public long AvailableSpaceBytes { get; set; }
    public long UsedSpaceBytes { get; set; }
    public Dictionary<string, long> FileTypeStatistics { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Выбранный файл
/// </summary>
public class SelectedFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public Stream? Stream { get; set; }
}
