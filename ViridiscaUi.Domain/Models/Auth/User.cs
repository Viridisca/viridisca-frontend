using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Пользователь системы
/// </summary>
public class User : ViewModelBase
{
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _middleName = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _profileImageUrl = string.Empty;
    private string _passwordHash = string.Empty;
    private DateTime _dateOfBirth;
    private bool _isEmailConfirmed;
    private bool _isActive;
    private DateTime? _lastLoginAt;
    private ObservableCollection<UserRole> _userRoles = new();
    private Student? _studentProfile;
    private Teacher? _teacherProfile;
    private Guid _roleId;
    private Role? _role;

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    /// <summary>
    /// Имя пользователя для входа
    /// </summary>
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    /// <summary>
    /// Отчество
    /// </summary>
    public string MiddleName
    {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
    }

    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
    }

    /// <summary>
    /// URL-адрес изображения профиля
    /// </summary>
    public string ProfileImageUrl
    {
        get => _profileImageUrl;
        set => this.RaiseAndSetIfChanged(ref _profileImageUrl, value);
    }

    /// <summary>
    /// Хеш пароля пользователя
    /// </summary>
    public string PasswordHash
    {
        get => _passwordHash;
        set => this.RaiseAndSetIfChanged(ref _passwordHash, value);
    }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => this.RaiseAndSetIfChanged(ref _dateOfBirth, value);
    }

    /// <summary>
    /// Флаг подтверждения электронной почты
    /// </summary>
    public bool IsEmailConfirmed
    {
        get => _isEmailConfirmed;
        set => this.RaiseAndSetIfChanged(ref _isEmailConfirmed, value);
    }

    /// <summary>
    /// Флаг активности учетной записи
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Дата последнего входа в систему
    /// </summary>
    public DateTime? LastLoginAt
    {
        get => _lastLoginAt;
        set => this.RaiseAndSetIfChanged(ref _lastLoginAt, value);
    }

    /// <summary>
    /// Идентификатор основной роли пользователя
    /// </summary>
    public Guid RoleId
    {
        get => _roleId;
        set => this.RaiseAndSetIfChanged(ref _roleId, value);
    }
    
    /// <summary>
    /// Основная роль пользователя
    /// </summary>
    public Role? Role
    {
        get => _role;
        set => this.RaiseAndSetIfChanged(ref _role, value);
    }

    /// <summary>
    /// Полное имя пользователя (Фамилия Имя Отчество)
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Роли пользователя
    /// </summary>
    public ObservableCollection<UserRole> UserRoles
    {
        get => _userRoles;
        set => this.RaiseAndSetIfChanged(ref _userRoles, value);
    }

    /// <summary>
    /// Профиль студента (если пользователь является студентом)
    /// </summary>
    public Student? StudentProfile
    {
        get => _studentProfile;
        set => this.RaiseAndSetIfChanged(ref _studentProfile, value);
    }

    /// <summary>
    /// Профиль преподавателя (если пользователь является преподавателем)
    /// </summary>
    public Teacher? TeacherProfile
    {
        get => _teacherProfile;
        set => this.RaiseAndSetIfChanged(ref _teacherProfile, value);
    }

    /// <summary>
    /// Создает новый экземпляр пользователя
    /// </summary>
    public User()
    {
    }

    /// <summary>
    /// Создает новый экземпляр пользователя с указанными параметрами
    /// </summary>
    public User(Guid uid, string email, string firstName, string lastName)
    {
        Uid = uid;
        _email = email;
        _firstName = firstName;
        _lastName = lastName;
        _username = email;
        _isActive = true;
    }

    /// <summary>
    /// Добавляет роль пользователю
    /// </summary>
    public void AddRole(Role role)
    {
        var userRole = new UserRole
        {
            Uid = Guid.NewGuid(),
            UserUid = this.Uid,
            RoleUid = role.Uid,
            Role = role,
            IsActive = true,
            AssignedAt = DateTime.UtcNow
        };

        if (!UserRoles.Any(r => r.RoleUid == role.Uid))
        {
            UserRoles.Add(userRole);
            this.RaisePropertyChanged(nameof(UserRoles));
        }
    }

    /// <summary>
    /// Удаляет роль у пользователя
    /// </summary>
    public void RemoveRole(Guid roleUid)
    {
        var roleToRemove = UserRoles.FirstOrDefault(r => r.RoleUid == roleUid);
        if (roleToRemove != null)
        {
            UserRoles.Remove(roleToRemove);
            this.RaisePropertyChanged(nameof(UserRoles));
        }
    }

    /// <summary>
    /// Деактивирует учетную запись
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Активирует учетную запись
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Подтверждает электронную почту
    /// </summary>
    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
    }

    /// <summary>
    /// Обновляет дату последнего входа
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
} 