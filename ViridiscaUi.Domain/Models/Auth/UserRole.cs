using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Связь пользователя с ролью
/// </summary>
public class UserRole : ViewModelBase
{
    private Guid _userUid;
    private Guid _roleUid;
    private Role _role = null!;
    private bool _isActive;
    private DateTime _assignedAt;
    private DateTime? _expiresAt;

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserUid
    {
        get => _userUid;
        set => this.RaiseAndSetIfChanged(ref _userUid, value);
    }

    /// <summary>
    /// Идентификатор роли
    /// </summary>
    public Guid RoleUid
    {
        get => _roleUid;
        set => this.RaiseAndSetIfChanged(ref _roleUid, value);
    }

    /// <summary>
    /// Роль
    /// </summary>
    public Role Role
    {
        get => _role;
        set => this.RaiseAndSetIfChanged(ref _role, value);
    }

    /// <summary>
    /// Флаг активности роли
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Дата назначения роли
    /// </summary>
    public DateTime AssignedAt
    {
        get => _assignedAt;
        set => this.RaiseAndSetIfChanged(ref _assignedAt, value);
    }

    /// <summary>
    /// Дата окончания действия роли
    /// </summary>
    public DateTime? ExpiresAt
    {
        get => _expiresAt;
        set => this.RaiseAndSetIfChanged(ref _expiresAt, value);
    }

    /// <summary>
    /// Пользователь
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Создает новый экземпляр связи пользователя с ролью
    /// </summary>
    public UserRole()
    {
        _assignedAt = DateTime.UtcNow;
        _isActive = true;
    }

    /// <summary>
    /// Создает новый экземпляр связи пользователя с ролью с указанными параметрами
    /// </summary>
    public UserRole(Guid userUid, Guid roleUid, Role role)
    {
        Uid = Guid.NewGuid();
        _userUid = userUid;
        _roleUid = roleUid;
        _role = role;
        _assignedAt = DateTime.UtcNow;
        _isActive = true;
    }

    /// <summary>
    /// Деактивирует роль
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активирует роль
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Устанавливает срок действия роли
    /// </summary>
    public void SetExpiration(DateTime? expirationDate)
    {
        ExpiresAt = expirationDate;
        LastModifiedAt = DateTime.UtcNow;
    }
} 