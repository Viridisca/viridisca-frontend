using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education.Enums;
using System.Collections.ObjectModel;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Библиотечный ресурс
/// </summary>
public class LibraryResource : ViewModelBase
{
    private string _title = string.Empty;
    private string _author = string.Empty;
    private string _isbn = string.Empty;
    private string _publisher = string.Empty;
    private DateTime? _publishedDate;
    private ResourceType _type = ResourceType.Book;
    private string _description = string.Empty;
    private string _location = string.Empty;
    private int _totalCopies = 1;
    private int _availableCopies = 1;
    private bool _isDigital;
    private string _digitalUrl = string.Empty;
    private string _tags = string.Empty;

    private ObservableCollection<LibraryLoan> _loans = [];

    /// <summary>
    /// Название ресурса
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Автор
    /// </summary>
    public string Author
    {
        get => _author;
        set => this.RaiseAndSetIfChanged(ref _author, value);
    }

    /// <summary>
    /// ISBN
    /// </summary>
    public string ISBN
    {
        get => _isbn;
        set => this.RaiseAndSetIfChanged(ref _isbn, value);
    }

    /// <summary>
    /// Издательство
    /// </summary>
    public string Publisher
    {
        get => _publisher;
        set => this.RaiseAndSetIfChanged(ref _publisher, value);
    }

    /// <summary>
    /// Дата публикации
    /// </summary>
    public DateTime? PublishedDate
    {
        get => _publishedDate;
        set => this.RaiseAndSetIfChanged(ref _publishedDate, value);
    }

    /// <summary>
    /// Тип ресурса
    /// </summary>
    public ResourceType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Описание
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Местоположение в библиотеке
    /// </summary>
    public string Location
    {
        get => _location;
        set => this.RaiseAndSetIfChanged(ref _location, value);
    }

    /// <summary>
    /// Общее количество экземпляров
    /// </summary>
    public int TotalCopies
    {
        get => _totalCopies;
        set => this.RaiseAndSetIfChanged(ref _totalCopies, value);
    }

    /// <summary>
    /// Доступное количество экземпляров
    /// </summary>
    public int AvailableCopies
    {
        get => _availableCopies;
        set => this.RaiseAndSetIfChanged(ref _availableCopies, value);
    }

    /// <summary>
    /// Цифровой ресурс
    /// </summary>
    public bool IsDigital
    {
        get => _isDigital;
        set => this.RaiseAndSetIfChanged(ref _isDigital, value);
    }

    /// <summary>
    /// URL цифрового ресурса
    /// </summary>
    public string DigitalUrl
    {
        get => _digitalUrl;
        set => this.RaiseAndSetIfChanged(ref _digitalUrl, value);
    }

    /// <summary>
    /// Теги для поиска
    /// </summary>
    public string Tags
    {
        get => _tags;
        set => this.RaiseAndSetIfChanged(ref _tags, value);
    }

    /// <summary>
    /// Займы ресурса
    /// </summary>
    public ObservableCollection<LibraryLoan> Loans
    {
        get => _loans;
        set => this.RaiseAndSetIfChanged(ref _loans, value);
    }

    /// <summary>
    /// Доступен ли ресурс для займа
    /// </summary>
    public bool IsAvailable => AvailableCopies > 0 || IsDigital;

    public LibraryResource()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 