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
    /// Реализация сервиса для работы с заданиями
    /// </summary>
    public class AssignmentService : IAssignmentService
    {
        private readonly LocalDbContext _dbContext;

        public AssignmentService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Assignment?> GetAssignmentAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(Guid courseUid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Assignment>> GetAssignmentsByModuleAsync(Guid moduleUid)
        {
            throw new NotImplementedException();
        }

        public Task AddAssignmentAsync(Assignment assignment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAssignmentAsync(Assignment assignment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAssignmentAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PublishAssignmentAsync(Guid uid)
        {
            throw new NotImplementedException();
        }
    }
} 