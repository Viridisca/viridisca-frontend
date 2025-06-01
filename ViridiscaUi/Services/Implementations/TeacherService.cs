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

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Получает преподавателей по кафедре
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetByDepartmentAsync(string department)
    {
        return await FindWithIncludesAsync(t => t.Department != null && t.Department.Name == department, t => t.Person);
    }

    /// <summary>
    /// Получает преподавателей по специализации
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetBySpecializationAsync(string specialization)
    {
        return await FindWithIncludesAsync(t => t.Specialization == specialization, t => t.Person);
    }

    /// <summary>
    /// Получает статистику преподавателя
    /// </summary>
    public async Task<TeacherStatistics> GetTeacherStatisticsAsync(Guid teacherUid)
    {
        try
        {
            var coursesCount = await _dbContext.CourseInstances
                .Where(c => c.TeacherUid == teacherUid)
                .CountAsync();

            var studentsCount = await _dbContext.Enrollments
                .Where(e => e.CourseInstance.TeacherUid == teacherUid)
                .Select(e => e.StudentUid)
                .Distinct()
                .CountAsync();

            var averageGrade = await _dbContext.Grades
                .Where(g => g.TeacherUid == teacherUid)
                .AverageAsync(g => (double?)g.Value) ?? 0;

            var totalGrades = await _dbContext.Grades
                .Where(g => g.TeacherUid == teacherUid)
                .CountAsync();

            return new TeacherStatistics
            {
                TotalCourses = coursesCount,
                ActiveCourses = coursesCount, // Simplified for now
                TotalStudents = studentsCount,
                AverageGrade = averageGrade
            };
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
    /// Получает группы преподавателя
    /// </summary>
    public async Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.Groups
                .Where(g => g.CuratorUid == teacherUid)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher groups: {TeacherUid}", teacherUid);
            throw;
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
