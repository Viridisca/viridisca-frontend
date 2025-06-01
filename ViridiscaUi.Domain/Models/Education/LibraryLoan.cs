using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Заем библиотечного ресурса
/// </summary>
public class LibraryLoan : ViewModelBase
{
    private Guid _resourceUid;
    private Guid _borrowerUid;
    private DateTime _loanDate;
    private DateTime _dueDate;
    private DateTime? _returnDate;
    private bool _isReturned;
    private decimal _fineAmount;
    private string _notes = string.Empty;

    private LibraryResource? _resource;
    private Person? _borrower;

    /// <summary>
    /// ID ресурса
    /// </summary>
    public Guid ResourceUid
    {
        get => _resourceUid;
        set => this.RaiseAndSetIfChanged(ref _resourceUid, value);
    }

    /// <summary>
    /// ID заемщика
    /// </summary>
    public Guid BorrowerUid
    {
        get => _borrowerUid;
        set => this.RaiseAndSetIfChanged(ref _borrowerUid, value);
    }

    /// <summary>
    /// Дата займа
    /// </summary>
    public DateTime LoanDate
    {
        get => _loanDate;
        set => this.RaiseAndSetIfChanged(ref _loanDate, value);
    }

    /// <summary>
    /// Дата возврата (планируемая)
    /// </summary>
    public DateTime DueDate
    {
        get => _dueDate;
        set => this.RaiseAndSetIfChanged(ref _dueDate, value);
    }

    /// <summary>
    /// Дата возврата (фактическая)
    /// </summary>
    public DateTime? ReturnDate
    {
        get => _returnDate;
        set => this.RaiseAndSetIfChanged(ref _returnDate, value);
    }

    /// <summary>
    /// Возвращен ли ресурс
    /// </summary>
    public bool IsReturned
    {
        get => _isReturned;
        set => this.RaiseAndSetIfChanged(ref _isReturned, value);
    }

    /// <summary>
    /// Сумма штрафа
    /// </summary>
    public decimal FineAmount
    {
        get => _fineAmount;
        set => this.RaiseAndSetIfChanged(ref _fineAmount, value);
    }

    /// <summary>
    /// Заметки
    /// </summary>
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    /// <summary>
    /// Ресурс
    /// </summary>
    public LibraryResource? Resource
    {
        get => _resource;
        set => this.RaiseAndSetIfChanged(ref _resource, value);
    }

    /// <summary>
    /// Заемщик
    /// </summary>
    public Person? Borrower
    {
        get => _borrower;
        set => this.RaiseAndSetIfChanged(ref _borrower, value);
    }

    /// <summary>
    /// Просрочен ли заем
    /// </summary>
    public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;

    /// <summary>
    /// Количество дней просрочки
    /// </summary>
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;

    public LibraryLoan()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        LoanDate = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(14); // По умолчанию 2 недели
    }
} 