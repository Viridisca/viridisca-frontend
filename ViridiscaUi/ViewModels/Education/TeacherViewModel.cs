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
/// ViewModel для преподавателя с полной реактивной поддержкой
/// </summary>
public class TeacherViewModel : ReactiveObject
{
    #region Core Properties

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string MiddleName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string PhoneNumber { get; set; } = string.Empty;
    [Reactive] public string EmployeeCode { get; set; } = string.Empty;
    [Reactive] public string Specialization { get; set; } = string.Empty;
    [Reactive] public string Qualification { get; set; } = string.Empty;
    [Reactive] public decimal Salary { get; set; }
    [Reactive] public DateTime HireDate { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime? BirthDate { get; set; }
    [Reactive] public string Address { get; set; } = string.Empty;
    [Reactive] public string OfficeLocation { get; set; } = string.Empty;
    [Reactive] public string WorkingHours { get; set; } = string.Empty;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Связанная модель Teacher
    /// </summary>
    [Reactive] public Teacher Teacher { get; set; } = new();

    #endregion

    #region Department Properties

    [Reactive] public Guid DepartmentUid { get; set; }
    [Reactive] public string DepartmentName { get; set; } = string.Empty;

    #endregion

    #region Statistics Properties

    [Reactive] public int CoursesCount { get; set; }
    [Reactive] public int StudentsCount { get; set; }
    [Reactive] public int GroupsCount { get; set; }
    [Reactive] public double AverageGrade { get; set; }

    #endregion

    #region UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Полное имя преподавателя
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Инициалы преподавателя
    /// </summary>
    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();

    /// <summary>
    /// Отображаемое имя для списков
    /// </summary>
    public string DisplayName => $"{LastName} {FirstName.FirstOrDefault()}.{(!string.IsNullOrEmpty(MiddleName) ? $" {MiddleName.FirstOrDefault()}." : "")}";

    /// <summary>
    /// Статус активности
    /// </summary>
    public string StatusText => IsActive ? "Активен" : "Неактивен";

    /// <summary>
    /// Цвет статуса
    /// </summary>
    public string StatusColor => IsActive ? "#4CAF50" : "#F44336";

    /// <summary>
    /// Стаж работы
    /// </summary>
    public string Experience
    {
        get
        {
            var years = (DateTime.Now - HireDate).Days / 365;
            return years switch
            {
                0 => "Менее года",
                1 => "1 год",
                < 5 => $"{years} года",
                _ => $"{years} лет"
            };
        }
    }

    /// <summary>
    /// Краткая статистика
    /// </summary>
    public string StatsText => $"{CoursesCount} курсов, {StudentsCount} студентов";

    #endregion

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public TeacherViewModel()
    {
        Uid = Guid.NewGuid();
        HireDate = DateTime.Today;
    }

    /// <summary>
    /// Конструктор с преподавателем
    /// </summary>
    public TeacherViewModel(Teacher teacher) : this()
    {
        UpdateFromTeacher(teacher);
    }

    /// <summary>
    /// Обновляет ViewModel из модели Teacher
    /// </summary>
    public void UpdateFromTeacher(Teacher teacher)
    {
        if (teacher == null) return;

        Teacher = teacher;
        Uid = teacher.Uid;
        EmployeeCode = teacher.EmployeeCode;
        Specialization = teacher.Specialization ?? string.Empty;
        Qualification = teacher.Qualification ?? string.Empty;
        HireDate = teacher.HireDate;
        Salary = teacher.Salary;
        IsActive = teacher.IsActive;
        OfficeLocation = teacher.OfficeLocation ?? string.Empty;
        WorkingHours = teacher.WorkingHours ?? string.Empty;
        DepartmentUid = teacher.DepartmentUid ?? Guid.Empty;
        
        // Получаем данные из связанной модели Person
        if (teacher.Person != null)
        {
            FirstName = teacher.Person.FirstName;
            LastName = teacher.Person.LastName;
            MiddleName = teacher.Person.MiddleName ?? string.Empty;
            Email = teacher.Person.Email;
            PhoneNumber = teacher.Person.PhoneNumber ?? string.Empty;
            BirthDate = teacher.Person.DateOfBirth == DateTime.MinValue ? null : teacher.Person.DateOfBirth;
            Address = teacher.Person.Address ?? string.Empty;
        }
        
        CreatedAt = teacher.CreatedAt;
        LastModifiedAt = teacher.LastModifiedAt;
    }

    /// <summary>
    /// Обновляет ViewModel из модели Teacher (алиас для совместимости)
    /// </summary>
    public void UpdateFromModel(Teacher teacher)
    {
        UpdateFromTeacher(teacher);
    }

    /// <summary>
    /// Преобразует ViewModel в модель Teacher
    /// </summary>
    public Teacher ToTeacher()
    {
        return new Teacher
        {
            Uid = Uid,
            EmployeeCode = EmployeeCode,
            Specialization = Specialization,
            Qualification = Qualification,
            Salary = Salary,
            HireDate = HireDate,
            IsActive = IsActive,
            DepartmentUid = DepartmentUid,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создает новый экземпляр ViewModel из модели Teacher
    /// </summary>
    public static TeacherViewModel FromTeacher(Teacher teacher)
    {
        return new TeacherViewModel(teacher);
    }

    /// <summary>
    /// Клонирует ViewModel
    /// </summary>
    public TeacherViewModel Clone()
    {
        return new TeacherViewModel
        {
            Uid = Uid,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            EmployeeCode = EmployeeCode,
            Specialization = Specialization,
            Qualification = Qualification,
            Salary = Salary,
            HireDate = HireDate,
            IsActive = IsActive,
            DepartmentUid = DepartmentUid,
            DepartmentName = DepartmentName,
            CoursesCount = CoursesCount,
            StudentsCount = StudentsCount,
            GroupsCount = GroupsCount,
            AverageGrade = AverageGrade
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is TeacherViewModel other && Uid.Equals(other.Uid);
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    public override string ToString()
    {
        return $"{FullName} ({EmployeeCode})";
    }
} 