using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для управления академическими периодами
/// </summary>
public interface IAcademicPeriodService
{
    Task<IEnumerable<AcademicPeriod>> GetAllAsync();
    Task<AcademicPeriod?> GetByIdAsync(Guid uid);
    Task<AcademicPeriod?> GetByUidAsync(Guid uid);
    Task<AcademicPeriod?> GetCurrentAsync();
    Task<IEnumerable<AcademicPeriod>> GetActiveAsync();
    Task<AcademicPeriod> CreateAsync(AcademicPeriod academicPeriod);
    Task<AcademicPeriod> UpdateAsync(AcademicPeriod academicPeriod);
    Task<bool> DeleteAsync(Guid uid);
    Task<bool> ExistsAsync(Guid uid);
    Task<int> GetCountAsync();
} 