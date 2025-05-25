using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _dbContext;

        public GroupService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Group?> GetGroupByIdAsync(Guid id)
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .FirstOrDefaultAsync(g => g.Uid == id);
        }

        public async Task<IEnumerable<Group>> GetGroupsAsync()
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            group.CreatedAt = DateTime.UtcNow;
            group.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();
            return group;
        }

        public async Task<Group> UpdateGroupAsync(Group group)
        {
            var existingGroup = await _dbContext.Groups.FindAsync(group.Uid);
            if (existingGroup == null)
                throw new ArgumentException($"Group with ID {group.Uid} not found");

            existingGroup.Name = group.Name;
            existingGroup.Description = group.Description;
            existingGroup.CuratorUid = group.CuratorUid;
            existingGroup.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return existingGroup;
        }

        public async Task DeleteGroupAsync(Guid id)
        {
            var group = await _dbContext.Groups.FindAsync(id);
            if (group == null)
                throw new ArgumentException($"Group with ID {id} not found");

            _dbContext.Groups.Remove(group);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> AssignCuratorAsync(Guid groupUid, Guid teacherUid)
        {
            var group = await _dbContext.Groups.FindAsync(groupUid);
            var teacher = await _dbContext.Teachers.FindAsync(teacherUid);
            
            if (group == null || teacher == null)
                return false;

            group.CuratorUid = teacherUid;
            group.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
} 