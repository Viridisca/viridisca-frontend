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
    /// Реализация сервиса для работы со студентами
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly LocalDbContext _dbContext;

        public StudentService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Student?> GetStudentAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid)
        {
            throw new NotImplementedException();
        }

        public Task AddStudentAsync(Student student)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateStudentAsync(Student student)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteStudentAsync(Guid uid)
        {
            throw new NotImplementedException();
        }
    }
} 