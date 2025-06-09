using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.ViewModels.Education;
using static ViridiscaUi.ViewModels.Education.CurriculumViewModel;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для управления учебными планами
/// </summary>
public interface ICurriculumService
{
    Task<IEnumerable<Curriculum>> GetAllAsync();
    Task<Curriculum?> GetByIdAsync(Guid uid);
    Task<Curriculum?> GetByCodeAsync(string code);
    Task<Curriculum?> GetByUidAsync(Guid uid);
    Task<Curriculum> CreateAsync(Curriculum curriculum);
    Task<Curriculum> UpdateAsync(Curriculum curriculum);
    Task<bool> DeleteAsync(Guid uid);
    Task<bool> ExistsAsync(Guid uid);
    Task<int> GetCountAsync();
    Task<int> GetStudentsCountAsync(Guid curriculumUid);
    Task<int> GetSubjectsCountAsync(Guid curriculumUid);
    Task<Curriculum> CopyAsync(Guid curriculumUid, string? newName = null);
    Task<Curriculum> ActivateAsync(Guid curriculumUid);
    Task<Curriculum> DeactivateAsync(Guid curriculumUid);
    Task<byte[]> ExportAsync(Guid curriculumUid);
    Task<Curriculum?> ImportAsync();
    Task<CurriculumStatistics> GetCurriculumStatisticsAsync(Guid? departmentUid = null);
    
    // Расширенные методы для пагинации и фильтрации
    Task<(IEnumerable<Curriculum> curricula, int totalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? departmentUid = null,
        bool? isActive = null,
        int? minCredits = null,
        int? maxCredits = null,
        int? academicYear = null);
}

public class CurriculumOverview
{
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int UnderDevelopment { get; set; }
} 