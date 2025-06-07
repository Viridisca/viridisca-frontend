using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Library.Enums;

namespace ViridiscaUi.Domain.Models.Library;

/// <summary>
/// Статистика библиотеки
/// </summary>
public class LibraryStatistics
{
    /// <summary>
    /// Общее количество ресурсов
    /// </summary>
    public int TotalResources { get; set; }
    
    /// <summary>
    /// Доступные ресурсы
    /// </summary>
    public int AvailableResources { get; set; }
    
    /// <summary>
    /// Выданные ресурсы
    /// </summary>
    public int LoanedResources { get; set; }
    
    /// <summary>
    /// Просроченные ресурсы
    /// </summary>
    public int OverdueResources { get; set; }
    
    /// <summary>
    /// Общее количество займов
    /// </summary>
    public int TotalLoans { get; set; }
    
    /// <summary>
    /// Активные займы
    /// </summary>
    public int ActiveLoans { get; set; }
    
    /// <summary>
    /// Общее количество пользователей
    /// </summary>
    public int TotalUsers { get; set; }
    
    /// <summary>
    /// Активные пользователи
    /// </summary>
    public int ActiveUsers { get; set; }
    
    /// <summary>
    /// Ресурсы по типам
    /// </summary>
    public Dictionary<ResourceType, int> ResourcesByType { get; set; } = new();
} 