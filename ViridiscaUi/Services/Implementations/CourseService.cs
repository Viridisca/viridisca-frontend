using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure;
using CourseStatus = ViridiscaUi.Domain.Models.Education.Enums.CourseStatus;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с курсами
    /// Наследуется от GenericCrudService для получения универсальных CRUD операций
    /// </summary>
    public class CourseService : GenericCrudService<Course>, ICourseService
    {
        public CourseService(ApplicationDbContext dbContext, ILogger<CourseService> logger)
            : base(dbContext, logger)
        {
        }

        #region Переопределение базовых методов для специфичной логики

        /// <summary>
        /// Применяет специфичный для курсов поиск
        /// </summary>
        protected override IQueryable<Course> ApplySearchFilter(IQueryable<Course> query, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return query;

            var lowerSearchTerm = searchTerm.ToLower();

            return query.Where(c => 
                c.Name.ToLower().Contains(lowerSearchTerm) ||
                c.Code.ToLower().Contains(lowerSearchTerm) ||
                c.Description.ToLower().Contains(lowerSearchTerm) ||
                c.Category.ToLower().Contains(lowerSearchTerm) ||
                (c.Teacher != null && (
                    c.Teacher.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    c.Teacher.LastName.ToLower().Contains(lowerSearchTerm)
                ))
            );
        }

        /// <summary>
        /// Валидирует специфичные для курса правила
        /// </summary>
        protected override async Task ValidateEntitySpecificRulesAsync(Course entity, List<string> errors, List<string> warnings, bool isCreate)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(entity.Name))
                errors.Add("Название курса обязательно для заполнения");

            if (string.IsNullOrWhiteSpace(entity.Code))
                errors.Add("Код курса обязателен для заполнения");

            if (string.IsNullOrWhiteSpace(entity.Description))
                warnings.Add("Рекомендуется добавить описание курса");

            // Проверка уникальности кода курса
            if (!string.IsNullOrWhiteSpace(entity.Code))
            {
                var codeExists = await _dbSet
                    .Where(c => c.Uid != entity.Uid && c.Code.ToLower() == entity.Code.ToLower())
                    .AnyAsync();

                if (codeExists)
                    errors.Add($"Курс с кодом '{entity.Code}' уже существует");
            }

            // Проверка уникальности названия курса
            if (!string.IsNullOrWhiteSpace(entity.Name))
            {
                var nameExists = await _dbSet
                    .Where(c => c.Uid != entity.Uid && c.Name.ToLower() == entity.Name.ToLower())
                    .AnyAsync();

                if (nameExists)
                    warnings.Add($"Курс с названием '{entity.Name}' уже существует");
            }

            // Проверка дат
            if (entity.StartDate.HasValue && entity.EndDate.HasValue)
            {
                if (entity.StartDate.Value >= entity.EndDate.Value)
                    errors.Add("Дата начала курса должна быть раньше даты окончания");

                if (entity.StartDate.Value < DateTime.Now.AddYears(-5))
                    warnings.Add("Дата начала курса более 5 лет назад");

                if (entity.EndDate.Value > DateTime.Now.AddYears(5))
                    warnings.Add("Дата окончания курса более чем через 5 лет");
            }

            // Проверка кредитов
            if (entity.Credits <= 0)
                errors.Add("Количество кредитов должно быть больше нуля");

            if (entity.Credits > 20)
                warnings.Add("Количество кредитов больше 20 - это необычно много");

            // Проверка преподавателя
            if (entity.TeacherUid.HasValue)
            {
                var teacherExists = await _dbContext.Teachers
                    .Where(t => t.Uid == entity.TeacherUid.Value)
                    .AnyAsync();

                if (!teacherExists)
                    errors.Add($"Преподаватель с Uid {entity.TeacherUid.Value} не найден");
            }

            // Проверка максимального количества студентов
            if (entity.MaxEnrollments <= 0)
                errors.Add("Максимальное количество студентов должно быть больше нуля");

            if (entity.MaxEnrollments > 1000)
                warnings.Add("Максимальное количество студентов больше 1000 - это очень много");

            await base.ValidateEntitySpecificRulesAsync(entity, errors, warnings, isCreate);
        }

        #endregion

        #region Реализация интерфейса ICourseService (существующие методы)

        public async Task<Course?> GetCourseAsync(Guid uid)
        {
            return await GetByUidWithIncludesAsync(uid, 
                c => c.Teacher, 
                c => c.Modules, 
                c => c.Enrollments);
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await GetAllWithIncludesAsync(c => c.Teacher, c => c.Enrollments);
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(Guid teacherUid)
        {
            return await FindWithIncludesAsync(c => c.TeacherUid == teacherUid, c => c.Teacher);
        }

        public async Task AddCourseAsync(Course course)
        {
            await CreateAsync(course);
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            return await UpdateAsync(course);
        }

        public async Task<bool> DeleteCourseAsync(Guid uid)
        {
            return await DeleteAsync(uid);
        }

        public async Task<bool> PublishCourseAsync(Guid uid)
        {
            try
            {
                var course = await GetByUidAsync(uid);
                if (course == null)
                {
                    _logger.LogWarning("Course not found for publishing: {CourseUid}", uid);
                    return false;
                }

                course.Status = CourseStatus.Published;
                return await UpdateAsync(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing course: {CourseUid}", uid);
                throw;
            }
        }

        public async Task<bool> ArchiveCourseAsync(Guid uid)
        {
            try
            {
                var course = await GetByUidAsync(uid);
                if (course == null)
                {
                    _logger.LogWarning("Course not found for archiving: {CourseUid}", uid);
                    return false;
                }

                course.Status = CourseStatus.Archived;
                return await UpdateAsync(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving course: {CourseUid}", uid);
                throw;
            }
        }

        #endregion

        #region Дополнительные методы (существующие)

        public async Task<IEnumerable<Course>> GetCoursesByStudentAsync(Guid studentUid)
        {
            try
            {
                return await _dbContext.Enrollments
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                    .Where(e => e.StudentUid == studentUid)
                    .Select(e => e.Course)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses by student: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<bool> EnrollStudentAsync(Guid courseUid, Guid studentUid)
        {
            try
            {
                // Проверяем, не зарегистрирован ли уже студент
                var existingEnrollment = await _dbContext.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseUid == courseUid && e.StudentUid == studentUid);
                
                if (existingEnrollment != null)
                {
                    _logger.LogWarning("Student {StudentUid} is already enrolled in course {CourseUid}", studentUid, courseUid);
                    return false;
                }

                var enrollment = new Enrollment
                {
                    Uid = Guid.NewGuid(),
                    CourseUid = courseUid,
                    StudentUid = studentUid,
                    EnrollmentDate = DateTime.UtcNow,
                    Status = EnrollmentStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                };

                await _dbContext.Enrollments.AddAsync(enrollment);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Student {StudentUid} enrolled in course {CourseUid}", studentUid, courseUid);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling student {StudentUid} in course {CourseUid}", studentUid, courseUid);
                throw;
            }
        }

        public async Task<bool> UnenrollStudentAsync(Guid courseUid, Guid studentUid)
        {
            try
            {
                var enrollment = await _dbContext.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseUid == courseUid && e.StudentUid == studentUid);
                
                if (enrollment == null)
                {
                    _logger.LogWarning("Enrollment not found for student {StudentUid} in course {CourseUid}", studentUid, courseUid);
                    return false;
                }

                _dbContext.Enrollments.Remove(enrollment);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Student {StudentUid} unenrolled from course {CourseUid}", studentUid, courseUid);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unenrolling student {StudentUid} from course {CourseUid}", studentUid, courseUid);
                throw;
            }
        }

        public async Task<CourseProgress> GetCourseProgressAsync(Guid courseUid, Guid studentUid)
        {
            try
            {
                var enrollment = await _dbContext.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseUid == courseUid && e.StudentUid == studentUid);

                if (enrollment == null)
                    throw new ArgumentException("Student is not enrolled in this course");

                // Получаем статистику по курсу
                var totalLessons = await _dbContext.Lessons
                    .Where(l => l.Module.CourseUid == courseUid)
                    .CountAsync();

                var totalAssignments = await _dbContext.Assignments
                    .Where(a => a.CourseUid == courseUid)
                    .CountAsync();

                var completedAssignments = await _dbContext.Grades
                    .Where(g => g.StudentUid == studentUid && g.Assignment.CourseUid == courseUid)
                    .CountAsync();

                var averageGrade = await _dbContext.Grades
                    .Where(g => g.StudentUid == studentUid && g.Assignment.CourseUid == courseUid)
                    .AverageAsync(g => (double?)g.Value) ?? 0;

                var lastActivity = await _dbContext.Grades
                    .Where(g => g.StudentUid == studentUid && g.Assignment.CourseUid == courseUid)
                    .MaxAsync(g => (DateTime?)g.CreatedAt);

                return new CourseProgress
                {
                    CourseUid = courseUid,
                    StudentUid = studentUid,
                    CompletedLessons = 0, // TODO: Implement lesson tracking
                    TotalLessons = totalLessons,
                    CompletedAssignments = completedAssignments,
                    TotalAssignments = totalAssignments,
                    AverageGrade = averageGrade,
                    CompletionPercentage = totalAssignments > 0 ? (double)completedAssignments / totalAssignments * 100 : 0,
                    LastActivityDate = lastActivity,
                    EnrolledAt = enrollment.EnrollmentDate,
                    TotalTimeSpent = TimeSpan.Zero // TODO: Implement time tracking
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course progress for student {StudentUid} in course {CourseUid}", studentUid, courseUid);
                throw;
            }
        }

        public async Task<CourseStatistics> GetCourseStatisticsAsync(Guid courseUid)
        {
            try
            {
                var totalStudents = await _dbContext.Enrollments
                    .Where(e => e.CourseUid == courseUid)
                    .CountAsync();

                var activeStudents = await _dbContext.Enrollments
                    .Where(e => e.CourseUid == courseUid && e.Status == EnrollmentStatus.Active)
                    .CountAsync();

                var completedStudents = await _dbContext.Enrollments
                    .Where(e => e.CourseUid == courseUid && e.CompletedAt.HasValue)
                    .CountAsync();

                var averageGrade = await _dbContext.Grades
                    .Where(g => g.Assignment.CourseUid == courseUid)
                    .AverageAsync(g => (double?)g.Value) ?? 0;

                var totalLessons = await _dbContext.Lessons
                    .Where(l => l.Module.CourseUid == courseUid)
                    .CountAsync();

                var totalAssignments = await _dbContext.Assignments
                    .Where(a => a.CourseUid == courseUid)
                    .CountAsync();

                var lastActivity = await _dbContext.Grades
                    .Where(g => g.Assignment.CourseUid == courseUid)
                    .MaxAsync(g => (DateTime?)g.CreatedAt);

                return new CourseStatistics
                {
                    CourseUid = courseUid,
                    TotalStudents = totalStudents,
                    ActiveStudents = activeStudents,
                    CompletedStudents = completedStudents,
                    AverageGrade = averageGrade,
                    AverageCompletionRate = totalStudents > 0 ? (double)completedStudents / totalStudents * 100 : 0,
                    TotalLessons = totalLessons,
                    TotalAssignments = totalAssignments,
                    LastActivityDate = lastActivity,
                    AverageTimeToComplete = TimeSpan.Zero, // TODO: Implement
                    GradeDistribution = new Dictionary<string, int>() // TODO: Implement
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course statistics: {CourseUid}", courseUid);
                throw;
            }
        }

        public async Task<(IEnumerable<Course> Courses, int TotalCount)> GetCoursesPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null,
            string? categoryFilter = null,
            string? statusFilter = null,
            string? difficultyFilter = null,
            Guid? teacherFilter = null)
        {
            try
            {
                var query = _dbSet.AsQueryable();

                // Применяем поиск
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = ApplySearchFilter(query, searchTerm);
                }

                // Применяем фильтры
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    query = query.Where(c => c.Category == categoryFilter);
                }

                if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<CourseStatus>(statusFilter, out var status))
                {
                    query = query.Where(c => c.Status == status);
                }

                if (teacherFilter.HasValue)
                {
                    query = query.Where(c => c.TeacherUid == teacherFilter.Value);
                }

                var totalCount = await query.CountAsync();
                
                var courses = await query
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .Include(c => c.Assignments)
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (courses, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged courses");
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync(
            string? categoryFilter = null,
            string? statusFilter = null,
            string? difficultyFilter = null)
        {
            try
            {
                var query = _dbSet.Include(c => c.Teacher).AsQueryable();
                
                if (!string.IsNullOrEmpty(categoryFilter))
                    query = query.Where(c => c.Category == categoryFilter);
                    
                if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<CourseStatus>(statusFilter, out var status))
                    query = query.Where(c => c.Status == status);
                
                return await query.OrderBy(c => c.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all courses with filters");
                throw;
            }
        }

        public async Task<Course?> CloneCourseAsync(Guid courseUid, string newName)
        {
            try
            {
                var originalCourse = await GetByUidAsync(courseUid);
                if (originalCourse == null)
                {
                    _logger.LogWarning("Course not found for cloning: {CourseUid}", courseUid);
                    return null;
                }

                var clonedCourse = new Course
                {
                    Uid = Guid.NewGuid(),
                    Name = newName,
                    Code = originalCourse.Code + "_COPY",
                    Description = originalCourse.Description,
                    Category = originalCourse.Category,
                    Status = CourseStatus.Draft,
                    TeacherUid = originalCourse.TeacherUid,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(3),
                    Credits = originalCourse.Credits,
                    Prerequisites = originalCourse.Prerequisites,
                    LearningOutcomes = originalCourse.LearningOutcomes,
                    MaxEnrollments = originalCourse.MaxEnrollments,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                };

                return await CreateAsync(clonedCourse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning course: {CourseUid}", courseUid);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetAvailableCoursesForStudentAsync(Guid studentUid)
        {
            try
            {
                var enrolledCourseUids = await _dbContext.Enrollments
                    .Where(e => e.StudentUid == studentUid)
                    .Select(e => e.CourseUid)
                    .ToListAsync();

                return await _dbSet
                    .Include(c => c.Teacher)
                    .Where(c => c.Status == CourseStatus.Published && !enrolledCourseUids.Contains(c.Uid))
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available courses for student: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<BulkEnrollmentResult> BulkEnrollGroupAsync(Guid courseUid, Guid groupUid)
        {
            try
            {
                var result = new BulkEnrollmentResult();

                var students = await _dbContext.Students
                    .Where(s => s.GroupUid == groupUid && s.Status == StudentStatus.Active)
                    .ToListAsync();

                foreach (var student in students)
                {
                    try
                    {
                        var enrolled = await EnrollStudentAsync(courseUid, student.Uid);
                        if (enrolled)
                        {
                            result.SuccessfulEnrollments++;
                            result.EnrolledStudentUids.Add(student.Uid);
                        }
                        else
                        {
                            result.FailedEnrollments++;
                            result.Errors.Add($"Студент {student.FullName} уже записан на курс");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add($"Ошибка при записи студента {student.FullName}: {ex.Message}");
                    }
                }

                _logger.LogInformation("Bulk enrollment completed: {Successful} successful, {Failed} failed", 
                    result.SuccessfulEnrollments, result.FailedEnrollments);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk enrollment for group {GroupUid} to course {CourseUid}", groupUid, courseUid);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetRecommendedCoursesAsync(Guid studentUid)
        {
            try
            {
                // Простая логика рекомендаций - курсы того же уровня, что и уже изучаемые
                var enrolledCourses = await GetCoursesByStudentAsync(studentUid);
                var enrolledCategories = enrolledCourses.Select(c => c.Category).Distinct();

                return await _dbSet
                    .Include(c => c.Teacher)
                    .Where(c => c.Status == CourseStatus.Published && 
                               enrolledCategories.Contains(c.Category) &&
                               !enrolledCourses.Select(ec => ec.Uid).Contains(c.Uid))
                    .Take(5)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended courses for student: {StudentUid}", studentUid);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetManagedCoursesAsync()
        {
            try
            {
                // Для простоты возвращаем все активные курсы
                // В реальной системе здесь была бы логика определения управляемых курсов
                return await _dbSet
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .Where(c => c.Status == CourseStatus.Published || c.Status == CourseStatus.Active)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting managed courses");
                throw;
            }
        }

        #endregion
    }
} 