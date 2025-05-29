using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с департаментами
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(ApplicationDbContext dbContext, ILogger<DepartmentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Department?> GetDepartmentAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Teachers)
                .Include(d => d.Groups)
                .Include(d => d.Subjects)
                .FirstOrDefaultAsync(d => d.Uid == uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department with UID {DepartmentUid}", uid);
            throw;
        }
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        try
        {
            return await _dbContext.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Teachers)
                .Include(d => d.Groups)
                .Include(d => d.Subjects)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all departments");
            throw;
        }
    }

    public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
    {
        try
        {
            return await _dbContext.Departments
                .Include(d => d.HeadOfDepartment)
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active departments");
            throw;
        }
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        try
        {
            department.Uid = Guid.NewGuid();
            department.CreatedAt = DateTime.UtcNow;
            department.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.Departments.AddAsync(department);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Department created successfully: {DepartmentName} ({DepartmentCode})", 
                department.Name, department.Code);

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department: {DepartmentName}", department.Name);
            throw;
        }
    }

    public async Task AddDepartmentAsync(Department department)
    {
        try
        {
            department.CreatedAt = DateTime.UtcNow;
            department.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.Departments.AddAsync(department);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Department added successfully: {DepartmentName} ({DepartmentCode})", 
                department.Name, department.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding department: {DepartmentName}", department.Name);
            throw;
        }
    }

    public async Task<bool> UpdateDepartmentAsync(Department department)
    {
        try
        {
            var existingDepartment = await _dbContext.Departments.FindAsync(department.Uid);
            if (existingDepartment == null)
            {
                _logger.LogWarning("Department not found for update: {DepartmentUid}", department.Uid);
                return false;
            }

            existingDepartment.Name = department.Name;
            existingDepartment.Code = department.Code;
            existingDepartment.Description = department.Description;
            existingDepartment.IsActive = department.IsActive;
            existingDepartment.HeadOfDepartmentUid = department.HeadOfDepartmentUid;
            existingDepartment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Department updated successfully: {DepartmentName} ({DepartmentCode})", 
                department.Name, department.Code);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department: {DepartmentUid}", department.Uid);
            throw;
        }
    }

    public async Task<bool> DeleteDepartmentAsync(Guid uid)
    {
        try
        {
            var department = await _dbContext.Departments
                .Include(d => d.Teachers)
                .Include(d => d.Groups)
                .Include(d => d.Subjects)
                .FirstOrDefaultAsync(d => d.Uid == uid);

            if (department == null)
            {
                _logger.LogWarning("Department not found for deletion: {DepartmentUid}", uid);
                return false;
            }

            // Проверяем, есть ли связанные данные
            if (department.Teachers.Any() || department.Groups.Any() || department.Subjects.Any())
            {
                _logger.LogWarning("Cannot delete department {DepartmentName} - has related data", department.Name);
                throw new InvalidOperationException("Нельзя удалить департамент, который содержит преподавателей, группы или предметы");
            }

            _dbContext.Departments.Remove(department);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Department deleted successfully: {DepartmentName} ({DepartmentCode})", 
                department.Name, department.Code);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department: {DepartmentUid}", uid);
            throw;
        }
    }

    public async Task<IEnumerable<Department>> SearchDepartmentsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllDepartmentsAsync();
            }

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Teachers)
                .Include(d => d.Groups)
                .Include(d => d.Subjects)
                .Where(d => 
                    d.Name.ToLower().Contains(lowerSearchTerm) ||
                    d.Code.ToLower().Contains(lowerSearchTerm) ||
                    d.Description.ToLower().Contains(lowerSearchTerm))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching departments with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<(IEnumerable<Department> Departments, int TotalCount)> GetDepartmentsPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        bool? isActive = null)
    {
        try
        {
            var query = _dbContext.Departments
                .Include(d => d.HeadOfDepartment)
                .Include(d => d.Teachers)
                .Include(d => d.Groups)
                .Include(d => d.Subjects)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(d => 
                    d.Name.ToLower().Contains(lowerSearchTerm) ||
                    d.Code.ToLower().Contains(lowerSearchTerm) ||
                    d.Description.ToLower().Contains(lowerSearchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(d => d.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var departments = await query
                .OrderBy(d => d.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (departments, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged departments");
            throw;
        }
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null)
    {
        try
        {
            var query = _dbContext.Departments.Where(d => d.Code == code);
            
            if (excludeUid.HasValue)
            {
                query = query.Where(d => d.Uid != excludeUid.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking department code existence: {Code}", code);
            throw;
        }
    }

    public async Task<DepartmentStatistics> GetDepartmentStatisticsAsync(Guid departmentUid)
    {
        try
        {
            var department = await _dbContext.Departments
                .Include(d => d.Teachers)
                .Include(d => d.Groups)
                    .ThenInclude(g => g.Students)
                .Include(d => d.Subjects)
                .FirstOrDefaultAsync(d => d.Uid == departmentUid);

            if (department == null)
            {
                throw new ArgumentException($"Department with UID {departmentUid} not found");
            }

            var statistics = new DepartmentStatistics
            {
                TeachersCount = department.Teachers.Count,
                GroupsCount = department.Groups.Count,
                SubjectsCount = department.Subjects.Count,
                StudentsCount = department.Groups.SelectMany(g => g.Students).Count(),
                ActiveCoursesCount = await _dbContext.Courses
                    .Where(c => c.Teacher != null && c.Teacher.DepartmentUid == departmentUid)
                    .Where(c => c.Status == CourseStatus.Active)
                    .CountAsync()
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department statistics: {DepartmentUid}", departmentUid);
            throw;
        }
    }

    public async Task<bool> SetDepartmentActiveStatusAsync(Guid uid, bool isActive)
    {
        try
        {
            var department = await _dbContext.Departments.FindAsync(uid);
            if (department == null)
            {
                _logger.LogWarning("Department not found for status update: {DepartmentUid}", uid);
                return false;
            }

            department.IsActive = isActive;
            department.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Department status updated: {DepartmentName} - Active: {IsActive}", 
                department.Name, isActive);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department status: {DepartmentUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Создает тестовые данные для департаментов
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        try
        {
            if (await _dbContext.Departments.AnyAsync())
            {
                return; // Данные уже существуют
            }

            var departments = new[]
            {
                new Department("Информационных технологий", "IT", "Кафедра информационных технологий и программирования"),
                new Department("Математики и физики", "MATH", "Кафедра математических и физических наук"),
                new Department("Гуманитарных наук", "HUM", "Кафедра гуманитарных и социальных наук"),
                new Department("Экономики и менеджмента", "ECON", "Кафедра экономических дисциплин и менеджмента"),
                new Department("Иностранных языков", "LANG", "Кафедра иностранных языков и лингвистики")
            };

            await _dbContext.Departments.AddRangeAsync(departments);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Test departments data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding test departments data");
            throw;
        }
    }
}
