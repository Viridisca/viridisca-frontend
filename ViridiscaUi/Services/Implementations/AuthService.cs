using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BCrypt.Net;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса аутентификации
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly LocalDbContext _context;
        private readonly BehaviorSubject<User?> _currentUserSubject = new BehaviorSubject<User?>(null);
        
        /// <summary>
        /// Создает новый экземпляр сервиса аутентификации
        /// </summary>
        public AuthService(LocalDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Аутентифицирует пользователя по логину и паролю
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            // Находим пользователя по имени
            var user = _context.GetUserByUsername(username);
            
            if (user == null)
            {
                _currentUserSubject.OnNext(null);
                return false;
            }
            
            // Проверяем пароль
            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            
            if (!passwordValid)
            {
                _currentUserSubject.OnNext(null);
                return false;
            }
            
            // Загружаем связанные данные - используем UserRoles для получения основной роли
            if (user.UserRoles.Count > 0)
            {
                var primaryUserRole = user.UserRoles.FirstOrDefault(ur => ur.IsActive);
                if (primaryUserRole != null)
                {
                    user.Role = _context.GetRoleByUid(primaryUserRole.RoleUid);
                    user.RoleId = primaryUserRole.RoleUid;
                }
            }
            
            // Загружаем профиль пользователя в зависимости от роли
            if (user.Role?.RoleType == RoleType.Student)
            {
                user.StudentProfile = _context.GetStudentByUid(user.Uid);
            }
            else if (user.Role?.RoleType == RoleType.Teacher)
            {
                user.TeacherProfile = _context.GetTeacherByUid(user.Uid);
            }
            
            // Успешная аутентификация
            _currentUserSubject.OnNext(user);
            return true;
        }
        
        /// <summary>
        /// Выходит из системы
        /// </summary>
        public Task LogoutAsync()
        {
            _currentUserSubject.OnNext(null);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Получает текущего аутентифицированного пользователя
        /// </summary>
        public Task<User?> GetCurrentUserAsync()
        {
            return Task.FromResult(_currentUserSubject.Value);
        }
        
        /// <summary>
        /// Наблюдаемый объект, отражающий текущего пользователя
        /// </summary>
        public IObservable<User?> CurrentUserObservable => _currentUserSubject.AsObservable();
        
        /// <summary>
        /// Проверяет наличие определенной роли у текущего пользователя
        /// </summary>
        public Task<bool> IsInRoleAsync(string roleName)
        {
            var currentUser = _currentUserSubject.Value;
            
            if (currentUser == null || currentUser.Role == null)
                return Task.FromResult(false);
                
            return Task.FromResult(currentUser.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Проверяет доступ текущего пользователя к определенному разрешению
        /// </summary>
        public async Task<bool> HasPermissionAsync(string permissionName)
        {
            var currentUser = _currentUserSubject.Value;
            
            if (currentUser == null || currentUser.Role == null)
                return false;
                
            // Администраторы имеют все разрешения
            if (currentUser.Role.Name.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
                return true;
                
            // Проверяем разрешения для роли пользователя
            var rolePermissions = _context.RolePermissions.Items;
            foreach (var rolePermission in rolePermissions)
            {
                if (rolePermission.RoleUid == currentUser.Role.Uid)
                {
                    var permission = _context.Permissions.Items.FirstOrDefault(p => p.Uid == rolePermission.PermissionUid);
                    if (permission != null && permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Аутентифицирует пользователя по логину и паролю
        /// </summary>
        public async Task<(bool Success, User? User, string ErrorMessage)> AuthenticateAsync(string username, string password)
        {
            // Находим пользователя по имени
            var user = _context.GetUserByUsername(username);
            
            if (user == null)
            {
                return (false, null, "Пользователь не найден");
            }
            
            // Проверяем пароль
            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            
            if (!passwordValid)
            {
                return (false, null, "Неверный пароль");
            }
            
            // Загружаем связанные данные - используем UserRoles для получения основной роли
            if (user.UserRoles.Count > 0)
            {
                var primaryUserRole = user.UserRoles.FirstOrDefault(ur => ur.IsActive);
                if (primaryUserRole != null)
                {
                    user.Role = _context.GetRoleByUid(primaryUserRole.RoleUid);
                    user.RoleId = primaryUserRole.RoleUid;
                }
            }
            
            // Загружаем профиль пользователя в зависимости от роли
            if (user.Role?.RoleType == RoleType.Student)
            {
                user.StudentProfile = _context.GetStudentByUid(user.Uid);
            }
            else if (user.Role?.RoleType == RoleType.Teacher)
            {
                user.TeacherProfile = _context.GetTeacherByUid(user.Uid);
            }
            
            // Успешная аутентификация
            _currentUserSubject.OnNext(user);
            return (true, user, string.Empty);
        }

        /// <summary>
        /// Проверяет доступ пользователя к определенному разрешению
        /// </summary>
        public Task<bool> HasPermissionAsync(Guid userUid, string permissionName)
        {
            // Если это текущий пользователь, то используем метод для текущего пользователя
            if (_currentUserSubject.Value?.Uid == userUid)
            {
                return HasPermissionAsync(permissionName);
            }
            
            // Находим пользователя
            var user = _context.GetUserByUid(userUid);
            if (user == null || user.Role == null)
                return Task.FromResult(false);
                
            // Загружаем роль пользователя
            if (user.UserRoles.Count > 0)
            {
                var primaryUserRole = user.UserRoles.FirstOrDefault(ur => ur.IsActive);
                if (primaryUserRole != null)
                {
                    user.Role = _context.GetRoleByUid(primaryUserRole.RoleUid);
                    user.RoleId = primaryUserRole.RoleUid;
                }
            }
            
            // Администраторы имеют все разрешения
            if (user.Role?.Name.Equals("Administrator", StringComparison.OrdinalIgnoreCase) == true)
                return Task.FromResult(true);
                
            // Проверяем разрешения для роли пользователя
            var rolePermissions = _context.RolePermissions.Items;
            foreach (var rolePermission in rolePermissions)
            {
                if (rolePermission.RoleUid == user.Role.Uid)
                {
                    var permission = _context.Permissions.Items.FirstOrDefault(p => p.Uid == rolePermission.PermissionUid);
                    if (permission != null && permission.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            
            return Task.FromResult(false);
        }

        /// <summary>
        /// Проверяет наличие определенной роли у пользователя
        /// </summary>
        public Task<bool> IsInRoleAsync(Guid userUid, string roleName)
        {
            // Если это текущий пользователь, то используем метод для текущего пользователя
            if (_currentUserSubject.Value?.Uid == userUid)
            {
                return IsInRoleAsync(roleName);
            }
            
            // Находим пользователя
            var user = _context.GetUserByUid(userUid);
            if (user == null)
                return Task.FromResult(false);
                
            // Загружаем роль пользователя
            if (user.UserRoles.Count > 0)
            {
                var primaryUserRole = user.UserRoles.FirstOrDefault(ur => ur.IsActive);
                if (primaryUserRole != null)
                {
                    user.Role = _context.GetRoleByUid(primaryUserRole.RoleUid);
                    user.RoleId = primaryUserRole.RoleUid;
                }
            }
            
            return Task.FromResult(user.Role?.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase) == true);
        }

        /// <summary>
        /// Изменяет пароль пользователя
        /// </summary>
        public Task<bool> ChangePasswordAsync(Guid userUid, string currentPassword, string newPassword)
        {
            // Находим пользователя
            var user = _context.GetUserByUid(userUid);
            if (user == null)
                return Task.FromResult(false);
                
            // Проверяем текущий пароль
            bool passwordValid = BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash);
            
            if (!passwordValid)
                return Task.FromResult(false);
                
            // Обновляем пароль
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.LastModifiedAt = DateTime.UtcNow;
            _context.UpdateUser(user);
            
            return Task.FromResult(true);
        }

        /// <summary>
        /// Запрашивает сброс пароля пользователя
        /// </summary>
        public Task<bool> RequestPasswordResetAsync(string email)
        {
            // В реальном приложении здесь будет логика для создания токена сброса
            // и отправки его на почту пользователю
            
            return Task.FromResult(true);
        }

        /// <summary>
        /// Сбрасывает пароль пользователя
        /// </summary>
        public Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            // В реальном приложении здесь будет проверка токена
            // и установка нового пароля для пользователя
            
            return Task.FromResult(true);
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        public Task<(bool Success, User? User, string ErrorMessage)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
        {
            // Проверяем, что пользователь с таким именем не существует
            var existingUser = _context.GetUserByUsername(username);
            if (existingUser != null)
            {
                return Task.FromResult((false, (User?)null, "Пользователь с таким именем уже существует"));
            }
            
            // Создаем нового пользователя
            var user = new User
            {
                Uid = Guid.NewGuid(),
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            // Назначаем роль студента по умолчанию
            var studentRole = _context.GetRoleByName("Student");
            if (studentRole != null)
            {
                user.RoleId = studentRole.Uid;
                user.Role = studentRole;
                
                // Создаем связь пользователя с ролью
                var userRole = new UserRole
                {
                    Uid = Guid.NewGuid(),
                    UserUid = user.Uid,
                    RoleUid = studentRole.Uid,
                    Role = studentRole,
                    User = user,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                };
                
                user.UserRoles.Add(userRole);
                
                // Используем метод для добавления связи пользователя с ролью
                _context.AddUserRole(userRole);
            }
            
            // Добавляем пользователя в базу
            _context.AddUser(user);
            
            return Task.FromResult((true, user, string.Empty));
        }
    }
} 