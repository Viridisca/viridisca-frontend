using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations;

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
                StatusLogger.LogError("Строка подключения к базе данных не настроена", "UserService");
            }
            else
            {
                _logger.LogDebug("✅ Строка подключения к базе данных настроена");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при инициализации UserService");
            StatusLogger.LogError($"Ошибка инициализации UserService: {ex.Message}", "UserService");
            throw;
        }
    }

    public async Task<User?> GetUserAsync(Guid uid)
    {
        try
        {
            _logger.LogDebug("Загружаем пользователя по Uid: {UserUid}", uid);
            
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Uid == uid);
            
            if (user != null)
            {
                _logger.LogDebug("Пользователь найден: {UserEmail}", user.Email);
            }
            else
            {
                _logger.LogDebug("Пользователь с Uid {UserUid} не найден", uid);
            }
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя по Uid {UserUid}: {ErrorMessage}", uid, ex.Message);
            StatusLogger.LogError($"Ошибка загрузки пользователя: {ex.Message}", "UserService");
            throw;
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
            _logger.LogDebug("Загружаем пользователя по username: {Username}", username);
            
            var user = await _dbContext.Users
                .Include(u => u.Role) // Загружаем основную роль
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
            
            if (user != null)
            {
                _logger.LogDebug("Пользователь найден: Uid={UserUid}, Email={UserEmail}, Role={RoleName}", 
                    user.Uid, user.Email, user.Role?.Name ?? "null");
            }
            else
            {
                _logger.LogDebug("Пользователь с username '{Username}' не найден", username);
            }
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя по имени {Username}: {ErrorMessage}", username, ex.Message);
            StatusLogger.LogError($"Ошибка загрузки пользователя: {ex.Message}", "UserService");
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            _logger.LogDebug("Загружаем пользователя по email: {Email}", email);
            
            var user = await _dbContext.Users
                .Include(u => u.Role) // Загружаем основную роль
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
            
            if (user != null)
            {
                _logger.LogDebug("Пользователь найден: Uid={UserUid}, Username={Username}, Role={RoleName}", 
                    user.Uid, user.Username, user.Role?.Name ?? "null");
            }
            else
            {
                _logger.LogDebug("Пользователь с email '{Email}' не найден", email);
            }
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя по email {Email}: {ErrorMessage}", email, ex.Message);
            StatusLogger.LogError($"Ошибка загрузки пользователя: {ex.Message}", "UserService");
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            return await _dbContext.Users
                .Include(u => u.Role)
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
