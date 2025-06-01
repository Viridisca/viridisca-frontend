using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Аккаунт для аутентификации
/// </summary>
public class Account : ViewModelBase
{
    private Guid _personUid;
    private string _username = string.Empty;
    private string _passwordHash = string.Empty;
    private bool _isEmailConfirmed;
    private bool _isLocked;
    private bool _isActive = true;
    private int _failedLoginAttempts;
    private DateTime? _lastLoginAt;
    private DateTime? _lastFailedLoginAt;
    private DateTime? _lockedUntil;

    private Person? _person;

    /// <summary>
    /// ID связанного человека
    /// </summary>
    public Guid PersonUid
    {
        get => _personUid;
        set => this.RaiseAndSetIfChanged(ref _personUid, value);
    }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    /// <summary>
    /// Хеш пароля
    /// </summary>
    public string PasswordHash
    {
        get => _passwordHash;
        set => this.RaiseAndSetIfChanged(ref _passwordHash, value);
    }

    /// <summary>
    /// Подтвержден ли email
    /// </summary>
    public bool IsEmailConfirmed
    {
        get => _isEmailConfirmed;
        set => this.RaiseAndSetIfChanged(ref _isEmailConfirmed, value);
    }

    /// <summary>
    /// Заблокирован ли аккаунт
    /// </summary>
    public bool IsLocked
    {
        get => _isLocked;
        set => this.RaiseAndSetIfChanged(ref _isLocked, value);
    }

    /// <summary>
    /// Активен ли аккаунт
    /// </summary>
    public bool IsActive
    {
        get => _isActive && (!IsLocked || (LockedUntil.HasValue && LockedUntil.Value < DateTime.UtcNow));
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Количество неудачных попыток входа
    /// </summary>
    public int FailedLoginAttempts
    {
        get => _failedLoginAttempts;
        set => this.RaiseAndSetIfChanged(ref _failedLoginAttempts, value);
    }

    /// <summary>
    /// Последний вход в систему
    /// </summary>
    public DateTime? LastLoginAt
    {
        get => _lastLoginAt;
        set => this.RaiseAndSetIfChanged(ref _lastLoginAt, value);
    }

    /// <summary>
    /// Последняя неудачная попытка входа
    /// </summary>
    public DateTime? LastFailedLoginAt
    {
        get => _lastFailedLoginAt;
        set => this.RaiseAndSetIfChanged(ref _lastFailedLoginAt, value);
    }

    /// <summary>
    /// Заблокирован до
    /// </summary>
    public DateTime? LockedUntil
    {
        get => _lockedUntil;
        set => this.RaiseAndSetIfChanged(ref _lockedUntil, value);
    }

    /// <summary>
    /// Связанный человек
    /// </summary>
    public Person? Person
    {
        get => _person;
        set => this.RaiseAndSetIfChanged(ref _person, value);
    }

    public Account()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Регистрирует успешный вход
    /// </summary>
    public void RegisterSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        IsLocked = false;
        LockedUntil = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Регистрирует неудачную попытку входа
    /// </summary>
    public void RegisterFailedLogin()
    {
        FailedLoginAttempts++;
        LastFailedLoginAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        // Блокируем аккаунт после 5 неудачных попыток
        if (FailedLoginAttempts >= 5)
        {
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.AddMinutes(30); // Блокируем на 30 минут
        }
    }

    /// <summary>
    /// Разблокирует аккаунт
    /// </summary>
    public void Unlock()
    {
        IsLocked = false;
        LockedUntil = null;
        FailedLoginAttempts = 0;
        LastModifiedAt = DateTime.UtcNow;
    }
} 