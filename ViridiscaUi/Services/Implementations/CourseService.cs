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
    /// Реализация сервиса для работы с курсами
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly LocalDbContext _dbContext;

        public CourseService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Course?> GetCourseAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Course>> GetCoursesByTeacherAsync(Guid teacherUid)
        {
            throw new NotImplementedException();
        }

        public Task AddCourseAsync(Course course)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateCourseAsync(Course course)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCourseAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PublishCourseAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ArchiveCourseAsync(Guid uid)
        {
            throw new NotImplementedException();
        }
    }
} 