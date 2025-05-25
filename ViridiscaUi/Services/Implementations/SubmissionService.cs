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
    /// Реализация сервиса для работы с выполненными заданиями
    /// </summary>
    public class SubmissionService : ISubmissionService
    {
                private readonly ApplicationDbContext _dbContext;        public SubmissionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Submission?> GetSubmissionAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Submission>> GetAllSubmissionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(Guid studentUid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid)
        {
            throw new NotImplementedException();
        }

        public Task<Submission?> GetSubmissionByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid)
        {
            throw new NotImplementedException();
        }

        public Task AddSubmissionAsync(Submission submission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateSubmissionAsync(Submission submission)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteSubmissionAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GradeSubmissionAsync(Guid uid, int grade, string feedback)
        {
            throw new NotImplementedException();
        }
    }
} 