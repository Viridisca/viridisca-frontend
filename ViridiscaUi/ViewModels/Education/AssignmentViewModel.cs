using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для отдельного задания с полной реактивной поддержкой
/// </summary>
public class AssignmentViewModel : ReactiveObject
{
    #region Core Properties

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string Type { get; set; } = string.Empty;
    [Reactive] public AssignmentStatus Status { get; set; }
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime DueDate { get; set; }
    [Reactive] public DateTime? PublishedAt { get; set; }
    [Reactive] public int MaxScore { get; set; }
    [Reactive] public bool AllowLateSubmissions { get; set; }
    [Reactive] public string Instructions { get; set; } = string.Empty;
    [Reactive] public string AttachmentsPath { get; set; } = string.Empty;

    #endregion

    #region Course and Teacher Properties

    [Reactive] public Guid CourseUid { get; set; }
    [Reactive] public string CourseName { get; set; } = string.Empty;
    [Reactive] public string CourseCode { get; set; } = string.Empty;
    [Reactive] public Guid TeacherUid { get; set; }
    [Reactive] public string TeacherName { get; set; } = string.Empty;
    [Reactive] public string TeacherFirstName { get; set; } = string.Empty;
    [Reactive] public string TeacherLastName { get; set; } = string.Empty;

    #endregion

    #region Statistics Properties

    [Reactive] public int SubmissionsCount { get; set; }
    [Reactive] public int GradedCount { get; set; }
    [Reactive] public int PendingCount { get; set; }
    [Reactive] public double AverageGrade { get; set; }
    [Reactive] public double CompletionRate { get; set; }
    [Reactive] public int TotalStudents { get; set; }

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
    /// Краткое описание (первые 100 символов)
    /// </summary>
    public string ShortDescription => _shortDescription?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _shortDescription;

    /// <summary>
    /// Форматированная дата создания
    /// </summary>
    public string FormattedCreatedAt => _formattedCreatedAt?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _formattedCreatedAt;

    /// <summary>
    /// Форматированная дата срока сдачи
    /// </summary>
    public string FormattedDueDate => _formattedDueDate?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _formattedDueDate;

    /// <summary>
    /// Время до срока сдачи
    /// </summary>
    public string TimeUntilDue => _timeUntilDue?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _timeUntilDue;

    /// <summary>
    /// Просрочено ли задание
    /// </summary>
    public bool IsOverdue => _isOverdue?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isOverdue;

    /// <summary>
    /// Активно ли задание
    /// </summary>
    public bool IsActive => _isActive?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isActive;

    /// <summary>
    /// Опубликовано ли задание
    /// </summary>
    public bool IsPublished => _isPublished?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isPublished;

    /// <summary>
    /// Можно ли редактировать задание
    /// </summary>
    public bool CanEdit => _canEdit?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canEdit;

    /// <summary>
    /// Можно ли удалить задание
    /// </summary>
    public bool CanDelete => _canDelete?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _canDelete;

    /// <summary>
    /// Процент выполнения
    /// </summary>
    public string CompletionPercentage => _completionPercentage?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _completionPercentage;

    /// <summary>
    /// Средний балл в виде строки
    /// </summary>
    public string AverageGradeDisplay => _averageGradeDisplay?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _averageGradeDisplay;

    /// <summary>
    /// Статистика сдач
    /// </summary>
    public string SubmissionStats => _submissionStats?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _submissionStats;

    /// <summary>
    /// Есть ли вложения
    /// </summary>
    public bool HasAttachments => _hasAttachments?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _hasAttachments;

    /// <summary>
    /// Количество дней до срока сдачи
    /// </summary>
    public int DaysUntilDue => _daysUntilDue?.Value ?? 0;
    private ObservableAsPropertyHelper<int>? _daysUntilDue;

    /// <summary>
    /// Приоритет задания (на основе срока сдачи)
    /// </summary>
    public string Priority => _priority?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _priority;

    /// <summary>
    /// Цвет приоритета
    /// </summary>
    public string PriorityColor => _priorityColor?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _priorityColor;

    /// <summary>
    /// Ошибки валидации
    /// </summary>
    public string ValidationErrors => _validationErrors?.Value ?? string.Empty;
    private ObservableAsPropertyHelper<string>? _validationErrors;

    /// <summary>
    /// Валидно ли задание
    /// </summary>
    public bool IsValid => _isValid?.Value ?? false;
    private ObservableAsPropertyHelper<bool>? _isValid;

    #endregion

    #region Constructor

    public AssignmentViewModel()
    {
        // Инициализация значений по умолчанию
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.Now;
        DueDate = DateTime.Now.AddDays(7);
        Status = AssignmentStatus.Draft;
        MaxScore = 100;
        AllowLateSubmissions = false;

        InitializeComputedProperties();
    }

    public AssignmentViewModel(Assignment assignment) : this()
    {
        UpdateFromAssignment(assignment);
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

        _shortDescription = this.WhenAnyValue(x => x.Description)
            .Select(desc => string.IsNullOrEmpty(desc) ? string.Empty : 
                desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc)
            .ToProperty(this, x => x.ShortDescription);

        _formattedCreatedAt = this.WhenAnyValue(x => x.CreatedAt)
            .Select(date => date.ToString("dd.MM.yyyy HH:mm"))
            .ToProperty(this, x => x.FormattedCreatedAt);

        _formattedDueDate = this.WhenAnyValue(x => x.DueDate)
            .Select(date => date.ToString("dd.MM.yyyy HH:mm"))
            .ToProperty(this, x => x.FormattedDueDate);

        _timeUntilDue = this.WhenAnyValue(x => x.DueDate)
            .Select(GetTimeUntilDue)
            .ToProperty(this, x => x.TimeUntilDue);

        _isOverdue = this.WhenAnyValue(x => x.DueDate, x => x.Status)
            .Select(tuple => tuple.Item1 < DateTime.Now && tuple.Item2 == AssignmentStatus.Active)
            .ToProperty(this, x => x.IsOverdue);

        _isActive = this.WhenAnyValue(x => x.Status)
            .Select(status => status == AssignmentStatus.Active)
            .ToProperty(this, x => x.IsActive);

        _isPublished = this.WhenAnyValue(x => x.Status)
            .Select(status => status != AssignmentStatus.Draft)
            .ToProperty(this, x => x.IsPublished);

        _canEdit = this.WhenAnyValue(x => x.Status)
            .Select(status => status != AssignmentStatus.Completed && status != AssignmentStatus.Archived)
            .ToProperty(this, x => x.CanEdit);

        _canDelete = this.WhenAnyValue(x => x.Status, x => x.SubmissionsCount)
            .Select(tuple => tuple.Item1 == AssignmentStatus.Draft || tuple.Item2 == 0)
            .ToProperty(this, x => x.CanDelete);

        _completionPercentage = this.WhenAnyValue(x => x.CompletionRate)
            .Select(rate => $"{rate:F1}%")
            .ToProperty(this, x => x.CompletionPercentage);

        _averageGradeDisplay = this.WhenAnyValue(x => x.AverageGrade, x => x.MaxScore)
            .Select(tuple => tuple.Item2 > 0 ? $"{tuple.Item1:F1}/{tuple.Item2}" : "—")
            .ToProperty(this, x => x.AverageGradeDisplay);

        _submissionStats = this.WhenAnyValue(
                x => x.SubmissionsCount,
                x => x.GradedCount,
                x => x.TotalStudents,
                (submitted, graded, total) => $"{submitted}/{total} сдано, {graded} оценено")
            .ToProperty(this, x => x.SubmissionStats);

        _hasAttachments = this.WhenAnyValue(x => x.AttachmentsPath)
            .Select(path => !string.IsNullOrEmpty(path))
            .ToProperty(this, x => x.HasAttachments);

        _daysUntilDue = this.WhenAnyValue(x => x.DueDate)
            .Select(date => (int)(date - DateTime.Now).TotalDays)
            .ToProperty(this, x => x.DaysUntilDue);

        _priority = this.WhenAnyValue(x => x.DaysUntilDue, x => x.Status)
            .Select(tuple => GetPriority(tuple.Item1, tuple.Item2))
            .ToProperty(this, x => x.Priority);

        _priorityColor = this.WhenAnyValue(x => x.Priority)
            .Select(GetPriorityColor)
            .ToProperty(this, x => x.PriorityColor);

        // Валидация
        _validationErrors = this.WhenAnyValue(
                x => x.Title,
                x => x.DueDate,
                x => x.MaxScore,
                x => x.CourseUid,
                x => x.TeacherUid)
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

    private static string GetStatusDisplayName(AssignmentStatus status)
    {
        return status switch
        {
            AssignmentStatus.Draft => "Черновик",
            AssignmentStatus.Published => "Опубликовано",
            AssignmentStatus.Active => "Активно",
            AssignmentStatus.Completed => "Завершено",
            AssignmentStatus.Overdue => "Просрочено",
            AssignmentStatus.Archived => "Архивировано",
            _ => "Неизвестно"
        };
    }

    private static string GetStatusColor(AssignmentStatus status)
    {
        return status switch
        {
            AssignmentStatus.Draft => "#9E9E9E",
            AssignmentStatus.Published => "#2196F3",
            AssignmentStatus.Active => "#4CAF50",
            AssignmentStatus.Completed => "#8BC34A",
            AssignmentStatus.Overdue => "#F44336",
            AssignmentStatus.Archived => "#607D8B",
            _ => "#9E9E9E"
        };
    }

    private static string GetTimeUntilDue(DateTime dueDate)
    {
        var timeSpan = dueDate - DateTime.Now;
        
        if (timeSpan.TotalDays < 0)
            return "Просрочено";
        
        if (timeSpan.TotalDays < 1)
            return $"Осталось {timeSpan.Hours} ч {timeSpan.Minutes} мин";
        
        if (timeSpan.TotalDays < 7)
            return $"Осталось {(int)timeSpan.TotalDays} дн.";
        
        return $"Осталось {(int)(timeSpan.TotalDays / 7)} нед.";
    }

    private static string GetPriority(int daysUntilDue, AssignmentStatus status)
    {
        if (status != AssignmentStatus.Active)
            return "Обычный";

        return daysUntilDue switch
        {
            < 0 => "Просрочено",
            < 1 => "Критический",
            < 3 => "Высокий",
            < 7 => "Средний",
            _ => "Низкий"
        };
    }

    private static string GetPriorityColor(string priority)
    {
        return priority switch
        {
            "Просрочено" => "#F44336",
            "Критический" => "#FF5722",
            "Высокий" => "#FF9800",
            "Средний" => "#FFC107",
            "Низкий" => "#4CAF50",
            _ => "#9E9E9E"
        };
    }

    private string GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Title))
            errors.Add("Название обязательно");

        if (Title?.Length > 200)
            errors.Add("Название не должно превышать 200 символов");

        if (DueDate <= DateTime.Now.AddMinutes(-5)) // Небольшая погрешность
            errors.Add("Срок сдачи должен быть в будущем");

        if (MaxScore <= 0)
            errors.Add("Максимальный балл должен быть больше 0");

        if (MaxScore > 1000)
            errors.Add("Максимальный балл не должен превышать 1000");

        if (CourseUid == Guid.Empty)
            errors.Add("Курс должен быть выбран");

        if (TeacherUid == Guid.Empty)
            errors.Add("Преподаватель должен быть выбран");

        if (Description?.Length > 5000)
            errors.Add("Описание не должно превышать 5000 символов");

        return string.Join("; ", errors);
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Создает Assignment из ViewModel
    /// </summary>
    public Assignment ToAssignment()
    {
        return new Assignment
        {
            Uid = Uid,
            Title = Title,
            Description = Description,
            Type = Enum.TryParse<AssignmentType>(Type, out var assignmentType) ? assignmentType : AssignmentType.Homework,
            Status = Status,
            DueDate = DueDate,
            MaxScore = MaxScore,
            Instructions = Instructions,
            CourseUid = CourseUid,
            // PublishedAt = PublishedAt, // Не существует в модели
            // AllowLateSubmissions = AllowLateSubmissions, // Не существует в модели
            // AttachmentsPath = AttachmentsPath, // Не существует в модели
            // TeacherUid = TeacherUid, // Не существует в модели
        };
    }

    /// <summary>
    /// Обновляет ViewModel из Assignment
    /// </summary>
    public void UpdateFromAssignment(Assignment assignment)
    {
        if (assignment == null) return;

        Uid = assignment.Uid;
        Title = assignment.Title ?? string.Empty;
        Description = assignment.Description ?? string.Empty;
        Type = assignment.Type.ToString();
        Status = assignment.Status;
        DueDate = assignment.DueDate ?? DateTime.Now.AddDays(7);
        MaxScore = (int)assignment.MaxScore;
        Instructions = assignment.Instructions ?? string.Empty;
        CourseUid = assignment.CourseUid;
        // PublishedAt = assignment.PublishedAt; // Не существует в модели
        // AllowLateSubmissions = assignment.AllowLateSubmissions; // Не существует в модели
        // AttachmentsPath = assignment.AttachmentsPath ?? string.Empty; // Не существует в модели
        // TeacherUid = assignment.TeacherUid; // Не существует в модели
    }

    /// <summary>
    /// Создает копию ViewModel
    /// </summary>
    public AssignmentViewModel Clone()
    {
        return new AssignmentViewModel
        {
            Uid = Uid,
            Title = Title,
            Description = Description,
            Type = Type,
            Status = Status,
            CreatedAt = CreatedAt,
            DueDate = DueDate,
            PublishedAt = PublishedAt,
            MaxScore = MaxScore,
            AllowLateSubmissions = AllowLateSubmissions,
            Instructions = Instructions,
            AttachmentsPath = AttachmentsPath,
            CourseUid = CourseUid,
            CourseName = CourseName,
            CourseCode = CourseCode,
            TeacherUid = TeacherUid,
            TeacherName = TeacherName,
            TeacherFirstName = TeacherFirstName,
            TeacherLastName = TeacherLastName,
            SubmissionsCount = SubmissionsCount,
            GradedCount = GradedCount,
            PendingCount = PendingCount,
            AverageGrade = AverageGrade,
            CompletionRate = CompletionRate,
            TotalStudents = TotalStudents,
            IsSelected = IsSelected,
            IsExpanded = IsExpanded,
            IsLoading = IsLoading
        };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Создает ViewModel из Assignment
    /// </summary>
    public static AssignmentViewModel FromAssignment(Assignment assignment)
    {
        var viewModel = new AssignmentViewModel();
        viewModel.UpdateFromAssignment(assignment);
        return viewModel;
    }

    /// <summary>
    /// Создает пустую ViewModel для нового задания
    /// </summary>
    public static AssignmentViewModel CreateNew(Guid courseUid, Guid teacherUid)
    {
        return new AssignmentViewModel
        {
            Uid = Guid.NewGuid(),
            CourseUid = courseUid,
            TeacherUid = teacherUid,
            CreatedAt = DateTime.Now,
            DueDate = DateTime.Now.AddDays(7),
            Status = AssignmentStatus.Draft,
            MaxScore = 100,
            AllowLateSubmissions = false
        };
    }

    #endregion

    #region Equality and Comparison

    public override bool Equals(object? obj)
    {
        return obj is AssignmentViewModel other && Uid.Equals(other.Uid);
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Title} ({CourseName}) - {StatusDisplayName}";
    }

    #endregion
} 