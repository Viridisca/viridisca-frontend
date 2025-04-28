using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с разрешениями
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Получает разрешение по идентификатору
        /// </summary>
        Task<Permission?> GetPermissionAsync(Guid uid);
        
        /// <summary>
        /// Получает разрешение по названию
        /// </summary>
        Task<Permission?> GetPermissionByNameAsync(string name);
        
        /// <summary>
        /// Получает все разрешения
        /// </summary>
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        
        /// <summary>
        /// Получает разрешения роли
        /// </summary>
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleUid);
        
        /// <summary>
        /// Получает разрешения пользователя
        /// </summary>
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userUid);
        
        /// <summary>
        /// Добавляет новое разрешение
        /// </summary>
        Task AddPermissionAsync(Permission permission);
        
        /// <summary>
        /// Обновляет существующее разрешение
        /// </summary>
        Task<bool> UpdatePermissionAsync(Permission permission);
        
        /// <summary>
        /// Удаляет разрешение
        /// </summary>
        Task<bool> DeletePermissionAsync(Guid uid);
    }
} 