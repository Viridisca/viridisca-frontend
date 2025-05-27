using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с разрешениями
/// </summary>
public class PermissionService(ApplicationDbContext dbContext, IRoleService roleService) : IPermissionService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IRoleService _roleService = roleService;

    /// <summary>
    /// Получает разрешение по идентификатору
    /// </summary>
    public async Task<Permission?> GetPermissionAsync(Guid uid)
    {
        return await _dbContext.Permissions.FindAsync(uid);
    }
    
    /// <summary>
    /// Получает разрешение по названию
    /// </summary>
    public async Task<Permission?> GetPermissionByNameAsync(string name)
    {
        return await _dbContext.Permissions
            .FirstOrDefaultAsync(p => p.Name == name);
    }
    
    /// <summary>
    /// Получает все разрешения
    /// </summary>
    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _dbContext.Permissions
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// Получает разрешения роли
    /// </summary>
    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleUid)
    {
        return await _dbContext.RolePermissions
            .Where(rp => rp.RoleUid == roleUid)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission!)
            .ToListAsync();
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
    public async Task AddPermissionAsync(Permission permission)
    {
        permission.CreatedAt = DateTime.UtcNow;
        permission.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.Permissions.AddAsync(permission);
        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Обновляет существующее разрешение
    /// </summary>
    public async Task<bool> UpdatePermissionAsync(Permission permission)
    {
        var existingPermission = await _dbContext.Permissions.FindAsync(permission.Uid);
        if (existingPermission == null)
            return false;

        existingPermission.Name = permission.Name;
        existingPermission.Description = permission.Description;
        existingPermission.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Удаляет разрешение
    /// </summary>
    public async Task<bool> DeletePermissionAsync(Guid uid)
    {
        var permission = await _dbContext.Permissions.FindAsync(uid);
        if (permission == null)
            return false;

        // Удаляем связанные RolePermission
        var rolePermissions = await _dbContext.RolePermissions
            .Where(rp => rp.PermissionUid == uid)
            .ToListAsync();

        _dbContext.RolePermissions.RemoveRange(rolePermissions);
        _dbContext.Permissions.Remove(permission);

        await _dbContext.SaveChangesAsync();
        return true;
    }
}
