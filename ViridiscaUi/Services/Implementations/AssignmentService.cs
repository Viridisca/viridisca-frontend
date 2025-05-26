using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using NotificationType = ViridiscaUi.Domain.Models.System.NotificationType;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с заданиями
    /// </summary>
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly INotificationService _notificationService;

        public AssignmentService(ApplicationDbContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task<Assignment?> GetAssignmentAsync(Guid uid)
        {
            return await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Uid == uid);
        }

        public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
        {
            return await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(Guid courseUid)
        {
            return await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Include(a => a.Submissions)
                .Where(a => a.Course.Uid == courseUid)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByModuleAsync(Guid moduleUid)
        {
            return await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Include(a => a.Submissions)
                .Where(a => a.ModuleId == moduleUid)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task AddAssignmentAsync(Assignment assignment)
        {
            assignment.CreatedAt = DateTime.UtcNow;
            assignment.LastModifiedAt = DateTime.UtcNow;
            assignment.Status = AssignmentStatus.Draft;
            
            await _dbContext.Assignments.AddAsync(assignment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateAssignmentAsync(Assignment assignment)
        {
            var existingAssignment = await _dbContext.Assignments.FindAsync(assignment.Uid);
            if (existingAssignment == null)
                return false;

            existingAssignment.Title = assignment.Title;
            existingAssignment.Description = assignment.Description;
            existingAssignment.Instructions = assignment.Instructions;
            existingAssignment.DueDate = assignment.DueDate;
            existingAssignment.MaxScore = assignment.MaxScore;
            existingAssignment.Type = assignment.Type;
            existingAssignment.Difficulty = assignment.Difficulty;
            existingAssignment.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAssignmentAsync(Guid uid)
        {
            var assignment = await _dbContext.Assignments.FindAsync(uid);
            if (assignment == null)
                return false;

            _dbContext.Assignments.Remove(assignment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishAssignmentAsync(Guid uid)
        {
            var assignment = await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Enrollments)
                .FirstOrDefaultAsync(a => a.Uid == uid);
            
            if (assignment == null)
                return false;

            assignment.Status = AssignmentStatus.Published;
            assignment.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();

            // Уведомляем всех студентов курса о новом задании
            var studentUids = assignment.Course.Enrollments.Select(e => e.StudentUid).ToList();
            await _notificationService.SendBulkNotificationAsync(
                studentUids,
                "Новое задание",
                $"Опубликовано новое задание '{assignment.Title}' по курсу '{assignment.Course.Name}'. Срок сдачи: {assignment.DueDate:dd.MM.yyyy HH:mm}",
                NotificationType.Info);

            return true;
        }

        // === РАСШИРЕНИЯ ЭТАПА 3 ===

        public async Task<IEnumerable<Assignment>> GetAssignmentsByStudentAsync(Guid studentUid)
        {
            var enrolledCourseUids = await _dbContext.Enrollments
                .Where(e => e.StudentUid == studentUid)
                .Select(e => e.CourseUid)
                .ToListAsync();

            return await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Include(a => a.Submissions.Where(s => s.StudentId == studentUid))
                .Where(a => enrolledCourseUids.Contains(a.Course.Uid) && a.Status == AssignmentStatus.Published)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherUid)
        {
            return await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Include(a => a.Submissions)
                .Where(a => a.Course.TeacherUid == teacherUid)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid)
        {
            return await _dbContext.Submissions
                .Include(s => s.Student)
                .ThenInclude(st => st.User)
                .Include(s => s.Assignment)
                .Where(s => s.AssignmentId == assignmentUid)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(Guid studentUid)
        {
            return await _dbContext.Submissions
                .Include(s => s.Assignment)
                .ThenInclude(a => a.Course)
                .Where(s => s.StudentId == studentUid)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }

        public async Task<Submission> CreateSubmissionAsync(Submission submission)
        {
            submission.SubmissionDate = DateTime.UtcNow;
            submission.CreatedAt = DateTime.UtcNow;
            submission.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.Submissions.AddAsync(submission);
            await _dbContext.SaveChangesAsync();

            // Уведомляем преподавателя о новой сдаче
            var assignment = await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(a => a.Uid == submission.AssignmentId);

            if (assignment?.Course.TeacherUid != null)
            {
                var student = await _dbContext.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Uid == submission.StudentId);

                await _notificationService.CreateNotificationAsync(
                    assignment.Course.TeacherUid.Value,
                    "Новая сдача задания",
                    $"Студент {student?.FirstName} {student?.LastName} сдал задание '{assignment.Title}'",
                    NotificationType.Info);
            }

            return submission;
        }

        public async Task<bool> UpdateSubmissionAsync(Submission submission)
        {
            var existingSubmission = await _dbContext.Submissions.FindAsync(submission.Uid);
            if (existingSubmission == null)
                return false;

            existingSubmission.Content = submission.Content;
            existingSubmission.FilePath = submission.FilePath;
            existingSubmission.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GradeSubmissionAsync(Guid submissionUid, double score, string? feedback = null, Guid? gradedByUid = null)
        {
            var submission = await _dbContext.Submissions
                .Include(s => s.Student)
                .ThenInclude(st => st.User)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Uid == submissionUid);
            
            if (submission == null)
                return false;

            submission.Score = score;
            submission.Feedback = feedback;
            submission.GradedDate = DateTime.UtcNow;
            submission.GradedByUid = gradedByUid;
            submission.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();

            // Уведомляем студента об оценке
            await _notificationService.CreateNotificationAsync(
                submission.Student.UserUid,
                "Задание оценено",
                $"Ваше задание '{submission.Assignment.Title}' оценено. Балл: {score}/{submission.Assignment.MaxScore}",
                NotificationType.Info);

            return true;
        }

        public async Task<AssignmentStatistics> GetAssignmentStatisticsAsync(Guid assignmentUid)
        {
            var assignment = await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Enrollments)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid);

            if (assignment == null)
                throw new ArgumentException($"Assignment with ID {assignmentUid} not found");

            var totalStudents = assignment.Course.Enrollments.Count;
            var submissions = assignment.Submissions.ToList();
            var submittedCount = submissions.Count;
            var gradedCount = submissions.Count(s => s.Score.HasValue);
            var pendingCount = submittedCount - gradedCount;
            var overdueCount = submissions.Count(s => s.SubmissionDate > assignment.DueDate);

            var averageScore = submissions.Where(s => s.Score.HasValue).Average(s => s.Score) ?? 0;
            var submissionRate = totalStudents > 0 ? (double)submittedCount / totalStudents * 100 : 0;

            var averageSubmissionTime = TimeSpan.Zero;
            if (submissions.Any())
            {
                var submissionTimes = submissions
                    .Where(s => s.SubmissionDate <= assignment.DueDate)
                    .Select(s => assignment.DueDate - s.SubmissionDate)
                    .Where(t => t.HasValue && t.Value.TotalDays >= 0)
                    .Select(t => t.Value);
                
                if (submissionTimes.Any())
                {
                    averageSubmissionTime = TimeSpan.FromTicks((long)submissionTimes.Average(t => t.Ticks));
                }
            }

            var scoreDistribution = new Dictionary<string, int>
            {
                { "Отлично (90-100)", 0 },
                { "Хорошо (70-89)", 0 },
                { "Удовлетворительно (50-69)", 0 },
                { "Неудовлетворительно (0-49)", 0 }
            };

            foreach (var submission in submissions.Where(s => s.Score.HasValue))
            {
                var percentage = (submission.Score.Value / assignment.MaxScore) * 100;
                if (percentage >= 90) scoreDistribution["Отлично (90-100)"]++;
                else if (percentage >= 70) scoreDistribution["Хорошо (70-89)"]++;
                else if (percentage >= 50) scoreDistribution["Удовлетворительно (50-69)"]++;
                else scoreDistribution["Неудовлетворительно (0-49)"]++;
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
                FirstSubmissionDate = submissions.Any() ? submissions.Min(s => s.SubmissionDate) : null,
                LastSubmissionDate = submissions.Any() ? submissions.Max(s => s.SubmissionDate) : null,
                ScoreDistribution = scoreDistribution
            };
        }

        public async Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentUid, Guid studentUid)
        {
            var assignment = await _dbContext.Assignments.FindAsync(assignmentUid);
            if (assignment == null)
                throw new ArgumentException($"Assignment with ID {assignmentUid} not found");

            var submissions = await _dbContext.Submissions
                .Where(s => s.AssignmentId == assignmentUid && s.StudentId == studentUid)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();

            var latestSubmission = submissions.FirstOrDefault();
            var isOverdue = DateTime.UtcNow > assignment.DueDate && latestSubmission == null;
            var timeRemaining = assignment.DueDate > DateTime.UtcNow ? assignment.DueDate - DateTime.UtcNow : null;

            var status = AssignmentProgressStatus.NotStarted;
            if (latestSubmission != null)
            {
                if (latestSubmission.Score.HasValue)
                    status = AssignmentProgressStatus.Graded;
                else
                    status = AssignmentProgressStatus.Submitted;
            }
            else if (isOverdue)
            {
                status = AssignmentProgressStatus.Overdue;
            }

            return new AssignmentProgress
            {
                AssignmentUid = assignmentUid,
                StudentUid = studentUid,
                Status = status,
                SubmissionDate = latestSubmission?.SubmissionDate,
                Score = latestSubmission?.Score,
                Feedback = latestSubmission?.Feedback,
                TimeSpent = TimeSpan.Zero, // TODO: реализовать отслеживание времени
                AttemptsCount = submissions.Count,
                IsOverdue = isOverdue,
                TimeRemaining = timeRemaining
            };
        }

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
            var query = _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.Title.Contains(searchTerm) || 
                                       (a.Description != null && a.Description.Contains(searchTerm)));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(a => a.Status == statusFilter.Value);
            }

            if (courseFilter.HasValue)
            {
                query = query.Where(a => a.Course.Uid == courseFilter.Value);
            }

            if (teacherFilter.HasValue)
            {
                query = query.Where(a => a.Course.TeacherUid == teacherFilter.Value);
            }

            if (dueDateFrom.HasValue)
            {
                query = query.Where(a => a.DueDate >= dueDateFrom.Value);
            }

            if (dueDateTo.HasValue)
            {
                query = query.Where(a => a.DueDate <= dueDateTo.Value);
            }

            var totalCount = await query.CountAsync();
            
            var assignments = await query
                .OrderBy(a => a.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (assignments, totalCount);
        }

        public async Task<IEnumerable<Assignment>> GetOverdueAssignmentsAsync()
        {
            return await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Where(a => a.Status == AssignmentStatus.Published && a.DueDate < DateTime.UtcNow)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetAssignmentsPendingGradingAsync(Guid? teacherUid = null)
        {
            var query = _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Include(a => a.Submissions)
                .Where(a => a.Submissions.Any(s => !s.Score.HasValue));

            if (teacherUid.HasValue)
            {
                query = query.Where(a => a.Course.TeacherUid == teacherUid.Value);
            }

            return await query
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<BulkGradingResult> BulkGradeSubmissionsAsync(IEnumerable<GradingRequest> gradingRequests)
        {
            var result = new BulkGradingResult();

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
                        result.Errors.Add($"Не удалось оценить сдачу {request.SubmissionUid}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedGradings++;
                    result.Errors.Add($"Ошибка оценивания сдачи {request.SubmissionUid}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task SendDueDateReminderAsync(Guid assignmentUid)
        {
            var assignment = await _dbContext.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid);

            if (assignment == null)
                return;

            var submittedStudentIds = await _dbContext.Submissions
                .Where(s => s.AssignmentId == assignmentUid)
                .Select(s => s.StudentId)
                .ToListAsync();

            var studentsToRemind = assignment.Course.Enrollments
                .Where(e => !submittedStudentIds.Contains(e.StudentUid))
                .Select(e => e.StudentUid)
                .ToList();

            if (studentsToRemind.Any())
            {
                await _notificationService.SendBulkNotificationAsync(
                    studentsToRemind,
                    "Напоминание о сроке сдачи",
                    $"Напоминаем о приближающемся сроке сдачи задания '{assignment.Title}'. Срок: {assignment.DueDate:dd.MM.yyyy HH:mm}",
                    NotificationType.Warning);
            }
        }

        public async Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid? courseUid = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.Assignments
                .Include(a => a.Course)
                .Include(a => a.Submissions)
                .AsQueryable();

            if (courseUid.HasValue)
            {
                query = query.Where(a => a.Course.Uid == courseUid.Value);
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
            var overdueAssignments = assignments.Count(a => a.Status == AssignmentStatus.Published && a.DueDate < DateTime.UtcNow);

            var allSubmissions = assignments.SelectMany(a => a.Submissions).ToList();
            var averageScore = allSubmissions.Where(s => s.Score.HasValue).Average(s => s.Score) ?? 0;

            var submissionRates = assignments.Select(a => 
            {
                var totalStudents = a.Course.Enrollments?.Count ?? 0;
                var submittedCount = a.Submissions.Count;
                return totalStudents > 0 ? (double)submittedCount / totalStudents * 100 : 0;
            });
            var averageSubmissionRate = submissionRates.Any() ? submissionRates.Average() : 0;

            var performanceData = assignments.Select(a => new AssignmentPerformanceData
            {
                AssignmentUid = a.Uid,
                AssignmentTitle = a.Title,
                AverageScore = a.Submissions.Where(s => s.Score.HasValue).Average(s => s.Score) ?? 0,
                SubmissionRate = (a.Course.Enrollments?.Count ?? 0) > 0 ? 
                    (double)a.Submissions.Count / (a.Course.Enrollments?.Count ?? 1) * 100 : 0,
                AverageCompletionTime = TimeSpan.Zero, // TODO: реализовать отслеживание времени
                TotalSubmissions = a.Submissions.Count
            }).ToList();

            return new AssignmentAnalytics
            {
                TotalAssignments = totalAssignments,
                PublishedAssignments = publishedAssignments,
                DraftAssignments = draftAssignments,
                OverdueAssignments = overdueAssignments,
                AverageScore = averageScore,
                AverageSubmissionRate = averageSubmissionRate,
                AverageCompletionTime = TimeSpan.Zero, // TODO: реализовать отслеживание времени
                AssignmentsByType = new Dictionary<string, int>(), // TODO: добавить типы заданий
                ScoresByDifficulty = new Dictionary<string, double>(), // TODO: добавить сложность заданий
                PerformanceData = performanceData
            };
        }
    }
} 