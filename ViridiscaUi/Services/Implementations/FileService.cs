using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using FileInfo = ViridiscaUi.Services.Interfaces.FileInfo;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с файлами
/// </summary>
public class FileService : IFileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FileService> _logger;
    private readonly string _uploadPath;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;

    public FileService(ApplicationDbContext dbContext, ILogger<FileService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        
        // Настройки файлового хранилища
        _uploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ViridiscaUi", "Files");
        _maxFileSize = 50 * 1024 * 1024; // 50 MB
        _allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };
        
        // Создаем директорию если не существует
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType, Guid? entityUid = null, string? entityType = null)
    {
        try
        {
            // Валидация файла
            var validationResult = await ValidateFileAsync(fileStream, fileName, contentType);
            if (!validationResult.IsValid)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", validationResult.Errors)
                };
            }

            // Генерируем уникальное имя файла
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // Сохраняем файл на диск
            using (var fileStreamDisk = new FileStream(filePath, FileMode.Create))
            {
                fileStream.Position = 0; // Сбрасываем позицию потока
                await fileStream.CopyToAsync(fileStreamDisk);
            }

            // Создаем запись в базе данных
            var fileRecord = new FileRecord
            {
                Uid = Guid.NewGuid(),
                OriginalFileName = fileName,
                StoredFileName = uniqueFileName,
                FilePath = filePath,
                ContentType = contentType,
                FileSize = fileStream.Length,
                EntityType = entityType, // ParseEntityType(entityType),
                EntityUid = entityUid,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.FileRecords.AddAsync(fileRecord);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("File uploaded successfully: {FileName}", fileName);

            return new FileUploadResult
            {
                Success = true,
                FileUid = fileRecord.Uid,
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileStream.Length,
                ContentType = contentType,
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = "Произошла ошибка при загрузке файла"
            };
        }
    }

    public async Task<FileUploadResult> UploadFileAsync(string filePath, Guid? entityUid = null, string? entityType = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Файл не найден"
                };
            }

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await UploadFileAsync(fileStream, fileName, contentType, entityUid, entityType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file from path: {FilePath}", filePath);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = "Произошла ошибка при загрузке файла"
            };
        }
    }

    public async Task<FileUploadResult> UploadFileAsync(byte[] fileData, string fileName, string contentType, Guid? entityUid = null, string? entityType = null)
    {
        try
        {
            using var memoryStream = new MemoryStream(fileData);
            return await UploadFileAsync(memoryStream, fileName, contentType, entityUid, entityType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file from byte array: {FileName}", fileName);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = "Произошла ошибка при загрузке файла"
            };
        }
    }

    public async Task<FileDownloadResult?> DownloadFileAsync(Guid fileUid)
    {
        try
        {
            var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            if (fileRecord == null)
            {
                return new FileDownloadResult
                {
                    Success = false,
                    ErrorMessage = "Файл не найден"
                };
            }

            if (!File.Exists(fileRecord.FilePath))
            {
                _logger.LogWarning("File not found on disk: {FilePath}", fileRecord.FilePath);
                return new FileDownloadResult
                {
                    Success = false,
                    ErrorMessage = "Файл не найден на диске"
                };
            }

            var fileStream = new FileStream(fileRecord.FilePath, FileMode.Open, FileAccess.Read);

            return new FileDownloadResult
            {
                Success = true,
                FileStream = fileStream,
                FileName = fileRecord.OriginalFileName,
                ContentType = fileRecord.ContentType,
                FileSize = fileRecord.FileSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileUid}", fileUid);
            return new FileDownloadResult
            {
                Success = false,
                ErrorMessage = "Произошла ошибка при скачивании файла"
            };
        }
    }

    public async Task<FileDownloadResult?> DownloadFileAsync(string fileName, Guid? entityUid = null)
    {
        try
        {
            var query = _dbContext.FileRecords.AsQueryable();
            
            if (entityUid.HasValue)
            {
                query = query.Where(f => f.EntityUid == entityUid.Value && f.OriginalFileName == fileName);
            }
            else
            {
                query = query.Where(f => f.OriginalFileName == fileName);
            }

            var fileRecord = await query.FirstOrDefaultAsync();
            if (fileRecord == null)
            {
                return new FileDownloadResult
                {
                    Success = false,
                    ErrorMessage = "Файл не найден"
                };
            }

            return await DownloadFileAsync(fileRecord.Uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file by name: {FileName}", fileName);
            return new FileDownloadResult
            {
                Success = false,
                ErrorMessage = "Произошла ошибка при скачивании файла"
            };
        }
    }

    public async Task<bool> DeleteFileAsync(Guid fileUid)
    {
        try
        {
            var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            if (fileRecord == null)
                return false;

            // Удаляем файл с диска
            if (File.Exists(fileRecord.FilePath))
            {
                File.Delete(fileRecord.FilePath);
            }

            // Удаляем запись из базы данных
            _dbContext.FileRecords.Remove(fileRecord);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("File deleted successfully: {FileUid}", fileUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUid}", fileUid);
            return false;
        }
    }

    public async Task<IEnumerable<FileInfo>> GetFilesByEntityAsync(Guid entityUid, string? entityType = null)
    {
        var query = _dbContext.FileRecords
            .Where(f => f.EntityUid == entityUid);

        if (!string.IsNullOrEmpty(entityType))
        {
            var parsedEntityType = entityType; // ParseEntityType(entityType);
            query = query.Where(f => f.EntityType == parsedEntityType);
        }

        var fileRecords = await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return fileRecords.Select(f => new FileInfo
        {
            Uid = f.Uid,
            FileName = f.OriginalFileName,
            ContentType = f.ContentType,
            Size = f.FileSize,
            CreatedAt = f.CreatedAt,
            LastModifiedAt = f.LastModifiedAt ?? f.CreatedAt,
            EntityUid = f.EntityUid,
            EntityType = f.EntityType.ToString(),
            FilePath = f.FilePath
        });
    }

    public async Task<FileInfo?> GetFileInfoAsync(Guid fileUid)
    {
        var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
        if (fileRecord == null)
            return null;

        return new FileInfo
        {
            Uid = fileRecord.Uid,
            FileName = fileRecord.OriginalFileName,
            ContentType = fileRecord.ContentType,
            Size = fileRecord.FileSize,
            CreatedAt = fileRecord.CreatedAt,
            LastModifiedAt = fileRecord.LastModifiedAt ?? fileRecord.CreatedAt,
            EntityUid = fileRecord.EntityUid,
            EntityType = fileRecord.EntityType.ToString(),
            FilePath = fileRecord.FilePath
        };
    }

    public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType, long? maxSizeBytes = null)
    {
        var result = new FileValidationResult
        {
            ContentType = contentType,
            FileSize = fileStream.Length
        };

        // Проверка размера файла
        var maxSize = maxSizeBytes ?? _maxFileSize;
        if (fileStream.Length > maxSize)
        {
            result.Errors.Add($"Размер файла превышает максимально допустимый ({maxSize / (1024 * 1024)} MB)");
        }

        // Проверка расширения файла
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(fileExtension))
        {
            result.Errors.Add($"Тип файла не поддерживается. Разрешенные типы: {string.Join(", ", _allowedExtensions)}");
        }

        // Проверка имени файла
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
        {
            result.Errors.Add("Недопустимое имя файла");
        }

        // Проверка на вредоносное содержимое (базовая)
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            result.Errors.Add("Имя файла содержит недопустимые символы");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    public async Task<FileValidationResult> ValidateFileAsync(string filePath, long? maxSizeBytes = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    Errors = { "Файл не найден" }
                };
            }

            var fileName = Path.GetFileName(filePath);
            var contentType = GetContentType(fileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return await ValidateFileAsync(fileStream, fileName, contentType, maxSizeBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file: {FilePath}", filePath);
            return new FileValidationResult
            {
                IsValid = false,
                Errors = { "Ошибка при проверке файла" }
            };
        }
    }

    public async Task<IEnumerable<string>> GetAllowedFileTypesAsync()
    {
        return await Task.FromResult(_allowedExtensions.AsEnumerable());
    }

    public async Task<long> GetMaxFileSizeAsync()
    {
        return await Task.FromResult(_maxFileSize);
    }

    public async Task<string?> GetFilePathAsync(Guid fileUid)
    {
        var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
        return fileRecord?.FilePath;
    }

    public async Task<bool> BackupFileAsync(Guid fileUid)
    {
        try
        {
            var fileRecord = await _dbContext.FileRecords.FindAsync(fileUid);
            if (fileRecord == null || !File.Exists(fileRecord.FilePath))
                return false;

            var backupPath = Path.Combine(_uploadPath, "Backups");
            Directory.CreateDirectory(backupPath);

            var backupFileName = $"{Path.GetFileNameWithoutExtension(fileRecord.StoredFileName)}_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(fileRecord.StoredFileName)}";
            var backupFilePath = Path.Combine(backupPath, backupFileName);

            File.Copy(fileRecord.FilePath, backupFilePath, true);

            _logger.LogInformation("File backup created: {FileUid} -> {BackupPath}", fileUid, backupFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup for file: {FileUid}", fileUid);
            return false;
        }
    }

    public async Task CleanupTemporaryFilesAsync(TimeSpan olderThan)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow - olderThan;
            
            var orphanedFiles = await _dbContext.FileRecords
                .Where(f => f.EntityUid == null && f.CreatedAt < cutoffDate)
                .ToListAsync();

            foreach (var file in orphanedFiles)
            {
                await DeleteFileAsync(file.Uid);
            }

            _logger.LogInformation("Cleaned up {Count} temporary files older than {CutoffDate}", orphanedFiles.Count, cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up temporary files");
        }
    }

    public async Task<StorageStatistics> GetStorageStatisticsAsync()
    {
        try
        {
            var totalFiles = await _dbContext.FileRecords.CountAsync();
            var totalSize = await _dbContext.FileRecords.SumAsync(f => f.FileSize);
            
            var filesByType = await _dbContext.FileRecords
                .GroupBy(f => f.ContentType)
                .Select(g => new { ContentType = g.Key, Count = g.Count(), Size = g.Sum(f => f.FileSize) })
                .ToListAsync();

            // Получаем информацию о доступном месте на диске
            var driveInfo = new DriveInfo(Path.GetPathRoot(_uploadPath) ?? "C:");
            var availableSpace = driveInfo.AvailableFreeSpace;
            var usedSpace = totalSize;

            return new StorageStatistics
            {
                TotalFiles = totalFiles,
                TotalSizeBytes = totalSize,
                AvailableSpaceBytes = availableSpace,
                UsedSpaceBytes = usedSpace,
                FileTypeStatistics = filesByType.ToDictionary(x => x.ContentType, x => (long)x.Count),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage statistics");
            return new StorageStatistics
            {
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    public async Task<SelectedFile?> SelectImageFileAsync()
    {
        try
        {
            // Получаем главное окно для диалога
            var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
            {
                _logger.LogWarning("Main window not found for file dialog");
                return null;
            }

            // Создаем диалог выбора файла
            var dialog = new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Выберите изображение",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new Avalonia.Platform.Storage.FilePickerFileType("Изображения")
                    {
                        Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" }
                    }
                }
            };

            var result = await mainWindow.StorageProvider.OpenFilePickerAsync(dialog);
            if (result == null || result.Count == 0)
            {
                return null;
            }

            var selectedFile = result[0];
            var fileInfo = new System.IO.FileInfo(selectedFile.Path.LocalPath);
            
            return new SelectedFile
            {
                Name = selectedFile.Name,
                Path = selectedFile.Path.LocalPath,
                Size = fileInfo.Length,
                ContentType = GetContentType(selectedFile.Name),
                Stream = await selectedFile.OpenReadAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting image file");
            return null;
        }
    }

    public async Task<FileUploadResult> UploadProfileImageAsync(SelectedFile selectedFile, Guid personUid)
    {
        try
        {
            // Валидация изображения
            if (selectedFile.Stream == null)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Поток файла недоступен"
                };
            }

            // Проверяем, что это изображение
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(selectedFile.Name).ToLowerInvariant();
            if (!imageExtensions.Contains(extension))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "Выбранный файл не является изображением"
                };
            }

            // Загружаем файл
            var uploadResult = await UploadFileAsync(
                selectedFile.Stream, 
                selectedFile.Name, 
                selectedFile.ContentType, 
                personUid, 
                "ProfileImage");

            if (uploadResult.Success)
            {
                _logger.LogInformation("Profile image uploaded for person {PersonUid}: {FileName}", personUid, selectedFile.Name);
            }

            return uploadResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile image for person {PersonUid}", personUid);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = "Произошла ошибка при загрузке изображения профиля"
            };
        }
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    } 
} 