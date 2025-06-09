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
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с преподавателями
/// Наследуется от GenericCrudService для получения универсальных CRUD операций
/// </summary>
public class TeacherService : GenericCrudService<Teacher>, ITeacherService
{
    public TeacherService(ApplicationDbContext dbContext, ILogger<TeacherService> logger)
        : base(dbContext, logger)
    {
    }

    #region Переопределение базовых методов для специфичной логики

    /// <summary>
    /// Применяет специфичный для преподавателей поиск
    /// </summary>
    protected override IQueryable<Teacher> ApplySearchFilter(IQueryable<Teacher> query, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.ToLower();

        return query.Where(t =>
            (t.Person != null && (
                t.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                t.Person.LastName.ToLower().Contains(lowerSearchTerm) ||
                t.Person.MiddleName != null && t.Person.MiddleName.ToLower().Contains(lowerSearchTerm) ||
                t.Person.Email.ToLower().Contains(lowerSearchTerm) ||
                t.Person.PhoneNumber != null && t.Person.PhoneNumber.Contains(searchTerm)
            )) ||
            t.Specialization.ToLower().Contains(lowerSearchTerm) ||
            (t.Department != null && t.Department.Name.ToLower().Contains(lowerSearchTerm))
        );
    }

    /// <summary>
    /// Валидирует специфичные для преподавателя правила
    /// </summary>
    protected override async Task ValidateEntitySpecificRulesAsync(Teacher entity, List<string> errors, List<string> warnings, bool isCreate)
    {
        // Проверка обязательных полей через Person
        if (entity.Person == null)
        {
            errors.Add("Информация о человеке обязательна для преподавателя");
            return;
        }

        if (string.IsNullOrWhiteSpace(entity.Person.FirstName))
            errors.Add("Имя преподавателя обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(entity.Person.LastName))
            errors.Add("Фамилия преподавателя обязательна для заполнения");

        if (string.IsNullOrWhiteSpace(entity.Person.Email))
            errors.Add("Email преподавателя обязателен для заполнения");

        if (string.IsNullOrWhiteSpace(entity.Specialization))
            warnings.Add("Рекомендуется указать специализацию преподавателя");

        if (entity.Department == null)
            warnings.Add("Рекомендуется указать кафедру преподавателя");

        // Проверка формата email
        if (!string.IsNullOrWhiteSpace(entity.Person.Email) && !IsValidEmail(entity.Person.Email))
            errors.Add("Некорректный формат email");

        // Проверка уникальности email
        if (!string.IsNullOrWhiteSpace(entity.Person.Email))
        {
            var emailExists = await _dbContext.Persons
                .Where(p => p.Uid != entity.PersonUid && p.Email.ToLower() == entity.Person.Email.ToLower())
                .AnyAsync();

            if (emailExists)
                errors.Add($"Преподаватель с email '{entity.Person.Email}' уже существует");
        }

        // Проверка даты рождения
        if (entity.Person.DateOfBirth > DateTime.Now.AddYears(-18))
            warnings.Add("Возраст преподавателя меньше 18 лет");

        if (entity.Person.DateOfBirth < DateTime.Now.AddYears(-100))
            errors.Add("Некорректная дата рождения");
        if (entity.Person.DateOfBirth > DateTime.Now.AddYears(-21))
            warnings.Add("Возраст преподавателя меньше 21 года");

        // Проверка даты найма
        if (entity.HireDate > DateTime.Now)
            errors.Add("Дата найма не может быть в будущем");

        if (entity.HireDate < DateTime.Now.AddYears(-50))
            warnings.Add("Дата найма более 50 лет назад");

        // Проверка пользователя
        if (entity.PersonUid != Guid.Empty)
        {
            var personExists = await _dbContext.Persons
                .Where(p => p.Uid == entity.PersonUid)
                .AnyAsync();

            if (!personExists)
                errors.Add($"Пользователь с Uid {entity.PersonUid} не найден");
        }

        await base.ValidateEntitySpecificRulesAsync(entity, errors, warnings, isCreate);
    }

    #endregion

    #region Реализация интерфейса ITeacherService (существующие методы)

    public async Task<Teacher?> GetTeacherAsync(Guid uid)
    {
        return await GetByUidWithIncludesAsync(uid, 
            t => t.Person, 
            t => t.Department, 
            t => t.CourseInstances);
    }

    public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
    {
        return await GetAllWithIncludesAsync(t => t.Person);
    }

    public async Task<IEnumerable<Teacher>> GetTeachersAsync()
    {
        return await GetAllTeachersAsync();
    }

    public async Task<Teacher> CreateTeacherAsync(Teacher teacher)
    {
        return await CreateAsync(teacher);
    }

    public async Task AddTeacherAsync(Teacher teacher)
    {
        await CreateAsync(teacher);
    }

    public async Task<bool> UpdateTeacherAsync(Teacher teacher)
    {
        return await UpdateAsync(teacher);
    }

    public async Task<bool> DeleteTeacherAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    /// <summary>
    /// Назначает преподавателя на курс
    /// </summary>
    public async Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseInstanceUid)
    {
        var teacher = await _dbContext.Teachers.FindAsync(teacherUid);
        var courseInstance = await _dbContext.CourseInstances.FindAsync(courseInstanceUid);

        if (teacher == null || courseInstance == null)
            return false;

        courseInstance.TeacherUid = teacherUid;
        courseInstance.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Отменяет назначение преподавателя на курс
    /// </summary>
    public async Task<bool> UnassignFromCourseAsync(Guid teacherUid, Guid courseInstanceUid)
    {
        var courseInstance = await _dbContext.CourseInstances.FindAsync(courseInstanceUid);

        if (courseInstance == null || courseInstance.TeacherUid != teacherUid)
            return false;

        courseInstance.TeacherUid = Guid.Empty;
        courseInstance.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Получает доступных кураторов
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetAvailableCuratorsAsync()
    {
        return await _dbContext.Teachers
            .Where(t => t.IsActive)
            .OrderBy(t => t.Person.LastName)
            .ToListAsync();
    }

    /// <summary>
    /// Получает группы преподавателя
    /// </summary>
    public async Task<IEnumerable<Group>> GetCuratedGroupsAsync(Guid teacherUid)
    {
        var teacher = await _dbContext.Teachers
            .Where(t => t.Uid == teacherUid)
            .Include(t => t.CourseInstances)
                .ThenInclude(ci => ci.Group)
            .FirstOrDefaultAsync();

        // Возвращаем группы из курсов, которые ведет преподаватель
        return teacher?.CourseInstances?.Select(ci => ci.Group).Distinct() ?? new List<Group>();
    }

    /// <summary>
    /// Получает группы преподавателя (алиас для GetCuratedGroupsAsync для совместимости с интерфейсом)
    /// </summary>
    public async Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid)
    {
        return await GetCuratedGroupsAsync(teacherUid);
    }

    /// <summary>
    /// Получает статистику преподавателя
    /// </summary>
    public async Task<TeacherStatistics> GetTeacherStatisticsAsync(Guid teacherUid)
    {
        try
        {
            var teacher = await _dbContext.Teachers
                .Include(t => t.CourseInstances)
                    .ThenInclude(ci => ci.Subject)
                .Include(t => t.CourseInstances)
                    .ThenInclude(ci => ci.Group)
                .Include(t => t.CourseInstances)
                    .ThenInclude(ci => ci.AcademicPeriod)
                .FirstOrDefaultAsync(t => t.PersonUid == teacherUid);

            if (teacher == null)
                return null;

            var teacherStatistics = new TeacherStatistics
            {
                TotalCourses = teacher.CourseInstances?.Count ?? 0,
                ActiveCourses = teacher.CourseInstances?.Count(c => c.Status == CourseStatus.Active) ?? 0,
                TotalStudents = teacher.CourseInstances?.SelectMany(c => c.Enrollments ?? new List<Enrollment>()).Count() ?? 0,
                CuratedGroups = teacher.CourseInstances?.Select(c => c.Group).Distinct().Count() ?? 0,
                AverageGrade = 0, // Будет вычислено позже
                CompletedCourses = teacher.CourseInstances?.Count(c => c.Status == CourseStatus.Completed) ?? 0,
                PendingAssignments = 0, // Будет вычислено позже
                TotalAssignments = teacher.CourseInstances?.SelectMany(c => c.Assignments ?? new List<Assignment>()).Count() ?? 0
            };

            // Логирование через стандартный ILogger
            _logger.LogInformation("Loaded statistics for teacher {TeacherName}: {TotalCourses} courses", 
                teacher.Person?.FirstName + " " + teacher.Person?.LastName, teacherStatistics.TotalCourses);

            return teacherStatistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher statistics: {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает курсы преподавателя
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetTeacherCoursesAsync(Guid teacherUid)
    {
        try
        {
            var teacher = await _dbContext.Teachers
                .Include(t => t.CourseInstances)
                    .ThenInclude(ci => ci.Subject)
                .Include(t => t.CourseInstances)
                    .ThenInclude(ci => ci.Group)
                .Include(t => t.CourseInstances)
                    .ThenInclude(ci => ci.AcademicPeriod)
                .FirstOrDefaultAsync(t => t.PersonUid == teacherUid);

            if (teacher == null)
                return new List<CourseInstance>();

            return teacher.CourseInstances?.ToList() ?? new List<CourseInstance>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher courses: {TeacherUid}", teacherUid);
            return new List<CourseInstance>();
        }
    }

    /// <summary>
    /// Назначает преподавателя на группу
    /// </summary>
    public async Task<bool> AssignToGroupAsync(Guid teacherUid, Guid groupUid)
    {
        try
        {
            var teacher = await GetByUidAsync(teacherUid);
            var group = await _dbContext.Groups.FindAsync(groupUid);

            if (teacher == null || group == null)
            {
                _logger.LogWarning("Teacher or group not found for assignment: Teacher={TeacherUid}, Group={GroupUid}", teacherUid, groupUid);
                return false;
            }

            group.CuratorUid = teacherUid;
            group.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Teacher {TeacherUid} assigned to group {GroupUid}", teacherUid, groupUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning teacher {TeacherUid} to group {GroupUid}", teacherUid, groupUid);
            throw;
        }
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
                .Include(t => t.Person)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t =>
                    (t.Person != null && (
                        t.Person.FirstName.Contains(searchTerm) ||
                        t.Person.LastName.Contains(searchTerm) ||
                        t.Person.Email.Contains(searchTerm)
                    )) ||
                    (t.Specialization != null && t.Specialization.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(specializationFilter))
            {
                query = query.Where(t => t.Specialization == specializationFilter);
            }

            var totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.Person != null ? t.Person.LastName : "")
                .ThenBy(t => t.Person != null ? t.Person.FirstName : "")
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
                    (t.Person != null && (
                        t.Person.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.Person.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        t.Person.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    )) ||
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
    /// Генерирует тестовые данные преподавателей
    /// </summary>
    private static IEnumerable<Teacher> GenerateSampleTeachers()
    {
        return new List<Teacher>
        {
            new Teacher
            {
                Uid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Specialization = "Программирование",
                CreatedAt = DateTime.UtcNow.AddDays(-100),
                LastModifiedAt = DateTime.UtcNow.AddDays(-5),
                HireDate = DateTime.UtcNow.AddDays(-365),
                Salary = 1500,
                Person = new Person
                {
                    Uid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Email = "petrov@example.com",
                    PhoneNumber = "+7 (999) 123-45-67",
                    FirstName = "Иван",
                    LastName = "Петров",
                    MiddleName = "Сергеевич",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            },
            new Teacher
            {
                Uid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Specialization = "Веб-разработка",
                CreatedAt = DateTime.UtcNow.AddDays(-80),
                LastModifiedAt = DateTime.UtcNow.AddDays(-3),
                HireDate = DateTime.UtcNow.AddDays(-300),
                Salary = 1800,
                Person = new Person
                {
                    Uid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Email = "sidorova@example.com",
                    PhoneNumber = "+7 (999) 234-56-78",
                    FirstName = "Мария",
                    LastName = "Сидорова",
                    MiddleName = "Александровна",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            },
            new Teacher
            {
                Uid = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Specialization = "Базы данных",
                CreatedAt = DateTime.UtcNow.AddDays(-120),
                LastModifiedAt = DateTime.UtcNow.AddDays(-1),
                HireDate = DateTime.UtcNow.AddDays(-400),
                Salary = 2000,
                Person = new Person
                {
                    Uid = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Email = "kozlov@example.com",
                    PhoneNumber = "+7 (999) 345-67-89",
                    FirstName = "Алексей",
                    LastName = "Козлов",
                    MiddleName = "Владимирович",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            }
        };
    }

    /// <summary>
    /// Получает доступных кураторов для назначения группе
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetAvailableCuratorsForGroupAsync(Guid groupUid)
    {
        try
        {
            // Получаем всех активных преподавателей, которые могут быть кураторами
            var availableCurators = await _dbContext.Teachers
                .Include(t => t.Person)
                .Where(t => t.IsActive)
                .ToListAsync();

            return availableCurators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении доступных кураторов для группы {GroupUid}", groupUid);
            return new List<Teacher>();
        }
    }

    /// <summary>
    /// Проверяет существование преподавателя по email
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .AnyAsync(t => t.Person != null && t.Person.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования преподавателя по email {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Проверяет существование преподавателя по email (с исключением определенного UID)
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email, Guid excludeUid)
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .AnyAsync(t => t.Person != null && t.Person.Email == email && t.Uid != excludeUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования преподавателя по email {Email} (исключая {ExcludeUid})", email, excludeUid);
            return false;
        }
    }

    /// <summary>
    /// Экспортирует данные преподавателей
    /// </summary>
    public async Task<string> ExportTeachersAsync(IEnumerable<Teacher> teachers, string format = "xlsx")
    {
        try
        {
            // TODO: Реализовать экспорт в Excel/CSV
            // Пока возвращаем заглушку
            await Task.Delay(100);
            
            var fileName = $"teachers_export_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
            _logger.LogInformation("Экспорт преподавателей в файл {FileName} (заглушка)", fileName);
            
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при экспорте преподавателей");
            throw;
        }
    }

    /// <summary>
    /// Получает информацию о связанных данных преподавателя для безопасного удаления
    /// </summary>
    public async Task<TeacherRelatedDataInfo> GetTeacherRelatedDataInfoAsync(Guid teacherUid)
    {
        var relatedData = new TeacherRelatedDataInfo();

        try
        {
            // Проверка экземпляров курсов
            var courseInstancesCount = await _dbContext.CourseInstances
                .Where(ci => ci.TeacherUid == teacherUid)
                .CountAsync();
            
            if (courseInstancesCount > 0)
            {
                relatedData.HasCourseInstances = true;
                relatedData.CourseInstancesCount = courseInstancesCount;
                relatedData.RelatedDataDescriptions.Add($"• {courseInstancesCount} экземпляров курсов будут удалены");
            }

            // Проверка групп, которые курирует
            var curatedGroupsCount = await _dbContext.Groups
                .Where(g => g.CuratorUid == teacherUid)
                .CountAsync();
            
            if (curatedGroupsCount > 0)
            {
                relatedData.HasCuratedGroups = true;
                relatedData.CuratedGroupsCount = curatedGroupsCount;
                relatedData.RelatedDataDescriptions.Add($"• {curatedGroupsCount} групп останутся без куратора");
            }

            relatedData.HasRelatedData = relatedData.HasCourseInstances || relatedData.HasCuratedGroups;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to check related data for teacher {TeacherUid}", teacherUid);
        }

        return relatedData;
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Проверяет корректность email
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
