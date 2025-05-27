using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для учебного плана
/// </summary>
public interface ICurriculumService
{
    Task<CurriculumOverview> GetCurriculumOverviewAsync();
}

public class CurriculumOverview
{
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int UnderDevelopment { get; set; }
} 