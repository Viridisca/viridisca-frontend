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
    /// Реализация сервиса для работы с преподавателями
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly LocalDbContext _dbContext;

        public TeacherService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Teacher?> GetTeacherAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Teacher>> GetAllTeachersAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddTeacherAsync(Teacher teacher)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateTeacherAsync(Teacher teacher)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTeacherAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid)
        {
            throw new NotImplementedException();
        }
    }
} 