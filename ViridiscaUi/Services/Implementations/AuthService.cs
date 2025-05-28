using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Сервис аутентификации
/// </summary>
/// <remarks>
/// Создает новый экземпляр сервиса аутентификации
/// </remarks>
public class AuthService(
    IUserService userService,
    IPermissionService permissionService,
    IRoleService roleService,
    IUserSessionService userSessionService) : IAuthService
{
    private readonly IUserService _userService = userService;
    private readonly IPermissionService _permissionService = permissionService;
    private readonly IRoleService _roleService = roleService;
    private readonly IUserSessionService _userSessionService = userSessionService;

    /// <summary>
    /// Аутентифицирует пользователя по логину и паролю
    /// </summary>
    public async Task<(bool Success, User? User, string ErrorMessage)> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Попробуем найти пользователя сначала по username, затем по email
            var user = await _userService.GetUserByUsernameAsync(username) ?? await _userService.GetUserByEmailAsync(username);

            if (user is null)
            {
                _userSessionService.ClearSession();
                return (false, null, "Пользователь не найден");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _userSessionService.ClearSession();
                return (false, null, "Неверный пароль");
            }

            if (!user.IsActive)
            {
                _userSessionService.ClearSession();
                return (false, null, "Учетная запись заблокирована");
            }

            // Логируем информацию о загруженном пользователе для отладки
            StatusLogger.LogInfo($"Пользователь успешно аутентифицирован: {user.Email} ({user.Role?.Name ?? "без роли"})", "AuthService");

            _userSessionService.SetCurrentUser(user);
            
            return (true, user, string.Empty);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка аутентификации: {ex.Message}", "AuthService");
            _userSessionService.ClearSession();
            return (false, null, $"Ошибка аутентификации: {ex.Message}");
        }
    }

    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    public async Task<(bool Success, User? User, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
    {
        // Используем роль студента по умолчанию
        var studentRole = await _roleService.GetRoleByNameAsync("Student");

        if (studentRole is null)
        {
            return (false, null, "Системная ошибка: роль студента не найдена");
        }

        return await RegisterAsync(username, email, password, firstName, lastName, studentRole.Uid);
    }

    /// <summary>
    /// Регистрирует нового пользователя с указанной ролью
    /// </summary>
    public async Task<(bool Success, User? User, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName, Guid roleId)
    {
        var existingUser = await _userService.GetUserByUsernameAsync(username);
      
        if (existingUser != null)
        {
            return (false, null, "Пользователь с таким именем уже существует");
        }

        var existingEmail = await _userService.GetUserByEmailAsync(email);
       
        if (existingEmail != null)
        {
            return (false, null, "Пользователь с таким email уже существует");
        }

        // Проверяем, что роль существует
        var role = await _roleService.GetRoleAsync(roleId);
        
        if (role == null)
        {
            return (false, null, "Указанная роль не найдена");
        }

        var user = new User
        {
            Uid = Guid.NewGuid(),
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            RoleId = roleId, // Устанавливаем выбранную роль
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        await _userService.AddUserAsync(user, password);
        return (true, user, string.Empty);
    }

    /// <summary>
    /// Выходит из системы
    /// </summary>
    public Task LogoutAsync()
    {
        _userSessionService.ClearSession();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Получает текущего аутентифицированного пользователя
    /// </summary>
    public async Task<User?> GetCurrentUserAsync()
    {
        return _userSessionService.CurrentUser;
    }

    /// <summary>
    /// Наблюдаемый объект, отражающий текущего пользователя
    /// </summary>
    public IObservable<User?> CurrentUserObservable => _userSessionService.CurrentUserObservable;

    /// <summary>
    /// Проверяет доступ пользователя к определенному разрешению
    /// </summary>
    public async Task<bool> HasPermissionAsync(Guid userUid, string permissionName)
    {
        var user = await _userService.GetUserAsync(userUid);
      
        if (user == null)
        {
            return false;
        }

        var roles = await _roleService.GetUserRolesAsync(userUid);

        foreach (var role in roles)
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(role.Uid);
      
            foreach (var permission in permissions)
            {
                if (permission.Name == permissionName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Проверяет наличие определенной роли у пользователя
    /// </summary>
    public async Task<bool> IsInRoleAsync(Guid userUid, string roleName)
    {
        var roles = await _roleService.GetUserRolesAsync(userUid);
        return roles.Any(r => r.Name == roleName);
    }

    /// <summary>
    /// Изменяет пароль пользователя
    /// </summary>
    public async Task<bool> ChangePasswordAsync(Guid userUid, string currentPassword, string newPassword)
    {
        var user = await _userService.GetUserAsync(userUid);
       
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.LastModifiedAt = DateTime.UtcNow;

        return await _userService.UpdateUserAsync(user);
    }

    /// <summary>
    /// Запрашивает сброс пароля пользователя
    /// </summary>
    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
       
        if (user == null)
        {
            return false;
        }

        // TODO: Реализовать генерацию токена и отправку email
        return true;
    }

    /// <summary>
    /// Сбрасывает пароль пользователя
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        // TODO: Реализовать проверку токена и сброс пароля
        return true;
    }

    public async Task<Guid> GetCurrentUserUidAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Uid ?? Guid.Empty;
    }
}