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
    /// Реализация сервиса для работы с преподавателями
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _dbContext;

        public TeacherService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Teacher?> GetTeacherAsync(Guid uid)
        {
            return await _dbContext.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Uid == uid);
        }

        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
        {
            return await _dbContext.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> GetTeachersAsync()
        {
            return await GetAllTeachersAsync();
        }

        public async Task AddTeacherAsync(Teacher teacher)
        {
            teacher.CreatedAt = DateTime.UtcNow;
            teacher.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.Teachers.AddAsync(teacher);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateTeacherAsync(Teacher teacher)
        {
            var existingTeacher = await _dbContext.Teachers.FindAsync(teacher.Uid);
            if (existingTeacher == null)
                return false;

            existingTeacher.FirstName = teacher.FirstName;
            existingTeacher.LastName = teacher.LastName;
            existingTeacher.MiddleName = teacher.MiddleName;
            existingTeacher.Specialization = teacher.Specialization;
            existingTeacher.AcademicTitle = teacher.AcademicTitle;
            existingTeacher.AcademicDegree = teacher.AcademicDegree;
            existingTeacher.HourlyRate = teacher.HourlyRate;
            existingTeacher.Bio = teacher.Bio;
            existingTeacher.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTeacherAsync(Guid uid)
        {
            var teacher = await _dbContext.Teachers.FindAsync(uid);
            if (teacher == null)
                return false;

            _dbContext.Teachers.Remove(teacher);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid)
        {
            var teacher = await _dbContext.Teachers.FindAsync(teacherUid);
            var course = await _dbContext.Courses.FindAsync(courseUid);
            
            if (teacher == null || course == null)
                return false;

            course.TeacherUid = teacherUid;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Получает преподавателей с пагинацией
        /// </summary>
        public async Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetTeachersPagedAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? specializationFilter = null,
            string? statusFilter = null)
        {
            var query = _dbContext.Teachers
                .Include(t => t.User)
                .AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.FirstName.Contains(searchTerm) || 
                                        t.LastName.Contains(searchTerm) ||
                                        t.MiddleName.Contains(searchTerm) ||
                                        t.Specialization.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(specializationFilter))
            {
                query = query.Where(t => t.Specialization == specializationFilter);
            }

            var totalCount = await query.CountAsync();
            var teachers = await query
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teachers, totalCount);
        }

        /// <summary>
        /// Получает статистику преподавателя
        /// </summary>
        public async Task<object> GetTeacherStatisticsAsync(Guid teacherUid)
        {
            var coursesCount = await _dbContext.Courses
                .Where(c => c.TeacherUid == teacherUid)
                .CountAsync();

            var studentsCount = await _dbContext.Enrollments
                .Where(e => e.Course.TeacherUid == teacherUid)
                .Select(e => e.StudentUid)
                .Distinct()
                .CountAsync();

            var averageGrade = await _dbContext.Grades
                .Where(g => g.TeacherUid == teacherUid)
                .AverageAsync(g => (double?)g.Value) ?? 0;

            return new
            {
                CoursesCount = coursesCount,
                StudentsCount = studentsCount,
                AverageGrade = averageGrade,
                TotalGrades = await _dbContext.Grades.Where(g => g.TeacherUid == teacherUid).CountAsync()
            };
        }

        /// <summary>
        /// Получает курсы преподавателя
        /// </summary>
        public async Task<IEnumerable<Course>> GetTeacherCoursesAsync(Guid teacherUid)
        {
            return await _dbContext.Courses
                .Include(c => c.Enrollments)
                .Where(c => c.TeacherUid == teacherUid)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Получает группы преподавателя
        /// </summary>
        public async Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid)
        {
            return await _dbContext.Groups
                .Where(g => g.CuratorUid == teacherUid)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Назначает преподавателя на группу
        /// </summary>
        public async Task<bool> AssignToGroupAsync(Guid teacherUid, Guid groupUid)
        {
            var teacher = await _dbContext.Teachers.FindAsync(teacherUid);
            var group = await _dbContext.Groups.FindAsync(groupUid);
            
            if (teacher == null || group == null)
                return false;

            group.CuratorUid = teacherUid;
            group.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Отменяет назначение преподавателя на группу
        /// </summary>
        public async Task<bool> UnassignFromGroupAsync(Guid teacherUid, Guid groupUid)
        {
            var group = await _dbContext.Groups.FindAsync(groupUid);
            
            if (group == null || group.CuratorUid != teacherUid)
                return false;

            group.CuratorUid = null;
            group.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Отменяет назначение преподавателя на курс
        /// </summary>
        public async Task<bool> UnassignFromCourseAsync(Guid teacherUid, Guid courseUid)
        {
            var course = await _dbContext.Courses.FindAsync(courseUid);
            
            if (course == null || course.TeacherUid != teacherUid)
                return false;

            course.TeacherUid = null;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
} 