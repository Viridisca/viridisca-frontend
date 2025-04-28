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

        public PermissionService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Permission?> GetPermissionAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<Permission?> GetPermissionByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleUid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userUid)
        {
            throw new NotImplementedException();
        }

        public Task AddPermissionAsync(Permission permission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePermissionAsync(Permission permission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePermissionAsync(Guid uid)
        {
            throw new NotImplementedException();
        }
    }
} 