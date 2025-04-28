using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с пользователями
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        Task<User?> GetUserAsync(Guid uid);
        
        /// <summary>
        /// Получает пользователя по имени пользователя
        /// </summary>
        Task<User?> GetUserByUsernameAsync(string username);
        
        /// <summary>
        /// Получает пользователя по электронной почте
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);
        
        /// <summary>
        /// Получает всех пользователей
        /// </summary>
        Task<IEnumerable<User>> GetAllUsersAsync();
        
        /// <summary>
        /// Добавляет нового пользователя
        /// </summary>
        Task AddUserAsync(User user, string password);
        
        /// <summary>
        /// Обновляет существующего пользователя
        /// </summary>
        Task<bool> UpdateUserAsync(User user);
        
        /// <summary>
        /// Удаляет пользователя
        /// </summary>
        Task<bool> DeleteUserAsync(Guid uid);
        
        /// <summary>
        /// Активирует пользователя
        /// </summary>
        Task<bool> ActivateUserAsync(Guid uid);
        
        /// <summary>
        /// Деактивирует пользователя
        /// </summary>
        Task<bool> DeactivateUserAsync(Guid uid);
        
        /// <summary>
        /// Обновляет профиль пользователя
        /// </summary>
        Task<bool> UpdateProfileAsync(Guid uid, string firstName, string lastName, string middleName, string phoneNumber);
        
        /// <summary>
        /// Обновляет фото профиля пользователя
        /// </summary>
        Task<bool> UpdateProfileImageAsync(Guid uid, string imageUrl);
    }
} 