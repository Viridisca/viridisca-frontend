using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Сервис для работы с оценками
    /// </summary>
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;

        public GradeService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает оценку по идентификатору
        /// </summary>
        public async Task<Grade?> GetGradeAsync(Guid uid)
        {
            return await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(g => g.Uid == uid);
        }

        /// <summary>
        /// Получает все оценки
        /// </summary>
        public async Task<IEnumerable<Grade>> GetAllGradesAsync(
            Guid? courseUid = null,
            Guid? groupUid = null,
            (decimal? min, decimal? max)? gradeRange = null,
            (DateTime? start, DateTime? end)? period = null)
        {
            var query = _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .AsQueryable();

            if (courseUid.HasValue)
                query = query.Where(g => g.Assignment.CourseUid == courseUid.Value);

            if (groupUid.HasValue)
                query = query.Where(g => g.Student.GroupUid == groupUid.Value);

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
                    query = query.Where(g => g.GradedAt >= period.Value.start.Value);
                if (period.Value.end.HasValue)
                    query = query.Where(g => g.GradedAt <= period.Value.end.Value);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получает оценки с пагинацией
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
            var query = _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(g => 
                    g.Student.FirstName.Contains(searchTerm) ||
                    g.Student.LastName.Contains(searchTerm) ||
                    g.Assignment.Title.Contains(searchTerm) ||
                    g.Assignment.Course.Name.Contains(searchTerm));
            }

            if (courseUid.HasValue)
                query = query.Where(g => g.Assignment.CourseUid == courseUid.Value);

            if (groupUid.HasValue)
                query = query.Where(g => g.Student.GroupUid == groupUid.Value);

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
                    query = query.Where(g => g.GradedAt >= period.Value.start.Value);
                if (period.Value.end.HasValue)
                    query = query.Where(g => g.GradedAt <= period.Value.end.Value);
            }

            var totalCount = await query.CountAsync();
            var grades = await query
                .OrderByDescending(g => g.GradedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (grades, totalCount);
        }

        /// <summary>
        /// Добавляет новую оценку
        /// </summary>
        public async Task AddGradeAsync(Grade grade)
        {
            grade.Uid = Guid.NewGuid();
            grade.GradedAt = DateTime.UtcNow;
            
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Обновляет существующую оценку
        /// </summary>
        public async Task<bool> UpdateGradeAsync(Grade grade)
        {
            var existingGrade = await _context.Grades.FindAsync(grade.Uid);
            if (existingGrade == null)
                return false;

            existingGrade.Value = grade.Value;
            existingGrade.Comment = grade.Comment;
            existingGrade.GradedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Удаляет оценку
        /// </summary>
        public async Task<bool> DeleteGradeAsync(Guid uid)
        {
            var grade = await _context.Grades.FindAsync(uid);
            if (grade == null)
                return false;

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Обновляет комментарий к оценке
        /// </summary>
        public async Task<bool> UpdateGradeCommentAsync(Guid gradeUid, string comment)
        {
            var grade = await _context.Grades.FindAsync(gradeUid);
            if (grade == null)
                return false;

            grade.Comment = comment;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Массовое добавление оценок
        /// </summary>
        public async Task<bool> BulkAddGradesAsync(IEnumerable<Grade> grades)
        {
            try
            {
                foreach (var grade in grades)
                {
                    grade.Uid = Guid.NewGuid();
                    grade.GradedAt = DateTime.UtcNow;
                }

                _context.Grades.AddRange(grades);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получает статистику оценок
        /// </summary>
        public async Task<GradeStatistics> GetGradeStatisticsAsync(
            Guid? courseUid = null,
            Guid? groupUid = null,
            (DateTime? start, DateTime? end)? period = null)
        {
            var query = _context.Grades.AsQueryable();

            if (courseUid.HasValue)
                query = query.Where(g => g.Assignment.CourseUid == courseUid.Value);

            if (groupUid.HasValue)
                query = query.Where(g => g.Student.GroupUid == groupUid.Value);

            if (period.HasValue)
            {
                if (period.Value.start.HasValue)
                    query = query.Where(g => g.GradedAt >= period.Value.start.Value);
                if (period.Value.end.HasValue)
                    query = query.Where(g => g.GradedAt <= period.Value.end.Value);
            }

            var grades = await query.ToListAsync();
            
            return new GradeStatistics
            {
                TotalGrades = grades.Count,
                AverageGrade = grades.Any() ? grades.Average(g => (double)g.Value) : 0,
                ExcellentCount = grades.Count(g => g.Value >= 4.5m),
                GoodCount = grades.Count(g => g.Value >= 3.5m && g.Value < 4.5m),
                SatisfactoryCount = grades.Count(g => g.Value >= 2.5m && g.Value < 3.5m),
                UnsatisfactoryCount = grades.Count(g => g.Value < 2.5m)
            };
        }

        /// <summary>
        /// Получает недавние оценки
        /// </summary>
        public async Task<IEnumerable<Grade>> GetRecentGradesAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            return await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .Where(g => g.GradedAt >= cutoffDate)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Генерирует аналитический отчет по оценкам
        /// </summary>
        public async Task<object> GenerateAnalyticsReportAsync(
            Guid? courseUid = null,
            Guid? groupUid = null,
            (DateTime? start, DateTime? end)? period = null)
        {
            var statistics = await GetGradeStatisticsAsync(courseUid, groupUid, period);
            var recentGrades = await GetRecentGradesAsync(30);

            return new
            {
                Statistics = statistics,
                RecentGrades = recentGrades.Take(10),
                GeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Получает оценки студента
        /// </summary>
        public async Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid)
        {
            return await _context.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.Course)
                .Where(g => g.StudentUid == studentUid)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Получает оценки по курсу
        /// </summary>
        public async Task<IEnumerable<Grade>> GetCourseGradesAsync(Guid courseUid)
        {
            return await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Assignment)
                .Where(g => g.Assignment.CourseUid == courseUid)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Получает оценки по заданию
        /// </summary>
        public async Task<IEnumerable<Grade>> GetAssignmentGradesAsync(Guid assignmentUid)
        {
            return await _context.Grades
                .Include(g => g.Student)
                .Where(g => g.AssignmentUid == assignmentUid)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();
        }
    }
} 