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
    /// Реализация сервиса для работы с зачислениями
    /// </summary>
    public class EnrollmentService : IEnrollmentService
    {
                private readonly ApplicationDbContext _dbContext;        public EnrollmentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Enrollment?> GetEnrollmentAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(Guid studentUid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(Guid courseUid)
        {
            throw new NotImplementedException();
        }

        public Task AddEnrollmentAsync(Enrollment enrollment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateEnrollmentAsync(Enrollment enrollment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteEnrollmentAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeEnrollmentStatusAsync(Guid uid, EnrollmentStatus status)
        {
            throw new NotImplementedException();
        }
    }
} 