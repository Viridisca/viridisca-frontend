using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using CourseStatus = ViridiscaUi.Domain.Models.Education.CourseStatus;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с курсами
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _dbContext;

        public CourseService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Course?> GetCourseAsync(Guid uid)
        {
            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Modules)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Uid == uid);
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(Guid teacherUid)
        {
            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .Where(c => c.TeacherUid == teacherUid)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task AddCourseAsync(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.Courses.AddAsync(course);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            var existingCourse = await _dbContext.Courses.FindAsync(course.Uid);
            if (existingCourse == null)
                return false;

            existingCourse.Name = course.Name;
            existingCourse.Description = course.Description;
            existingCourse.TeacherUid = course.TeacherUid;
            existingCourse.StartDate = course.StartDate;
            existingCourse.EndDate = course.EndDate;
            existingCourse.Credits = course.Credits;
            existingCourse.Status = course.Status;
            existingCourse.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCourseAsync(Guid uid)
        {
            var course = await _dbContext.Courses.FindAsync(uid);
            if (course == null)
                return false;

            _dbContext.Courses.Remove(course);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishCourseAsync(Guid uid)
        {
            var course = await _dbContext.Courses.FindAsync(uid);
            if (course == null)
                return false;

            course.Status = CourseStatus.Published;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveCourseAsync(Guid uid)
        {
            var course = await _dbContext.Courses.FindAsync(uid);
            if (course == null)
                return false;

            course.Status = CourseStatus.Archived;
            course.LastModifiedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // === РАСШИРЕНИЯ ЭТАПА 2 ===

        public async Task<IEnumerable<Course>> GetCoursesByStudentAsync(Guid studentUid)
        {
            return await _dbContext.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                .Where(e => e.StudentUid == studentUid)
                .Select(e => e.Course)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> EnrollStudentAsync(Guid courseUid, Guid studentUid)
        {
            // Проверяем, не зарегистрирован ли уже студент
            var existingEnrollment = await _dbContext.Enrollments
                .FirstOrDefaultAsync(e => e.CourseUid == courseUid && e.StudentUid == studentUid);
            
            if (existingEnrollment != null)
                return false;

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
            return true;
        }

        public async Task<bool> UnenrollStudentAsync(Guid courseUid, Guid studentUid)
        {
            var enrollment = await _dbContext.Enrollments
                .FirstOrDefaultAsync(e => e.CourseUid == courseUid && e.StudentUid == studentUid);
            
            if (enrollment == null)
                return false;

            _dbContext.Enrollments.Remove(enrollment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<CourseProgress> GetCourseProgressAsync(Guid courseUid, Guid studentUid)
        {
            var enrollment = await _dbContext.Enrollments
                .FirstOrDefaultAsync(e => e.CourseUid == courseUid && e.StudentUid == studentUid);

            if (enrollment == null)
                throw new ArgumentException("Student is not enrolled in this course");

            // Получаем все уроки курса
            var totalLessons = await _dbContext.Lessons
                .Where(l => l.Module.CourseUid == courseUid)
                .CountAsync();

            // Получаем выполненные уроки студента
            var completedLessons = await _dbContext.LessonProgress
                .Where(lp => lp.StudentUid == studentUid && lp.Lesson.Module.CourseUid == courseUid && lp.IsCompleted)
                .CountAsync();

            // Получаем все задания курса
            var totalAssignments = await _dbContext.Assignments
                .Where(a => a.Course.Uid == courseUid)
                .CountAsync();

            // Получаем выполненные задания студента
            var completedAssignments = await _dbContext.Submissions
                .Where(s => s.StudentId == studentUid && s.Assignment.Course.Uid == courseUid && s.Score != null)
                .CountAsync();

            // Вычисляем средний балл
            var averageGrade = await _dbContext.Submissions
                .Where(s => s.StudentId == studentUid && s.Assignment.Course.Uid == courseUid && s.Score != null)
                .AverageAsync(s => s.Score) ?? 0;

            // Получаем последнюю активность
            var lastActivity = await _dbContext.Submissions
                .Where(s => s.StudentId == studentUid && s.Assignment.Course.Uid == courseUid)
                .OrderByDescending(s => s.SubmissionDate)
                .Select(s => s.SubmissionDate)
                .FirstOrDefaultAsync();

            var completionPercentage = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;

            return new CourseProgress
            {
                CourseUid = courseUid,
                StudentUid = studentUid,
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons,
                CompletedAssignments = completedAssignments,
                TotalAssignments = totalAssignments,
                AverageGrade = averageGrade,
                CompletionPercentage = completionPercentage,
                LastActivityDate = lastActivity == DateTime.MinValue ? null : lastActivity,
                EnrolledAt = enrollment.EnrollmentDate,
                TotalTimeSpent = TimeSpan.Zero // TODO: реализовать отслеживание времени
            };
        }

        public async Task<CourseStatistics> GetCourseStatisticsAsync(Guid courseUid)
        {
            var course = await _dbContext.Courses.FindAsync(courseUid);
            if (course == null)
                throw new ArgumentException($"Course with ID {courseUid} not found");

            var enrollments = await _dbContext.Enrollments
                .Where(e => e.CourseUid == courseUid)
                .ToListAsync();

            var totalStudents = enrollments.Count;
            var activeStudents = enrollments.Count(e => e.Status == EnrollmentStatus.Active);

            // Получаем прогресс всех студентов
            var progressList = new List<CourseProgress>();
            foreach (var enrollment in enrollments)
            {
                try
                {
                    var progress = await GetCourseProgressAsync(courseUid, enrollment.StudentUid);
                    progressList.Add(progress);
                }
                catch
                {
                    // Игнорируем ошибки отдельных студентов
                }
            }

            var completedStudents = progressList.Count(p => p.CompletionPercentage >= 100);
            var averageGrade = progressList.Any() ? progressList.Average(p => p.AverageGrade) : 0;
            var averageCompletionRate = progressList.Any() ? progressList.Average(p => p.CompletionPercentage) : 0;

            var totalLessons = await _dbContext.Lessons
                .Where(l => l.Module.CourseUid == courseUid)
                .CountAsync();

            var totalAssignments = await _dbContext.Assignments
                .Where(a => a.Course.Uid == courseUid)
                .CountAsync();

            var lastActivity = await _dbContext.Submissions
                .Where(s => s.Assignment.Course.Uid == courseUid)
                .OrderByDescending(s => s.SubmissionDate)
                .Select(s => s.SubmissionDate)
                .FirstOrDefaultAsync();

            // Распределение оценок
            var gradeDistribution = new Dictionary<string, int>
            {
                { "Отлично (90-100)", 0 },
                { "Хорошо (70-89)", 0 },
                { "Удовлетворительно (50-69)", 0 },
                { "Неудовлетворительно (0-49)", 0 }
            };

            foreach (var progress in progressList.Where(p => p.AverageGrade > 0))
            {
                if (progress.AverageGrade >= 90) gradeDistribution["Отлично (90-100)"]++;
                else if (progress.AverageGrade >= 70) gradeDistribution["Хорошо (70-89)"]++;
                else if (progress.AverageGrade >= 50) gradeDistribution["Удовлетворительно (50-69)"]++;
                else gradeDistribution["Неудовлетворительно (0-49)"]++;
            }

            return new CourseStatistics
            {
                CourseUid = courseUid,
                TotalStudents = totalStudents,
                ActiveStudents = activeStudents,
                CompletedStudents = completedStudents,
                AverageGrade = averageGrade,
                AverageCompletionRate = averageCompletionRate,
                TotalLessons = totalLessons,
                TotalAssignments = totalAssignments,
                LastActivityDate = lastActivity == DateTime.MinValue ? null : lastActivity,
                AverageTimeToComplete = TimeSpan.Zero, // TODO: реализовать отслеживание времени
                GradeDistribution = gradeDistribution
            };
        }

        public async Task<(IEnumerable<Course> Courses, int TotalCount)> GetCoursesPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null,
            CourseStatus? statusFilter = null,
            Guid? teacherFilter = null)
        {
            var query = _dbContext.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || 
                                       (c.Description != null && c.Description.Contains(searchTerm)));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(c => c.Status == statusFilter.Value);
            }

            if (teacherFilter.HasValue)
            {
                query = query.Where(c => c.TeacherUid == teacherFilter.Value);
            }

            var totalCount = await query.CountAsync();
            
            var courses = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (courses, totalCount);
        }

        public async Task<IEnumerable<Course>> GetAvailableCoursesForStudentAsync(Guid studentUid)
        {
            var enrolledCourseUids = await _dbContext.Enrollments
                .Where(e => e.StudentUid == studentUid)
                .Select(e => e.CourseUid)
                .ToListAsync();

            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .Where(c => c.Status == CourseStatus.Published && !enrolledCourseUids.Contains(c.Uid))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<BulkEnrollmentResult> BulkEnrollGroupAsync(Guid courseUid, Guid groupUid)
        {
            var result = new BulkEnrollmentResult();

            var students = await _dbContext.Students
                .Where(s => s.GroupUid == groupUid)
                .ToListAsync();

            foreach (var student in students)
            {
                try
                {
                    var success = await EnrollStudentAsync(courseUid, student.Uid);
                    if (success)
                    {
                        result.SuccessfulEnrollments++;
                        result.EnrolledStudentUids.Add(student.Uid);
                    }
                    else
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add($"Студент {student.FirstName} {student.LastName} уже зарегистрирован на курс");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedEnrollments++;
                    result.Errors.Add($"Ошибка регистрации студента {student.FirstName} {student.LastName}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task<IEnumerable<Course>> GetRecommendedCoursesAsync(Guid studentUid)
        {
            // Простая логика рекомендаций: курсы той же специальности
            var student = await _dbContext.Students
                .Include(s => s.Group)
                .FirstOrDefaultAsync(s => s.Uid == studentUid);

            if (student?.Group == null)
                return new List<Course>();

            var enrolledCourseUids = await _dbContext.Enrollments
                .Where(e => e.StudentUid == studentUid)
                .Select(e => e.CourseUid)
                .ToListAsync();

            // TODO: Добавить логику по специальности, когда будет реализована модель Speciality
            return await _dbContext.Courses
                .Include(c => c.Teacher)
                .Where(c => c.Status == CourseStatus.Published && !enrolledCourseUids.Contains(c.Uid))
                .OrderBy(c => c.Name)
                .Take(5)
                .ToListAsync();
        }
    }
} 