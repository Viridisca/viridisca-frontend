using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с оценками
/// Наследуется от GenericCrudService для получения универсальных CRUD операций
/// </summary>
public class GradeService : GenericCrudService<Grade>, IGradeService
{
    public GradeService(ApplicationDbContext dbContext, ILogger<GradeService> logger)
        : base(dbContext, logger)
    {
    }

    #region Переопределение базовых методов для специфичной логики

    /// <summary>
    /// Применяет фильтр поиска к запросу оценок
    /// </summary>
    protected override IQueryable<Grade> ApplySearchFilter(IQueryable<Grade> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.ToLower();

        return query.Where(g => 
            g.Comment.ToLower().Contains(lowerSearchTerm) ||
            (g.Student != null && g.Student.Person != null && (
                g.Student.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                g.Student.Person.LastName.ToLower().Contains(lowerSearchTerm) ||
                g.Student.StudentCode.ToLower().Contains(lowerSearchTerm)
            )) ||
            (g.Assignment != null && (
                g.Assignment.Title.ToLower().Contains(lowerSearchTerm) ||
                g.Assignment.CourseInstance.Subject.Name.ToLower().Contains(lowerSearchTerm)
            )) ||
            (g.Teacher != null && g.Teacher.Person != null && (
                g.Teacher.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                g.Teacher.Person.LastName.ToLower().Contains(lowerSearchTerm)
            ))
        );
    }

    /// <summary>
    /// Валидирует специфичные для оценки правила
    /// </summary>
    protected override async Task ValidateEntitySpecificRulesAsync(Grade entity, List<string> errors, List<string> warnings, bool isCreate)
    {
        // Проверка обязательных полей
        if (entity.StudentUid == Guid.Empty)
            errors.Add("Студент обязателен для оценки");

        if (entity.AssignmentUid == Guid.Empty)
            errors.Add("Задание обязательно для оценки");

        if (entity.TeacherUid == Guid.Empty)
            errors.Add("Преподаватель обязателен для оценки");

        // Проверка значения оценки
        if (entity.Value < 0 || entity.Value > 100)
            errors.Add("Оценка должна быть от 0 до 100 баллов");

        if (entity.Value < 60)
            warnings.Add("Оценка ниже проходного балла (60)");

        // Проверка существования студента
        var studentExists = await _dbContext.Students
            .Where(s => s.Uid == entity.StudentUid)
            .AnyAsync();

        if (!studentExists)
            errors.Add($"Студент с Uid {entity.StudentUid} не найден");

        // Проверка существования задания
        var assignmentExists = await _dbContext.Assignments
            .Where(a => a.Uid == entity.AssignmentUid)
            .AnyAsync();

        if (!assignmentExists)
            errors.Add($"Задание с Uid {entity.AssignmentUid} не найдено");

        // Проверка существования преподавателя
        var teacherExists = await _dbContext.Teachers
            .Where(t => t.Uid == entity.TeacherUid)
            .AnyAsync();

        if (!teacherExists)
            errors.Add($"Преподаватель с Uid {entity.TeacherUid} не найден");

        // Проверка дублирования оценки
        if (isCreate)
        {
            var duplicateExists = await _dbSet
                .Where(g => g.StudentUid == entity.StudentUid && g.AssignmentUid == entity.AssignmentUid)
                .AnyAsync();

            if (duplicateExists)
                errors.Add("Оценка за это задание для данного студента уже существует");
        }

        // Проверка срока сдачи
        if (assignmentExists)
        {
            var assignment = await _dbContext.Assignments
                .FirstOrDefaultAsync(a => a.Uid == entity.AssignmentUid);

            if (assignment != null && assignment.DueDate.HasValue)
            {
                if (entity.IssuedAt > assignment.DueDate.Value)
                    warnings.Add("Задание оценено после срока сдачи");
            }
        }

        await base.ValidateEntitySpecificRulesAsync(entity, errors, warnings, isCreate);
    }

    #endregion

    #region Реализация интерфейса IGradeService (существующие методы)

    public async Task<Grade?> GetGradeAsync(Guid uid)
    {
        return await GetByUidWithIncludesAsync(uid, 
            g => g.Student, 
            g => g.Assignment, 
            g => g.Teacher);
    }

    public async Task<IEnumerable<Grade>> GetAllGradesAsync()
    {
        return await GetAllWithIncludesAsync(g => g.Student, g => g.Assignment, g => g.Teacher);
    }

    public async Task<IEnumerable<Grade>> GetGradesAsync()
    {
        return await GetAllGradesAsync();
    }

    public async Task<Grade> CreateGradeAsync(Grade grade)
    {
        return await CreateAsync(grade);
    }

    public async Task AddGradeAsync(Grade grade)
    {
        await CreateAsync(grade);
    }

    public async Task<bool> UpdateGradeAsync(Grade grade)
    {
        return await UpdateAsync(grade);
    }

    public async Task<bool> DeleteGradeAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    public async Task<IEnumerable<Grade>> GetGradesByStudentAsync(Guid studentUid)
    {
        return await FindWithIncludesAsync(g => g.StudentUid == studentUid, 
            g => g.Assignment, g => g.Teacher);
    }

    public async Task<IEnumerable<Grade>> GetGradesByAssignmentAsync(Guid assignmentUid)
    {
        return await FindWithIncludesAsync(g => g.AssignmentUid == assignmentUid, 
            g => g.Student, g => g.Teacher);
    }

    public async Task<IEnumerable<Grade>> GetGradesByTeacherAsync(Guid teacherUid)
    {
        return await FindWithIncludesAsync(g => g.TeacherUid == teacherUid, 
            g => g.Student, g => g.Assignment);
    }

    public async Task<IEnumerable<Grade>> GetGradesByCourseAsync(Guid courseInstanceUid)
    {
        return await FindWithIncludesAsync(g => g.Assignment.CourseInstanceUid == courseInstanceUid,
            g => g.Student,
            g => g.Assignment,
            g => g.Assignment.CourseInstance);
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Получает статистику оценок студента
    /// </summary>
    public async Task<StudentGradeStatistics> GetStudentGradeStatisticsAsync(Guid studentUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new StudentGradeStatistics
                {
                    StudentUid = studentUid,
                    TotalGrades = 0,
                    AverageGrade = 0,
                    HighestGrade = 0,
                    LowestGrade = 0,
                    PassingGrades = 0,
                    FailingGrades = 0
                };
            }

            return new StudentGradeStatistics
            {
                StudentUid = studentUid,
                TotalGrades = grades.Count,
                AverageGrade = (double)grades.Average(g => g.Value),
                HighestGrade = (double)grades.Max(g => g.Value),
                LowestGrade = (double)grades.Min(g => g.Value),
                PassingGrades = grades.Count(g => g.Value >= 60),
                FailingGrades = grades.Count(g => g.Value < 60),
                GradesBySubject = grades
                    .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                    .ToDictionary(
                        group => group.Key,
                        group => (double)group.Average(g => g.Value)
                    )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics: {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок по заданию
    /// </summary>
    public async Task<AssignmentGradeStatistics> GetAssignmentGradeStatisticsAsync(Guid assignmentUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.AssignmentUid == assignmentUid)
                .Include(g => g.Student)
                .ToListAsync();

            if (!grades.Any())
            {
                return new AssignmentGradeStatistics
                {
                    AssignmentUid = assignmentUid,
                    TotalSubmissions = 0,
                    AverageGrade = 0,
                    HighestGrade = 0,
                    LowestGrade = 0,
                    PassingSubmissions = 0,
                    FailingSubmissions = 0
                };
            }

            return new AssignmentGradeStatistics
            {
                AssignmentUid = assignmentUid,
                TotalSubmissions = grades.Count,
                AverageGrade = (double)grades.Average(g => g.Value),
                HighestGrade = (double)grades.Max(g => g.Value),
                LowestGrade = (double)grades.Min(g => g.Value),
                PassingSubmissions = grades.Count(g => g.Value >= 60),
                FailingSubmissions = grades.Count(g => g.Value < 60),
                GradeDistribution = grades
                    .GroupBy(g => GetGradeRange((double)g.Value))
                    .ToDictionary(
                        group => group.Key,
                        group => group.Count()
                    )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment grade statistics: {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает оценки с пагинацией (правильная сигнатура интерфейса)
    /// </summary>
    public async Task<(IEnumerable<Grade> Grades, int TotalCount)> GetGradesPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Guid? courseUid = null,
        Guid? groupUid = null,
        (decimal? min, decimal? max)? gradeRange = null,
        (DateTime? start, DateTime? end)? period = null)
    {
        try
        {
            var query = _dbContext.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .Include(g => g.Teacher)
                .AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            if (courseUid.HasValue)
            {
                query = query.Where(g => g.Assignment != null && g.Assignment.CourseInstanceUid == courseUid.Value);
            }

            if (groupUid.HasValue)
            {
                query = query.Where(g => g.Student != null && g.Student.GroupUid == groupUid.Value);
            }

            if (gradeRange.HasValue)
            {
                if (gradeRange.Value.min.HasValue)
                    query = query.Where(g => g.Value >= gradeRange.Value.min.Value);
                
                if (gradeRange.Value.max.HasValue)
                    query = query.Where(g => g.Value <= gradeRange.Value.max.Value);
            }

            if (period.HasValue)
            {
                if (period.Value.start.HasValue)
                    query = query.Where(g => g.IssuedAt >= period.Value.start.Value);
                
                if (period.Value.end.HasValue)
                    query = query.Where(g => g.IssuedAt <= period.Value.end.Value);
            }

            var totalCount = await query.CountAsync();

            var grades = await query
                .OrderByDescending(g => g.IssuedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (grades, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged grades");
            throw;
        }
    }

    /// <summary>
    /// Массовое выставление оценок
    /// </summary>
    public async Task<BulkGradeResult> BulkCreateGradesAsync(IEnumerable<Grade> grades)
    {
        var result = new BulkGradeResult();

        foreach (var grade in grades)
        {
            try
            {
                await CreateAsync(grade);
                result.SuccessfulGrades++;
                result.CreatedGradeUids.Add(grade.Uid);
            }
            catch (Exception ex)
            {
                result.FailedGrades++;
                result.Errors.Add($"Ошибка при создании оценки для студента {grade.StudentUid}: {ex.Message}");
            }
        }

        _logger.LogInformation("Bulk grade creation completed: {Successful} successful, {Failed} failed", 
            result.SuccessfulGrades, result.FailedGrades);

        return result;
    }

    /// <summary>
    /// Получает недавние оценки студента
    /// </summary>
    public async Task<IEnumerable<Grade>> GetRecentGradesByStudentAsync(Guid studentUid, int count = 10)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .Include(g => g.Teacher)
                .Where(g => g.StudentUid == studentUid)
                .OrderByDescending(g => g.IssuedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent grades for student: {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Обновляет комментарий к оценке
    /// </summary>
    public async Task<bool> UpdateGradeCommentAsync(Guid gradeUid, string comment)
    {
        try
        {
            var grade = await GetByUidAsync(gradeUid);
            if (grade == null)
            {
                _logger.LogWarning("Grade {GradeUid} not found for comment update", gradeUid);
                return false;
            }

            grade.Comment = comment;
            grade.LastModifiedAt = DateTime.UtcNow;

            return await UpdateAsync(grade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment for grade {GradeUid}", gradeUid);
            return false;
        }
    }

    /// <summary>
    /// Массовое добавление оценок
    /// </summary>
    public async Task<bool> BulkAddGradesAsync(IEnumerable<Grade> grades)
    {
        try
        {
            var gradesList = grades.ToList();
            foreach (var grade in gradesList)
            {
                grade.Uid = Guid.NewGuid();
                grade.IssuedAt = DateTime.UtcNow;
                grade.LastModifiedAt = DateTime.UtcNow;
            }

            await CreateManyAsync(gradesList);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk adding grades");
            return false;
        }
    }

    /// <summary>
    /// Получает статистику оценок
    /// </summary>
    public async Task<ViewModels.Education.GradeStatistics> GetGradeStatisticsAsync(
        Guid? courseUid = null,
        Guid? groupUid = null,
        (DateTime? start, DateTime? end)? period = null)
    {
        try
        {
            var query = _dbContext.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .Include(g => g.Student)
                .AsQueryable();

            if (courseUid.HasValue)
            {
                query = query.Where(g => g.Assignment.CourseInstanceUid == courseUid.Value);
            }

            if (groupUid.HasValue)
            {
                query = query.Where(g => g.Student.GroupUid == groupUid.Value);
            }

            if (period.HasValue)
            {
                if (period.Value.start.HasValue)
                    query = query.Where(g => g.IssuedAt >= period.Value.start.Value);
                
                if (period.Value.end.HasValue)
                    query = query.Where(g => g.IssuedAt <= period.Value.end.Value);
            }

            var grades = await query.ToListAsync();

            if (!grades.Any())
            {
                return new ViewModels.Education.GradeStatistics
                {
                    TotalGrades = 0,
                    AverageGrade = 0,
                    ExcellentCount = 0,
                    GoodCount = 0,
                    SatisfactoryCount = 0,
                    UnsatisfactoryCount = 0,
                    SuccessRate = 0,
                    QualityRate = 0
                };
            }

            var totalGrades = grades.Count;
            var averageGrade = (double)grades.Average(g => g.Value);
            
            var excellentCount = grades.Count(g => g.Value >= 4.5m);
            var goodCount = grades.Count(g => g.Value >= 3.5m && g.Value < 4.5m);
            var satisfactoryCount = grades.Count(g => g.Value >= 2.5m && g.Value < 3.5m);
            var unsatisfactoryCount = grades.Count(g => g.Value < 2.5m);
            
            var successRate = totalGrades > 0 ? (double)(excellentCount + goodCount + satisfactoryCount) / totalGrades * 100 : 0;
            var qualityRate = totalGrades > 0 ? (double)(excellentCount + goodCount) / totalGrades * 100 : 0;

            return new ViewModels.Education.GradeStatistics
            {
                TotalGrades = totalGrades,
                AverageGrade = averageGrade,
                ExcellentCount = excellentCount,
                GoodCount = goodCount,
                SatisfactoryCount = satisfactoryCount,
                UnsatisfactoryCount = unsatisfactoryCount,
                SuccessRate = successRate,
                QualityRate = qualityRate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grade statistics");
            throw;
        }
    }

    /// <summary>
    /// Получает недавние оценки
    /// </summary>
    public async Task<IEnumerable<Grade>> GetRecentGradesAsync(int days)
    {
        try
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            
            return await _dbContext.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .Include(g => g.Teacher)
                .Where(g => g.IssuedAt >= fromDate)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent grades for {Days} days", days);
            throw;
        }
    }

    /// <summary>
    /// Генерирует аналитический отчет по оценкам
    /// </summary>
    public async Task<object> GenerateAnalyticsReportAsync(
        Guid? courseUid = null,
        Guid? groupUid = null,
        (DateTime? start, DateTime? end)? period = null)
    {
        try
        {
            var statistics = await GetGradeStatisticsAsync(courseUid, groupUid, period);
            
            var query = _dbContext.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .Include(g => g.Student)
                .AsQueryable();

            if (courseUid.HasValue)
                query = query.Where(g => g.Assignment.CourseInstanceUid == courseUid.Value);

            if (groupUid.HasValue)
                query = query.Where(g => g.Student.GroupUid == groupUid.Value);

            if (period.HasValue)
            {
                if (period.Value.start.HasValue)
                    query = query.Where(g => g.IssuedAt >= period.Value.start.Value);
                
                if (period.Value.end.HasValue)
                    query = query.Where(g => g.IssuedAt <= period.Value.end.Value);
            }

            var grades = await query.ToListAsync();

            var topStudents = grades
                .GroupBy(g => new { g.StudentUid, g.Student?.Person?.FirstName, g.Student?.Person?.LastName })
                .Select(g => new
                {
                    StudentUid = g.Key.StudentUid,
                    StudentName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                    AverageGrade = g.Average(x => x.Value),
                    TotalGrades = g.Count()
                })
                .OrderByDescending(s => s.AverageGrade)
                .Take(10)
                .ToList();

            var gradesByMonth = grades
                .GroupBy(g => new { g.IssuedAt.Year, g.IssuedAt.Month })
                .Select(g => new
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    AverageGrade = g.Average(x => x.Value),
                    TotalGrades = g.Count()
                })
                .OrderBy(g => g.Period)
                .ToList();

            return new
            {
                Statistics = statistics,
                TopStudents = topStudents,
                GradesByMonth = gradesByMonth,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics report");
            throw;
        }
    }

    /// <summary>
    /// Получает оценки студента (алиас)
    /// </summary>
    public async Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid)
    {
        return await GetGradesByStudentAsync(studentUid);
    }

    /// <summary>
    /// Получает оценки по курсу (алиас)
    /// </summary>
    public async Task<IEnumerable<Grade>> GetCourseGradesAsync(Guid courseInstanceUid)
    {
        return await GetGradesByCourseAsync(courseInstanceUid);
    }

    /// <summary>
    /// Получает оценки по заданию (алиас)
    /// </summary>
    public async Task<IEnumerable<Grade>> GetAssignmentGradesAsync(Guid assignmentUid)
    {
        return await GetGradesByAssignmentAsync(assignmentUid);
    }

    /// <summary>
    /// Получает оценки курса для студента
    /// </summary>
    public async Task<IEnumerable<Grade>> GetCourseGradesByStudentAsync(Guid courseInstanceUid, Guid studentUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a != null ? a.CourseInstance : null)
                .Include(g => g.Teacher)
                .Where(g => g.Assignment != null && g.Assignment.CourseInstanceUid == courseInstanceUid && g.StudentUid == studentUid)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course grades for student {StudentUid} in course {CourseInstanceUid}", studentUid, courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Определяет диапазон оценки
    /// </summary>
    private static string GetGradeRange(double grade)
    {
        return grade switch
        {
            >= 90 => "Отлично (90-100)",
            >= 80 => "Хорошо (80-89)",
            >= 70 => "Удовлетворительно (70-79)",
            >= 60 => "Зачет (60-69)",
            _ => "Неудовлетворительно (0-59)"
        };
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid)
    {
        try
        {
            var gradesByCourse = await _dbContext.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Where(g => g.StudentUid == studentUid)
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new
                {
                    CourseName = group.Key,
                    AverageGrade = group.Average(g => (double)g.Value),
                    GradeCount = group.Count()
                })
                .ToListAsync();

            return gradesByCourse.Select(item => new GradeStatisticsBySubject
            {
                SubjectName = item.CourseName,
                AverageGrade = item.AverageGrade,
                TotalGrades = item.GradeCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.Assignment.CourseInstanceUid == courseInstanceUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}", studentUid, courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid, Guid groupUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.Assignment.CourseInstanceUid == courseInstanceUid && g.Student.GroupUid == groupUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}, {GroupUid}", studentUid, courseInstanceUid, groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid, Guid groupUid, Guid subjectInstanceUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.Assignment.CourseInstanceUid == courseInstanceUid && g.Student.GroupUid == groupUid && g.Assignment.CourseInstance.SubjectUid == subjectInstanceUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}, {GroupUid}, {SubjectInstanceUid}", studentUid, courseInstanceUid, groupUid, subjectInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid, Guid groupUid, Guid subjectInstanceUid, Guid assignmentUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.AssignmentUid == assignmentUid && g.Assignment.CourseInstanceUid == courseInstanceUid && g.Student.GroupUid == groupUid && g.Assignment.CourseInstance.SubjectUid == subjectInstanceUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}, {GroupUid}, {SubjectInstanceUid}, {AssignmentUid}", studentUid, courseInstanceUid, groupUid, subjectInstanceUid, assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid, Guid groupUid, Guid subjectInstanceUid, Guid assignmentUid, Guid studentGroupUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.AssignmentUid == assignmentUid && g.Assignment.CourseInstanceUid == courseInstanceUid && g.Student.GroupUid == groupUid && g.Assignment.CourseInstance.SubjectUid == subjectInstanceUid && g.Student.GroupUid == studentGroupUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}, {GroupUid}, {SubjectInstanceUid}, {AssignmentUid}, {StudentGroupUid}", studentUid, courseInstanceUid, groupUid, subjectInstanceUid, assignmentUid, studentGroupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid, Guid groupUid, Guid subjectInstanceUid, Guid assignmentUid, Guid studentGroupUid, Guid studentSubjectInstanceUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.AssignmentUid == assignmentUid && g.Assignment.CourseInstanceUid == courseInstanceUid && g.Student.GroupUid == groupUid && g.Assignment.CourseInstance.SubjectUid == subjectInstanceUid && g.Student.GroupUid == studentGroupUid && g.Assignment.CourseInstance.SubjectUid == studentSubjectInstanceUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}, {GroupUid}, {SubjectInstanceUid}, {AssignmentUid}, {StudentGroupUid}, {StudentSubjectInstanceUid}", studentUid, courseInstanceUid, groupUid, subjectInstanceUid, assignmentUid, studentGroupUid, studentSubjectInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid, Guid groupUid, Guid subjectInstanceUid, Guid assignmentUid, Guid studentGroupUid, Guid studentSubjectInstanceUid, Guid studentGroupSubjectInstanceUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.AssignmentUid == assignmentUid && g.Assignment.CourseInstanceUid == courseInstanceUid && g.Student.GroupUid == groupUid && g.Assignment.CourseInstance.SubjectUid == subjectInstanceUid && g.Student.GroupUid == studentGroupUid && g.Assignment.CourseInstance.SubjectUid == studentSubjectInstanceUid && g.Student.GroupUid == studentGroupSubjectInstanceUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}, {GroupUid}, {SubjectInstanceUid}, {AssignmentUid}, {StudentGroupUid}, {StudentSubjectInstanceUid}, {StudentGroupSubjectInstanceUid}", studentUid, courseInstanceUid, groupUid, subjectInstanceUid, assignmentUid, studentGroupUid, studentSubjectInstanceUid, studentGroupSubjectInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок студента по предметам
    /// </summary>
    public async Task<IEnumerable<GradeStatisticsBySubject>> GetStudentGradeStatisticsBySubjectAsync(Guid studentUid, Guid courseInstanceUid, Guid groupUid, Guid subjectInstanceUid, Guid assignmentUid, Guid studentGroupUid, Guid studentSubjectInstanceUid, Guid studentGroupSubjectInstanceUid, Guid studentGroupAssignmentUid)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && g.AssignmentUid == assignmentUid && g.Assignment.CourseInstanceUid == courseInstanceUid && g.Student.GroupUid == groupUid && g.Assignment.CourseInstance.SubjectUid == subjectInstanceUid && g.Student.GroupUid == groupUid && g.Assignment.CourseInstance.SubjectUid == studentSubjectInstanceUid && g.Student.GroupUid == studentGroupSubjectInstanceUid && g.Assignment.CourseInstanceUid == assignmentUid)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                .ToListAsync();

            if (!grades.Any())
            {
                return new List<GradeStatisticsBySubject>();
            }

            var statisticsBySubject = grades
                .GroupBy(g => g.Assignment.CourseInstance.Subject.Name)
                .Select(group => new GradeStatisticsBySubject
                {
                    SubjectName = group.Key,
                    TotalGrades = group.Count(),
                    AverageGrade = (double)group.Average(g => g.Value),
                    HighestGrade = (double)group.Max(g => g.Value),
                    LowestGrade = (double)group.Min(g => g.Value)
                })
                .ToList();

            return statisticsBySubject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student grade statistics by subject: {StudentUid}, {CourseInstanceUid}, {GroupUid}, {SubjectInstanceUid}, {AssignmentUid}, {StudentGroupUid}, {StudentSubjectInstanceUid}, {StudentGroupSubjectInstanceUid}, {StudentGroupAssignmentUid}", studentUid, courseInstanceUid, groupUid, subjectInstanceUid, assignmentUid, studentGroupUid, studentSubjectInstanceUid, studentGroupSubjectInstanceUid, studentGroupAssignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику оценок
    /// </summary>
    public async Task<object> GetStatisticsAsync()
    {
        var totalGrades = await _dbContext.Grades.CountAsync();
        var averageGrade = await _dbContext.Grades.AverageAsync(g => g.Value);
        
        return new
        {
            TotalGrades = totalGrades,
            AverageGrade = averageGrade,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Получает оценку по студенту и заданию
    /// </summary>
    public async Task<Grade?> GetByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid)
    {
        return await _dbContext.Grades
            .FirstOrDefaultAsync(g => g.StudentUid == studentUid && g.AssignmentUid == assignmentUid);
    }

    /// <summary>
    /// Получает оценки с пагинацией и фильтрацией (расширенная версия)
    /// </summary>
    public async Task<(IEnumerable<Grade> grades, int totalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? studentUid = null,
        Guid? courseInstanceUid = null,
        Guid? assignmentUid = null,
        GradeType? gradeType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _dbContext.Grades
            .Include(g => g.Student)
                .ThenInclude(s => s.Person)
            .Include(g => g.Assignment)
            .AsQueryable();

        // Применяем фильтры
        if (studentUid.HasValue)
            query = query.Where(g => g.StudentUid == studentUid.Value);

        if (courseInstanceUid.HasValue)
            query = query.Where(g => g.Assignment != null && g.Assignment.CourseInstanceUid == courseInstanceUid.Value);

        if (assignmentUid.HasValue)
            query = query.Where(g => g.AssignmentUid == assignmentUid.Value);

        if (gradeType.HasValue)
            query = query.Where(g => g.Type == gradeType.Value);

        if (fromDate.HasValue)
            query = query.Where(g => g.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(g => g.CreatedAt <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(g => 
                g.Comment != null && g.Comment.Contains(searchTerm) ||
                g.Student != null && g.Student.Person != null && 
                (g.Student.Person.FirstName + " " + g.Student.Person.LastName).Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var grades = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (grades, totalCount);
    }

    /// <summary>
    /// Получает статистику оценок
    /// </summary>
    public async Task<object> GetStatisticsAsync(Guid? studentUid = null, Guid? courseInstanceUid = null)
    {
        var query = _dbContext.Grades.AsQueryable();

        if (studentUid.HasValue)
            query = query.Where(g => g.StudentUid == studentUid.Value);

        if (courseInstanceUid.HasValue)
            query = query.Where(g => g.Assignment != null && g.Assignment.CourseInstanceUid == courseInstanceUid.Value);

        var totalGrades = await query.CountAsync();
        var averageGrade = await query.AverageAsync(g => (double?)g.Value) ?? 0;
        var maxGrade = await query.MaxAsync(g => (double?)g.Value) ?? 0;
        var minGrade = await query.MinAsync(g => (double?)g.Value) ?? 0;

        var gradeDistribution = await query
            .GroupBy(g => g.Value)
            .Select(g => new { Grade = g.Key, Count = g.Count() })
            .OrderBy(g => g.Grade)
            .ToListAsync();

        return new
        {
            TotalGrades = totalGrades,
            AverageGrade = Math.Round(averageGrade, 2),
            MaxGrade = maxGrade,
            MinGrade = minGrade,
            GradeDistribution = gradeDistribution
        };
    }

    #endregion
}

/// <summary>
/// Статистика оценок студента
/// </summary>
public class StudentGradeStatistics
{
    public Guid StudentUid { get; set; }
    public int TotalGrades { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int PassingGrades { get; set; }
    public int FailingGrades { get; set; }
    public Dictionary<string, double> GradesBySubject { get; set; } = new();
}

/// <summary>
/// Статистика оценок по заданию
/// </summary>
public class AssignmentGradeStatistics
{
    public Guid AssignmentUid { get; set; }
    public int TotalSubmissions { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int PassingSubmissions { get; set; }
    public int FailingSubmissions { get; set; }
    public Dictionary<string, int> GradeDistribution { get; set; } = new();
}

/// <summary>
/// Результат массового создания оценок
/// </summary>
public class BulkGradeResult
{
    public int SuccessfulGrades { get; set; }
    public int FailedGrades { get; set; }
    public List<Guid> CreatedGradeUids { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Статистика оценок по предметам
/// </summary>
public class GradeStatisticsBySubject
{
    public string SubjectName { get; set; } = string.Empty;
    public int TotalGrades { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
}
