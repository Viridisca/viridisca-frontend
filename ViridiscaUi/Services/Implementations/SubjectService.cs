using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с предметами
    /// </summary>
    public class SubjectService : ISubjectService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SubjectService> _logger;

        public SubjectService(ApplicationDbContext dbContext, ILogger<SubjectService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Subject?> GetSubjectAsync(Guid uid)
        {
            try
            {
                return await _dbContext.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.TeacherSubjects)
                        .ThenInclude(ts => ts.Teacher)
                    .FirstOrDefaultAsync(s => s.Uid == uid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subject with UID {SubjectUid}", uid);
                throw;
            }
        }

        public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
        {
            try
            {
                return await _dbContext.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.TeacherSubjects)
                        .ThenInclude(ts => ts.Teacher)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subjects");
                throw;
            }
        }

        public async Task<IEnumerable<Subject>> GetActiveSubjectsAsync()
        {
            try
            {
                return await _dbContext.Subjects
                    .Include(s => s.Department)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subjects");
                throw;
            }
        }

        public async Task<IEnumerable<Subject>> GetSubjectsByDepartmentAsync(Guid departmentUid)
        {
            try
            {
                return await _dbContext.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.TeacherSubjects)
                        .ThenInclude(ts => ts.Teacher)
                    .Where(s => s.DepartmentUid == departmentUid)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subjects by department {DepartmentUid}", departmentUid);
                throw;
            }
        }

        public async Task<Subject> CreateSubjectAsync(Subject subject)
        {
            try
            {
                subject.Uid = Guid.NewGuid();
                subject.CreatedAt = DateTime.UtcNow;
                subject.LastModifiedAt = DateTime.UtcNow;

                await _dbContext.Subjects.AddAsync(subject);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Subject created successfully: {SubjectName} ({SubjectCode})", 
                    subject.Name, subject.Code);

                return subject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject: {SubjectName}", subject.Name);
                throw;
            }
        }

        public async Task AddSubjectAsync(Subject subject)
        {
            try
            {
                subject.CreatedAt = DateTime.UtcNow;
                subject.LastModifiedAt = DateTime.UtcNow;

                await _dbContext.Subjects.AddAsync(subject);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Subject added successfully: {SubjectName} ({SubjectCode})", 
                    subject.Name, subject.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding subject: {SubjectName}", subject.Name);
                throw;
            }
        }

        public async Task<bool> UpdateSubjectAsync(Subject subject)
        {
            try
            {
                var existingSubject = await _dbContext.Subjects.FindAsync(subject.Uid);
                if (existingSubject == null)
                {
                    _logger.LogWarning("Subject not found for update: {SubjectUid}", subject.Uid);
                    return false;
                }

                existingSubject.Name = subject.Name;
                existingSubject.Code = subject.Code;
                existingSubject.Description = subject.Description;
                existingSubject.IsActive = subject.IsActive;
                existingSubject.Credits = subject.Credits;
                existingSubject.LessonsPerWeek = subject.LessonsPerWeek;
                existingSubject.DepartmentUid = subject.DepartmentUid;
                existingSubject.LastModifiedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Subject updated successfully: {SubjectName} ({SubjectCode})", 
                    subject.Name, subject.Code);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject: {SubjectUid}", subject.Uid);
                throw;
            }
        }

        public async Task<bool> DeleteSubjectAsync(Guid uid)
        {
            try
            {
                var subject = await _dbContext.Subjects
                    .Include(s => s.TeacherSubjects)
                    .FirstOrDefaultAsync(s => s.Uid == uid);

                if (subject == null)
                {
                    _logger.LogWarning("Subject not found for deletion: {SubjectUid}", uid);
                    return false;
                }

                // Проверяем, есть ли связанные данные
                if (subject.TeacherSubjects.Any())
                {
                    _logger.LogWarning("Cannot delete subject {SubjectName} - has related teachers", subject.Name);
                    throw new InvalidOperationException("Нельзя удалить предмет, который преподается преподавателями");
                }

                // Проверяем связанные курсы
                var hasRelatedCourses = await _dbContext.Courses
                    .AnyAsync(c => c.SubjectUid == uid);

                if (hasRelatedCourses)
                {
                    _logger.LogWarning("Cannot delete subject {SubjectName} - has related courses", subject.Name);
                    throw new InvalidOperationException("Нельзя удалить предмет, по которому есть курсы");
                }

                _dbContext.Subjects.Remove(subject);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Subject deleted successfully: {SubjectName} ({SubjectCode})", 
                    subject.Name, subject.Code);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject: {SubjectUid}", uid);
                throw;
            }
        }

        public async Task<IEnumerable<Subject>> SearchSubjectsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllSubjectsAsync();
                }

                var lowerSearchTerm = searchTerm.ToLower();

                return await _dbContext.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.TeacherSubjects)
                        .ThenInclude(ts => ts.Teacher)
                    .Where(s => 
                        s.Name.ToLower().Contains(lowerSearchTerm) ||
                        s.Code.ToLower().Contains(lowerSearchTerm) ||
                        s.Description.ToLower().Contains(lowerSearchTerm))
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching subjects with term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetSubjectsPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null, 
            bool? isActive = null,
            Guid? departmentUid = null)
        {
            try
            {
                var query = _dbContext.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.TeacherSubjects)
                        .ThenInclude(ts => ts.Teacher)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(s => 
                        s.Name.ToLower().Contains(lowerSearchTerm) ||
                        s.Code.ToLower().Contains(lowerSearchTerm) ||
                        s.Description.ToLower().Contains(lowerSearchTerm));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(s => s.IsActive == isActive.Value);
                }

                if (departmentUid.HasValue)
                {
                    query = query.Where(s => s.DepartmentUid == departmentUid.Value);
                }

                var totalCount = await query.CountAsync();

                var subjects = await query
                    .OrderBy(s => s.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (subjects, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged subjects");
                throw;
            }
        }

        public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null)
        {
            try
            {
                var query = _dbContext.Subjects.Where(s => s.Code == code);
                
                if (excludeUid.HasValue)
                {
                    query = query.Where(s => s.Uid != excludeUid.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subject code existence: {Code}", code);
                throw;
            }
        }

        public async Task<SubjectStatistics> GetSubjectStatisticsAsync(Guid subjectUid)
        {
            try
            {
                var subject = await _dbContext.Subjects
                    .Include(s => s.TeacherSubjects)
                        .ThenInclude(ts => ts.Teacher)
                    .FirstOrDefaultAsync(s => s.Uid == subjectUid);

                if (subject == null)
                {
                    throw new ArgumentException($"Subject with UID {subjectUid} not found");
                }

                var coursesCount = await _dbContext.Courses
                    .Where(c => c.SubjectUid == subjectUid)
                    .CountAsync();

                var activeCoursesCount = await _dbContext.Courses
                    .Where(c => c.SubjectUid == subjectUid && c.Status == CourseStatus.Active)
                    .CountAsync();

                var studentsCount = await _dbContext.Enrollments
                    .Where(e => e.Course.SubjectUid == subjectUid)
                    .Select(e => e.StudentUid)
                    .Distinct()
                    .CountAsync();

                var averageGrade = await _dbContext.Grades
                    .Where(g => g.Assignment.Course.SubjectUid == subjectUid)
                    .AverageAsync(g => (decimal?)g.Value) ?? 0;

                var statistics = new SubjectStatistics
                {
                    TeachersCount = subject.TeacherSubjects.Count,
                    CoursesCount = coursesCount,
                    ActiveCoursesCount = activeCoursesCount,
                    StudentsCount = studentsCount,
                    AverageGrade = averageGrade
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subject statistics: {SubjectUid}", subjectUid);
                throw;
            }
        }

        public async Task<bool> SetSubjectActiveStatusAsync(Guid uid, bool isActive)
        {
            try
            {
                var subject = await _dbContext.Subjects.FindAsync(uid);
                if (subject == null)
                {
                    _logger.LogWarning("Subject not found for status update: {SubjectUid}", uid);
                    return false;
                }

                subject.IsActive = isActive;
                subject.LastModifiedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Subject status updated: {SubjectName} - Active: {IsActive}", 
                    subject.Name, isActive);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject status: {SubjectUid}", uid);
                throw;
            }
        }

        /// <summary>
        /// Создает тестовые данные для предметов
        /// </summary>
        public async Task SeedTestDataAsync()
        {
            try
            {
                if (await _dbContext.Subjects.AnyAsync())
                {
                    return; // Данные уже существуют
                }

                var department = await _dbContext.Departments.FirstOrDefaultAsync();

                var subjects = new[]
                {
                    new Subject("CS101", "Программирование на C#", "Основы программирования на языке C#", 4, 3, SubjectType.Required, department?.Uid ?? Guid.Empty),
                    new Subject("DB101", "Базы данных", "Проектирование и работа с базами данных", 3, 2, SubjectType.Required, department?.Uid ?? Guid.Empty),
                    new Subject("WEB101", "Веб-разработка", "Создание веб-приложений", 4, 3, SubjectType.Specialized, department?.Uid ?? Guid.Empty),
                    new Subject("MATH101", "Математический анализ", "Основы математического анализа", 5, 4, SubjectType.Required, department?.Uid ?? Guid.Empty),
                    new Subject("ENG101", "Английский язык", "Английский язык для IT специалистов", 2, 2, SubjectType.Elective, department?.Uid ?? Guid.Empty)
                };

                await _dbContext.Subjects.AddRangeAsync(subjects);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Test subjects data seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding test subjects data");
                throw;
            }
        }
    }
} 