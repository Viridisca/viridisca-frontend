using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education.DTOs;

/// <summary>
/// DTO для деталей студента
/// </summary>
public class StudentDetailsDto
{
    public Guid Uid { get; set; }
    public Guid PersonUid { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public StudentStatus Status { get; set; }
    public Guid? GroupUid { get; set; }
    public Guid? CurriculumUid { get; set; }
    
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