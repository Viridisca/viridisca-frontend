using System.ComponentModel.DataAnnotations;
using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Модель запроса на смену пароля
/// </summary>
public class PasswordChangeRequest : ViewModelBase
{
    private string _oldPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;

    /// <summary>
    /// Текущий пароль пользователя
    /// </summary>
    [Required(ErrorMessage = "Текущий пароль обязателен")]
    public string OldPassword
    {
        get => _oldPassword;
        set => this.RaiseAndSetIfChanged(ref _oldPassword, value);
    }

    /// <summary>
    /// Новый пароль
    /// </summary>
    [Required(ErrorMessage = "Новый пароль обязателен")]
    [MinLength(8, ErrorMessage = "Новый пароль должен содержать минимум 8 символов")]
    public string NewPassword
    {
        get => _newPassword;
        set => this.RaiseAndSetIfChanged(ref _newPassword, value);
    }

    /// <summary>
    /// Подтверждение нового пароля
    /// </summary>
    [Required(ErrorMessage = "Необходимо подтвердить новый пароль")]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
    }

    /// <summary>
    /// Создает новый экземпляр запроса на смену пароля
    /// </summary>
    public PasswordChangeRequest()
    {
        Uid = Guid.NewGuid();
    }
} 