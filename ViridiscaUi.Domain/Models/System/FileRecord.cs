using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Запись о файле в системе
/// </summary>
public class FileRecord : ViewModelBase
{
    private string _originalFileName = string.Empty;
    private string _storedFileName = string.Empty;
    private string _filePath = string.Empty;
    private string _contentType = string.Empty;
    private long _fileSize;
    private Guid _uploadedByUid;
    private string? _entityType;
    private Guid? _entityUid;
    private string _description = string.Empty;
    private bool _isPublic;
    private DateTime? _expiresAt;

    /// <summary>
    /// Оригинальное имя файла
    /// </summary>
    public string OriginalFileName
    {
        get => _originalFileName;
        set => this.RaiseAndSetIfChanged(ref _originalFileName, value);
    }

    /// <summary>
    /// Имя файла в хранилище
    /// </summary>
    public string StoredFileName
    {
        get => _storedFileName;
        set => this.RaiseAndSetIfChanged(ref _storedFileName, value);
    }

    /// <summary>
    /// Путь к файлу
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    /// <summary>
    /// MIME-тип файла
    /// </summary>
    public string ContentType
    {
        get => _contentType;
        set => this.RaiseAndSetIfChanged(ref _contentType, value);
    }

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSize
    {
        get => _fileSize;
        set => this.RaiseAndSetIfChanged(ref _fileSize, value);
    }

    /// <summary>
    /// Идентификатор пользователя, загрузившего файл
    /// </summary>
    public Guid UploadedByUid
    {
        get => _uploadedByUid;
        set => this.RaiseAndSetIfChanged(ref _uploadedByUid, value);
    }

    /// <summary>
    /// Тип сущности, к которой привязан файл
    /// </summary>
    public string? EntityType
    {
        get => _entityType;
        set => this.RaiseAndSetIfChanged(ref _entityType, value);
    }

    /// <summary>
    /// Идентификатор сущности, к которой привязан файл
    /// </summary>
    public Guid? EntityUid
    {
        get => _entityUid;
        set => this.RaiseAndSetIfChanged(ref _entityUid, value);
    }

    /// <summary>
    /// Описание файла
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Флаг публичного доступа к файлу
    /// </summary>
    public bool IsPublic
    {
        get => _isPublic;
        set => this.RaiseAndSetIfChanged(ref _isPublic, value);
    }

    /// <summary>
    /// Дата истечения срока действия файла
    /// </summary>
    public DateTime? ExpiresAt
    {
        get => _expiresAt;
        set => this.RaiseAndSetIfChanged(ref _expiresAt, value);
    }

    /// <summary>
    /// Создает новый экземпляр записи о файле
    /// </summary>
    public FileRecord()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр записи о файле с указанными параметрами
    /// </summary>
    public FileRecord(string originalFileName, string storedFileName, string filePath, 
                     string contentType, long fileSize, Guid uploadedByUid)
    {
        Uid = Guid.NewGuid();
        _originalFileName = originalFileName;
        _storedFileName = storedFileName;
        _filePath = filePath;
        _contentType = contentType;
        _fileSize = fileSize;
        _uploadedByUid = uploadedByUid;
        CreatedAt = DateTime.UtcNow;
    }
} 