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
    /// Реализация сервиса для работы с курсами
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _dbContext;

        public CourseService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Course?> GetCourseAsync(Guid uid)
        {
            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Modules)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Uid == uid);
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(Guid teacherUid)
        {
            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .Where(c => c.TeacherUid == teacherUid)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task AddCourseAsync(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.Courses.AddAsync(course);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            var existingCourse = await _dbContext.Courses.FindAsync(course.Uid);
            if (existingCourse == null)
                return false;

            existingCourse.Name = course.Name;
            existingCourse.Description = course.Description;
            existingCourse.TeacherUid = course.TeacherUid;
            existingCourse.StartDate = course.StartDate;
            existingCourse.EndDate = course.EndDate;
            existingCourse.Credits = course.Credits;
            existingCourse.Status = course.Status;
            existingCourse.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCourseAsync(Guid uid)
        {
            var course = await _dbContext.Courses.FindAsync(uid);
            if (course == null)
                return false;

            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishCourseAsync(Guid uid)
        {
            var course = await _dbContext.Courses.FindAsync(uid);
            if (course == null)
                return false;

            course.Status = CourseStatus.Active;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveCourseAsync(Guid uid)
        {
            var course = await _dbContext.Courses.FindAsync(uid);
            if (course == null)
                return false;

            course.Status = CourseStatus.Archived;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
} 