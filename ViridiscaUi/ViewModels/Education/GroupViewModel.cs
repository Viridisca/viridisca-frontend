using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для отдельной группы с полной реактивной поддержкой
/// </summary>
public class GroupViewModel : ReactiveObject
{
    #region Core Properties

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public int Year { get; set; }
    [Reactive] public DateTime StartDate { get; set; }
    [Reactive] public DateTime? EndDate { get; set; }
    [Reactive] public int MaxStudents { get; set; }
    [Reactive] public GroupStatus Status { get; set; }
    [Reactive] public string Specialization { get; set; } = string.Empty;
    [Reactive] public int AcademicYear { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Curator Properties

    [Reactive] public Guid? CuratorUid { get; set; }
    [Reactive] public string? CuratorName { get; set; }
    [Reactive] public string? CuratorFirstName { get; set; }
    [Reactive] public string? CuratorLastName { get; set; }
    [Reactive] public string? CuratorEmail { get; set; }

    #endregion

    #region Department Properties

    [Reactive] public Guid DepartmentUid { get; set; }
    [Reactive] public string? DepartmentName { get; set; }
    [Reactive] public string? DepartmentCode { get; set; }

    #endregion

    #region Statistics Properties

    [Reactive] public int StudentsCount { get; set; }
    [Reactive] public int ActiveStudents { get; set; }
    [Reactive] public int GraduatedStudents { get; set; }
    [Reactive] public int CoursesCount { get; set; }
    [Reactive] public double AverageGPA { get; set; }
    [Reactive] public int AssignmentsCount { get; set; }

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Инициалы куратора
    /// </summary>
    public string CuratorInitials => _curatorInitials?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _curatorInitials;

    /// <summary>
    /// Отображаемое название статуса
    /// </summary>
    public string StatusDisplayName => _statusDisplayName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _statusDisplayName;

    /// <summary>
    /// Цвет статуса
    /// </summary>
    public string StatusColor => _statusColor?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _statusColor;

    /// <summary>
    /// Краткое описание (первые 100 символов)
    /// </summary>
    public string ShortDescription => _shortDescription?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _shortDescription;

    /// <summary>
    /// Форматированная дата начала
    /// </summary>
    public string FormattedStartDate => _formattedStartDate?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _formattedStartDate;

    /// <summary>
    /// Форматированная дата окончания
    /// </summary>
    public string FormattedEndDate => _formattedEndDate?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _formattedEndDate;

    /// <summary>
    /// Продолжительность обучения
    /// </summary>
    public string Duration => _duration?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _duration;

    /// <summary>
    /// Активна ли группа
    /// </summary>
    public bool IsActive => _isActive?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isActive;

    /// <summary>
    /// Завершена ли группа
    /// </summary>
    public bool IsCompleted => _isCompleted?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isCompleted;

    /// <summary>
    /// Формируется ли группа
    /// </summary>
    public bool IsForming => _isForming?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isForming;

    /// <summary>
    /// Процент заполненности
    /// </summary>
    public double FillPercentage => _fillPercentage?.Value ?? 0.0;
    private ObservableAsPropertyHelper<double>? _fillPercentage;

    /// <summary>
    /// Средний GPA в виде строки
    /// </summary>
    public string AverageGPADisplay => _averageGPADisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _averageGPADisplay;

    /// <summary>
    /// Статистика студентов
    /// </summary>
    public string StudentsStats => _studentsStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _studentsStats;

    /// <summary>
    /// Отображение департамента с кодом
    /// </summary>
    public string DepartmentDisplay => _departmentDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _departmentDisplay;

    /// <summary>
    /// Полное название группы
    /// </summary>
    public string FullName => _fullName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _fullName;

    /// <summary>
    /// Академический год в виде строки
    /// </summary>
    public string AcademicYearDisplay => _academicYearDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _academicYearDisplay;

    /// <summary>
    /// Есть ли куратор
    /// </summary>
    public bool HasCurator => _hasCurator?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _hasCurator;

    /// <summary>
    /// Можно ли редактировать группу
    /// </summary>
    public bool CanEdit => _canEdit?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canEdit;

    /// <summary>
    /// Можно ли удалить группу
    /// </summary>
    public bool CanDelete => _canDelete?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canDelete;

    /// <summary>
    /// Можно ли добавить студентов
    /// </summary>
    public bool CanAddStudents => _canAddStudents?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canAddStudents;

    /// <summary>
    /// Можно ли выпустить группу
    /// </summary>
    public bool CanGraduate => _canGraduate?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canGraduate;

    #endregion

    #region Validation Properties

    /// <summary>
    /// Ошибки валидации
    /// </summary>
    public string ValidationErrors => _validationErrors?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _validationErrors;

    /// <summary>
    /// Валидна ли группа
    /// </summary>
    public bool IsValid => _isValid?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isValid;

    #endregion

    #region Constructor

    public GroupViewModel()
    {
        // Инициализация значений по умолчанию
        Uid = Guid.NewGuid();
        StartDate = DateTime.Now;
        Status = GroupStatus.Forming;
        MaxStudents = 25;
        AcademicYear = 1;
        Year = DateTime.Now.Year;

        InitializeComputedProperties();
    }

    public GroupViewModel(Group group) : this()
    {
        UpdateFromGroup(group);
    }

    private void InitializeComputedProperties()
    {
        // Настройка computed properties
        _curatorInitials = this.WhenAnyValue(
                x => x.CuratorFirstName,
                x => x.CuratorLastName,
                (first, last) => GetInitials(first, last))
            .ToProperty(this, x => x.CuratorInitials);

        _statusDisplayName = this.WhenAnyValue(x => x.Status)
            .Select(GetStatusDisplayName)
            .ToProperty(this, x => x.StatusDisplayName);

        _statusColor = this.WhenAnyValue(x => x.Status)
            .Select(GetStatusColor)
            .ToProperty(this, x => x.StatusColor);

        _shortDescription = this.WhenAnyValue(x => x.Description)
            .Select(desc => string.IsNullOrEmpty(desc) ? string.Empty : 
                desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc)
            .ToProperty(this, x => x.ShortDescription);

        _formattedStartDate = this.WhenAnyValue(x => x.StartDate)
            .Select(date => date.ToString("dd.MM.yyyy"))
            .ToProperty(this, x => x.FormattedStartDate);

        _formattedEndDate = this.WhenAnyValue(x => x.EndDate)
            .Select(date => date?.ToString("dd.MM.yyyy") ?? "Не указана")
            .ToProperty(this, x => x.FormattedEndDate);

        _duration = this.WhenAnyValue(x => x.StartDate, x => x.EndDate)
            .Select(tuple => GetDuration(tuple.Item1, tuple.Item2))
            .ToProperty(this, x => x.Duration);

        _isActive = this.WhenAnyValue(x => x.Status)
            .Select(status => status == GroupStatus.Active)
            .ToProperty(this, x => x.IsActive);

        _isCompleted = this.WhenAnyValue(x => x.Status)
            .Select(status => status == GroupStatus.Completed) // Use Completed instead of Graduated
            .ToProperty(this, x => x.IsCompleted);

        _isForming = this.WhenAnyValue(x => x.Status)
            .Select(status => status == GroupStatus.Forming)
            .ToProperty(this, x => x.IsForming);

        _fillPercentage = this.WhenAnyValue(x => x.StudentsCount, x => x.MaxStudents)
            .Select(tuple => tuple.Item2 > 0 ? (double)tuple.Item1 / tuple.Item2 * 100 : 0)
            .ToProperty(this, x => x.FillPercentage);

        _averageGPADisplay = this.WhenAnyValue(x => x.AverageGPA)
            .Select(gpa => gpa > 0 ? $"{gpa:F2}" : "—")
            .ToProperty(this, x => x.AverageGPADisplay);

        _studentsStats = this.WhenAnyValue(
                x => x.StudentsCount,
                x => x.ActiveStudents,
                x => x.GraduatedStudents,
                (total, active, graduated) => $"{total} всего, {active} активных, {graduated} выпущено")
            .ToProperty(this, x => x.StudentsStats);

        _departmentDisplay = this.WhenAnyValue(x => x.DepartmentName, x => x.DepartmentCode)
            .Select(tuple => GetDepartmentDisplay(tuple.Item1, tuple.Item2))
            .ToProperty(this, x => x.DepartmentDisplay);

        _fullName = this.WhenAnyValue(x => x.Code, x => x.Name)
            .Select(tuple => string.IsNullOrEmpty(tuple.Item1) ? tuple.Item2 : $"{tuple.Item1} - {tuple.Item2}")
            .ToProperty(this, x => x.FullName);

        _academicYearDisplay = this.WhenAnyValue(x => x.AcademicYear)
            .Select(year => $"{year} курс")
            .ToProperty(this, x => x.AcademicYearDisplay);

        _hasCurator = this.WhenAnyValue(x => x.CuratorUid)
            .Select(curatorUid => curatorUid.HasValue && curatorUid != Guid.Empty)
            .ToProperty(this, x => x.HasCurator);

        _canEdit = this.WhenAnyValue(x => x.Status)
            .Select(status => status != GroupStatus.Archived)
            .ToProperty(this, x => x.CanEdit);

        _canDelete = this.WhenAnyValue(x => x.StudentsCount)
            .Select(studentsCount => studentsCount == 0)
            .ToProperty(this, x => x.CanDelete);

        _canAddStudents = this.WhenAnyValue(x => x.StudentsCount, x => x.MaxStudents, x => x.Status)
            .Select(tuple => tuple.Item1 < tuple.Item2 && tuple.Item3 == GroupStatus.Active)
            .ToProperty(this, x => x.CanAddStudents);

        _canGraduate = this.WhenAnyValue(x => x.Status, x => x.StudentsCount)
            .Select(tuple => tuple.Item1 == GroupStatus.Active && tuple.Item2 > 0)
            .ToProperty(this, x => x.CanGraduate);

        // Валидация
        _validationErrors = this.WhenAnyValue(
                x => x.Name,
                x => x.Code,
                x => x.Year,
                x => x.MaxStudents,
                x => x.AcademicYear,
                x => x.DepartmentUid)
            .Select(_ => GetValidationErrors())
            .ToProperty(this, x => x.ValidationErrors);

        _isValid = this.WhenAnyValue(x => x.ValidationErrors)
            .Select(errors => string.IsNullOrEmpty(errors))
            .ToProperty(this, x => x.IsValid);
    }

    #endregion

    #region Helper Methods

    private static string GetInitials(string? firstName, string? lastName)
    {
        var first = string.IsNullOrEmpty(firstName) ? "" : firstName.Substring(0, 1).ToUpper();
        var last = string.IsNullOrEmpty(lastName) ? "" : lastName.Substring(0, 1).ToUpper();
        return first + last;
    }

    private static string GetStatusDisplayName(GroupStatus status)
    {
        return status switch
        {
            GroupStatus.Forming => "Формируется",
            GroupStatus.Active => "Активная",
            GroupStatus.Suspended => "Приостановлена",
            GroupStatus.Completed => "Выпущена",
            GroupStatus.Archived => "Архивирована",
            _ => "Неизвестно"
        };
    }

    private static string GetStatusColor(GroupStatus status)
    {
        return status switch
        {
            GroupStatus.Forming => "#FF9800",
            GroupStatus.Active => "#4CAF50",
            GroupStatus.Suspended => "#F44336",
            GroupStatus.Completed => "#8BC34A",
            GroupStatus.Archived => "#607D8B",
            _ => "#9E9E9E"
        };
    }

    private static string GetDuration(DateTime startDate, DateTime? endDate)
    {
        if (!endDate.HasValue)
            return "В процессе";

        var duration = endDate.Value - startDate;
        var years = (int)(duration.TotalDays / 365);
        
        if (years >= 1)
            return $"{years} г.";
        
        var months = (int)(duration.TotalDays / 30);
        return $"{months} мес.";
    }

    private static string GetDepartmentDisplay(string? departmentName, string? departmentCode)
    {
        if (string.IsNullOrEmpty(departmentName))
            return "Не назначен";
            
        return string.IsNullOrEmpty(departmentCode) 
            ? departmentName 
            : $"{departmentName} ({departmentCode})";
    }

    private string GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название обязательно");

        if (Name?.Length > 100)
            errors.Add("Название не должно превышать 100 символов");

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код группы обязателен");

        if (Code?.Length > 20)
            errors.Add("Код группы не должен превышать 20 символов");

        if (Year < 2000 || Year > DateTime.Now.Year + 10)
            errors.Add("Некорректный год");

        if (MaxStudents <= 0)
            errors.Add("Максимальное количество студентов должно быть больше 0");

        if (MaxStudents > 100)
            errors.Add("Максимальное количество студентов не должно превышать 100");

        if (AcademicYear <= 0 || AcademicYear > 6)
            errors.Add("Академический год должен быть от 1 до 6");

        if (DepartmentUid == Guid.Empty)
            errors.Add("Департамент должен быть выбран");

        if (Description?.Length > 1000)
            errors.Add("Описание не должно превышать 1000 символов");

        if (EndDate.HasValue && EndDate <= StartDate)
            errors.Add("Дата окончания должна быть позже даты начала");

        return string.Join("; ", errors);
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Создает Group из ViewModel
    /// </summary>
    public Group ToGroup()
    {
        return new Group
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            Year = Year,
            StartDate = StartDate,
            EndDate = EndDate,
            MaxStudents = MaxStudents,
            Status = Status,
            CuratorUid = CuratorUid,
            DepartmentUid = DepartmentUid
        };
    }

    /// <summary>
    /// Обновляет ViewModel из Group
    /// </summary>
    public void UpdateFromGroup(Group group)
    {
        if (group == null) return;

        Uid = group.Uid;
        Name = group.Name ?? string.Empty;
        Code = group.Code ?? string.Empty;
        Description = group.Description ?? string.Empty;
        Year = group.Year;
        StartDate = group.StartDate;
        EndDate = group.EndDate;
        MaxStudents = group.MaxStudents;
        Status = group.Status;
        Specialization = group.Description ?? "Не указано";
        AcademicYear = group.Year;
        LastModifiedAt = group.LastModifiedAt;
        CuratorUid = group.CuratorUid;
        DepartmentUid = group.DepartmentUid;
        
        // Update curator info if available
        if (group.Curator != null)
        {
            CuratorName = group.Curator.FullName;
            CuratorFirstName = group.Curator.FirstName;
            CuratorLastName = group.Curator.LastName;
            CuratorEmail = group.Curator.Email;
        }
        
        // Update students count
        StudentsCount = group.Students?.Count ?? 0;
    }

    /// <summary>
    /// Создает копию ViewModel
    /// </summary>
    public GroupViewModel Clone()
    {
        return new GroupViewModel
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            Year = Year,
            StartDate = StartDate,
            EndDate = EndDate,
            MaxStudents = MaxStudents,
            Status = Status,
            Specialization = Specialization,
            AcademicYear = AcademicYear,
            CuratorUid = CuratorUid,
            CuratorName = CuratorName,
            CuratorFirstName = CuratorFirstName,
            CuratorLastName = CuratorLastName,
            CuratorEmail = CuratorEmail,
            DepartmentUid = DepartmentUid,
            DepartmentName = DepartmentName,
            DepartmentCode = DepartmentCode,
            StudentsCount = StudentsCount,
            ActiveStudents = ActiveStudents,
            GraduatedStudents = GraduatedStudents,
            CoursesCount = CoursesCount,
            AverageGPA = AverageGPA,
            AssignmentsCount = AssignmentsCount,
            IsSelected = IsSelected,
            IsExpanded = IsExpanded,
            IsLoading = IsLoading
        };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Создает ViewModel из Group
    /// </summary>
    public static GroupViewModel FromGroup(Group group)
    {
        var viewModel = new GroupViewModel();
        viewModel.UpdateFromGroup(group);
        return viewModel;
    }

    /// <summary>
    /// Создает пустую ViewModel для новой группы
    /// </summary>
    public static GroupViewModel CreateNew(Guid departmentUid, Guid? curatorUid = null)
    {
        return new GroupViewModel
        {
            Uid = Guid.NewGuid(),
            DepartmentUid = departmentUid,
            CuratorUid = curatorUid,
            Year = DateTime.Now.Year,
            StartDate = DateTime.Now,
            Status = GroupStatus.Forming,
            MaxStudents = 30,
            AcademicYear = 1
        };
    }

    #endregion

    #region Equality and Comparison

    public override bool Equals(object? obj)
    {
        return obj is GroupViewModel other && Uid.Equals(other.Uid);
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    public override string ToString()
    {
        return $"{FullName} - {StatusDisplayName}";
    }

    #endregion
} 