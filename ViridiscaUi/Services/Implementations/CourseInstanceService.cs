using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с экземплярами курсов
/// Наследуется от GenericCrudService для получения универсальных CRUD операций
/// </summary>
public class CourseInstanceService : GenericCrudService<CourseInstance>, ICourseInstanceService
{
    public CourseInstanceService(ApplicationDbContext dbContext, ILogger<CourseInstanceService> logger)
        : base(dbContext, logger)
    {
    }

    #region Переопределение базовых методов для специфичной логики

    /// <summary>
    /// Применяет специфичный для экземпляров курсов поиск
    /// </summary>
    protected override IQueryable<CourseInstance> ApplySearchFilter(IQueryable<CourseInstance> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.ToLower();

        return query.Where(ci => 
            ci.Subject.Name.ToLower().Contains(lowerSearchTerm) ||
            ci.Subject.Code.ToLower().Contains(lowerSearchTerm) ||
            ci.Group.Name.ToLower().Contains(lowerSearchTerm) ||
            ci.Group.Code.ToLower().Contains(lowerSearchTerm) ||
            (ci.Teacher != null && (
                ci.Teacher.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                ci.Teacher.Person.LastName.ToLower().Contains(lowerSearchTerm)
            ))
        );
    }

    /// <summary>
    /// Валидирует специфичные для экземпляра курса правила
    /// </summary>
    protected override async Task ValidateEntitySpecificRulesAsync(CourseInstance entity, List<string> errors, List<string> warnings, bool isCreate)
    {
        // Проверка обязательных полей
        if (entity.SubjectUid == Guid.Empty)
            errors.Add("Предмет обязателен для экземпляра курса");

        if (entity.GroupUid == Guid.Empty)
            errors.Add("Группа обязательна для экземпляра курса");

        if (entity.AcademicPeriodUid == Guid.Empty)
            errors.Add("Академический период обязателен для экземпляра курса");

        // Проверка существования связанных сущностей
        if (entity.SubjectUid != Guid.Empty)
        {
            var subjectExists = await _dbContext.Subjects
                .Where(s => s.Uid == entity.SubjectUid && s.IsActive)
                .AnyAsync();

            if (!subjectExists)
                errors.Add($"Предмет с Uid {entity.SubjectUid} не найден или неактивен");
        }

        if (entity.GroupUid != Guid.Empty)
        {
            var groupExists = await _dbContext.Groups
                .Where(g => g.Uid == entity.GroupUid)
                .AnyAsync();

            if (!groupExists)
                errors.Add($"Группа с Uid {entity.GroupUid} не найдена");
        }

        if (entity.AcademicPeriodUid != Guid.Empty)
        {
            var periodExists = await _dbContext.AcademicPeriods
                .Where(ap => ap.Uid == entity.AcademicPeriodUid)
                .AnyAsync();

            if (!periodExists)
                errors.Add($"Академический период с Uid {entity.AcademicPeriodUid} не найден");
        }

        if (entity.TeacherUid != Guid.Empty)
        {
            var teacherExists = await _dbContext.Teachers
                .Where(t => t.Uid == entity.TeacherUid && t.IsActive)
                .AnyAsync();

            if (!teacherExists)
                errors.Add($"Преподаватель с Uid {entity.TeacherUid} не найден или неактивен");
        }

        // Проверка уникальности комбинации Subject + Group + AcademicPeriod
        if (entity.SubjectUid != Guid.Empty && entity.GroupUid != Guid.Empty && entity.AcademicPeriodUid != Guid.Empty)
        {
            var duplicateExists = await _dbSet
                .Where(ci => ci.Uid != entity.Uid && 
                            ci.SubjectUid == entity.SubjectUid &&
                            ci.GroupUid == entity.GroupUid &&
                            ci.AcademicPeriodUid == entity.AcademicPeriodUid)
                .AnyAsync();

            if (duplicateExists)
                errors.Add("Экземпляр курса для данной комбинации предмета, группы и академического периода уже существует");
        }

        await base.ValidateEntitySpecificRulesAsync(entity, errors, warnings, isCreate);
    }

    #endregion

    #region Реализация интерфейса ICourseInstanceService

    public async Task<CourseInstance?> GetCourseInstanceAsync(Guid uid)
    {
        return await GetByUidWithIncludesAsync(uid, 
            ci => ci.Subject, 
            ci => ci.Group,
            ci => ci.AcademicPeriod,
            ci => ci.Teacher,
            ci => ci.Teacher.Person,
            ci => ci.Enrollments);
    }

    public async Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync()
    {
        return await GetAllWithIncludesAsync(
            ci => ci.Subject, 
            ci => ci.Group,
            ci => ci.AcademicPeriod,
            ci => ci.Teacher,
            ci => ci.Teacher.Person);
    }

    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByTeacherAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.Teacher)
                .Include(ci => ci.AcademicPeriod)
                .Where(ci => ci.TeacherUid == teacherUid)
                .OrderBy(ci => ci.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for teacher {TeacherUid}", teacherUid);
            return [];
        }
    }

    public async Task AddCourseInstanceAsync(CourseInstance courseInstance)
    {
        await CreateAsync(courseInstance);
    }

    public async Task<bool> UpdateCourseInstanceAsync(CourseInstance courseInstance)
    {
        return await UpdateAsync(courseInstance);
    }

    public async Task<bool> DeleteCourseInstanceAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    public async Task<bool> ActivateCourseInstanceAsync(Guid uid)
    {
        try
        {
            var courseInstance = await GetByUidAsync(uid);
            if (courseInstance == null)
            {
                _logger.LogWarning("CourseInstance not found for activation: {CourseInstanceUid}", uid);
                return false;
            }

            courseInstance.IsActive = true;
            return await UpdateAsync(courseInstance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating course instance: {CourseInstanceUid}", uid);
            throw;
        }
    }

    public async Task<bool> CompleteCourseInstanceAsync(Guid uid)
    {
        try
        {
            var courseInstance = await GetByUidAsync(uid);
            if (courseInstance == null)
            {
                _logger.LogWarning("CourseInstance not found for completion: {CourseInstanceUid}", uid);
                return false;
            }

            courseInstance.IsActive = false;
            return await UpdateAsync(courseInstance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing course instance: {CourseInstanceUid}", uid);
            throw;
        }
    }

    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByStudentAsync(Guid studentUid)
    {
        return await _dbSet
            .Include(ci => ci.Subject)
            .Include(ci => ci.Group)
            .Include(ci => ci.AcademicPeriod)
            .Include(ci => ci.Teacher)
            .ThenInclude(t => t.Person)
            .Include(ci => ci.Enrollments)
            .Where(ci => ci.Enrollments.Any(e => e.StudentUid == studentUid))
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByGroupAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.Teacher)
                .Include(ci => ci.AcademicPeriod)
                .Where(ci => ci.GroupUid == groupUid)
                .OrderBy(ci => ci.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for group {GroupUid}", groupUid);
            return [];
        }
    }

    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByAcademicPeriodAsync(Guid academicPeriodUid)
    {
        return await FindWithIncludesAsync(
            ci => ci.AcademicPeriodUid == academicPeriodUid,
            ci => ci.Subject,
            ci => ci.Group,
            ci => ci.Teacher,
            ci => ci.Teacher.Person);
    }

    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesBySubjectAsync(Guid subjectUid)
    {
        return await FindWithIncludesAsync(
            ci => ci.SubjectUid == subjectUid,
            ci => ci.Group,
            ci => ci.AcademicPeriod,
            ci => ci.Teacher,
            ci => ci.Teacher.Person);
    }

    public async Task<bool> EnrollStudentAsync(Guid courseInstanceUid, Guid studentUid)
    {
        try
        {
            // Проверяем существование экземпляра курса
            var courseInstance = await GetByUidAsync(courseInstanceUid);
            if (courseInstance == null)
            {
                _logger.LogWarning("CourseInstance not found for enrollment: {CourseInstanceUid}", courseInstanceUid);
                return false;
            }

            // Проверяем существование студента
            var student = await _dbContext.Students.FindAsync(studentUid);
            if (student == null)
            {
                _logger.LogWarning("Student not found for enrollment: {StudentUid}", studentUid);
                return false;
            }

            // Проверяем, не записан ли уже студент
            var existingEnrollment = await _dbContext.Enrollments
                .Where(e => e.CourseInstanceUid == courseInstanceUid && e.StudentUid == studentUid)
                .FirstOrDefaultAsync();

            if (existingEnrollment != null)
            {
                _logger.LogWarning("Student already enrolled: {StudentUid} in {CourseInstanceUid}", studentUid, courseInstanceUid);
                return false;
            }

            // Создаем новую запись
            var enrollment = new Enrollment
            {
                Uid = Guid.NewGuid(),
                CourseInstanceUid = courseInstanceUid,
                StudentUid = studentUid,
                Status = EnrollmentStatus.Active,
                EnrollmentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            _dbContext.Enrollments.Add(enrollment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Student enrolled successfully: {StudentUid} in {CourseInstanceUid}", studentUid, courseInstanceUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling student: {StudentUid} in {CourseInstanceUid}", studentUid, courseInstanceUid);
            throw;
        }
    }

    public async Task<bool> UnenrollStudentAsync(Guid courseInstanceUid, Guid studentUid)
    {
        try
        {
            var enrollment = await _dbContext.Enrollments
                .Where(e => e.CourseInstanceUid == courseInstanceUid && e.StudentUid == studentUid)
                .FirstOrDefaultAsync();

            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found for unenrollment: {StudentUid} from {CourseInstanceUid}", studentUid, courseInstanceUid);
                return false;
            }

            enrollment.Status = EnrollmentStatus.Cancelled;
            enrollment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Student unenrolled successfully: {StudentUid} from {CourseInstanceUid}", studentUid, courseInstanceUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unenrolling student: {StudentUid} from {CourseInstanceUid}", studentUid, courseInstanceUid);
            throw;
        }
    }

    public async Task<CourseInstanceProgress> GetCourseInstanceProgressAsync(Guid courseInstanceUid, Guid studentUid)
    {
        var enrollment = await _dbContext.Enrollments
            .Include(e => e.CourseInstance)
            .Where(e => e.CourseInstanceUid == courseInstanceUid && e.StudentUid == studentUid)
            .FirstOrDefaultAsync();

        if (enrollment == null)
            throw new InvalidOperationException($"Student {studentUid} is not enrolled in course instance {courseInstanceUid}");

        var totalLessons = await _dbContext.Lessons
            .Where(l => l.CourseInstanceUid == courseInstanceUid)
            .CountAsync();

        var totalAssignments = await _dbContext.Assignments
            .Where(a => a.CourseInstanceUid == courseInstanceUid)
            .CountAsync();

        var activeEnrollments = await _dbContext.Enrollments
            .Where(e => e.CourseInstanceUid == courseInstanceUid && 
                       e.Status == EnrollmentStatus.Active)
            .CountAsync();

        var completedAssignments = await _dbContext.Submissions
            .Where(s => s.Assignment.CourseInstanceUid == courseInstanceUid && 
                       s.StudentUid == studentUid &&
                       s.Status == SubmissionStatus.Graded)
            .CountAsync();

        var averageGrade = await _dbContext.Grades
            .Where(g => g.StudentUid == studentUid && 
                       g.Assignment.CourseInstanceUid == courseInstanceUid)
            .AverageAsync(g => (double?)g.Value) ?? 0.0;

        var completionPercentage = totalAssignments > 0 ? 
            (double)completedAssignments / totalAssignments * 100 : 0;

        return new CourseInstanceProgress
        {
            CourseInstanceUid = courseInstanceUid,
            StudentUid = studentUid,
            CompletedLessons = 0, // TODO: Implement lesson completion tracking
            TotalLessons = totalLessons,
            CompletedAssignments = completedAssignments,
            TotalAssignments = totalAssignments,
            AverageGrade = averageGrade,
            CompletionPercentage = completionPercentage,
            EnrolledAt = enrollment.EnrollmentDate,
            LastActivityDate = enrollment.LastModifiedAt,
            TotalTimeSpent = TimeSpan.Zero // TODO: Implement time tracking
        };
    }

    public async Task<CourseInstanceStatistics> GetCourseInstanceStatisticsAsync(Guid courseInstanceUid)
    {
        var enrollments = await _dbContext.Enrollments
            .Where(e => e.CourseInstanceUid == courseInstanceUid)
            .ToListAsync();

        var totalStudents = enrollments.Count;
        var activeStudents = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
        var completedStudents = enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

        var totalLessons = await _dbContext.Lessons
            .Where(l => l.CourseInstanceUid == courseInstanceUid)
            .CountAsync();

        var totalAssignments = await _dbContext.Assignments
            .Where(a => a.CourseInstanceUid == courseInstanceUid)
            .CountAsync();

        var grades = await _dbContext.Grades
            .Where(g => g.Assignment.CourseInstanceUid == courseInstanceUid)
            .Select(g => g.Value)
            .ToListAsync();

        var averageGrade = grades.Any() ? grades.Average(g => (double)g) : 0.0;

        var gradeDistribution = grades
            .GroupBy(g => GetGradeCategory(g))
            .ToDictionary(group => group.Key, group => group.Count());

        return new CourseInstanceStatistics
        {
            CourseInstanceUid = courseInstanceUid,
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            CompletedStudents = completedStudents,
            AverageGrade = averageGrade,
            AverageCompletionRate = totalStudents > 0 ? (double)completedStudents / totalStudents * 100 : 0,
            TotalLessons = totalLessons,
            TotalAssignments = totalAssignments,
            LastActivityDate = enrollments.Any() ? enrollments.Max(e => e.LastModifiedAt) : null,
            AverageTimeToComplete = TimeSpan.Zero, // TODO: Implement
            GradeDistribution = gradeDistribution
        };
    }

    private static string GetGradeCategory(decimal grade)
    {
        return grade switch
        {
            >= 90 => "A",
            >= 80 => "B", 
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
    }

    public async Task<(IEnumerable<CourseInstance> CourseInstances, int TotalCount)> GetCourseInstancesPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? subjectFilter = null,
        Guid? teacherFilter = null,
        Guid? groupFilter = null,
        Guid? academicPeriodFilter = null)
    {
        var query = _dbSet
            .Include(ci => ci.Subject)
            .Include(ci => ci.Group)
            .Include(ci => ci.AcademicPeriod)
            .Include(ci => ci.Teacher)
            .ThenInclude(t => t.Person)
            .AsQueryable();

        // Применяем фильтры
        if (subjectFilter.HasValue)
            query = query.Where(ci => ci.SubjectUid == subjectFilter.Value);

        if (teacherFilter.HasValue)
            query = query.Where(ci => ci.TeacherUid == teacherFilter.Value);

        if (groupFilter.HasValue)
            query = query.Where(ci => ci.GroupUid == groupFilter.Value);

        if (academicPeriodFilter.HasValue)
            query = query.Where(ci => ci.AcademicPeriodUid == academicPeriodFilter.Value);

        // Применяем поиск
        query = ApplySearchFilter(query, searchTerm);

        var totalCount = await query.CountAsync();

        var courseInstances = await query
            .OrderBy(ci => ci.Subject.Name)
            .ThenBy(ci => ci.Group.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (courseInstances, totalCount);
    }

    public async Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync(
        Guid? subjectFilter = null,
        Guid? teacherFilter = null,
        Guid? groupFilter = null,
        Guid? academicPeriodFilter = null)
    {
        var query = _dbSet
            .Include(ci => ci.Subject)
            .Include(ci => ci.Group)
            .Include(ci => ci.AcademicPeriod)
            .Include(ci => ci.Teacher)
            .ThenInclude(t => t.Person)
            .AsQueryable();

        // Применяем фильтры
        if (subjectFilter.HasValue)
            query = query.Where(ci => ci.SubjectUid == subjectFilter.Value);

        if (teacherFilter.HasValue)
            query = query.Where(ci => ci.TeacherUid == teacherFilter.Value);

        if (groupFilter.HasValue)
            query = query.Where(ci => ci.GroupUid == groupFilter.Value);

        if (academicPeriodFilter.HasValue)
            query = query.Where(ci => ci.AcademicPeriodUid == academicPeriodFilter.Value);

        return await query
            .OrderBy(ci => ci.Subject.Name)
            .ThenBy(ci => ci.Group.Name)
            .ToListAsync();
    }

    public async Task<CourseInstance?> CloneCourseInstanceAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid)
    {
        try
        {
            var original = await GetCourseInstanceAsync(courseInstanceUid);
            if (original == null)
            {
                _logger.LogWarning("CourseInstance not found for cloning: {CourseInstanceUid}", courseInstanceUid);
                return null;
            }

            var clone = new CourseInstance
            {
                Uid = Guid.NewGuid(),
                SubjectUid = original.SubjectUid,
                GroupUid = original.GroupUid,
                AcademicPeriodUid = newAcademicPeriodUid,
                TeacherUid = original.TeacherUid,
                IsActive = false, // Клон создается неактивным
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            await _dbContext.CourseInstances.AddAsync(clone);
            await _dbContext.SaveChangesAsync();
            return clone;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning course instance: {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    public async Task<IEnumerable<CourseInstance>> GetAvailableCourseInstancesForStudentAsync(Guid studentUid)
    {
        // Получаем экземпляры курсов, на которые студент еще не записан
        var enrolledCourseInstanceIds = await _dbContext.Enrollments
            .Where(e => e.StudentUid == studentUid && e.Status == EnrollmentStatus.Active)
            .Select(e => e.CourseInstanceUid)
            .ToListAsync();

        return await _dbSet
            .Include(ci => ci.Subject)
            .Include(ci => ci.Group)
            .Include(ci => ci.AcademicPeriod)
            .Include(ci => ci.Teacher)
            .ThenInclude(t => t.Person)
            .Where(ci => ci.IsActive && !enrolledCourseInstanceIds.Contains(ci.Uid))
            .OrderBy(ci => ci.Subject.Name)
            .ToListAsync();
    }

    public async Task<BulkEnrollmentResult> BulkEnrollGroupAsync(Guid courseInstanceUid, Guid groupUid)
    {
        var result = new BulkEnrollmentResult();
        var startTime = DateTime.UtcNow;

        try
        {
            // Получаем всех студентов группы
            var students = await _dbContext.Students
                .Where(s => s.GroupUid == groupUid && s.IsActive)
                .ToListAsync();

            result.TotalProcessed = students.Count;

            foreach (var student in students)
            {
                try
                {
                    var enrolled = await EnrollStudentAsync(courseInstanceUid, student.Uid);
                    if (enrolled)
                    {
                        result.SuccessCount++;
                        result.SuccessfulEnrollments.Add(student.Uid);
                    }
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add(new BulkEnrollmentError
                        {
                            StudentUid = student.Uid,
                            CourseInstanceUid = courseInstanceUid,
                            ErrorMessage = "Не удалось записать студента",
                            ErrorCode = "ENROLLMENT_FAILED"
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add(new BulkEnrollmentError
                    {
                        StudentUid = student.Uid,
                        CourseInstanceUid = courseInstanceUid,
                        ErrorMessage = ex.Message,
                        ErrorCode = "EXCEPTION"
                    });
                }
            }

            result.ExecutionTime = DateTime.UtcNow - startTime;

            _logger.LogInformation("Bulk enrollment completed: {Successful} successful, {Failed} failed", 
                result.SuccessCount, result.FailureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk enrollment for group {GroupUid} to course instance {CourseInstanceUid}", 
                groupUid, courseInstanceUid);
            result.Errors.Add(new BulkEnrollmentError
            {
                StudentUid = Guid.Empty,
                CourseInstanceUid = courseInstanceUid,
                ErrorMessage = $"Общая ошибка: {ex.Message}",
                ErrorCode = "GENERAL_ERROR"
            });
        }

        return result;
    }

    public async Task<IEnumerable<CourseInstance>> GetRecommendedCourseInstancesAsync(Guid studentUid)
    {
        // Простая логика рекомендаций - можно расширить
        var student = await _dbContext.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.Uid == studentUid);

        if (student == null)
            return Enumerable.Empty<CourseInstance>();

        // Рекомендуем курсы для группы студента
        return await GetCourseInstancesByGroupAsync(student.GroupUid ?? Guid.Empty);
    }

    public async Task<IEnumerable<CourseInstance>> GetManagedCourseInstancesAsync()
    {
        // TODO: Implement based on current user's role and permissions
        return await GetAllCourseInstancesAsync();
    }

    /// <summary>
    /// Получает все экземпляры курсов
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetAllCoursesAsync()
    {
        return await GetAllWithIncludesAsync(
            ci => ci.Subject,
            ci => ci.Teacher,
            ci => ci.Group,
            ci => ci.AcademicPeriod);
    }

    /// <summary>
    /// Получает экземпляры курсов с пагинацией
    /// </summary>
    public async Task<(IEnumerable<CourseInstance> Courses, int TotalCount)> GetCoursesPagedAsync(
        int page, int pageSize, string? searchTerm = null)
    {
        var query = _dbContext.CourseInstances
            .Include(ci => ci.Subject)
            .Include(ci => ci.Teacher)
            .Include(ci => ci.Group)
            .Include(ci => ci.AcademicPeriod)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = ApplySearchFilter(query, searchTerm);
        }

        var totalCount = await query.CountAsync();
        var courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (courses, totalCount);
    }

    /// <summary>
    /// Получает статистику курса
    /// </summary>
    public async Task<object> GetCourseStatisticsAsync(Guid courseInstanceUid)
    {
        var courseInstance = await GetByUidAsync(courseInstanceUid);
        if (courseInstance == null) return new { };

        var enrollmentCount = await _dbContext.Enrollments
            .Where(e => e.CourseInstanceUid == courseInstanceUid)
            .CountAsync();

        var assignmentCount = await _dbContext.Assignments
            .Where(a => a.CourseInstanceUid == courseInstanceUid)
            .CountAsync();

        var averageGrade = await _dbContext.Grades
            .Where(g => g.Assignment.CourseInstanceUid == courseInstanceUid)
            .AverageAsync(g => (double?)g.Value) ?? 0;

        return new
        {
            EnrollmentCount = enrollmentCount,
            AssignmentCount = assignmentCount,
            AverageGrade = averageGrade
        };
    }

    /// <summary>
    /// Получает курсы студента
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCoursesByStudentAsync(Guid studentUid)
    {
        return await _dbContext.CourseInstances
            .Include(ci => ci.Subject)
            .Include(ci => ci.Teacher)
            .Include(ci => ci.Group)
            .Include(ci => ci.AcademicPeriod)
            .Where(ci => ci.Enrollments.Any(e => e.StudentUid == studentUid))
            .ToListAsync();
    }

    /// <summary>
    /// Получает экземпляр курса по ID
    /// </summary>
    public async Task<CourseInstance?> GetCourseAsync(Guid courseInstanceUid)
    {
        return await GetByUidWithIncludesAsync(courseInstanceUid,
            ci => ci.Subject,
            ci => ci.Teacher,
            ci => ci.Group,
            ci => ci.AcademicPeriod,
            ci => ci.Enrollments);
    }

    /// <summary>
    /// Клонирует экземпляр курса
    /// </summary>
    public async Task<CourseInstance?> CloneCourseAsync(Guid sourceUid, Guid? newGroupUid = null, Guid? newAcademicPeriodUid = null)
    {
        try
        {
            var source = await GetByUidAsync(sourceUid);
            if (source == null) return null;

            var clone = new CourseInstance
            {
                Uid = Guid.NewGuid(),
                SubjectUid = source.SubjectUid,
                GroupUid = newGroupUid ?? source.GroupUid,
                TeacherUid = source.TeacherUid,
                AcademicPeriodUid = newAcademicPeriodUid ?? source.AcademicPeriodUid,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            await _dbContext.CourseInstances.AddAsync(clone);
            await _dbContext.SaveChangesAsync();
            return clone;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning course instance {SourceUid}", sourceUid);
            return null;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов по фильтрам
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByFiltersAsync(
        Guid? subjectUid = null,
        Guid? teacherUid = null,
        Guid? groupUid = null,
        Guid? academicPeriodUid = null)
    {
        try
        {
            var query = _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.Teacher)
                .Include(ci => ci.AcademicPeriod)
                .AsQueryable();

            if (subjectUid.HasValue)
                query = query.Where(ci => ci.SubjectUid == subjectUid.Value);

            if (teacherUid.HasValue)
                query = query.Where(ci => ci.TeacherUid == teacherUid.Value);

            if (groupUid.HasValue)
                query = query.Where(ci => ci.GroupUid == groupUid.Value);

            if (academicPeriodUid.HasValue)
                query = query.Where(ci => ci.AcademicPeriodUid == academicPeriodUid.Value);

            return await query.OrderBy(ci => ci.Name).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances by filters");
            return [];
        }
    }

    /// <summary>
    /// Получает неназначенные экземпляры курсов (без преподавателя)
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetUnassignedAsync()
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Where(ci => ci.TeacherUid == null || ci.TeacherUid == Guid.Empty)
                .OrderBy(ci => ci.Subject.Name)
                .ThenBy(ci => ci.Group.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unassigned course instances");
            return [];
        }
    }

    /// <summary>
    /// Получает экземпляры курсов по UID преподавателя (алиас для GetCourseInstancesByTeacherAsync)
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetByTeacherUidAsync(Guid teacherUid)
    {
        return await GetCourseInstancesByTeacherAsync(teacherUid);
    }

    #endregion
} 