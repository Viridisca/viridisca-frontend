using System;
using System.ComponentModel.DataAnnotations;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель запроса на регистрацию нового пользователя
/// </summary>
public class RegisterRequest : ViewModelBase
{
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _middleName = string.Empty;
    private string _phoneNumber = string.Empty;
    private DateTime _dateOfBirth;

    /// <summary>
    /// Электронная почта
    /// </summary>
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [MinLength(4, ErrorMessage = "Имя пользователя должно содержать минимум 4 символа")]
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    /// <summary>
    /// Пароль
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    [Required(ErrorMessage = "Необходимо подтвердить пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
    }

    /// <summary>
    /// Имя
    /// </summary>
    [Required(ErrorMessage = "Имя обязательно")]
    public string FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    /// <summary>
    /// Фамилия
    /// </summary>
    [Required(ErrorMessage = "Фамилия обязательна")]
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
    [Phone(ErrorMessage = "Некорректный формат номера телефона")]
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
    /// Создает новый экземпляр запроса на регистрацию
    /// </summary>
    public RegisterRequest()
    {
        Uid = Guid.NewGuid();
        _dateOfBirth = DateTime.Today.AddYears(-18); // По умолчанию 18 лет назад
    }
} 