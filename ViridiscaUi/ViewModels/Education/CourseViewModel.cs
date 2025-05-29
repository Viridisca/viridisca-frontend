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
/// ViewModel для отдельного курса с полной реактивной поддержкой
/// </summary>
public class CourseViewModel : ReactiveObject
{
    #region Core Properties

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string Category { get; set; } = string.Empty;
    [Reactive] public CourseStatus Status { get; set; }
    [Reactive] public DateTime StartDate { get; set; }
    [Reactive] public DateTime EndDate { get; set; }
    [Reactive] public int Credits { get; set; }
    [Reactive] public int MaxEnrollments { get; set; }
    [Reactive] public string Prerequisites { get; set; } = string.Empty;
    [Reactive] public string LearningOutcomes { get; set; } = string.Empty;
    [Reactive] public CourseDifficulty Difficulty { get; set; }

    #endregion

    #region Teacher Properties

    [Reactive] public Guid TeacherUid { get; set; }
    [Reactive] public string TeacherName { get; set; } = string.Empty;
    [Reactive] public string TeacherFirstName { get; set; } = string.Empty;
    [Reactive] public string TeacherLastName { get; set; } = string.Empty;
    [Reactive] public string TeacherEmail { get; set; } = string.Empty;

    #endregion

    #region Department Properties

    [Reactive] public Guid? DepartmentUid { get; set; }
    [Reactive] public string? DepartmentName { get; set; }
    [Reactive] public string? DepartmentCode { get; set; }

    #endregion

    #region Statistics Properties

    [Reactive] public int EnrollmentsCount { get; set; }
    [Reactive] public int AssignmentsCount { get; set; }
    [Reactive] public int CompletedAssignments { get; set; }
    [Reactive] public double AverageGrade { get; set; }
    [Reactive] public int ActiveStudents { get; set; }
    [Reactive] public int CompletedStudents { get; set; }

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Инициалы преподавателя
    /// </summary>
    public string TeacherInitials => _teacherInitials?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _teacherInitials;

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
    /// Отображаемое название сложности
    /// </summary>
    public string DifficultyDisplayName => _difficultyDisplayName?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _difficultyDisplayName;

    /// <summary>
    /// Цвет сложности
    /// </summary>
    public string DifficultyColor => _difficultyColor?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _difficultyColor;

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
    /// Продолжительность курса
    /// </summary>
    public string Duration => _duration?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _duration;

    /// <summary>
    /// Количество недель
    /// </summary>
    public int WeeksCount => _weeksCount?.Value ?? 0;
    private ObservableAsPropertyHelper<int>? _weeksCount;

    /// <summary>
    /// Активен ли курс
    /// </summary>
    public bool IsActive => _isActive?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isActive;

    /// <summary>
    /// Завершен ли курс
    /// </summary>
    public bool IsCompleted => _isCompleted?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isCompleted;

    /// <summary>
    /// Начался ли курс
    /// </summary>
    public bool HasStarted => _hasStarted?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _hasStarted;

    /// <summary>
    /// Процент заполненности
    /// </summary>
    public double FillPercentage => _fillPercentage?.Value ?? 0.0;
    private ObservableAsPropertyHelper<double>? _fillPercentage;

    /// <summary>
    /// Процент завершения
    /// </summary>
    public double CompletionRate => _completionRate?.Value ?? 0.0;
    private ObservableAsPropertyHelper<double>? _completionRate;

    /// <summary>
    /// Средний балл в виде строки
    /// </summary>
    public string AverageGradeDisplay => _averageGradeDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _averageGradeDisplay;

    /// <summary>
    /// Статистика студентов
    /// </summary>
    public string StudentsStats => _studentsStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _studentsStats;

    /// <summary>
    /// Статистика заданий
    /// </summary>
    public string AssignmentsStats => _assignmentsStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _assignmentsStats;

    /// <summary>
    /// Есть ли предварительные требования
    /// </summary>
    public bool HasPrerequisites => _hasPrerequisites?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _hasPrerequisites;

    /// <summary>
    /// Можно ли редактировать курс
    /// </summary>
    public bool CanEdit => _canEdit?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canEdit;

    /// <summary>
    /// Можно ли удалить курс
    /// </summary>
    public bool CanDelete => _canDelete?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canDelete;

    /// <summary>
    /// Можно ли записаться на курс
    /// </summary>
    public bool CanEnroll => _canEnroll?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canEnroll;

    /// <summary>
    /// Ошибки валидации
    /// </summary>
    public string ValidationErrors => _validationErrors?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _validationErrors;

    /// <summary>
    /// Валиден ли курс
    /// </summary>
    public bool IsValid => _isValid?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isValid;

    #endregion

    #region Constructor

    public CourseViewModel()
    {
        // Инициализация значений по умолчанию
        Uid = Guid.NewGuid();
        StartDate = DateTime.Now;
        EndDate = DateTime.Now.AddMonths(4);
        Status = CourseStatus.Draft;
        Credits = 3;
        MaxEnrollments = 30;
        Difficulty = CourseDifficulty.Beginner;

        InitializeComputedProperties();
    }

    public CourseViewModel(Course course) : this()
    {
        UpdateFromCourse(course);
    }

    private void InitializeComputedProperties()
    {
        // Настройка computed properties
        _teacherInitials = this.WhenAnyValue(
                x => x.TeacherFirstName,
                x => x.TeacherLastName,
                (first, last) => GetInitials(first, last))
            .ToProperty(this, x => x.TeacherInitials);

        _statusDisplayName = this.WhenAnyValue(x => x.Status)
            .Select(GetStatusDisplayName)
            .ToProperty(this, x => x.StatusDisplayName);

        _statusColor = this.WhenAnyValue(x => x.Status)
            .Select(GetStatusColor)
            .ToProperty(this, x => x.StatusColor);

        _difficultyDisplayName = this.WhenAnyValue(x => x.Difficulty)
            .Select(GetDifficultyDisplayName)
            .ToProperty(this, x => x.DifficultyDisplayName);

        _difficultyColor = this.WhenAnyValue(x => x.Difficulty)
            .Select(GetDifficultyColor)
            .ToProperty(this, x => x.DifficultyColor);

        _shortDescription = this.WhenAnyValue(x => x.Description)
            .Select(desc => string.IsNullOrEmpty(desc) ? string.Empty : 
                desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc)
            .ToProperty(this, x => x.ShortDescription);

        _formattedStartDate = this.WhenAnyValue(x => x.StartDate)
            .Select(date => date.ToString("dd.MM.yyyy"))
            .ToProperty(this, x => x.FormattedStartDate);

        _formattedEndDate = this.WhenAnyValue(x => x.EndDate)
            .Select(date => date.ToString("dd.MM.yyyy"))
            .ToProperty(this, x => x.FormattedEndDate);

        _duration = this.WhenAnyValue(x => x.StartDate, x => x.EndDate)
            .Select(tuple => GetDuration(tuple.Item1, tuple.Item2))
            .ToProperty(this, x => x.Duration);

        _weeksCount = this.WhenAnyValue(x => x.StartDate, x => x.EndDate)
            .Select(tuple => (int)Math.Ceiling((tuple.Item2 - tuple.Item1).TotalDays / 7))
            .ToProperty(this, x => x.WeeksCount);

        _isActive = this.WhenAnyValue(x => x.Status)
            .Select(status => status == CourseStatus.Active)
            .ToProperty(this, x => x.IsActive);

        _isCompleted = this.WhenAnyValue(x => x.Status)
            .Select(status => status == CourseStatus.Completed)
            .ToProperty(this, x => x.IsCompleted);

        _hasStarted = this.WhenAnyValue(x => x.StartDate)
            .Select(startDate => startDate <= DateTime.Now)
            .ToProperty(this, x => x.HasStarted);

        _fillPercentage = this.WhenAnyValue(x => x.EnrollmentsCount, x => x.MaxEnrollments)
            .Select(tuple => tuple.Item2 > 0 ? (double)tuple.Item1 / tuple.Item2 * 100 : 0)
            .ToProperty(this, x => x.FillPercentage);

        _completionRate = this.WhenAnyValue(x => x.CompletedStudents, x => x.EnrollmentsCount)
            .Select(tuple => tuple.Item2 > 0 ? (double)tuple.Item1 / tuple.Item2 * 100 : 0)
            .ToProperty(this, x => x.CompletionRate);

        _averageGradeDisplay = this.WhenAnyValue(x => x.AverageGrade)
            .Select(grade => grade > 0 ? $"{grade:F1}" : "—")
            .ToProperty(this, x => x.AverageGradeDisplay);

        _studentsStats = this.WhenAnyValue(
                x => x.EnrollmentsCount,
                x => x.ActiveStudents,
                x => x.CompletedStudents,
                (total, active, completed) => $"{total} всего, {active} активных, {completed} завершили")
            .ToProperty(this, x => x.StudentsStats);

        _assignmentsStats = this.WhenAnyValue(
                x => x.AssignmentsCount,
                x => x.CompletedAssignments,
                (total, completed) => $"{total} всего, {completed} завершено")
            .ToProperty(this, x => x.AssignmentsStats);

        _hasPrerequisites = this.WhenAnyValue(x => x.Prerequisites)
            .Select(prereq => !string.IsNullOrEmpty(prereq))
            .ToProperty(this, x => x.HasPrerequisites);

        _canEdit = this.WhenAnyValue(x => x.Status)
            .Select(status => status != CourseStatus.Completed && status != CourseStatus.Archived)
            .ToProperty(this, x => x.CanEdit);

        _canDelete = this.WhenAnyValue(x => x.Status, x => x.EnrollmentsCount)
            .Select(tuple => tuple.Item1 == CourseStatus.Draft || tuple.Item2 == 0)
            .ToProperty(this, x => x.CanDelete);

        _canEnroll = this.WhenAnyValue(x => x.Status, x => x.EnrollmentsCount, x => x.MaxEnrollments)
            .Select(tuple => tuple.Item1 == CourseStatus.Active && tuple.Item2 < tuple.Item3)
            .ToProperty(this, x => x.CanEnroll);

        // Валидация
        _validationErrors = this.WhenAnyValue(
                x => x.Name,
                x => x.Code,
                x => x.StartDate,
                x => x.EndDate,
                x => x.Credits,
                x => x.MaxEnrollments)
            .Select(_ => GetValidationErrors())
            .ToProperty(this, x => x.ValidationErrors);

        _isValid = this.WhenAnyValue(x => x.ValidationErrors)
            .Select(errors => string.IsNullOrEmpty(errors))
            .ToProperty(this, x => x.IsValid);
    }

    #endregion

    #region Helper Methods

    private static string GetInitials(string firstName, string lastName)
    {
        var first = string.IsNullOrEmpty(firstName) ? "" : firstName.Substring(0, 1).ToUpper();
        var last = string.IsNullOrEmpty(lastName) ? "" : lastName.Substring(0, 1).ToUpper();
        return first + last;
    }

    private static string GetStatusDisplayName(CourseStatus status)
    {
        return status switch
        {
            CourseStatus.Draft => "Черновик",
            CourseStatus.Published => "Опубликован",
            CourseStatus.Active => "Активный",
            CourseStatus.Completed => "Завершен",
            CourseStatus.Suspended => "Приостановлен",
            CourseStatus.Archived => "Архивирован",
            _ => "Неизвестно"
        };
    }

    private static string GetStatusColor(CourseStatus status)
    {
        return status switch
        {
            CourseStatus.Draft => "#9E9E9E",
            CourseStatus.Published => "#2196F3",
            CourseStatus.Active => "#4CAF50",
            CourseStatus.Completed => "#8BC34A",
            CourseStatus.Suspended => "#FF9800",
            CourseStatus.Archived => "#607D8B",
            _ => "#9E9E9E"
        };
    }

    private static string GetDifficultyDisplayName(CourseDifficulty difficulty)
    {
        return difficulty switch
        {
            CourseDifficulty.Beginner => "Начальный",
            CourseDifficulty.Intermediate => "Средний",
            CourseDifficulty.Advanced => "Продвинутый",
            CourseDifficulty.Expert => "Экспертный",
            _ => "Неизвестно"
        };
    }

    private static string GetDifficultyColor(CourseDifficulty difficulty)
    {
        return difficulty switch
        {
            CourseDifficulty.Beginner => "#4CAF50",
            CourseDifficulty.Intermediate => "#FFC107",
            CourseDifficulty.Advanced => "#FF9800",
            CourseDifficulty.Expert => "#F44336",
            _ => "#9E9E9E"
        };
    }

    private static string GetDuration(DateTime startDate, DateTime endDate)
    {
        var duration = endDate - startDate;
        var days = (int)duration.TotalDays;
        
        if (days < 7)
            return $"{days} дн.";
        
        if (days < 30)
            return $"{days / 7} нед.";
        
        return $"{days / 30} мес.";
    }

    private string GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название обязательно");

        if (Name?.Length > 200)
            errors.Add("Название не должно превышать 200 символов");

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код курса обязателен");

        if (Code?.Length > 20)
            errors.Add("Код курса не должен превышать 20 символов");

        if (StartDate >= EndDate)
            errors.Add("Дата начала должна быть раньше даты окончания");

        if (Credits <= 0)
            errors.Add("Количество кредитов должно быть больше 0");

        if (Credits > 10)
            errors.Add("Количество кредитов не должно превышать 10");

        if (MaxEnrollments <= 0)
            errors.Add("Максимальное количество записей должно быть больше 0");

        if (MaxEnrollments > 1000)
            errors.Add("Максимальное количество записей не должно превышать 1000");

        if (TeacherUid == Guid.Empty)
            errors.Add("Преподаватель должен быть выбран");

        if (Description?.Length > 5000)
            errors.Add("Описание не должно превышать 5000 символов");

        return string.Join("; ", errors);
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Создает Course из ViewModel
    /// </summary>
    public Course ToCourse()
    {
        return new Course
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            Category = Category,
            TeacherUid = TeacherUid,
            StartDate = StartDate,
            EndDate = EndDate,
            Credits = Credits,
            Status = Status,
            Prerequisites = Prerequisites,
            LearningOutcomes = LearningOutcomes,
            MaxEnrollments = MaxEnrollments
        };
    }

    /// <summary>
    /// Обновляет ViewModel из Course
    /// </summary>
    public void UpdateFromCourse(Course course)
    {
        Uid = course.Uid;
        Name = course.Name;
        Code = course.Code;
        Description = course.Description;
        Category = course.Category;
        TeacherUid = course.TeacherUid ?? Guid.Empty;
        StartDate = course.StartDate ?? DateTime.Now;
        EndDate = course.EndDate ?? DateTime.Now.AddMonths(3);
        Credits = course.Credits;
        Status = course.Status;
        Prerequisites = course.Prerequisites;
        LearningOutcomes = course.LearningOutcomes;
        MaxEnrollments = course.MaxEnrollments;
    }

    /// <summary>
    /// Создает копию ViewModel
    /// </summary>
    public CourseViewModel Clone()
    {
        return new CourseViewModel
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            Category = Category,
            Status = Status,
            StartDate = StartDate,
            EndDate = EndDate,
            Credits = Credits,
            MaxEnrollments = MaxEnrollments,
            Prerequisites = Prerequisites,
            LearningOutcomes = LearningOutcomes,
            Difficulty = Difficulty,
            TeacherUid = TeacherUid,
            TeacherName = TeacherName,
            TeacherFirstName = TeacherFirstName,
            TeacherLastName = TeacherLastName,
            TeacherEmail = TeacherEmail,
            DepartmentUid = DepartmentUid,
            DepartmentName = DepartmentName,
            DepartmentCode = DepartmentCode,
            EnrollmentsCount = EnrollmentsCount,
            AssignmentsCount = AssignmentsCount,
            CompletedAssignments = CompletedAssignments,
            AverageGrade = AverageGrade,
            ActiveStudents = ActiveStudents,
            CompletedStudents = CompletedStudents,
            IsSelected = IsSelected,
            IsExpanded = IsExpanded,
            IsLoading = IsLoading
        };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Создает ViewModel из Course
    /// </summary>
    public static CourseViewModel FromCourse(Course course)
    {
        var viewModel = new CourseViewModel();
        viewModel.UpdateFromCourse(course);
        return viewModel;
    }

    /// <summary>
    /// Создает пустую ViewModel для нового курса
    /// </summary>
    public static CourseViewModel CreateNew(Guid teacherUid, Guid? departmentUid = null)
    {
        return new CourseViewModel
        {
            Uid = Guid.NewGuid(),
            TeacherUid = teacherUid,
            DepartmentUid = departmentUid,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(4),
            Status = CourseStatus.Draft,
            Difficulty = CourseDifficulty.Beginner,
            Credits = 3,
            MaxEnrollments = 30
        };
    }

    #endregion

    #region Equality and Comparison

    public override bool Equals(object? obj)
    {
        return obj is CourseViewModel other && Uid.Equals(other.Uid);
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name} ({Code}) - {StatusDisplayName}";
    }

    #endregion
} 