using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Роль человека в системе
/// </summary>
public class PersonRole : ViewModelBase
{
    private Guid _personUid;
    private Guid _roleUid;
    private DateTime _assignedAt;
    private DateTime? _expiresAt;
    private bool _isActive = true;
    private string _assignedBy = string.Empty;
    private string _context = string.Empty; // Контекст роли (например, для какой группы/департамента)

    private Person? _person;
    private Role? _role;

    /// <summary>
    /// ID человека
    /// </summary>
    public Guid PersonUid
    {
        get => _personUid;
        set => this.RaiseAndSetIfChanged(ref _personUid, value);
    }

    /// <summary>
    /// ID роли
    /// </summary>
    public Guid RoleUid
    {
        get => _roleUid;
        set => this.RaiseAndSetIfChanged(ref _roleUid, value);
    }

    /// <summary>
    /// Когда назначена роль
    /// </summary>
    public DateTime AssignedAt
    {
        get => _assignedAt;
        set => this.RaiseAndSetIfChanged(ref _assignedAt, value);
    }

    /// <summary>
    /// Когда истекает роль
    /// </summary>
    public DateTime? ExpiresAt
    {
        get => _expiresAt;
        set => this.RaiseAndSetIfChanged(ref _expiresAt, value);
    }

    /// <summary>
    /// Активна ли роль
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Кем назначена роль
    /// </summary>
    public string AssignedBy
    {
        get => _assignedBy;
        set => this.RaiseAndSetIfChanged(ref _assignedBy, value);
    }

    /// <summary>
    /// Контекст роли (группа, департамент и т.д.)
    /// </summary>
    public string Context
    {
        get => _context;
        set => this.RaiseAndSetIfChanged(ref _context, value);
    }

    /// <summary>
    /// Человек
    /// </summary>
    public Person? Person
    {
        get => _person;
        set => this.RaiseAndSetIfChanged(ref _person, value);
    }

    /// <summary>
    /// Роль
    /// </summary>
    public Role? Role
    {
        get => _role;
        set => this.RaiseAndSetIfChanged(ref _role, value);
    }

    /// <summary>
    /// Истекла ли роль
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    public PersonRole()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        AssignedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Extend(DateTime newExpiryDate)
    {
        ExpiresAt = newExpiryDate;
        LastModifiedAt = DateTime.UtcNow;
    }
} 