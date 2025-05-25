using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с ролями
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _dbContext;

        public RoleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Role?> GetRoleAsync(Guid uid)
        {
            return await _dbContext.Roles.FindAsync(uid);
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            return await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _dbContext.Roles
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userUid)
        {
            return await _dbContext.UserRoles
                .Where(ur => ur.UserUid == userUid && ur.IsActive)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role!)
                .ToListAsync();
        }

        public async Task AddRoleAsync(Role role)
        {
            role.CreatedAt = DateTime.UtcNow;
            role.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.Roles.AddAsync(role);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateRoleAsync(Role role)
        {
            var existingRole = await _dbContext.Roles.FindAsync(role.Uid);
            if (existingRole == null)
                return false;

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;
            existingRole.RoleType = role.RoleType;
            existingRole.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRoleAsync(Guid uid)
        {
            // Проверяем, есть ли пользователи с этой ролью
            var usersWithRole = await _dbContext.UserRoles
                .Where(ur => ur.RoleUid == uid && ur.IsActive)
                .AnyAsync();
                
            if (usersWithRole)
                return false; // Нельзя удалить роль, используемую пользователями

            var role = await _dbContext.Roles.FindAsync(uid);
            if (role == null)
                return false;

            _dbContext.Roles.Remove(role);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRoleToUserAsync(Guid roleUid, Guid userUid)
        {
            // Проверяем существование роли и пользователя
            var role = await _dbContext.Roles.FindAsync(roleUid);
            var user = await _dbContext.Users.FindAsync(userUid);
            
            if (role == null || user == null)
                return false;
                
            // Проверяем, не назначена ли уже роль
            var existingUserRole = await _dbContext.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserUid == userUid && ur.RoleUid == roleUid && ur.IsActive);
                
            if (existingUserRole != null)
                return true; // Роль уже назначена
                
            // Создаем новую связь
            var userRole = new UserRole
            {
                Uid = Guid.NewGuid(),
                UserUid = userUid,
                RoleUid = roleUid,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            await _dbContext.UserRoles.AddAsync(userRole);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(Guid roleUid, Guid userUid)
        {
            // Ищем активную связь пользователя с указанной ролью
            var userRole = await _dbContext.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserUid == userUid && ur.RoleUid == roleUid && ur.IsActive);
                
            if (userRole == null)
                return false; // Роль не назначена
                
            // Деактивируем связь
            userRole.IsActive = false;
            userRole.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
} 