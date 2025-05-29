using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.ViewModels;
using System.Text.RegularExpressions;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual teacher with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
public class TeacherViewModel : ViewModelBase
{
    // === CORE PROPERTIES ===
    
    public Guid Uid { get; }
    
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string? MiddleName { get; set; }
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string? Phone { get; set; }
    [Reactive] public string? Address { get; set; }
    [Reactive] public DateTime? BirthDate { get; set; }
    [Reactive] public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    [Reactive] public DateTime? HireDate { get; set; }
    [Reactive] public DateTime? TerminationDate { get; set; }
    [Reactive] public string? Specialization { get; set; }
    [Reactive] public string? AcademicDegree { get; set; }
    [Reactive] public string? AcademicTitle { get; set; }
    [Reactive] public decimal? Salary { get; set; }
    [Reactive] public string? OfficeNumber { get; set; }
    [Reactive] public string? Biography { get; set; }
    
    // Department Information
    [Reactive] public Guid? DepartmentUid { get; set; }
    [Reactive] public string? DepartmentName { get; set; }
    [Reactive] public string? DepartmentCode { get; set; }
    
    // Statistics
    [Reactive] public int CoursesCount { get; set; }
    [Reactive] public int CuratedGroupsCount { get; set; }
    [Reactive] public int StudentsCount { get; set; }
    [Reactive] public int PublicationsCount { get; set; }
    
    // Selection State for DataGrid
    [Reactive] public bool IsSelected { get; set; }
    
    // Audit Information
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; }
    public Guid? UpdatedBy { get; private set; }

    // === COMPUTED PROPERTIES ===
    
    /// <summary>
    /// Full name of the teacher
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
    /// Display name for status with localization
    /// </summary>
    public string StatusDisplayName => Status switch
    {
        TeacherStatus.Active => "Активный",
        TeacherStatus.Inactive => "Неактивный",
        TeacherStatus.OnLeave => "В отпуске",
        TeacherStatus.Terminated => "Уволен",
        TeacherStatus.Retired => "На пенсии",
        TeacherStatus.Suspended => "Отстранен",
        _ => "Неизвестно"
    };
    
    /// <summary>
    /// Color for status badge
    /// </summary>
    public string StatusColor => Status switch
    {
        TeacherStatus.Active => "#4CAF50",      // Green
        TeacherStatus.Inactive => "#FF9800",    // Orange
        TeacherStatus.OnLeave => "#9C27B0",     // Purple
        TeacherStatus.Terminated => "#F44336",  // Red
        TeacherStatus.Retired => "#607D8B",     // Blue Grey
        TeacherStatus.Suspended => "#FF5722",   // Deep Orange
        _ => "#757575"                          // Grey
    };
    
    /// <summary>
    /// Age of the teacher
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
    /// Formatted hire date
    /// </summary>
    public string HireDateDisplay => HireDate?.ToString("dd.MM.yyyy") ?? "Не указана";
    
    /// <summary>
    /// Formatted termination date
    /// </summary>
    public string TerminationDateDisplay => TerminationDate?.ToString("dd.MM.yyyy") ?? "Не указана";
    
    /// <summary>
    /// Years of experience
    /// </summary>
    public int ExperienceYears
    {
        get
        {
            if (!HireDate.HasValue) return 0;
            
            var endDate = TerminationDate ?? DateTime.Today;
            var years = endDate.Year - HireDate.Value.Year;
            
            if (HireDate.Value.Date > endDate.AddYears(-years))
                years--;
                
            return Math.Max(0, years);
        }
    }
    
    /// <summary>
    /// Experience display
    /// </summary>
    public string ExperienceDisplay => $"{ExperienceYears} лет";
    
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
                
            if (!string.IsNullOrWhiteSpace(OfficeNumber))
                parts.Add($"Каб: {OfficeNumber}");
                
            return string.Join(" | ", parts);
        }
    }
    
    /// <summary>
    /// Department display with code
    /// </summary>
    public string DepartmentDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DepartmentName))
                return "Не назначена";
                
            return string.IsNullOrWhiteSpace(DepartmentCode) 
                ? DepartmentName 
                : $"{DepartmentName} ({DepartmentCode})";
        }
    }
    
    /// <summary>
    /// Academic credentials display
    /// </summary>
    public string AcademicCredentials
    {
        get
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(AcademicDegree))
                parts.Add(AcademicDegree);
                
            if (!string.IsNullOrWhiteSpace(AcademicTitle))
                parts.Add(AcademicTitle);
                
            return parts.Any() ? string.Join(", ", parts) : "Не указано";
        }
    }
    
    /// <summary>
    /// Formatted salary display
    /// </summary>
    public string SalaryDisplay => Salary.HasValue ? $"{Salary:C}" : "Не указана";
    
    /// <summary>
    /// Indicates if the teacher is currently active
    /// </summary>
    public bool IsActive => Status == TeacherStatus.Active;
    
    /// <summary>
    /// Indicates if the teacher is on leave
    /// </summary>
    public bool IsOnLeave => Status == TeacherStatus.OnLeave;
    
    /// <summary>
    /// Indicates if the teacher is terminated
    /// </summary>
    public bool IsTerminated => Status == TeacherStatus.Terminated;
    
    /// <summary>
    /// Indicates if the teacher is retired
    /// </summary>
    public bool IsRetired => Status == TeacherStatus.Retired;
    
    /// <summary>
    /// Teaching load summary
    /// </summary>
    public string TeachingLoadSummary => $"{CoursesCount} курсов, {CuratedGroupsCount} групп, {StudentsCount} студентов";
    
    /// <summary>
    /// Research activity summary
    /// </summary>
    public string ResearchSummary => $"{PublicationsCount} публикаций";

    // === CONSTRUCTORS ===
    
    /// <summary>
    /// Creates a new TeacherViewModel from a Teacher domain model
    /// </summary>
    public TeacherViewModel(Teacher teacher)
    {
        if (teacher == null)
            throw new ArgumentNullException(nameof(teacher));
            
        Uid = teacher.Uid;
        FirstName = teacher.FirstName ?? string.Empty;
        LastName = teacher.LastName ?? string.Empty;
        MiddleName = teacher.MiddleName;
        Email = teacher.Email ?? string.Empty;
        Phone = teacher.Phone;
        // Address = teacher.Address; // Не существует в модели
        // BirthDate = teacher.BirthDate; // Не существует в модели
        Status = teacher.Status;
        HireDate = teacher.HireDate;
        TerminationDate = teacher.TerminationDate;
        Specialization = teacher.Specialization;
        AcademicDegree = teacher.AcademicDegree;
        AcademicTitle = teacher.AcademicTitle;
        Salary = teacher.HourlyRate; // HourlyRate is decimal, Salary is decimal?, direct assignment works
        // OfficeNumber = teacher.OfficeNumber; // Не существует в модели
        Biography = teacher.Bio; // Используем Bio вместо Biography
        
        // Department information
        DepartmentUid = teacher.DepartmentUid;
        // DepartmentName = teacher.Department?.Name; // Department - это строка, не объект
        // DepartmentCode = teacher.Department?.Code; // Department - это строка, не объект
        
        // Statistics
        CoursesCount = teacher.Courses?.Count ?? 0;
        CuratedGroupsCount = teacher.CuratedGroups?.Count ?? 0;
        StudentsCount = teacher.Courses?.SelectMany(c => c.Enrollments ?? new List<Enrollment>()).Count() ?? 0;
        // PublicationsCount = teacher.Publications?.Count ?? 0; // Не существует в модели
        
        // Audit information
        CreatedAt = teacher.CreatedAt;
        UpdatedAt = DateTime.UtcNow; // Используем текущее время вместо несуществующего свойства
        // CreatedBy = teacher.CreatedBy; // Не существует в модели
        // UpdatedBy = teacher.UpdatedBy; // Не существует в модели
        
        // Setup reactive property change notifications
        SetupPropertyChangeNotifications();
    }
    
    /// <summary>
    /// Creates an empty TeacherViewModel for new teacher creation
    /// </summary>
    public TeacherViewModel()
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
                this.RaisePropertyChanged(nameof(IsOnLeave));
                this.RaisePropertyChanged(nameof(IsTerminated));
                this.RaisePropertyChanged(nameof(IsRetired));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.BirthDate)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(Age));
                this.RaisePropertyChanged(nameof(AgeDisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.HireDate, x => x.TerminationDate)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(HireDateDisplay));
                this.RaisePropertyChanged(nameof(TerminationDateDisplay));
                this.RaisePropertyChanged(nameof(ExperienceYears));
                this.RaisePropertyChanged(nameof(ExperienceDisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Salary)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(SalaryDisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Email, x => x.Phone, x => x.OfficeNumber)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ContactInfo));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.DepartmentName, x => x.DepartmentCode)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(DepartmentDisplay));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.AcademicDegree, x => x.AcademicTitle)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(AcademicCredentials));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.CoursesCount, x => x.CuratedGroupsCount, x => x.StudentsCount)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(TeachingLoadSummary));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.PublicationsCount)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ResearchSummary));
            })
            .DisposeWith(Disposables);
    }
    
    /// <summary>
    /// Converts this ViewModel back to a Teacher domain model
    /// </summary>
    public Teacher ToTeacher()
    {
        return new Teacher
        {
            Uid = Uid,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            // Email = Email, // Email только для чтения
            Phone = Phone,
            // Address = Address, // Не существует в модели
            // BirthDate = BirthDate, // Не существует в модели
            Status = Status,
            HireDate = HireDate ?? DateTime.UtcNow, // HireDate не nullable в модели
            TerminationDate = TerminationDate,
            Specialization = Specialization,
            AcademicDegree = AcademicDegree,
            AcademicTitle = AcademicTitle,
            HourlyRate = Salary ?? 0m, // Используем Salary как HourlyRate, с явным значением по умолчанию
            // UpdatedAt = UpdatedAt, // Не существует в модели
            // UpdatedBy = UpdatedBy, // Не существует в модели
            Bio = Biography, // Используем Bio вместо Biography
            DepartmentUid = DepartmentUid,
            CreatedAt = CreatedAt,
            // UpdatedAt = DateTime.UtcNow, // Не существует в модели
            // CreatedBy = CreatedBy, // Не существует в модели
            // UpdatedBy = UpdatedBy // Не существует в модели
        };
    }
    
    /// <summary>
    /// Updates this ViewModel from a Teacher domain model
    /// </summary>
    public void UpdateFromTeacher(Teacher teacher)
    {
        if (teacher == null)
            throw new ArgumentNullException(nameof(teacher));
            
        if (teacher.Uid != Uid)
            throw new ArgumentException("Cannot update from teacher with different UID", nameof(teacher));
            
        FirstName = teacher.FirstName ?? string.Empty;
        LastName = teacher.LastName ?? string.Empty;
        MiddleName = teacher.MiddleName;
        Email = teacher.Email ?? string.Empty;
        Phone = teacher.Phone;
        // Address = teacher.Address; // Не существует в модели
        // BirthDate = teacher.BirthDate; // Не существует в модели
        Status = teacher.Status;
        HireDate = teacher.HireDate;
        TerminationDate = teacher.TerminationDate;
        Specialization = teacher.Specialization;
        AcademicDegree = teacher.AcademicDegree;
        AcademicTitle = teacher.AcademicTitle;
        Salary = teacher.HourlyRate; // HourlyRate is decimal, Salary is decimal?, direct assignment works
        // OfficeNumber = teacher.OfficeNumber; // Не существует в модели
        Biography = teacher.Bio; // Используем Bio вместо Biography
        
        // Department information
        DepartmentUid = teacher.DepartmentUid;
        // DepartmentName = teacher.Department?.Name; // Department - это строка, не объект
        // DepartmentCode = teacher.Department?.Code; // Department - это строка, не объект
        
        // Statistics
        CoursesCount = teacher.Courses?.Count ?? 0;
        CuratedGroupsCount = teacher.CuratedGroups?.Count ?? 0;
        StudentsCount = teacher.Courses?.SelectMany(c => c.Enrollments ?? new List<Enrollment>()).Count() ?? 0;
        // PublicationsCount = teacher.Publications?.Count ?? 0; // Не существует в модели
        
        // Audit information
        UpdatedAt = DateTime.UtcNow; // Используем текущее время
        // UpdatedBy = teacher.UpdatedBy; // Не существует в модели
    }
    
    /// <summary>
    /// Creates a copy of this TeacherViewModel
    /// </summary>
    public TeacherViewModel Clone()
    {
        return new TeacherViewModel(ToTeacher());
    }
    
    /// <summary>
    /// Validates the teacher data
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
            
        // Business logic validation
        if (BirthDate.HasValue)
        {
            var age = Age;
            if (age < 18)
                errors.Add("Возраст преподавателя должен быть не менее 18 лет");
            else if (age > 80)
                warnings.Add("Возраст преподавателя больше 80 лет");
        }
        
        if (HireDate.HasValue && HireDate.Value > DateTime.Today)
            errors.Add("Дата найма не может быть в будущем");
            
        if (TerminationDate.HasValue)
        {
            if (TerminationDate.Value > DateTime.Today)
                warnings.Add("Дата увольнения в будущем");
                
            if (HireDate.HasValue && TerminationDate.Value <= HireDate.Value)
                errors.Add("Дата увольнения должна быть позже даты найма");
        }
        
        if (Salary.HasValue && Salary.Value < 0)
            errors.Add("Зарплата не может быть отрицательной");
            
        if (string.IsNullOrWhiteSpace(Specialization))
            warnings.Add("Рекомендуется указать специализацию");
        
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
            return email.Contains("@") && email.Contains(".");
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Returns a string representation of the teacher
    /// </summary>
    public override string ToString()
    {
        return $"{FullName} ({Specialization}) - {StatusDisplayName}";
    }
    
    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is TeacherViewModel other && Uid.Equals(other.Uid);
    }
    
    /// <summary>
    /// Returns hash code based on UID
    /// </summary>
    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }
} 