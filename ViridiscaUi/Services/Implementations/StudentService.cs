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
    /// Реализация сервиса для работы со студентами
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _dbContext;

        public StudentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Student?> GetStudentAsync(Guid uid)
        {
            return await _dbContext.Students.FindAsync(uid);
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _dbContext.Students.ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid)
        {
            return await _dbContext.Students
                .Where(s => s.GroupUid == groupUid)
                .ToListAsync();
        }

        public async Task AddStudentAsync(Student student)
        {
            await _dbContext.Students.AddAsync(student);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            var existingStudent = await _dbContext.Students.FindAsync(student.Uid);
            if (existingStudent == null)
                return false;

            _dbContext.Entry(existingStudent).CurrentValues.SetValues(student);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStudentAsync(Guid uid)
        {
            var student = await _dbContext.Students.FindAsync(uid);
            if (student == null)
                return false;

            _dbContext.Students.Remove(student);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
} 