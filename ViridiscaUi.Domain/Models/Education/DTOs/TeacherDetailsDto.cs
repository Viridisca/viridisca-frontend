using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education.DTOs;

/// <summary>
/// DTO для деталей преподавателя
/// </summary>
public class TeacherDetailsDto
{
    public Guid Uid { get; set; }
    public Guid PersonUid { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public Guid? DepartmentUid { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal HourlyRate { get; set; }
    
    // Данные из Person
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Address { get; set; } = string.Empty;
    
    // Дополнительные поля
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
} 