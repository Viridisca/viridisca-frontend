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
    /// Реализация сервиса для работы с ролями
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly LocalDbContext _dbContext;

        public RoleService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Role?> GetRoleAsync(Guid uid)
        {
            var role = _dbContext.GetRoleByUid(uid);
            return Task.FromResult(role);
        }

        public Task<Role?> GetRoleByNameAsync(string name)
        {
            var role = _dbContext.GetRoleByName(name);
            return Task.FromResult(role);
        }

        public Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return Task.FromResult(_dbContext.GetAllRoles());
        }

        public Task<IEnumerable<Role>> GetUserRolesAsync(Guid userUid)
        {
            // Получаем связи пользователя с ролями
            var userRoles = _dbContext.GetUserRolesByUserUid(userUid);
            
            // Получаем роли на основе связей
            var roles = userRoles
                .Select(ur => _dbContext.GetRoleByUid(ur.RoleUid))
                .Where(r => r != null)
                .Cast<Role>();
                
            return Task.FromResult(roles);
        }

        public Task AddRoleAsync(Role role)
        {
            _dbContext.AddRole(role);
            return Task.CompletedTask;
        }

        public Task<bool> UpdateRoleAsync(Role role)
        {
            var existingRole = _dbContext.GetRoleByUid(role.Uid);
            if (existingRole == null)
                return Task.FromResult(false);
                
            _dbContext.UpdateRole(role);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteRoleAsync(Guid uid)
        {
            var existingRole = _dbContext.GetRoleByUid(uid);
            if (existingRole == null)
                return Task.FromResult(false);
                
            // Проверяем, есть ли пользователи с этой ролью
            var usersWithRole = _dbContext.GetUserRolesByRoleUid(uid);
            if (usersWithRole.Any())
                return Task.FromResult(false); // Нельзя удалить роль, используемую пользователями
                
            _dbContext.DeleteRole(uid);
            return Task.FromResult(true);
        }

        public Task<bool> AssignRoleToUserAsync(Guid roleUid, Guid userUid)
        {
            // Проверяем существование роли и пользователя
            var role = _dbContext.GetRoleByUid(roleUid);
            var user = _dbContext.GetUserByUid(userUid);
            
            if (role == null || user == null)
                return Task.FromResult(false);
                
            // Проверяем, не назначена ли уже роль
            var existingUserRole = _dbContext.GetUserRolesByUserUid(userUid)
                .FirstOrDefault(ur => ur.RoleUid == roleUid && ur.IsActive);
                
            if (existingUserRole != null)
                return Task.FromResult(true); // Роль уже назначена
                
            // Создаем новую связь
            var userRole = new UserRole
            {
                Uid = Guid.NewGuid(),
                UserUid = userUid,
                User = user,
                RoleUid = roleUid,
                Role = role,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            // Добавляем связь в базу и в коллекцию пользователя
            _dbContext.AddUserRole(userRole);
            user.UserRoles.Add(userRole);
            
            // Если это первая роль пользователя, устанавливаем её как основную
            if (user.Role == null)
            {
                user.Role = role;
                user.RoleId = roleUid;
                _dbContext.UpdateUser(user);
            }
            
            return Task.FromResult(true);
        }

        public Task<bool> RemoveRoleFromUserAsync(Guid roleUid, Guid userUid)
        {
            // Проверяем существование пользователя
            var user = _dbContext.GetUserByUid(userUid);
            if (user == null)
                return Task.FromResult(false);
                
            // Ищем активную связь пользователя с указанной ролью
            var userRole = _dbContext.GetUserRolesByUserUid(userUid)
                .FirstOrDefault(ur => ur.RoleUid == roleUid && ur.IsActive);
                
            if (userRole == null)
                return Task.FromResult(false); // Роль не назначена
                
            // Деактивируем связь, но не удаляем
            userRole.IsActive = false;
            userRole.LastModifiedAt = DateTime.UtcNow;
            _dbContext.UpdateUserRole(userRole);
            
            // Если это была основная роль пользователя, ищем другую активную роль
            if (user.RoleId == roleUid)
            {
                var otherActiveRole = _dbContext.GetUserRolesByUserUid(userUid)
                    .FirstOrDefault(ur => ur.RoleUid != roleUid && ur.IsActive);
                    
                if (otherActiveRole != null)
                {
                    user.Role = _dbContext.GetRoleByUid(otherActiveRole.RoleUid);
                    user.RoleId = otherActiveRole.RoleUid;
                }
                else
                {
                    user.Role = null;
                    user.RoleId = Guid.Empty;
                }
                
                _dbContext.UpdateUser(user);
            }
            
            return Task.FromResult(true);
        }
    }
} 