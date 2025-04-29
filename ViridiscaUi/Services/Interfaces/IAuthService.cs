using System;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для аутентификации и авторизации
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Аутентифицирует пользователя по логину и паролю
        /// </summary>
        Task<(bool Success, User? User, string ErrorMessage)> AuthenticateAsync(string username, string password);
        
        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        Task<(bool Success, User? User, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName);
        
        /// <summary>
        /// Выходит из системы
        /// </summary>
        Task LogoutAsync();
        
        /// <summary>
        /// Получает текущего аутентифицированного пользователя
        /// </summary>
        Task<User?> GetCurrentUserAsync();
        
        /// <summary>
        /// Наблюдаемый объект, отражающий текущего пользователя
        /// </summary>
        IObservable<User?> CurrentUserObservable { get; }
        
        /// <summary>
        /// Проверяет доступ пользователя к определенному разрешению
        /// </summary>
        Task<bool> HasPermissionAsync(Guid userUid, string permissionName);
        
        /// <summary>
        /// Проверяет наличие определенной роли у пользователя
        /// </summary>
        Task<bool> IsInRoleAsync(Guid userUid, string roleName);
        
        /// <summary>
        /// Изменяет пароль пользователя
        /// </summary>
        Task<bool> ChangePasswordAsync(Guid userUid, string currentPassword, string newPassword);
        
        /// <summary>
        /// Запрашивает сброс пароля пользователя
        /// </summary>
        Task<bool> RequestPasswordResetAsync(string email);
        
        /// <summary>
        /// Сбрасывает пароль пользователя
        /// </summary>
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
} 