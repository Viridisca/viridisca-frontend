using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Базовая сущность для всех людей в системе
/// </summary>
public class Person : ViewModelBase
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _middleName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    private DateTime _dateOfBirth;
    private string _profileImageUrl = string.Empty;
    private string _address = string.Empty;
    private bool _isActive = true;

    private ObservableCollection<PersonRole> _personRoles = [];

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
    /// Email
    /// </summary>
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    /// <summary>
    /// Телефон
    /// </summary>
    public string PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
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
    /// URL изображения профиля
    /// </summary>
    public string ProfileImageUrl
    {
        get => _profileImageUrl;
        set => this.RaiseAndSetIfChanged(ref _profileImageUrl, value);
    }

    /// <summary>
    /// Адрес
    /// </summary>
    public string Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    /// <summary>
    /// Активен ли человек
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Роли человека в системе
    /// </summary>
    public ObservableCollection<PersonRole> PersonRoles
    {
        get => _personRoles;
        set => this.RaiseAndSetIfChanged(ref _personRoles, value);
    }

    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Возраст
    /// </summary>
    public int Age => DateTime.Now.Year - DateOfBirth.Year;

    public Person()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 