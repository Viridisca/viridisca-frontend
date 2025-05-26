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

        public async Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            return await GetGroupsAsync();
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

        // === РАСШИРЕНИЯ ЭТАПА 1 ===

        public async Task<IEnumerable<Group>> GetGroupsByYearAsync(int year)
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .Where(g => g.CreatedAt.Year == year)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Group>> GetGroupsByCuratorAsync(Guid curatorUid)
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .Where(g => g.CuratorUid == curatorUid)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<bool> AddStudentToGroupAsync(Guid groupUid, Guid studentUid)
        {
            var group = await _dbContext.Groups
                .Include(g => g.Students)
                .FirstOrDefaultAsync(g => g.Uid == groupUid);
            
            var student = await _dbContext.Students.FindAsync(studentUid);
            
            if (group == null || student == null)
                return false;

            // Проверяем, не состоит ли студент уже в группе
            if (student.GroupUid != null)
                return false;

            student.GroupUid = groupUid;
            student.LastModifiedAt = DateTime.UtcNow;
            group.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveStudentFromGroupAsync(Guid groupUid, Guid studentUid)
        {
            var student = await _dbContext.Students
                .FirstOrDefaultAsync(s => s.Uid == studentUid && s.GroupUid == groupUid);
            
            if (student == null)
                return false;

            student.GroupUid = null;
            student.LastModifiedAt = DateTime.UtcNow;
            
            var group = await _dbContext.Groups.FindAsync(groupUid);
            if (group != null)
            {
                group.LastModifiedAt = DateTime.UtcNow;
            }
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<GroupStatistics> GetGroupStatisticsAsync(Guid groupUid)
        {
            var group = await _dbContext.Groups
                .Include(g => g.Students)
                .FirstOrDefaultAsync(g => g.Uid == groupUid);

            if (group == null)
                throw new ArgumentException($"Group with ID {groupUid} not found");

            var studentsCount = group.Students.Count;
            
            // Получаем средний балл студентов группы
            var averageGrade = 0.0;
            if (studentsCount > 0)
            {
                var studentUids = group.Students.Select(s => s.Uid).ToList();
                var grades = await _dbContext.Grades
                    .Where(g => studentUids.Contains(g.StudentUid))
                    .ToListAsync();
                
                if (grades.Any())
                {
                    averageGrade = (double)grades.Average(g => g.Value);
                }
            }

            // Получаем количество активных курсов
            var activeCoursesCount = await _dbContext.Enrollments
                .Where(e => group.Students.Select(s => s.Uid).Contains(e.StudentUid))
                .Select(e => e.CourseUid)
                .Distinct()
                .CountAsync();

            // Получаем статистику по заданиям
            var assignments = await _dbContext.Assignments
                .Where(a => _dbContext.Enrollments
                    .Where(e => group.Students.Select(s => s.Uid).Contains(e.StudentUid))
                    .Select(e => e.CourseUid)
                    .Contains(a.CourseId))
                .ToListAsync();

            var totalAssignments = assignments.Count;
            var completedAssignments = await _dbContext.Submissions
                .Where(s => group.Students.Select(st => st.Uid).Contains(s.StudentId) && s.Score != null)
                .CountAsync();

            var lastActivity = await _dbContext.Submissions
                .Where(s => group.Students.Select(st => st.Uid).Contains(s.StudentId))
                .OrderByDescending(s => s.SubmissionDate)
                .Select(s => s.SubmissionDate)
                .FirstOrDefaultAsync();

            return new GroupStatistics
            {
                TotalStudents = studentsCount,
                AverageGrade = averageGrade,
                TotalCourses = activeCoursesCount,
                CompletedAssignments = completedAssignments,
                PendingAssignments = totalAssignments - completedAssignments,
                LastActivityDate = lastActivity == DateTime.MinValue ? null : lastActivity
            };
        }

        public async Task<(IEnumerable<Group> Groups, int TotalCount)> GetGroupsPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            var query = _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(g => g.Name.Contains(searchTerm) || 
                                       (g.Description != null && g.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            
            var groups = await query
                .OrderBy(g => g.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (groups, totalCount);
        }
    }
} 