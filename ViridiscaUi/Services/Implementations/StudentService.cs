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
            return await _dbContext.Students
                .Include(s => s.Group)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Uid == uid);
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _dbContext.Students
                .Include(s => s.User)
                .Include(s => s.Group)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsAsync()
        {
            return await GetAllStudentsAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid)
        {
            return await _dbContext.Students
                .Include(s => s.Group)
                .Include(s => s.User)
                .Where(s => s.GroupUid == groupUid)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task AddStudentAsync(Student student)
        {
            student.CreatedAt = DateTime.UtcNow;
            student.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.Students.AddAsync(student);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            var existingStudent = await _dbContext.Students.FindAsync(student.Uid);
            if (existingStudent == null)
                return false;

            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.MiddleName = student.MiddleName;
            existingStudent.Email = student.Email;
            existingStudent.Phone = student.Phone;
            existingStudent.Status = student.Status;
            existingStudent.GroupUid = student.GroupUid;
            existingStudent.LastModifiedAt = DateTime.UtcNow;

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

        // === РАСШИРЕНИЯ ЭТАПА 1 ===

        public async Task<StudentPerformance> GetStudentPerformanceAsync(Guid studentUid)
        {
            var student = await _dbContext.Students.FindAsync(studentUid);
            if (student == null)
                throw new ArgumentException($"Student with ID {studentUid} not found");

            // Получаем курсы студента
            var enrollments = await _dbContext.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentUid == studentUid)
                .ToListAsync();

            var totalCourses = enrollments.Count;
            var activeCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Active);
            var completedCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

            // Получаем оценки студента
            var grades = await _dbContext.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .Where(g => g.StudentUid == studentUid)
                .ToListAsync();

            var averageGrade = grades.Any() ? (double)grades.Average(g => g.Value) : 0.0;

            // Получаем задания студента
            var courseUids = enrollments.Select(e => e.CourseUid).ToList();
            var assignments = await _dbContext.Assignments
                .Where(a => courseUids.Contains(a.CourseId))
                .ToListAsync();

            var submissions = await _dbContext.Submissions
                .Where(s => s.StudentId == studentUid)
                .ToListAsync();

            var totalAssignments = assignments.Count;
            var completedAssignments = submissions.Count(s => s.Score != null);
            var pendingAssignments = submissions.Count(s => s.Score == null);
            var overdueAssignments = assignments.Count(a =>
                a.DueDate.HasValue &&
                a.DueDate < DateTime.UtcNow &&
                !submissions.Any(s => s.AssignmentId == a.Uid && s.Score != null));

            // Получаем оценки по предметам
            var gradesBySubject = grades
                .GroupBy(g => g.Assignment.Course.Title)
                .ToDictionary(g => g.Key, g => (double)g.Average(grade => grade.Value));

            // Последняя активность
            var lastActivity = submissions.Any() ? submissions.Max(s => s.SubmissionDate) : (DateTime?)null;

            return new StudentPerformance
            {
                StudentUid = studentUid,
                AverageGrade = averageGrade,
                TotalCourses = totalCourses,
                CompletedCourses = completedCourses,
                ActiveCourses = activeCourses,
                TotalAssignments = totalAssignments,
                CompletedAssignments = completedAssignments,
                PendingAssignments = pendingAssignments,
                OverdueAssignments = overdueAssignments,
                LastActivityDate = lastActivity,
                GradesBySubject = gradesBySubject
            };
        }

        public async Task<bool> TransferStudentToGroupAsync(Guid studentUid, Guid newGroupUid)
        {
            var student = await _dbContext.Students.FindAsync(studentUid);
            var newGroup = await _dbContext.Groups.FindAsync(newGroupUid);

            if (student == null || newGroup == null)
                return false;

            student.GroupUid = newGroupUid;
            student.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignToGroupAsync(Guid studentUid, Guid groupUid)
        {
            // Используем тот же механизм, что и TransferStudentToGroupAsync
            return await TransferStudentToGroupAsync(studentUid, groupUid);
        }

        public async Task<IEnumerable<Course>> GetStudentCoursesAsync(Guid studentUid)
        {
            return await _dbContext.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                .Where(e => e.StudentUid == studentUid)
                .Select(e => e.Course)
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid)
        {
            return await _dbContext.Grades
                .Include(g => g.Assignment)
                .ThenInclude(a => a.Course)
                .Where(g => g.StudentUid == studentUid)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Student> Students, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            var query = _dbContext.Students
                .Include(s => s.Group)
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    s.FirstName.Contains(searchTerm) ||
                    s.LastName.Contains(searchTerm) ||
                    s.Email.Contains(searchTerm) ||
                    (s.Group != null && s.Group.Name.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (students, totalCount);
        }

        public async Task<StudentStatistics> GetStudentStatisticsAsync(Guid studentUid)
        {
            var student = await _dbContext.Students.FindAsync(studentUid);
            if (student == null)
                throw new ArgumentException($"Student with ID {studentUid} not found");

            // Получаем все сдачи студента
            var submissions = await _dbContext.Submissions
                .Include(s => s.Assignment)
                .Where(s => s.StudentId == studentUid)
                .ToListAsync();

            var totalSubmissions = submissions.Count;
            var gradedSubmissions = submissions.Count(s => s.Score != null);
            var averageScore = gradedSubmissions > 0 ?
                submissions.Where(s => s.Score != null).Average(s => s.Score!.Value) : 0.0;

            // Получаем статистику по типам заданий
            var assignmentsByType = submissions
                .GroupBy(s => s.Assignment.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Получаем месячный прогресс
            var monthlyProgress = submissions
                .Where(s => s.Score != null)
                .GroupBy(s => new { s.SubmissionDate.Year, s.SubmissionDate.Month })
                .Select(g => new MonthlyProgress
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AverageGrade = g.Average(s => s.Score!.Value),
                    CompletedAssignments = g.Count(),
                    TotalAssignments = g.Count() // Упрощенная версия
                })
                .OrderBy(mp => mp.Year)
                .ThenBy(mp => mp.Month)
                .ToList();

            // Получаем посещаемость (упрощенная версия)
            var attendanceRecords = await _dbContext.Attendances
                .Where(a => a.StudentUid == studentUid)
                .ToListAsync();

            var attendanceRate = attendanceRecords.Any() ?
                (int)(attendanceRecords.Count(a => a.IsPresent) * 100.0 / attendanceRecords.Count) : 100;

            return new StudentStatistics
            {
                StudentUid = studentUid,
                TotalSubmissions = totalSubmissions,
                GradedSubmissions = gradedSubmissions,
                AverageScore = averageScore,
                TotalStudyTime = TimeSpan.Zero, // Можно добавить отслеживание времени
                AttendanceRate = attendanceRate,
                EnrollmentDate = student.CreatedAt,
                AssignmentsByType = assignmentsByType,
                MonthlyProgress = monthlyProgress
            };
        }

        public async Task<IEnumerable<Assignment>> GetStudentActiveAssignmentsAsync(Guid studentUid)
        {
            var courseUids = await _dbContext.Enrollments
                .Where(e => e.StudentUid == studentUid && e.Status == EnrollmentStatus.Active)
                .Select(e => e.CourseUid)
                .ToListAsync();

            var submittedAssignmentIds = await _dbContext.Submissions
                .Where(s => s.StudentId == studentUid && s.Score != null)
                .Select(s => s.AssignmentId)
                .ToListAsync();

            return await _dbContext.Assignments
                .Include(a => a.Course)
                .Where(a => courseUids.Contains(a.CourseId) &&
                           !submittedAssignmentIds.Contains(a.Uid) &&
                           (!a.DueDate.HasValue || a.DueDate > DateTime.UtcNow))
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Assignment>> GetStudentOverdueAssignmentsAsync(Guid studentUid)
        {
            var courseUids = await _dbContext.Enrollments
                .Where(e => e.StudentUid == studentUid && e.Status == EnrollmentStatus.Active)
                .Select(e => e.CourseUid)
                .ToListAsync();

            var submittedAssignmentIds = await _dbContext.Submissions
                .Where(s => s.StudentId == studentUid && s.Score != null)
                .Select(s => s.AssignmentId)
                .ToListAsync();

            return await _dbContext.Assignments
                .Include(a => a.Course)
                .Where(a => courseUids.Contains(a.CourseId) &&
                           !submittedAssignmentIds.Contains(a.Uid) &&
                           a.DueDate.HasValue && a.DueDate < DateTime.UtcNow)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
    }
} 