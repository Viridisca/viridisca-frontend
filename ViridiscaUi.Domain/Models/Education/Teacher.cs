using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Преподаватель
/// </summary>
public class Teacher : ViewModelBase
{
    private Guid _personUid;
    private string _employeeCode = string.Empty;
    private DateTime _hireDate;
    private DateTime? _terminationDate;
    private string _qualification = string.Empty;
    private string _specialization = string.Empty;
    private decimal _salary;
    private bool _isActive = true;
    private string _officeLocation = string.Empty;
    private string _workingHours = string.Empty;

    private Guid? _departmentUid;

    private Person? _person;
    private Department? _department;

    private ObservableCollection<CourseInstance> _courseInstances = [];

    /// <summary>
    /// ID связанного человека
    /// </summary>
    public Guid PersonUid
    {
        get => _personUid;
        set => this.RaiseAndSetIfChanged(ref _personUid, value);
    }

    /// <summary>
    /// Код сотрудника
    /// </summary>
    public string EmployeeCode
    {
        get => _employeeCode;
        set => this.RaiseAndSetIfChanged(ref _employeeCode, value);
    }

    /// <summary>
    /// Дата найма
    /// </summary>
    public DateTime HireDate
    {
        get => _hireDate;
        set => this.RaiseAndSetIfChanged(ref _hireDate, value);
    }

    /// <summary>
    /// Дата увольнения
    /// </summary>
    public DateTime? TerminationDate
    {
        get => _terminationDate;
        set => this.RaiseAndSetIfChanged(ref _terminationDate, value);
    }

    /// <summary>
    /// Квалификация
    /// </summary>
    public string Qualification
    {
        get => _qualification;
        set => this.RaiseAndSetIfChanged(ref _qualification, value);
    }

    /// <summary>
    /// Специализация
    /// </summary>
    public string Specialization
    {
        get => _specialization;
        set => this.RaiseAndSetIfChanged(ref _specialization, value);
    }

    /// <summary>
    /// Зарплата
    /// </summary>
    public decimal Salary
    {
        get => _salary;
        set => this.RaiseAndSetIfChanged(ref _salary, value);
    }

    /// <summary>
    /// Активен ли преподаватель
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Расположение офиса
    /// </summary>
    public string OfficeLocation
    {
        get => _officeLocation;
        set => this.RaiseAndSetIfChanged(ref _officeLocation, value);
    }

    /// <summary>
    /// Рабочие часы
    /// </summary>
    public string WorkingHours
    {
        get => _workingHours;
        set => this.RaiseAndSetIfChanged(ref _workingHours, value);
    }

    /// <summary>
    /// ID департамента
    /// </summary>
    public Guid? DepartmentUid
    {
        get => _departmentUid;
        set => this.RaiseAndSetIfChanged(ref _departmentUid, value);
    }

    /// <summary>
    /// Связанный человек
    /// </summary>
    public Person? Person
    {
        get => _person;
        set => this.RaiseAndSetIfChanged(ref _person, value);
    }

    /// <summary>
    /// Департамент
    /// </summary>
    public Department? Department
    {
        get => _department;
        set => this.RaiseAndSetIfChanged(ref _department, value);
    }

    /// <summary>
    /// Экземпляры курсов, которые ведет преподаватель
    /// </summary>
    public ObservableCollection<CourseInstance> CourseInstances
    {
        get => _courseInstances;
        set => this.RaiseAndSetIfChanged(ref _courseInstances, value);
    }

    /// <summary>
    /// Полное имя преподавателя
    /// </summary>
    public string FullName => Person?.FullName ?? "Неизвестный преподаватель";

    /// <summary>
    /// Email преподавателя
    /// </summary>
    public string Email => Person?.Email ?? string.Empty;

    /// <summary>
    /// Имя (получается из Person)
    /// </summary>
    public string FirstName => Person?.FirstName ?? string.Empty;
    
    /// <summary>
    /// Фамилия (получается из Person)
    /// </summary>
    public string LastName => Person?.LastName ?? string.Empty;
    
    /// <summary>
    /// Отчество (получается из Person)
    /// </summary>
    public string MiddleName => Person?.MiddleName ?? string.Empty;
    
    /// <summary>
    /// Академическая степень
    /// </summary>
    public string AcademicDegree => Qualification;
    
    /// <summary>
    /// Академическое звание
    /// </summary>
    public string AcademicTitle => Specialization;
    
    /// <summary>
    /// Почасовая ставка (вычисляется из зарплаты)
    /// </summary>
    public decimal HourlyRate => Salary / 160; // Примерно 160 часов в месяц
    
    /// <summary>
    /// Статус преподавателя
    /// </summary>
    public string Status => IsActive ? "Active" : "Inactive";

    public Teacher()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        HireDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет данные преподавателя
    /// </summary>
    public void UpdateDetails(
        string specialization,
        decimal salary,
        string? qualification,
        string? officeLocation,
        string? workingHours)
    {
        Specialization = specialization;
        Salary = salary;
        Qualification = qualification ?? string.Empty;
        OfficeLocation = officeLocation ?? string.Empty;
        WorkingHours = workingHours ?? string.Empty;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет статус преподавателя
    /// </summary>
    public void UpdateStatus(bool isActive)
    {
        IsActive = isActive;
        
        if (!IsActive && !TerminationDate.HasValue)
        {
            TerminationDate = DateTime.UtcNow;
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Устанавливает дату увольнения
    /// </summary>
    public void SetTerminationDate(DateTime terminationDate)
    {
        if (terminationDate <= HireDate)
            return;

        TerminationDate = terminationDate;
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }
}