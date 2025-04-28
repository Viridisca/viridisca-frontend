using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с учебными группами
    /// </summary>
    public class GroupService : IGroupService
    {
        private readonly LocalDbContext _dbContext;

        public GroupService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Group?> GetGroupAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddGroupAsync(Group group)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateGroupAsync(Group group)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteGroupAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AssignCuratorAsync(Guid groupUid, Guid teacherUid)
        {
            throw new NotImplementedException();
        }
    }
} 