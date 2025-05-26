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
    }
} 