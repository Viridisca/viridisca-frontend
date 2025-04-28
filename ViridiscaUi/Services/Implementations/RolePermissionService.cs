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
    /// Реализация сервиса для работы со связями ролей и разрешений
    /// </summary>
    public class RolePermissionService : IRolePermissionService
    {
        private readonly LocalDbContext _dbContext;

        public RolePermissionService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<RolePermission?> GetRolePermissionAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RolePermission>> GetAllRolePermissionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RolePermission?> GetRolePermissionByRoleAndPermissionAsync(Guid roleUid, Guid permissionUid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleAsync(Guid roleUid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionAsync(Guid permissionUid)
        {
            throw new NotImplementedException();
        }

        public Task AddRolePermissionAsync(RolePermission rolePermission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddPermissionToRoleAsync(Guid roleUid, Guid permissionUid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRolePermissionAsync(RolePermission rolePermission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRolePermissionAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemovePermissionFromRoleAsync(Guid roleUid, Guid permissionUid)
        {
            throw new NotImplementedException();
        }
    }
} 