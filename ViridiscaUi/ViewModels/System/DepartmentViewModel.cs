using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.ViewModels.Bases;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для управления данными Department
/// </summary>
public class DepartmentViewModel : ReactiveObject
{
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string? Description { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public Guid? HeadOfDepartmentUid { get; set; }
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    // Дополнительные свойства для UI
    [Reactive] public int TeachersCount { get; set; }
    [Reactive] public int StudentsCount { get; set; }
    [Reactive] public int SubjectsCount { get; set; }

    public DepartmentViewModel()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public DepartmentViewModel(Department department)
    {
        Uid = department.Uid;
        Name = department.Name;
        Code = department.Code;
        Description = department.Description;
        IsActive = department.IsActive;
        HeadOfDepartmentUid = department.HeadOfDepartmentUid;
        CreatedAt = department.CreatedAt;
        LastModifiedAt = department.LastModifiedAt;

        // Дополнительные данные из связанных сущностей
        TeachersCount = department.Teachers?.Count ?? 0;
        StudentsCount = department.Groups?.SelectMany(g => g.Students ?? new List<ViridiscaUi.Domain.Models.Education.Student>()).Count() ?? 0;
        SubjectsCount = department.Subjects?.Count ?? 0;
    }

    public Department ToDepartment()
    {
        return new Department
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            IsActive = IsActive,
            HeadOfDepartmentUid = HeadOfDepartmentUid,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    /// <summary>
    /// Обновляет данные из модели департамента
    /// </summary>
    public void UpdateFromModel(Department department)
    {
        Name = department.Name ?? string.Empty;
        Code = department.Code ?? string.Empty;
        Description = department.Description;
        IsActive = department.IsActive;
        HeadOfDepartmentUid = department.HeadOfDepartmentUid;
        LastModifiedAt = department.LastModifiedAt;

        // Обновляем счетчики
        TeachersCount = department.Teachers?.Count ?? 0;
        StudentsCount = department.Groups?.SelectMany(g => g.Students ?? new List<ViridiscaUi.Domain.Models.Education.Student>()).Count() ?? 0;
        SubjectsCount = department.Subjects?.Count ?? 0;
    }
} 