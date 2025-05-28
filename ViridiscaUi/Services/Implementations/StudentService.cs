using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using System.Linq.Expressions;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы со студентами
    /// Наследуется от GenericCrudService для получения универсальных CRUD операций
    /// </summary>
    public class StudentService : GenericCrudService<Student>, IStudentService
    {
        public StudentService(ApplicationDbContext dbContext, ILogger<StudentService> logger)
            : base(dbContext, logger)
        {
        }

        #region Переопределение базовых методов для специфичной логики

        /// <summary>
        /// Применяет специфичный для студентов поиск
        /// </summary>
        protected override IQueryable<Student> ApplySearchFilter(IQueryable<Student> query, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return query;

            var lowerSearchTerm = searchTerm.ToLower();

            return query.Where(s => 
                s.FirstName.ToLower().Contains(lowerSearchTerm) ||
                s.LastName.ToLower().Contains(lowerSearchTerm) ||
                s.MiddleName.ToLower().Contains(lowerSearchTerm) ||
                s.Email.ToLower().Contains(lowerSearchTerm) ||
                s.StudentCode.ToLower().Contains(lowerSearchTerm) ||
                s.PhoneNumber.Contains(searchTerm) ||
                (s.Group != null && s.Group.Name.ToLower().Contains(lowerSearchTerm)) ||
                (s.Group != null && s.Group.Code.ToLower().Contains(lowerSearchTerm))
            );
        }

        /// <summary>
        /// Валидирует специфичные для студента правила
        /// </summary>
        protected override async Task ValidateEntitySpecificRulesAsync(Student entity, List<string> errors, List<string> warnings, bool isCreate)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(entity.FirstName))
                errors.Add("Имя студента обязательно для заполнения");

            if (string.IsNullOrWhiteSpace(entity.LastName))
                errors.Add("Фамилия студента обязательна для заполнения");

            if (string.IsNullOrWhiteSpace(entity.Email))
                errors.Add("Email студента обязателен для заполнения");

            if (string.IsNullOrWhiteSpace(entity.StudentCode))
                errors.Add("Код студента обязателен для заполнения");

            // Проверка формата email
            if (!string.IsNullOrWhiteSpace(entity.Email) && !IsValidEmail(entity.Email))
                errors.Add("Некорректный формат email");

            // Проверка уникальности email
            if (!string.IsNullOrWhiteSpace(entity.Email))
            {
                var emailExists = await _dbSet
                    .Where(s => s.Uid != entity.Uid && s.Email.ToLower() == entity.Email.ToLower())
                    .AnyAsync();

                if (emailExists)
                    errors.Add($"Студент с email '{entity.Email}' уже существует");
            }

            // Проверка уникальности кода студента
            if (!string.IsNullOrWhiteSpace(entity.StudentCode))
            {
                var codeExists = await _dbSet
                    .Where(s => s.Uid != entity.Uid && s.StudentCode.ToLower() == entity.StudentCode.ToLower())
                    .AnyAsync();

                if (codeExists)
                    errors.Add($"Студент с кодом '{entity.StudentCode}' уже существует");
            }

            // Проверка даты рождения
            if (entity.BirthDate > DateTime.Now.AddYears(-14))
                warnings.Add("Возраст студента меньше 14 лет");

            if (entity.BirthDate < DateTime.Now.AddYears(-100))
                errors.Add("Некорректная дата рождения");

            // Проверка даты поступления
            if (entity.EnrollmentDate > DateTime.Now)
                warnings.Add("Дата поступления в будущем");

            if (entity.EnrollmentDate < DateTime.Now.AddYears(-10))
                warnings.Add("Дата поступления более 10 лет назад");

            // Проверка группы
            if (entity.GroupUid.HasValue)
            {
                var groupExists = await _dbContext.Groups
                    .Where(g => g.Uid == entity.GroupUid.Value)
                    .AnyAsync();

                if (!groupExists)
                    errors.Add($"Группа с Uid {entity.GroupUid.Value} не найдена");
                else
                {
                    // Проверка лимита студентов в группе
                    var group = await _dbContext.Groups
                        .Include(g => g.Students)
                        .FirstOrDefaultAsync(g => g.Uid == entity.GroupUid.Value);

                    if (group != null && isCreate)
                    {
                        var currentStudentsCount = group.Students.Count;
                        if (currentStudentsCount >= group.MaxStudents)
                            errors.Add($"Группа '{group.Name}' уже заполнена (максимум {group.MaxStudents} студентов)");
                    }
                }
            }

            // Проверка пользователя
            if (entity.UserUid != Guid.Empty)
            {
                var userExists = await _dbContext.Users
                    .Where(u => u.Uid == entity.UserUid)
                    .AnyAsync();

                if (!userExists)
                    errors.Add($"Пользователь с Uid {entity.UserUid} не найден");
            }

            await base.ValidateEntitySpecificRulesAsync(entity, errors, warnings, isCreate);
        }

        /// <summary>
        /// Переопределяем создание для генерации кода студента
        /// </summary>
        public override async Task<Student> CreateAsync(Student entity)
        {
            // Генерируем код студента если не указан
            if (string.IsNullOrEmpty(entity.StudentCode))
            {
                entity.StudentCode = await GenerateStudentCodeAsync();
            }

            return await base.CreateAsync(entity);
        }

        #endregion

        #region Реализация интерфейса IStudentService (существующие методы)

        public async Task<Student?> GetStudentAsync(Guid uid)
        {
            return await GetByUidWithIncludesAsync(uid, 
                s => s.Group, 
                s => s.User, 
                s => s.Parents, 
                s => s.Grades);
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await GetAllWithIncludesAsync(s => s.Group, s => s.User);
        }

        public async Task<IEnumerable<Student>> GetStudentsAsync()
        {
            return await GetAllStudentsAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid)
        {
            return await FindWithIncludesAsync(s => s.GroupUid == groupUid, s => s.Group, s => s.User);
        }

        public async Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentStatus status)
        {
            return await FindWithIncludesAsync(s => s.Status == status, s => s.Group, s => s.User);
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            return await CreateAsync(student);
        }

        public async Task AddStudentAsync(Student student)
        {
            await CreateAsync(student);
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            return await UpdateAsync(student);
        }

        public async Task<bool> DeleteStudentAsync(Guid uid)
        {
            return await DeleteAsync(uid);
        }

        public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
        {
            var (items, _) = await GetPagedAsync(1, int.MaxValue, searchTerm);
            return items;
        }

        public async Task<(IEnumerable<Student> Students, int TotalCount)> GetStudentsPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null, 
            StudentStatus? status = null,
            Guid? groupUid = null)
        {
            // Используем базовый метод пагинации с дополнительными фильтрами
            var query = _dbSet.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                query = ApplySearchFilter(query, searchTerm);
                }

                if (status.HasValue)
                {
                    query = query.Where(s => s.Status == status.Value);
                }

                if (groupUid.HasValue)
                {
                    query = query.Where(s => s.GroupUid == groupUid.Value);
                }

                var totalCount = await query.CountAsync();

            var items = await query
                .Include(s => s.Group)
                .Include(s => s.User)
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUid = null)
        {
            Expression<Func<Student, bool>> predicate = excludeUid.HasValue 
                ? s => s.Email == email && s.Uid != excludeUid.Value
                : s => s.Email == email;

            return await ExistsAsync(predicate);
        }

        #endregion

        #region Дополнительные методы (существующие)

        public async Task<StudentStatistics> GetStudentStatisticsAsync(Guid studentUid)
        {
            try
            {
                var student = await _dbContext.Students
                    .Include(s => s.Grades)
                        .ThenInclude(g => g.Assignment)
                    .FirstOrDefaultAsync(s => s.Uid == studentUid);

                if (student == null)
                {
                    throw new ArgumentException($"Student with UID {studentUid} not found");
                }

                var enrollmentsCount = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid)
                    .CountAsync();

                var completedCoursesCount = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid && e.CompletedAt.HasValue)
                    .CountAsync();

                var activeCoursesCount = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid && !e.CompletedAt.HasValue)
                    .CountAsync();

                var averageGrade = student.Grades.Any() 
                    ? (decimal)student.Grades.Average(g => g.Value) 
                    : 0;

                return new StudentStatistics
                {
                    StudentUid = studentUid,
                    TotalSubmissions = student.Grades.Count,
                    GradedSubmissions = student.Grades.Count,
                    AverageScore = (double)averageGrade,
                    TotalStudyTime = TimeSpan.Zero, // Заглушка
                    AttendanceRate = 100, // Заглушка
                    EnrollmentDate = student.CreatedAt,
                    EnrollmentsCount = enrollmentsCount,
                    CompletedCoursesCount = completedCoursesCount,
                    ActiveCoursesCount = activeCoursesCount,
                    AverageGrade = averageGrade,
                    TotalCredits = 0, // Заглушка
                    CompletedCredits = 0 // Заглушка
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student statistics: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<bool> ChangeStudentStatusAsync(Guid uid, StudentStatus status)
        {
            try
            {
                var student = await GetByUidAsync(uid);
                if (student == null)
                {
                    _logger.LogWarning("Student not found for status change: {StudentUid}", uid);
                    return false;
                }

                var oldStatus = student.Status;
                student.Status = status;

                var result = await UpdateAsync(student);

                if (result)
                {
                    _logger.LogInformation("Student status changed from {OldStatus} to {NewStatus}: {StudentUid}", 
                        oldStatus, status, uid);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing student status: {StudentUid}", uid);
                throw;
            }
        }

        public async Task<bool> TransferStudentToGroupAsync(Guid studentUid, Guid? newGroupUid)
        {
            try
            {
                var student = await GetByUidAsync(studentUid);
                if (student == null)
                {
                    _logger.LogWarning("Student not found for transfer: {StudentUid}", studentUid);
                    return false;
                }

                var oldGroupUid = student.GroupUid;
                student.GroupUid = newGroupUid;

                var result = await UpdateAsync(student);

                if (result)
                {
                    _logger.LogInformation("Student transferred from group {OldGroupUid} to group {NewGroupUid}: {StudentUid}", 
                        oldGroupUid, newGroupUid, studentUid);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring student: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<StudentPerformance> GetStudentPerformanceAsync(Guid studentUid)
        {
            try
            {
                var student = await _dbContext.Students
                    .Include(s => s.Grades)
                        .ThenInclude(g => g.Assignment)
                            .ThenInclude(a => a.Course)
                    .FirstOrDefaultAsync(s => s.Uid == studentUid);

                if (student == null)
                {
                    throw new ArgumentException($"Student with UID {studentUid} not found");
                }

                var enrollments = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid)
                    .Include(e => e.Course)
                    .ToListAsync();

                var assignments = await _dbContext.Assignments
                    .Where(a => enrollments.Select(e => e.CourseUid).Contains(a.CourseUid))
                    .ToListAsync();

                var performance = new StudentPerformance
                {
                    StudentUid = studentUid,
                    AverageGrade = student.Grades.Any() ? (double)student.Grades.Average(g => g.Value) : 0,
                    TotalCourses = enrollments.Count,
                    CompletedCourses = enrollments.Count(e => e.CompletedAt.HasValue),
                    ActiveCourses = enrollments.Count(e => !e.CompletedAt.HasValue),
                    TotalAssignments = assignments.Count,
                    CompletedAssignments = student.Grades.Count,
                    PendingAssignments = assignments.Count - student.Grades.Count,
                    OverdueAssignments = assignments.Count(a => a.DueDate < DateTime.UtcNow && 
                        !student.Grades.Any(g => g.AssignmentUid == a.Uid)),
                    LastActivityDate = student.Grades.Any() ? student.Grades.Max(g => g.CreatedAt) : null
                };

                return performance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student performance: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<bool> AssignToGroupAsync(Guid studentUid, Guid groupUid)
        {
            return await TransferStudentToGroupAsync(studentUid, groupUid);
        }

        public async Task<IEnumerable<Course>> GetStudentCoursesAsync(Guid studentUid)
        {
            try
            {
                var courses = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid)
                    .Include(e => e.Course)
                        .ThenInclude(c => c!.Teacher)
                    .Select(e => e.Course)
                    .Where(c => c != null)
                    .ToListAsync();
                
                return courses.Where(c => c != null).Cast<Course>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student courses: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid)
        {
            try
            {
                return await _dbContext.Grades
                    .Where(g => g.StudentUid == studentUid)
                    .Include(g => g.Assignment)
                        .ThenInclude(a => a.Course)
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student grades: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<IEnumerable<Assignment>> GetStudentActiveAssignmentsAsync(Guid studentUid)
        {
            try
            {
                var enrolledCourses = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid && !e.CompletedAt.HasValue)
                    .Select(e => e.CourseUid)
                    .ToListAsync();

                var completedAssignments = await _dbContext.Grades
                    .Where(g => g.StudentUid == studentUid)
                    .Select(g => g.AssignmentUid)
                    .Where(aUid => aUid.HasValue)
                    .Select(aUid => aUid!.Value)
                    .ToListAsync();

                return await _dbContext.Assignments
                    .Where(a => enrolledCourses.Contains(a.CourseUid) && 
                               !completedAssignments.Contains(a.Uid) &&
                               a.DueDate >= DateTime.UtcNow)
                    .Include(a => a.Course)
                    .OrderBy(a => a.DueDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student active assignments: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<IEnumerable<Assignment>> GetStudentOverdueAssignmentsAsync(Guid studentUid)
        {
            try
            {
                var enrolledCourses = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid && !e.CompletedAt.HasValue)
                    .Select(e => e.CourseUid)
                    .ToListAsync();

                var completedAssignments = await _dbContext.Grades
                    .Where(g => g.StudentUid == studentUid)
                    .Select(g => g.AssignmentUid)
                    .Where(aUid => aUid.HasValue)
                    .Select(aUid => aUid!.Value)
                    .ToListAsync();

                return await _dbContext.Assignments
                    .Where(a => enrolledCourses.Contains(a.CourseUid) && 
                               !completedAssignments.Contains(a.Uid) &&
                               a.DueDate < DateTime.UtcNow)
                    .Include(a => a.Course)
                    .OrderBy(a => a.DueDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student overdue assignments: {StudentUid}", studentUid);
                throw;
            }
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Генерирует уникальный код студента
        /// </summary>
        private async Task<string> GenerateStudentCodeAsync()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var lastCode = await _dbContext.Students
                .Where(s => s.StudentCode.StartsWith($"ST{year}"))
                .OrderByDescending(s => s.StudentCode)
                .Select(s => s.StudentCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length >= 7)
            {
                if (int.TryParse(lastCode.Substring(4), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"ST{year}{nextNumber:D3}";
        }

        /// <summary>
        /// Проверяет корректность email
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Заполняет тестовыми данными (для разработки)
        /// </summary>
        public async Task SeedTestDataAsync()
        {
            // Существующая реализация...
            await Task.CompletedTask;
            }

        #endregion
    }
} 