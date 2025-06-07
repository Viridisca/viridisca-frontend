using System;
using System.ComponentModel.DataAnnotations;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель запроса на регистрацию нового пользователя
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Электронная почта
    /// </summary>
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [MinLength(4, ErrorMessage = "Имя пользователя должно содержать минимум 4 символа")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    [Required(ErrorMessage = "Необходимо подтвердить пароль")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Имя
    /// </summary>
    [Required(ErrorMessage = "Имя обязательно")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия
    /// </summary>
    [Required(ErrorMessage = "Фамилия обязательна")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    [Phone(ErrorMessage = "Некорректный формат номера телефона")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime? DateOfBirth { get; set; }
} 