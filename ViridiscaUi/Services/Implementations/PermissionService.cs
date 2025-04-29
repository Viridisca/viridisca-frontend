using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с разрешениями
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly LocalDbContext _dbContext;
        private readonly IRoleService _roleService;

        public PermissionService(LocalDbContext dbContext, IRoleService roleService)
        {
            _dbContext = dbContext;
            _roleService = roleService;
        }

        /// <summary>
        /// Получает разрешение по идентификатору
        /// </summary>
        public Task<Permission?> GetPermissionAsync(Guid uid)
        {
            var permission = _dbContext.Permissions.Items.FirstOrDefault(p => p.Uid == uid);
            return Task.FromResult(permission);
        }
        
        /// <summary>
        /// Получает разрешение по названию
        /// </summary>
        public Task<Permission?> GetPermissionByNameAsync(string name)
        {
            var permission = _dbContext.Permissions.Items.FirstOrDefault(p => 
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(permission);
        }
        
        /// <summary>
        /// Получает все разрешения
        /// </summary>
        public Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return Task.FromResult<IEnumerable<Permission>>(_dbContext.Permissions.Items.ToList());
        }
        
        /// <summary>
        /// Получает разрешения роли
        /// </summary>
        public Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleUid)
        {
            var rolePermissions = _dbContext.RolePermissions.Items
                .Where(rp => rp.RoleUid == roleUid)
                .ToList();
                
            var permissions = new List<Permission>();
            
            foreach (var rolePermission in rolePermissions)
            {
                var permission = _dbContext.Permissions.Items
                    .FirstOrDefault(p => p.Uid == rolePermission.PermissionUid);
                    
                if (permission != null)
                {
                    permissions.Add(permission);
                }
            }
            
            return Task.FromResult<IEnumerable<Permission>>(permissions);
        }
        
        /// <summary>
        /// Получает разрешения пользователя
        /// </summary>
        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userUid)
        {
            // Получаем роли пользователя
            var roles = await _roleService.GetUserRolesAsync(userUid);
            
            var permissions = new List<Permission>();
            
            // Для каждой роли получаем связанные разрешения
            foreach (var role in roles)
            {
                var rolePermissions = await GetRolePermissionsAsync(role.Uid);
                permissions.AddRange(rolePermissions);
            }
            
            // Удаляем дубликаты
            return permissions.GroupBy(p => p.Uid).Select(g => g.First());
        }
        
        /// <summary>
        /// Добавляет новое разрешение
        /// </summary>
        public Task AddPermissionAsync(Permission permission)
        {
            if (permission.Uid == Guid.Empty)
            {
                permission.Uid = Guid.NewGuid();
            }
            
            permission.CreatedAt = DateTime.UtcNow;
            permission.LastModifiedAt = DateTime.UtcNow;
            
            _dbContext.Permissions.Add(permission);
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Обновляет существующее разрешение
        /// </summary>
        public Task<bool> UpdatePermissionAsync(Permission permission)
        {
            var existingPermission = _dbContext.Permissions.Items
                .FirstOrDefault(p => p.Uid == permission.Uid);
                
            if (existingPermission == null)
            {
                return Task.FromResult(false);
            }
            
            // Обновляем свойства
            _dbContext.Permissions.Remove(existingPermission);
            
            permission.LastModifiedAt = DateTime.UtcNow;
            _dbContext.Permissions.Add(permission);
            
            return Task.FromResult(true);
        }
        
        /// <summary>
        /// Удаляет разрешение
        /// </summary>
        public Task<bool> DeletePermissionAsync(Guid uid)
        {
            var permission = _dbContext.Permissions.Items.FirstOrDefault(p => p.Uid == uid);
            
            if (permission == null)
            {
                return Task.FromResult(false);
            }
            
            _dbContext.Permissions.Remove(permission);
            
            // Удаляем связанные RolePermission
            var rolePermissions = _dbContext.RolePermissions.Items
                .Where(rp => rp.PermissionUid == uid)
                .ToList();
                
            foreach (var rolePermission in rolePermissions)
            {
                _dbContext.RolePermissions.Remove(rolePermission);
            }
            
            return Task.FromResult(true);
        }
    }
} 