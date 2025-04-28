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
            throw new NotImplementedException();
        }

        public Task<Role?> GetRoleByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Role>> GetUserRolesAsync(Guid userUid)
        {
            throw new NotImplementedException();
        }

        public Task AddRoleAsync(Role role)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateRoleAsync(Role role)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRoleAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AssignRoleToUserAsync(Guid roleUid, Guid userUid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveRoleFromUserAsync(Guid roleUid, Guid userUid)
        {
            throw new NotImplementedException();
        }
    }
} 