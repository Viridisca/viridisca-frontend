using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с предметами
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(ApplicationDbContext dbContext, ILogger<SubjectService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
    {
        try
        {
            var subjects = await _dbContext.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех предметов");
            return [];
        }
    }

    public async Task<Subject?> GetSubjectAsync(Guid uid)
    {
        try
        {
            var subject = await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Uid == uid);

            return subject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предмета {SubjectUid}", uid);
            return null;
        }
    }

    public async Task<Subject?> GetSubjectByUidAsync(Guid uid)
    {
        return await GetSubjectAsync(uid);
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByTeacherAsync(Guid teacherUid)
    {
        try
        {
            // Используем CourseInstances для получения предметов преподавателя
            var subjects = await _dbContext.CourseInstances
                .Where(ci => ci.TeacherUid == teacherUid)
                .Select(ci => ci.Subject)
                .Distinct()
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов преподавателя {TeacherUid}", teacherUid);
            return [];
        }
    }

    public async Task<Subject> CreateSubjectAsync(Subject subject)
    {
        try
        {
            _dbContext.Subjects.Add(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Создан предмет {SubjectName} с ID {SubjectUid}", subject.Name, subject.Uid);
            return subject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании предмета {SubjectName}", subject.Name);
            throw;
        }
    }

    public async Task AddSubjectAsync(Subject subject)
    {
        try
        {
            _dbContext.Subjects.Add(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Добавлен предмет {SubjectName} с ID {SubjectUid}", subject.Name, subject.Uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении предмета {SubjectName}", subject.Name);
            throw;
        }
    }

    public async Task<bool> UpdateSubjectAsync(Subject subject)
    {
        try
        {
            _dbContext.Subjects.Update(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Обновлен предмет {SubjectName} с ID {SubjectUid}", subject.Name, subject.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении предмета {SubjectUid}", subject.Uid);
            return false;
        }
    }

    public async Task<bool> DeleteSubjectAsync(Guid uid)
    {
        try
        {
            var subject = await _dbContext.Subjects.FindAsync(uid);
            if (subject == null)
            {
                _logger.LogWarning("Предмет с ID {SubjectUid} не найден для удаления", uid);
                return false;
            }

            // Проверяем, есть ли связанные CourseInstances
            var hasRelatedCourses = await _dbContext.CourseInstances
                .AnyAsync(ci => ci.SubjectUid == uid);

            if (hasRelatedCourses)
            {
                _logger.LogWarning("Нельзя удалить предмет {SubjectUid}, так как есть связанные курсы", uid);
                return false;
            }

            _dbContext.Subjects.Remove(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Удален предмет {SubjectName} с ID {SubjectUid}", subject.Name, uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении предмета {SubjectUid}", uid);
            return false;
        }
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByDepartmentAsync(Guid departmentUid)
    {
        try
        {
            var subjects = await _dbContext.Subjects
                .Where(s => s.DepartmentUid == departmentUid)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов департамента {DepartmentUid}", departmentUid);
            return [];
        }
    }

    public async Task<IEnumerable<Subject>> GetActiveSubjectsAsync()
    {
        try
        {
            var subjects = await _dbContext.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных предметов");
            return [];
        }
    }

    public async Task<IEnumerable<Subject>> SearchSubjectsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllSubjectsAsync();
            }

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Subjects
                .Where(s => 
                    s.Name.ToLower().Contains(lowerSearchTerm) ||
                    s.Code.ToLower().Contains(lowerSearchTerm) ||
                    s.Description.ToLower().Contains(lowerSearchTerm))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске предметов с термином: {SearchTerm}", searchTerm);
            return [];
        }
    }

    public async Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetSubjectsPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        bool? isActive = null, 
        Guid? departmentUid = null)
    {
        try
        {
            var query = _dbContext.Subjects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(s => 
                    s.Name.ToLower().Contains(lowerSearchTerm) ||
                    s.Code.ToLower().Contains(lowerSearchTerm) ||
                    s.Description.ToLower().Contains(lowerSearchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            if (departmentUid.HasValue)
            {
                query = query.Where(s => s.DepartmentUid == departmentUid.Value);
            }

            var totalCount = await query.CountAsync();

            var subjects = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (subjects, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов с пагинацией");
            return ([], 0);
        }
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null)
    {
        try
        {
            var query = _dbContext.Subjects.Where(s => s.Code == code);
            
            if (excludeUid.HasValue)
            {
                query = query.Where(s => s.Uid != excludeUid.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования кода предмета: {Code}", code);
            return false;
        }
    }

    public async Task<SubjectStatistics> GetSubjectStatisticsAsync(Guid subjectUid)
    {
        try
        {
            var subject = await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Uid == subjectUid);

            if (subject == null)
            {
                throw new ArgumentException($"Subject with UID {subjectUid} not found");
            }

            var coursesCount = await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == subjectUid)
                .CountAsync();

            var activeCoursesCount = await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == subjectUid && ci.IsActive)
                .CountAsync();

            var statistics = new SubjectStatistics
            {
                TeachersCount = 0, // Teachers count is no longer available in the new architecture
                CoursesCount = coursesCount,
                ActiveCoursesCount = activeCoursesCount,
                StudentsCount = 0, // Will need to calculate from enrollments
                AverageGrade = 0 // Will need to calculate from grades
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики предмета: {SubjectUid}", subjectUid);
            throw;
        }
    }

    public async Task<bool> SetSubjectActiveStatusAsync(Guid uid, bool isActive)
    {
        try
        {
            var subject = await _dbContext.Subjects.FindAsync(uid);
            if (subject == null)
            {
                _logger.LogWarning("Предмет с ID {SubjectUid} не найден для обновления статуса", uid);
                return false;
            }

            subject.IsActive = isActive;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Статус предмета обновлен: {SubjectName} - Активен: {IsActive}", 
                subject.Name, isActive);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса предмета: {SubjectUid}", uid);
            return false;
        }
    }

    /// <summary>
    /// Создает тестовые данные для предметов
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        try
        {
            if (await _dbContext.Subjects.AnyAsync())
            {
                return; // Данные уже существуют
            }

            var department = await _dbContext.Departments.FirstOrDefaultAsync();

            var subjects = new[]
            {
                new Subject("CS101", "Программирование на C#", "Основы программирования на языке C#", 4, SubjectType.Required, "Программирование", department?.Uid),
                new Subject("DB101", "Базы данных", "Проектирование и работа с базами данных", 3, SubjectType.Required, "Базы данных", department?.Uid),
                new Subject("WEB101", "Веб-разработка", "Создание веб-приложений", 4, SubjectType.Specialized, "Веб-технологии", department?.Uid),
                new Subject("MATH101", "Математический анализ", "Основы математического анализа", 5, SubjectType.Required, "Математика", department?.Uid),
                new Subject("ENG101", "Английский язык", "Английский язык для IT специалистов", 2, SubjectType.Elective, "Языки", department?.Uid)
            };

            await _dbContext.Subjects.AddRangeAsync(subjects);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Test subjects data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding test subjects data");
            throw;
        }
    }

    private async Task<IEnumerable<Subject>> GetSubjectsWithStatisticsAsync(string? status)
    {
        try
        {
            var query = _dbContext.Subjects.AsQueryable();

            // Фильтрация по статусу через CourseInstances
            if (!string.IsNullOrEmpty(status))
            {
                // Временная заглушка - нужно определить как фильтровать по статусу
                query = query.Where(s => s.IsActive);
            }

            var subjects = await query
                .OrderBy(s => s.Name)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов со статистикой");
            return [];
        }
    }

    /// <summary>
    /// Получает предметы с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetPagedAsync(
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null, 
        Guid? departmentUid = null)
    {
        var query = _dbContext.Subjects.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(s => s.Name.Contains(searchTerm) || s.Code.Contains(searchTerm));
        }

        if (departmentUid.HasValue)
        {
            query = query.Where(s => s.DepartmentUid == departmentUid.Value);
        }

        var totalCount = await query.CountAsync();
        var subjects = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (subjects, totalCount);
    }

    /// <summary>
    /// Создает новый предмет
    /// </summary>
    public async Task<Subject> CreateAsync(Subject subject)
    {
        _dbContext.Subjects.Add(subject);
        await _dbContext.SaveChangesAsync();
        return subject;
    }

    /// <summary>
    /// Получает предмет по идентификатору
    /// </summary>
    public async Task<Subject?> GetByUidAsync(Guid uid)
    {
        return await _dbContext.Subjects.FindAsync(uid);
    }

    /// <summary>
    /// Обновляет предмет
    /// </summary>
    public async Task<Subject> UpdateAsync(Subject subject)
    {
        _dbContext.Subjects.Update(subject);
        await _dbContext.SaveChangesAsync();
        return subject;
    }

    /// <summary>
    /// Удаляет предмет
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        var subject = await _dbContext.Subjects.FindAsync(uid);
        if (subject == null) return false;

        _dbContext.Subjects.Remove(subject);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Получает предмет по коду
    /// </summary>
    public async Task<Subject?> GetByCodeAsync(string code)
    {
        return await _dbContext.Subjects
            .FirstOrDefaultAsync(s => s.Code == code);
    }

    /// <summary>
    /// Получает количество экземпляров курсов для предмета
    /// </summary>
    public async Task<int> GetCourseInstancesCountAsync(Guid subjectUid)
    {
        return await _dbContext.CourseInstances
            .CountAsync(ci => ci.SubjectUid == subjectUid);
    }

    /// <summary>
    /// Получает количество предметов в учебных планах
    /// </summary>
    public async Task<int> GetCurriculumSubjectsCountAsync(Guid subjectUid)
    {
        return await _dbContext.CurriculumSubjects
            .CountAsync(cs => cs.SubjectUid == subjectUid);
    }

    /// <summary>
    /// Получает информацию о связанных данных предмета
    /// </summary>
    public async Task<ViridiscaUi.Domain.Models.Base.SubjectRelatedDataInfo> GetSubjectRelatedDataInfoAsync(Guid subjectUid)
    {
        var info = new ViridiscaUi.Domain.Models.Base.SubjectRelatedDataInfo();
        
        try
        {
            // Проверяем экземпляры курсов
            var courseInstancesCount = await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == subjectUid)
                .CountAsync();
            
            if (courseInstancesCount > 0)
            {
                info.HasCourseInstances = true;
                info.CourseInstancesCount = courseInstancesCount;
                info.RelatedDataDescriptions.Add($"• Экземпляры курсов: {courseInstancesCount}");
            }
            
            // Проверяем задания
            var assignmentsCount = await _dbContext.Assignments
                .Where(a => a.CourseInstance != null && a.CourseInstance.SubjectUid == subjectUid)
                .CountAsync();
            
            if (assignmentsCount > 0)
            {
                info.HasAssignments = true;
                info.AssignmentsCount = assignmentsCount;
                info.RelatedDataDescriptions.Add($"• Задания: {assignmentsCount}");
            }
            
            // Проверяем оценки
            var gradesCount = await _dbContext.Grades
                .Where(g => g.Assignment != null && 
                           g.Assignment.CourseInstance != null && 
                           g.Assignment.CourseInstance.SubjectUid == subjectUid)
                .CountAsync();
            
            if (gradesCount > 0)
            {
                info.HasGrades = true;
                info.GradesCount = gradesCount;
                info.RelatedDataDescriptions.Add($"• Оценки: {gradesCount}");
            }
            
            info.HasRelatedData = info.HasCourseInstances || info.HasAssignments || info.HasGrades;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking related data for subject: {SubjectUid}", subjectUid);
        }
        
        return info;
    }

    /// <summary>
    /// Получает предмет по названию
    /// </summary>
    public async Task<Subject?> GetByNameAsync(string name)
    {
        try
        {
            return await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Name == name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject by name: {Name}", name);
            return null;
        }
    }

    /// <summary>
    /// Получает общее количество предметов
    /// </summary>
    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await _dbContext.Subjects.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total subjects count");
            return 0;
        }
    }

    /// <summary>
    /// Получает курсы предмета
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetSubjectCoursesAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == subjectUid)
                .Include(ci => ci.Subject)
                .Include(ci => ci.Teacher)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject courses: {SubjectUid}", subjectUid);
            return [];
        }
    }

    /// <summary>
    /// Получает учебные планы предмета
    /// </summary>
    public async Task<IEnumerable<Curriculum>> GetSubjectCurriculaAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.CurriculumSubjects
                .Where(cs => cs.SubjectUid == subjectUid)
                .Select(cs => cs.Curriculum)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject curricula: {SubjectUid}", subjectUid);
            return [];
        }
    }

    /// <summary>
    /// Получает задания предмета
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetSubjectAssignmentsAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Where(a => a.CourseInstance != null && a.CourseInstance.SubjectUid == subjectUid)
                .Include(a => a.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject assignments: {SubjectUid}", subjectUid);
            return [];
        }
    }
} 