using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с преподавателями
/// </summary>
public class TeacherService(ApplicationDbContext dbContext) : ITeacherService
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Teacher?> GetTeacherAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Uid == uid);
        }
        catch
        {
            // Заглушка при ошибке базы данных
            await Task.Delay(100);
            return GenerateSampleTeachers().FirstOrDefault(t => t.Uid == uid);
        }
    }

    public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToListAsync();
        }
        catch
        {
            // Заглушка при ошибке базы данных
            await Task.Delay(100);
            return GenerateSampleTeachers();
        }
    }

    public async Task<IEnumerable<Teacher>> GetTeachersAsync()
    {
        return await GetAllTeachersAsync();
    }

    public async Task<Teacher> CreateTeacherAsync(Teacher teacher)
    {
        teacher.Uid = Guid.NewGuid();
        teacher.CreatedAt = DateTime.UtcNow;
        teacher.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.Teachers.AddAsync(teacher);
        await _dbContext.SaveChangesAsync();

        return teacher;
    }

    public async Task AddTeacherAsync(Teacher teacher)
    {
        teacher.CreatedAt = DateTime.UtcNow;
        teacher.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.Teachers.AddAsync(teacher);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> UpdateTeacherAsync(Teacher teacher)
    {
        var existingTeacher = await _dbContext.Teachers.FindAsync(teacher.Uid);
        if (existingTeacher == null)
            return false;

        existingTeacher.FirstName = teacher.FirstName;
        existingTeacher.LastName = teacher.LastName;
        existingTeacher.MiddleName = teacher.MiddleName;
        existingTeacher.Specialization = teacher.Specialization;
        existingTeacher.AcademicTitle = teacher.AcademicTitle;
        existingTeacher.AcademicDegree = teacher.AcademicDegree;
        existingTeacher.HourlyRate = teacher.HourlyRate;
        existingTeacher.Bio = teacher.Bio;
        existingTeacher.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTeacherAsync(Guid uid)
    {
        var teacher = await _dbContext.Teachers.FindAsync(uid);
        if (teacher == null)
            return false;

        _dbContext.Teachers.Remove(teacher);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid)
    {
        var teacher = await _dbContext.Teachers.FindAsync(teacherUid);
        var course = await _dbContext.Courses.FindAsync(courseUid);

        if (teacher == null || course == null)
            return false;

        course.TeacherUid = teacherUid;
        course.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Получает преподавателей с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetTeachersPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? specializationFilter = null,
        string? statusFilter = null)
    {
        try
        {
            var query = _dbContext.Teachers
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    t.FirstName.Contains(searchTerm) ||
                    t.LastName.Contains(searchTerm) ||
                    t.Email.Contains(searchTerm) ||
                    (t.Specialization != null && t.Specialization.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(specializationFilter))
            {
                query = query.Where(t => t.Specialization == specializationFilter);
            }

            var totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teachers, totalCount);
        }
        catch
        {
            // Заглушка при ошибке базы данных
            await Task.Delay(100);
            var sampleTeachers = GenerateSampleTeachers().ToList();
            
            // Применяем фильтры к тестовым данным
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sampleTeachers = sampleTeachers.Where(t =>
                    t.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (t.Specialization?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(specializationFilter))
            {
                sampleTeachers = sampleTeachers.Where(t => t.Specialization == specializationFilter).ToList();
            }

            var totalCount = sampleTeachers.Count;
            var pagedTeachers = sampleTeachers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedTeachers, totalCount);
        }
    }

    /// <summary>
    /// Получает статистику преподавателя
    /// </summary>
    public async Task<TeacherStatistics> GetTeacherStatisticsAsync(Guid teacherUid)
    {
        var coursesCount = await _dbContext.Courses
            .Where(c => c.TeacherUid == teacherUid)
            .CountAsync();

        var studentsCount = await _dbContext.Enrollments
            .Where(e => e.Course.TeacherUid == teacherUid)
            .Select(e => e.StudentUid)
            .Distinct()
            .CountAsync();

        var averageGrade = await _dbContext.Grades
            .Where(g => g.TeacherUid == teacherUid)
            .AverageAsync(g => (double?)g.Value) ?? 0;

        var totalGrades = await _dbContext.Grades.Where(g => g.TeacherUid == teacherUid).CountAsync();

        return new TeacherStatistics
        {
            // Uid = Guid.NewGuid(),
            TotalCourses = coursesCount,
            ActiveCourses = coursesCount, // Simplified for now
            TotalStudents = studentsCount,
            // ActiveStudents = studentsCount, // Simplified for now
            AverageGrade = averageGrade,
            //PendingAssignments = 0, // TODO: Implement
            //GradedAssignments = totalGrades,
            //AttendanceRate = 85.0, // TODO: Implement
            //LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Получает курсы преподавателя
    /// </summary>
    public async Task<IEnumerable<Course>> GetTeacherCoursesAsync(Guid teacherUid)
    {
        return await _dbContext.Courses
            .Include(c => c.Enrollments)
            .Where(c => c.TeacherUid == teacherUid)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Получает группы преподавателя
    /// </summary>
    public async Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid)
    {
        return await _dbContext.Groups
            .Where(g => g.CuratorUid == teacherUid)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Назначает преподавателя на группу
    /// </summary>
    public async Task<bool> AssignToGroupAsync(Guid teacherUid, Guid groupUid)
    {
        var teacher = await _dbContext.Teachers.FindAsync(teacherUid);
        var group = await _dbContext.Groups.FindAsync(groupUid);

        if (teacher == null || group == null)
            return false;

        group.CuratorUid = teacherUid;
        group.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Отменяет назначение преподавателя на группу
    /// </summary>
    public async Task<bool> UnassignFromGroupAsync(Guid teacherUid, Guid groupUid)
    {
        var group = await _dbContext.Groups.FindAsync(groupUid);

        if (group == null || group.CuratorUid != teacherUid)
            return false;

        group.CuratorUid = null;
        group.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Отменяет назначение преподавателя на курс
    /// </summary>
    public async Task<bool> UnassignFromCourseAsync(Guid teacherUid, Guid courseUid)
    {
        var course = await _dbContext.Courses.FindAsync(courseUid);

        if (course == null || course.TeacherUid != teacherUid)
            return false;

        course.TeacherUid = null;
        course.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Генерирует тестовые данные преподавателей
    /// </summary>
    private static IEnumerable<Teacher> GenerateSampleTeachers()
    {
        return new List<Teacher>
        {
            new Teacher
            {
                Uid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Иван",
                LastName = "Петров",
                MiddleName = "Сергеевич",
                Specialization = "Программирование",
                AcademicTitle = "Доцент",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                LastModifiedAt = DateTime.UtcNow.AddDays(-5),
                Status = TeacherStatus.Active,
                HireDate = DateTime.UtcNow.AddDays(-365),
                HourlyRate = 1500,
                User = new User
                {
                    Uid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Email = "petrov@example.com",
                    PhoneNumber = "+7 (999) 123-45-67",
                    FirstName = "Иван",
                    LastName = "Петров",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            },
            new Teacher
            {
                Uid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Мария",
                LastName = "Сидорова",
                MiddleName = "Александровна",
                Specialization = "Веб-разработка",
                AcademicTitle = "Старший преподаватель",
                CreatedAt = DateTime.UtcNow.AddDays(-80),
                LastModifiedAt = DateTime.UtcNow.AddDays(-3),
                Status = TeacherStatus.Active,
                HireDate = DateTime.UtcNow.AddDays(-300),
                HourlyRate = 1800,
                User = new User
                {
                    Uid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Email = "sidorova@example.com",
                    PhoneNumber = "+7 (999) 234-56-78",
                    FirstName = "Мария",
                    LastName = "Сидорова",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            },
            new Teacher
            {
                Uid = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FirstName = "Алексей",
                LastName = "Козлов",
                MiddleName = "Владимирович",
                Specialization = "Базы данных",
                AcademicTitle = "Профессор",
                CreatedAt = DateTime.UtcNow.AddDays(-120),
                LastModifiedAt = DateTime.UtcNow.AddDays(-1),
                Status = TeacherStatus.Active,
                HireDate = DateTime.UtcNow.AddDays(-400),
                HourlyRate = 2000,
                User = new User
                {
                    Uid = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Email = "kozlov@example.com",
                    PhoneNumber = "+7 (999) 345-67-89",
                    FirstName = "Алексей",
                    LastName = "Козлов",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            }
        };
    }
}
