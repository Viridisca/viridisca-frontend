using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.System.Enums;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces; 

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с заданиями
/// Наследуется от GenericCrudService для получения универсальных CRUD операций
/// </summary>
public class AssignmentService(ApplicationDbContext dbContext, INotificationService notificationService, ILogger<AssignmentService> logger) : GenericCrudService<Assignment>(dbContext, logger), IAssignmentService
{
    private new readonly ApplicationDbContext _dbContext = dbContext;
    private readonly INotificationService _notificationService = notificationService;
    private new readonly ILogger<AssignmentService> _logger = logger;

    #region Переопределение базовых методов для специфичной логики

    /// <summary>
    /// Применяет специфичный для заданий поиск
    /// </summary>
    protected override IQueryable<Assignment> ApplySearchFilter(IQueryable<Assignment> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.ToLower();

        return query.Where(a => 
            a.Title.ToLower().Contains(lowerSearchTerm) ||
            a.Description.ToLower().Contains(lowerSearchTerm) ||
            a.Instructions.ToLower().Contains(lowerSearchTerm) ||
            (a.CourseInstance != null && a.CourseInstance.Subject != null && a.CourseInstance.Subject.Name.ToLower().Contains(lowerSearchTerm)) ||
            (a.CourseInstance != null && a.CourseInstance.Code.ToLower().Contains(lowerSearchTerm))
        );
    }

    /// <summary>
    /// Валидирует специфичные для задания правила
    /// </summary>
    protected override async Task ValidateEntitySpecificRulesAsync(Assignment entity, List<string> errors, List<string> warnings, bool isCreate)
    {
        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(entity.Title))
            errors.Add("Название задания обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(entity.Description))
            warnings.Add("Рекомендуется добавить описание задания");

        if (string.IsNullOrWhiteSpace(entity.Instructions))
            warnings.Add("Рекомендуется добавить инструкции к заданию");

        if (entity.CourseInstanceUid == Guid.Empty)
            errors.Add("Курс обязателен для задания");

        // Проверка дат
        if (entity.DueDate.HasValue)
        {
            if (entity.DueDate.Value <= DateTime.Now)
                warnings.Add("Срок сдачи задания уже прошел или наступает сегодня");

            if (entity.DueDate.Value > DateTime.Now.AddYears(1))
                warnings.Add("Срок сдачи задания более чем через год");
        }

        if (entity.DueDate.HasValue)
        {
            if (entity.CreatedAt >= entity.DueDate.Value)
                errors.Add("Дата начала должна быть раньше срока сдачи");
        }

        // Проверка максимального балла
        if (entity.MaxScore <= 0)
            errors.Add("Максимальный балл должен быть больше нуля");

        if (entity.MaxScore > 1000)
            warnings.Add("Максимальный балл больше 1000 - это очень много");

        // Проверка существования курса
        var courseExists = await _dbContext.CourseInstances
            .Where(c => c.Uid == entity.CourseInstanceUid)
            .AnyAsync();

        if (!courseExists)
            errors.Add($"Курс с Uid {entity.CourseInstanceUid} не найден");

        // Проверка уникальности названия в рамках курса
        if (!string.IsNullOrWhiteSpace(entity.Title))
        {
            var titleExists = await _dbSet
                .Where(a => a.Uid != entity.Uid && 
                           a.CourseInstanceUid == entity.CourseInstanceUid && 
                           a.Title.ToLower() == entity.Title.ToLower())
                .AnyAsync();

            if (titleExists)
                warnings.Add($"Задание с названием '{entity.Title}' уже существует в этом курсе");
        }

        await base.ValidateEntitySpecificRulesAsync(entity, errors, warnings, isCreate);
    }

    #endregion

    #region Реализация интерфейса IAssignmentService (существующие методы)

    public async Task<Assignment?> GetAssignmentAsync(Guid uid)
    {
        return await GetByUidWithIncludesAsync(uid, 
            a => a.CourseInstance, 
            a => a.Submissions);
    }

    public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
    {
        return await GetAllWithIncludesAsync(a => a.CourseInstance);
    }

    public async Task<IEnumerable<Assignment>> GetAssignmentsAsync()
    {
        return await GetAllAssignmentsAsync();
    }

    public async Task<Assignment> CreateAssignmentAsync(Assignment assignment)
    {
        return await CreateAsync(assignment);
    }

    public async Task AddAssignmentAsync(Assignment assignment)
    {
        await CreateAsync(assignment);
    }

    public async Task<bool> UpdateAssignmentAsync(Assignment assignment)
    {
        return await UpdateAsync(assignment);
    }

    public async Task<bool> DeleteAssignmentAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(Guid courseInstanceUid)
    {
        return await FindWithIncludesAsync(a => a.CourseInstanceUid == courseInstanceUid, a => a.CourseInstance, a => a.Submissions);
    }

    public async Task<IEnumerable<Assignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
    {
        return await FindWithIncludesAsync(a => a.Status == status, a => a.CourseInstance);
    }

    public async Task<IEnumerable<Assignment>> GetAssignmentsByTypeAsync(AssignmentType type)
    {
        return await FindWithIncludesAsync(a => a.Type == type, a => a.CourseInstance);
    }

    #endregion

    #region Новые методы интерфейса

    /// <summary>
    /// Получает задания студента
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByStudentAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .ThenInclude(c => c.Enrollments)
                .Where(a => a.CourseInstance.Enrollments.Any(e => e.StudentUid == studentUid))
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает задания преподавателя
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .Where(a => a.CourseInstance.TeacherUid == teacherUid)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает сдачи задания
    /// </summary>
    public async Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Submissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentUid == assignmentUid)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submissions for assignment {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает сдачи студента
    /// </summary>
    public async Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Submissions
                .Include(s => s.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .Where(s => s.StudentUid == studentUid)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submissions for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Создает сдачу задания
    /// </summary>
    public async Task<Submission> CreateSubmissionAsync(Submission submission)
    {
        try
        {
            submission.Uid = Guid.NewGuid();
            submission.SubmissionDate = DateTime.UtcNow;
            submission.Status = SubmissionStatus.Submitted;

            _dbContext.Submissions.Add(submission);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created submission {SubmissionUid} for assignment {AssignmentUid}", 
                submission.Uid, submission.AssignmentUid);

            return submission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating submission for assignment {AssignmentUid}", submission.AssignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Обновляет сдачу задания
    /// </summary>
    public async Task<bool> UpdateSubmissionAsync(Submission submission)
    {
        try
        {
            _dbContext.Submissions.Update(submission);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated submission {SubmissionUid}", submission.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating submission {SubmissionUid}", submission.Uid);
            return false;
        }
    }

    /// <summary>
    /// Оценивает сдачу задания
    /// </summary>
    public async Task<bool> GradeSubmissionAsync(Guid submissionUid, double score, string? feedback = null, Guid? gradedByUid = null)
    {
        try
        {
            var submission = await _dbContext.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Uid == submissionUid);

            if (submission == null)
            {
                _logger.LogWarning("Submission {SubmissionUid} not found for grading", submissionUid);
                return false;
            }

            submission.Score = score;
            submission.Feedback = feedback;
            submission.GradedDate = DateTime.UtcNow;
            submission.GradedByUid = gradedByUid;
            submission.Status = SubmissionStatus.Graded;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Graded submission {SubmissionUid} with score {Score}", submissionUid, score);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error grading submission {SubmissionUid}", submissionUid);
            return false;
        }
    }

    /// <summary>
    /// Получает статистику задания
    /// </summary>
    public async Task<AssignmentStatistics> GetAssignmentStatisticsAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .ThenInclude(c => c.Enrollments)
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid);

            if (assignment == null)
                throw new ArgumentException($"Assignment {assignmentUid} not found");

            var submissions = await _dbContext.Submissions
                .Where(s => s.AssignmentUid == assignmentUid)
                .ToListAsync();

            var totalStudents = assignment.CourseInstance.Enrollments.Count;
            var submittedCount = submissions.Count;
            var gradedCount = submissions.Count(s => s.Status == SubmissionStatus.Graded);
            var pendingCount = submissions.Count(s => s.Status == SubmissionStatus.Submitted);
            var overdueCount = submissions.Count(s => s.SubmissionDate > assignment.DueDate);

            var averageScore = submissions.Where(s => s.Score.HasValue).Average(s => s.Score) ?? 0;
            var submissionRate = totalStudents > 0 ? (double)submittedCount / totalStudents * 100 : 0;

            var firstSubmission = submissions.OrderBy(s => s.SubmissionDate).FirstOrDefault();
            var lastSubmission = submissions.OrderByDescending(s => s.SubmissionDate).FirstOrDefault();

            var averageSubmissionTime = TimeSpan.Zero;
            if (submissions.Any())
            {
                var submissionTimes = submissions
                    .Where(s => s.SubmissionDate >= assignment.CreatedAt)
                    .Select(s => s.SubmissionDate - assignment.CreatedAt);
                
                if (submissionTimes.Any())
                {
                    averageSubmissionTime = TimeSpan.FromTicks((long)submissionTimes.Average(t => t.Ticks));
                }
            }

            return new AssignmentStatistics
            {
                AssignmentUid = assignmentUid,
                TotalStudents = totalStudents,
                SubmittedCount = submittedCount,
                GradedCount = gradedCount,
                PendingCount = pendingCount,
                OverdueCount = overdueCount,
                AverageScore = averageScore,
                SubmissionRate = submissionRate,
                AverageSubmissionTime = averageSubmissionTime,
                FirstSubmissionDate = firstSubmission?.SubmissionDate,
                LastSubmissionDate = lastSubmission?.SubmissionDate,
                ScoreDistribution = new Dictionary<string, int>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment statistics for {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает прогресс студента по заданию
    /// </summary>
    public async Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentUid, Guid studentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid);

            if (assignment == null)
                throw new ArgumentException($"Assignment {assignmentUid} not found");

            var submission = await _dbContext.Submissions
                .Where(s => s.AssignmentUid == assignmentUid && s.StudentUid == studentUid)
                .OrderByDescending(s => s.SubmissionDate)
                .FirstOrDefaultAsync();

            var status = AssignmentProgressStatus.NotStarted;
            var isOverdue = assignment.DueDate.HasValue && DateTime.UtcNow > assignment.DueDate.Value;

            if (submission != null)
            {
                status = submission.Status switch
                {
                    SubmissionStatus.Submitted => AssignmentProgressStatus.Submitted,
                    SubmissionStatus.Graded => AssignmentProgressStatus.Graded,
                    _ => AssignmentProgressStatus.InProgress
                };

                if (isOverdue && status != AssignmentProgressStatus.Graded)
                    status = AssignmentProgressStatus.Overdue;
            }
            else if (isOverdue)
            {
                status = AssignmentProgressStatus.Overdue;
            }

            var timeRemaining = assignment.DueDate.HasValue 
                ? assignment.DueDate.Value - DateTime.UtcNow 
                : (TimeSpan?)null;

            return new AssignmentProgress
            {
                AssignmentUid = assignmentUid,
                StudentUid = studentUid,
                Status = status,
                SubmissionDate = submission?.SubmissionDate,
                Score = submission?.Score,
                Feedback = submission?.Feedback,
                AttemptsCount = await _dbContext.Submissions
                    .CountAsync(s => s.AssignmentUid == assignmentUid && s.StudentUid == studentUid),
                IsOverdue = isOverdue,
                TimeRemaining = timeRemaining
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment progress for assignment {AssignmentUid} and student {StudentUid}", 
                assignmentUid, studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает задания с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetAssignmentsPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        AssignmentStatus? statusFilter = null,
        Guid? courseFilter = null,
        Guid? teacherFilter = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null)
    {
        try
        {
            var query = _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(a => a.Status == statusFilter.Value);
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(a => a.CourseInstanceUid == courseFilter.Value);
            }

            if (teacherFilter.HasValue)
            {
                query = query.Where(a => a.CourseInstance.TeacherUid == teacherFilter.Value);
            }

            if (dueDateFrom.HasValue)
            {
                query = query.Where(a => !a.DueDate.HasValue || a.DueDate.Value >= dueDateFrom.Value);
            }

            if (dueDateTo.HasValue)
            {
                query = query.Where(a => !a.DueDate.HasValue || a.DueDate.Value <= dueDateTo.Value);
            }

            var totalCount = await query.CountAsync();

            var assignments = await query
                .OrderBy(a => a.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (assignments, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged assignments");
            throw;
        }
    }

    /// <summary>
    /// Получает просроченные задания
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetOverdueAssignmentsAsync()
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .Where(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.UtcNow && a.Status == AssignmentStatus.Published)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue assignments");
            throw;
        }
    }

    /// <summary>
    /// Получает задания, требующие проверки
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsPendingGradingAsync(Guid? teacherUid = null)
    {
        try
        {
            var query = _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .Where(a => _dbContext.Submissions
                    .Any(s => s.AssignmentUid == a.Uid && s.Status == SubmissionStatus.Submitted));

            if (teacherUid.HasValue)
            {
                query = query.Where(a => a.CourseInstance.TeacherUid == teacherUid.Value);
            }

            return await query
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments pending grading");
            throw;
        }
    }

    /// <summary>
    /// Массовое оценивание сдач
    /// </summary>
    public async Task<BulkGradingResult> BulkGradeSubmissionsAsync(IEnumerable<GradingRequest> gradingRequests)
    {
        var result = new BulkGradingResult();

        try
        {
            foreach (var request in gradingRequests)
            {
                try
                {
                    var success = await GradeSubmissionAsync(request.SubmissionUid, request.Score, request.Feedback, request.GradedByUid);
                    if (success)
                    {
                        result.SuccessfulGradings++;
                        result.GradedSubmissionUids.Add(request.SubmissionUid);
                    }
                    else
                    {
                        result.FailedGradings++;
                        result.Errors.Add($"Failed to grade submission {request.SubmissionUid}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedGradings++;
                    result.Errors.Add($"Error grading submission {request.SubmissionUid}: {ex.Message}");
                }
            }

            _logger.LogInformation("Bulk grading completed: {Successful} successful, {Failed} failed", 
                result.SuccessfulGradings, result.FailedGradings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk grading submissions");
            throw;
        }

        return result;
    }

    /// <summary>
    /// Отправляет напоминание о сроке сдачи
    /// </summary>
    public async Task SendDueDateReminderAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .ThenInclude(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentUid} not found for reminder", assignmentUid);
                return;
            }

            var courseInstance = await _dbContext.CourseInstances.FindAsync(assignment.CourseInstanceUid);

            foreach (var enrollment in assignment.CourseInstance.Enrollments)
            {
                await _notificationService.SendNotificationAsync(
                    enrollment.StudentUid,
                    "Напоминание о сроке сдачи",
                    $"Напоминаем о приближающемся сроке сдачи задания '{assignment.Title}' по курсу '{courseInstance?.Name}'. Срок сдачи: {assignment.DueDate:dd.MM.yyyy HH:mm}",
                    NotificationType.Assignment);
            }

            _logger.LogInformation("Sent due date reminders for assignment {AssignmentUid}", assignmentUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending due date reminder for assignment {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает аналитику по заданиям
    /// </summary>
    public async Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid? courseUid = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _dbContext.Assignments.AsQueryable();

            if (courseUid.HasValue)
            {
                query = query.Where(a => a.CourseInstanceUid == courseUid.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= toDate.Value);
            }

            var assignments = await query.ToListAsync();
            var totalAssignments = assignments.Count;
            var publishedAssignments = assignments.Count(a => a.Status == AssignmentStatus.Published);
            var draftAssignments = assignments.Count(a => a.Status == AssignmentStatus.Draft);
            var overdueAssignments = assignments.Count(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.UtcNow);

            var submissions = await _dbContext.Submissions
                .Where(s => assignments.Select(a => a.Uid).Contains(s.AssignmentUid))
                .ToListAsync();

            var averageScore = submissions.Where(s => s.Score.HasValue).Average(s => s.Score) ?? 0;
            var averageSubmissionRate = totalAssignments > 0 ? (double)submissions.Count / totalAssignments : 0;

            return new AssignmentAnalytics
            {
                TotalAssignments = totalAssignments,
                PublishedAssignments = publishedAssignments,
                DraftAssignments = draftAssignments,
                OverdueAssignments = overdueAssignments,
                AverageScore = averageScore,
                AverageSubmissionRate = averageSubmissionRate,
                AverageCompletionTime = TimeSpan.Zero, // Можно добавить расчет
                AssignmentsByType = assignments.GroupBy(a => a.Type.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                ScoresByDifficulty = new Dictionary<string, double>(),
                PerformanceData = new List<AssignmentPerformanceData>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment analytics");
            throw;
        }
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Получает активные задания
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetActiveAssignmentsAsync()
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .Where(a => a.Status == AssignmentStatus.Published && 
                           (!a.DueDate.HasValue || a.DueDate.Value > DateTime.Now))
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active assignments");
            throw;
        }
    }

    /// <summary>
    /// Публикует задание
    /// </summary>
    public async Task<bool> PublishAssignmentAsync(Guid uid)
    {
        try
        {
            var assignment = await GetByUidAsync(uid);
            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentUid} not found for publishing", uid);
                return false;
            }

            assignment.Status = AssignmentStatus.Published;
            assignment.LastModifiedAt = DateTime.UtcNow;

            return await UpdateAsync(assignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing assignment {AssignmentUid}", uid);
            return false;
        }
    }

    #endregion
}