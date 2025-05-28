using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.ViewModels;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Infrastructure;
using System.Text.RegularExpressions;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual student with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
public class StudentViewModel : ViewModelBase
{
    // === CORE PROPERTIES ===
    
    public Guid Uid { get; }
    
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string? MiddleName { get; set; }
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string? Phone { get; set; }
    [Reactive] public string StudentCode { get; set; } = string.Empty;
    [Reactive] public DateTime? BirthDate { get; set; }
    [Reactive] public string? Address { get; set; }
    [Reactive] public StudentStatus Status { get; set; } = StudentStatus.Active;
    [Reactive] public DateTime? EnrollmentDate { get; set; }
    [Reactive] public DateTime? GraduationDate { get; set; }
    [Reactive] public int AcademicYear { get; set; } = 1;
    [Reactive] public decimal? GPA { get; set; }
    
    // Group Information
    [Reactive] public Guid? GroupUid { get; set; }
    [Reactive] public string? GroupName { get; set; }
    [Reactive] public string? GroupCode { get; set; }
    
    // Department Information
    [Reactive] public Guid? DepartmentUid { get; set; }
    [Reactive] public string? DepartmentName { get; set; }
    
    // Selection State for DataGrid
    [Reactive] public bool IsSelected { get; set; }
    
    // Audit Information
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; }
    public Guid? UpdatedBy { get; private set; }

    // === COMPUTED PROPERTIES ===
    
    /// <summary>
    /// Full name of the student
    /// </summary>
    public string FullName => string.IsNullOrWhiteSpace(MiddleName) 
        ? $"{LastName} {FirstName}".Trim()
        : $"{LastName} {FirstName} {MiddleName}".Trim();
    
    /// <summary>
    /// Initials for avatar display
    /// </summary>
    public string Initials
    {
        get
        {
            var firstInitial = !string.IsNullOrEmpty(FirstName) ? FirstName[0].ToString().ToUpperInvariant() : "";
            var lastInitial = !string.IsNullOrEmpty(LastName) ? LastName[0].ToString().ToUpperInvariant() : "";
            return $"{firstInitial}{lastInitial}";
        }
    }
    
    /// <summary>
    /// Отображаемое название статуса с локализацией
    /// </summary>
    public string StatusDisplayName => Status switch
    {
        StudentStatus.Active => "Активный",
        StudentStatus.Inactive => "Неактивный",
        StudentStatus.AcademicLeave => "В отпуске",
        StudentStatus.Expelled => "Отчислен",
        StudentStatus.Graduated => "Выпущен",
        StudentStatus.Transferred => "Переведен",
        StudentStatus.Suspended => "Приостановлен",
        _ => "Неизвестно"
    };
    
    /// <summary>
    /// Цвет для значка статуса
    /// </summary>
    public string StatusColor => Status switch
    {
        StudentStatus.Active => "#4CAF50",      // Green
        StudentStatus.Inactive => "#FF9800",    // Orange
        StudentStatus.AcademicLeave => "#9C27B0",     // Purple
        StudentStatus.Expelled => "#F44336",    // Red
        StudentStatus.Graduated => "#8BC34A",   // Light Green
        StudentStatus.Transferred => "#2196F3", // Blue
        StudentStatus.Suspended => "#FF5722",   // Deep Orange
        _ => "#757575"                          // Grey
    };
    
    /// <summary>
    /// Age of the student
    /// </summary>
    public int? Age
    {
        get
        {
            if (!BirthDate.HasValue) return null;
            
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Value.Year;
            
            if (BirthDate.Value.Date > today.AddYears(-age))
                age--;
                
            return age;
        }
    }
    
    /// <summary>
    /// Formatted age display
    /// </summary>
    public string AgeDisplay => Age.HasValue ? $"{Age} лет" : "Не указан";
    
    /// <summary>
    /// Formatted enrollment date
    /// </summary>
    public string EnrollmentDateDisplay => EnrollmentDate?.ToString("dd.MM.yyyy") ?? "Не указана";
    
    /// <summary>
    /// Formatted graduation date
    /// </summary>
    public string GraduationDateDisplay => GraduationDate?.ToString("dd.MM.yyyy") ?? "Не указана";
    
    /// <summary>
    /// GPA display with formatting
    /// </summary>
    public string GPADisplay => GPA.HasValue ? $"{GPA:F2}" : "Не рассчитан";
    
    /// <summary>
    /// Academic year display
    /// </summary>
    public string AcademicYearDisplay => $"{AcademicYear} курс";
    
    /// <summary>
    /// Full contact information
    /// </summary>
    public string ContactInfo
    {
        get
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(Email))
                parts.Add($"Email: {Email}");
                
            if (!string.IsNullOrWhiteSpace(Phone))
                parts.Add($"Тел: {Phone}");
                
            return string.Join(" | ", parts);
        }
    }
    
    /// <summary>
    /// Group display with code
    /// </summary>
    public string GroupDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(GroupName))
                return "Не назначена";
                
            return string.IsNullOrWhiteSpace(GroupCode) 
                ? GroupName 
                : $"{GroupName} ({GroupCode})";
        }
    }
    
    /// <summary>
    /// Indicates if the student is currently active
    /// </summary>
    public bool IsActive => Status == StudentStatus.Active;
    
    /// <summary>
    /// Indicates if the student has graduated
    /// </summary>
    public bool IsGraduated => Status == StudentStatus.Graduated;
    
    /// <summary>
    /// Indicates if the student is on leave
    /// </summary>
    public bool IsOnLeave => Status == StudentStatus.AcademicLeave;
    
    /// <summary>
    /// Years since enrollment
    /// </summary>
    public int YearsSinceEnrollment
    {
        get
        {
            if (!EnrollmentDate.HasValue) return 0;
            
            var today = DateTime.Today;
            var years = today.Year - EnrollmentDate.Value.Year;
            
            if (EnrollmentDate.Value.Date > today.AddYears(-years))
                years--;
                
            return Math.Max(0, years);
        }
    }

    // === CONSTRUCTORS ===
    
    /// <summary>
    /// Creates a new StudentViewModel from a Student domain model
    /// </summary>
    public StudentViewModel(Student student)
    {
        if (student == null)
            throw new ArgumentNullException(nameof(student));
            
        Uid = student.Uid;
        StudentCode = student.StudentCode ?? string.Empty;
        FirstName = student.FirstName ?? string.Empty;
        LastName = student.LastName ?? string.Empty;
        MiddleName = student.MiddleName;
        Email = student.Email ?? string.Empty;
        Phone = student.PhoneNumber ?? string.Empty;
        BirthDate = student.BirthDate;
        Address = student.Address;
        Status = student.Status;
        EnrollmentDate = student.EnrollmentDate;
        GraduationDate = student.GraduationDate;
        AcademicYear = DateTime.Now.Year;
        GPA = 0.0m;
        
        // Group information
        GroupUid = student.GroupUid;
        GroupName = student.Group?.Name ?? "Не назначена";
        
        // Department information - Group doesn't have Department property
        // DepartmentName = student.Group?.Department?.Name;
        // DepartmentCode = student.Group?.Department?.Code;
        
        // Audit information
        CreatedAt = student.CreatedAt;
        UpdatedAt = DateTime.UtcNow;
        // CreatedBy = student.CreatedBy; // Doesn't exist in domain model
        // UpdatedBy = student.UpdatedBy; // Doesn't exist in domain model
        
        // Setup reactive property change notifications
        SetupPropertyChangeNotifications();
    }
    
    /// <summary>
    /// Creates an empty StudentViewModel for new student creation
    /// </summary>
    public StudentViewModel()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        SetupPropertyChangeNotifications();
    }

    // === METHODS ===
    
    /// <summary>
    /// Sets up reactive property change notifications for computed properties
    /// </summary>
    private void SetupPropertyChangeNotifications()
    {
        // Notify when computed properties should update
        this.WhenAnyValue(x => x.FirstName, x => x.LastName, x => x.MiddleName)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(FullName));
                this.RaisePropertyChanged(nameof(Initials));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Status)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StatusDisplayName));
                this.RaisePropertyChanged(nameof(StatusColor));
                this.RaisePropertyChanged(nameof(IsActive));
                this.RaisePropertyChanged(nameof(IsGraduated));
                this.RaisePropertyChanged(nameof(IsOnLeave));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.BirthDate)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(Age));
                this.RaisePropertyChanged(nameof(AgeDisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.EnrollmentDate)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(EnrollmentDateDisplay));
                this.RaisePropertyChanged(nameof(YearsSinceEnrollment));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.GraduationDate)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(GraduationDateDisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.GPA)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(GPADisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.AcademicYear)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(AcademicYearDisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Email, x => x.Phone)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ContactInfo));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.GroupName, x => x.GroupCode)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(GroupDisplay));
            })
            .DisposeWith(Disposables);
    }
    
    /// <summary>
    /// Converts this ViewModel back to a Student domain model
    /// </summary>
    public Student ToStudent()
    {
        return new Student
        {
            Uid = Uid,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Email = Email,
            PhoneNumber = Phone ?? string.Empty,
            StudentCode = StudentCode,
            BirthDate = BirthDate ?? DateTime.Now,
            Address = Address ?? string.Empty,
            Status = Status,
            EnrollmentDate = EnrollmentDate ?? DateTime.Now,
            GraduationDate = GraduationDate,
            GroupUid = GroupUid,
            CreatedAt = CreatedAt,
        };
    }
    
    /// <summary>
    /// Updates this ViewModel from a Student domain model
    /// </summary>
    public void UpdateFromStudent(Student student)
    {
        if (student == null)
            throw new ArgumentNullException(nameof(student));
            
        if (student.Uid != Uid)
            throw new ArgumentException("Cannot update from student with different UID", nameof(student));
            
        StudentCode = student.StudentCode ?? string.Empty;
        FirstName = student.FirstName ?? string.Empty;
        LastName = student.LastName ?? string.Empty;
        MiddleName = student.MiddleName;
        Email = student.Email ?? string.Empty;
        Phone = student.PhoneNumber ?? string.Empty;
        BirthDate = student.BirthDate;
        Address = student.Address;
        Status = student.Status;
        EnrollmentDate = student.EnrollmentDate;
        GraduationDate = student.GraduationDate;
        AcademicYear = DateTime.Now.Year;
        GPA = 0.0m;
        
        // Group information
        GroupUid = student.GroupUid;
        GroupName = student.Group?.Name ?? "Не назначена";
        
        // Department information - Group doesn't have Department property
        // DepartmentName = student.Group?.Department?.Name;
        // DepartmentCode = student.Group?.Department?.Code;
        
        // Audit information
        UpdatedAt = DateTime.UtcNow;
        // CreatedBy = student.CreatedBy; // Doesn't exist in domain model
        // UpdatedBy = student.UpdatedBy; // Doesn't exist in domain model
    }
    
    /// <summary>
    /// Creates a copy of this StudentViewModel
    /// </summary>
    public StudentViewModel Clone()
    {
        return new StudentViewModel(ToStudent());
    }
    
    /// <summary>
    /// Validates the student data
    /// </summary>
    public ValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("Имя обязательно для заполнения");
            
        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Фамилия обязательна для заполнения");
            
        if (string.IsNullOrWhiteSpace(Email))
            errors.Add("Email обязателен для заполнения");
        else if (!IsValidEmail(Email))
            errors.Add("Некорректный формат email");
            
        if (string.IsNullOrWhiteSpace(StudentCode))
            errors.Add("Номер студенческого билета обязателен");
            
        // Business logic validation
        if (BirthDate.HasValue)
        {
            var age = Age;
            if (age < 16)
                warnings.Add("Возраст студента меньше 16 лет");
            else if (age > 65)
                warnings.Add("Возраст студента больше 65 лет");
        }
        
        if (EnrollmentDate.HasValue && EnrollmentDate.Value > DateTime.Today)
            errors.Add("Дата поступления не может быть в будущем");
            
        if (GraduationDate.HasValue)
        {
            if (GraduationDate.Value > DateTime.Today)
                warnings.Add("Дата выпуска в будущем");
                
            if (EnrollmentDate.HasValue && GraduationDate.Value <= EnrollmentDate.Value)
                errors.Add("Дата выпуска должна быть позже даты поступления");
        }
        
        if (GPA.HasValue && (GPA.Value < 0 || GPA.Value > 5))
            errors.Add("Средний балл должен быть от 0 до 5");
            
        if (AcademicYear < 1 || AcademicYear > 6)
            warnings.Add("Необычный курс обучения (обычно от 1 до 6)");
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }
    
    /// <summary>
    /// Validates email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            // Simple email validation without System.Net.Mail dependency
            return !string.IsNullOrWhiteSpace(email) && 
                   email.Contains("@") && 
                   email.Contains(".") &&
                   email.IndexOf("@") > 0 &&
                   email.LastIndexOf(".") > email.IndexOf("@");
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Returns a string representation of the student
    /// </summary>
    public override string ToString()
    {
        return $"{FullName} ({StudentCode}) - {StatusDisplayName}";
    }
    
    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is StudentViewModel other && Uid.Equals(other.Uid);
    }
    
    /// <summary>
    /// Returns hash code based on UID
    /// </summary>
    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }
}

/// <summary>
/// Validation result for student data
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(IEnumerable<string> errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
} 