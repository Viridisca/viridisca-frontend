using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с пользователями
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext dbContext, ILogger<UserService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "DbContext не может быть null. Проверьте конфигурацию DI и строку подключения к базе данных.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Простая проверка наличия DbContext без блокирующих операций
            try
            {
                _logger.LogDebug("✅ UserService: DbContext успешно получен");
                
                // Проверяем только что строка подключения существует
                var connectionString = _dbContext.Database.GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("❌ Строка подключения к базе данных пуста");
                }
                else
                {
                    _logger.LogDebug("✅ Строка подключения к базе данных настроена");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при проверке DbContext: {ErrorMessage}", ex.Message);
                
                // Дополнительная информация для диагностики
                if (ex.InnerException != null)
                {
                    _logger.LogError("Внутренняя ошибка: {InnerException}", ex.InnerException.Message);
                }
            }
        }

        private void ValidateDbContext()
        {
            if (_dbContext == null)
            {
                var errorMessage = "DbContext равен null. Проверьте регистрацию сервисов в DI контейнере.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }

        public async Task<User?> GetUserAsync(Guid uid)
        {
            ValidateDbContext();
            try
            {
                return await _dbContext.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Uid == uid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по Uid {UserUid}: {ErrorMessage}", uid, ex.Message);
                throw;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            ValidateDbContext();
            try
            {
                return await _dbContext.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по имени {Username}: {ErrorMessage}", username, ex.Message);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            ValidateDbContext();
            try
            {
                return await _dbContext.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по email {Email}: {ErrorMessage}", email, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            ValidateDbContext();
            try
            {
                return await _dbContext.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех пользователей: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task AddUserAsync(User user, string password)
        {
            ValidateDbContext();
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.CreatedAt = DateTime.UtcNow;
                user.LastModifiedAt = DateTime.UtcNow;
                
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Пользователь {Username} успешно добавлен", user.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении пользователя {Username}: {ErrorMessage}", user.Username, ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = await _dbContext.Users.FindAsync(user.Uid);
            if (existingUser == null)
                return false;

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.MiddleName = user.MiddleName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.IsActive = user.IsActive;
            existingUser.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid uid)
        {
            var user = await _dbContext.Users.FindAsync(uid);
            if (user == null)
                return false;

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateUserAsync(Guid uid)
        {
            var user = await _dbContext.Users.FindAsync(uid);
            if (user == null)
                return false;

            user.IsActive = true;
            user.LastModifiedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(Guid uid)
        {
            var user = await _dbContext.Users.FindAsync(uid);
            if (user == null)
                return false;

            user.IsActive = false;
            user.LastModifiedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfileAsync(Guid uid, string firstName, string lastName, string middleName, string phoneNumber)
        {
            var user = await _dbContext.Users.FindAsync(uid);
            if (user == null)
                return false;

            user.FirstName = firstName;
            user.LastName = lastName;
            user.MiddleName = middleName;
            user.PhoneNumber = phoneNumber;
            user.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfileImageAsync(Guid uid, string imageUrl)
        {
            var user = await _dbContext.Users.FindAsync(uid);
            if (user == null)
                return false;

            user.ProfileImageUrl = imageUrl;
            user.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
} 