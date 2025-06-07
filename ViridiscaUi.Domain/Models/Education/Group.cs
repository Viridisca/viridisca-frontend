using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.System;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Учебная группа
/// </summary>
public class Group : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MaxStudents { get; set; }
    public Guid DepartmentUid { get; set; }
    public Guid? CuratorUid { get; set; }
    public GroupStatus Status { get; set; }
    
    /// <summary>
    /// Активна ли группа
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Department? Department { get; set; }
    public Teacher? Curator { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
} 