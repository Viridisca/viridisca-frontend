using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для отдельной оценки с полной реактивной поддержкой
/// </summary>
public class GradeViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    #region Core Properties

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public decimal Value { get; set; }
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string Comment { get; set; } = string.Empty;
    [Reactive] public GradeType Type { get; set; }
    [Reactive] public DateTime IssuedAt { get; set; }
    [Reactive] public DateTime GradedAt { get; set; }
    [Reactive] public bool IsPublished { get; set; }
    [Reactive] public DateTime? PublishedAt { get; set; }

    #endregion

    #region Student Properties

    [Reactive] public Guid StudentUid { get; set; }
    [Reactive] public string StudentName { get; set; } = string.Empty;
    [Reactive] public string StudentFirstName { get; set; } = string.Empty;
    [Reactive] public string StudentLastName { get; set; } = string.Empty;
    [Reactive] public string StudentGroup { get; set; } = string.Empty;

    #endregion

    #region Subject Properties

    [Reactive] public Guid SubjectUid { get; set; }
    [Reactive] public string SubjectName { get; set; } = string.Empty;
    [Reactive] public string SubjectCode { get; set; } = string.Empty;

    #endregion

    #region Teacher Properties

    [Reactive] public Guid TeacherUid { get; set; }
    [Reactive] public string TeacherName { get; set; } = string.Empty;
    [Reactive] public string TeacherFirstName { get; set; } = string.Empty;
    [Reactive] public string TeacherLastName { get; set; } = string.Empty;

    #endregion

    #region Assignment Properties

    [Reactive] public Guid? AssignmentUid { get; set; }
    [Reactive] public string AssignmentTitle { get; set; } = string.Empty;
    [Reactive] public DateTime? AssignmentDueDate { get; set; }

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    private ObservableAsPropertyHelper<string> _studentInitials;
    public string StudentInitials => _studentInitials.Value;

    private ObservableAsPropertyHelper<string> _teacherInitials;
    public string TeacherInitials => _teacherInitials.Value;

    private ObservableAsPropertyHelper<string> _typeDisplayName;
    public string TypeDisplayName => _typeDisplayName.Value;

    private ObservableAsPropertyHelper<string> _formattedValue;
    public string FormattedValue => _formattedValue.Value;

    private ObservableAsPropertyHelper<string> _formattedIssuedAt;
    public string FormattedIssuedAt => _formattedIssuedAt.Value;

    private ObservableAsPropertyHelper<string> _formattedGradedAt;
    public string FormattedGradedAt => _formattedGradedAt.Value;

    private ObservableAsPropertyHelper<string> _formattedDueDate;
    public string FormattedDueDate => _formattedDueDate.Value;

    private ObservableAsPropertyHelper<bool> _hasComments;
    public bool HasComments => _hasComments.Value;

    private ObservableAsPropertyHelper<string> _shortComment;
    public string ShortComment => _shortComment.Value;

    private ObservableAsPropertyHelper<bool> _canEdit;
    public bool CanEdit => _canEdit.Value;

    private ObservableAsPropertyHelper<bool> _canDelete;
    public bool CanDelete => _canDelete.Value;

    private ObservableAsPropertyHelper<bool> _canPublish;
    public bool CanPublish => _canPublish.Value;

    private ObservableAsPropertyHelper<string> _validationErrors;
    public string ValidationErrors => _validationErrors.Value;

    private ObservableAsPropertyHelper<bool> _isValid;
    public bool IsValid => _isValid.Value;

    // Добавляем свойства для совместимости с GradesViewModel
    public double Grade => (double)Value;
    public double MaxGrade => 5.0; // Предполагаем 5-балльную систему

    #endregion

    #region Constructors

    public GradeViewModel()
    {
        // Инициализация значений по умолчанию
        Uid = Guid.NewGuid();
        IssuedAt = DateTime.Now;
        GradedAt = DateTime.Now;
        Type = GradeType.Homework;
        Value = 0;

        InitializeComputedProperties();
    }

    public GradeViewModel(Grade grade) : this()
    {
        UpdateFromGrade(grade);
    }

    #endregion

    private void InitializeComputedProperties()
    {
        // Настройка computed properties
        _studentInitials = this.WhenAnyValue(
                x => x.StudentFirstName,
                x => x.StudentLastName,
                (first, last) => GetInitials(first, last))
            .ToProperty(this, x => x.StudentInitials)
            .DisposeWith(_disposables);

        _teacherInitials = this.WhenAnyValue(
                x => x.TeacherFirstName,
                x => x.TeacherLastName,
                (first, last) => GetInitials(first, last))
            .ToProperty(this, x => x.TeacherInitials)
            .DisposeWith(_disposables);

        _typeDisplayName = this.WhenAnyValue(x => x.Type)
            .Select(GetTypeDisplayName)
            .ToProperty(this, x => x.TypeDisplayName)
            .DisposeWith(_disposables);

        _formattedValue = this.WhenAnyValue(x => x.Value)
            .Select(value => value.ToString("F1"))
            .ToProperty(this, x => x.FormattedValue)
            .DisposeWith(_disposables);

        _formattedIssuedAt = this.WhenAnyValue(x => x.IssuedAt)
            .Select(date => date.ToString("dd.MM.yyyy HH:mm"))
            .ToProperty(this, x => x.FormattedIssuedAt)
            .DisposeWith(_disposables);

        _formattedGradedAt = this.WhenAnyValue(x => x.GradedAt)
            .Select(date => date.ToString("dd.MM.yyyy HH:mm"))
            .ToProperty(this, x => x.FormattedGradedAt)
            .DisposeWith(_disposables);

        _formattedDueDate = this.WhenAnyValue(x => x.AssignmentDueDate)
            .Select(date => date?.ToString("dd.MM.yyyy HH:mm") ?? "Не указано")
            .ToProperty(this, x => x.FormattedDueDate)
            .DisposeWith(_disposables);

        _hasComments = this.WhenAnyValue(x => x.Comment)
            .Select(comment => !string.IsNullOrEmpty(comment))
            .ToProperty(this, x => x.HasComments)
            .DisposeWith(_disposables);

        _shortComment = this.WhenAnyValue(x => x.Comment)
            .Select(comment => string.IsNullOrEmpty(comment) ? string.Empty : 
                comment.Length > 50 ? comment.Substring(0, 50) + "..." : comment)
            .ToProperty(this, x => x.ShortComment)
            .DisposeWith(_disposables);

        _canEdit = this.WhenAnyValue(x => x.IsPublished)
            .Select(isPublished => !isPublished)
            .ToProperty(this, x => x.CanEdit)
            .DisposeWith(_disposables);

        _canDelete = this.WhenAnyValue(x => x.IsPublished)
            .Select(isPublished => !isPublished)
            .ToProperty(this, x => x.CanDelete)
            .DisposeWith(_disposables);

        _canPublish = this.WhenAnyValue(x => x.IsPublished, x => x.IsValid)
            .Select(tuple => !tuple.Item1 && tuple.Item2)
            .ToProperty(this, x => x.CanPublish)
            .DisposeWith(_disposables);

        // Валидация
        _validationErrors = this.WhenAnyValue(
                x => x.Value,
                x => x.StudentUid,
                x => x.SubjectUid,
                x => x.TeacherUid)
            .Select(_ => GetValidationErrors())
            .ToProperty(this, x => x.ValidationErrors)
            .DisposeWith(_disposables);

        _isValid = this.WhenAnyValue(x => x.ValidationErrors)
            .Select(errors => string.IsNullOrEmpty(errors))
            .ToProperty(this, x => x.IsValid)
            .DisposeWith(_disposables);
    }

    #region Helper Methods

    private static string GetInitials(string firstName, string lastName)
    {
        var first = string.IsNullOrEmpty(firstName) ? "" : firstName.Substring(0, 1).ToUpper();
        var last = string.IsNullOrEmpty(lastName) ? "" : lastName.Substring(0, 1).ToUpper();
        return first + last;
    }

    private static string GetTypeDisplayName(GradeType type)
    {
        return type switch
        {
            GradeType.Homework => "Домашнее задание",
            GradeType.Quiz => "Тест/Опрос",
            GradeType.Test => "Контрольная работа",
            GradeType.Exam => "Экзамен",
            GradeType.Project => "Проект",
            GradeType.Participation => "Участие",
            GradeType.FinalGrade => "Итоговая оценка",
            GradeType.Other => "Другое",
            _ => "Неизвестно"
        };
    }

    private string GetValidationErrors()
    {
        var errors = new List<string>();

        if (Value < 0 || Value > 5)
            errors.Add("Оценка должна быть от 0 до 5");

        if (StudentUid == Guid.Empty)
            errors.Add("Студент должен быть выбран");

        if (SubjectUid == Guid.Empty)
            errors.Add("Предмет должен быть выбран");

        if (TeacherUid == Guid.Empty)
            errors.Add("Преподаватель должен быть указан");

        if (Comment?.Length > 1000)
            errors.Add("Комментарий не должен превышать 1000 символов");

        return string.Join("; ", errors);
    }

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Создает Grade из ViewModel
    /// </summary>
    public Grade ToGrade()
    {
        return new Grade
        {
            Uid = Uid,
            Value = Value,
            Description = Description,
            Comment = Comment,
            Type = Type,
            IssuedAt = IssuedAt,
            GradedAt = GradedAt,
            IsPublished = IsPublished,
            PublishedAt = PublishedAt,
            StudentUid = StudentUid,
            SubjectUid = SubjectUid,
            TeacherUid = TeacherUid,
            AssignmentUid = AssignmentUid
        };
    }

    /// <summary>
    /// Обновляет ViewModel из Grade
    /// </summary>
    public void UpdateFromGrade(Grade grade)
    {
        if (grade == null) return;

        Uid = grade.Uid;
        Value = grade.Value;
        Description = grade.Description;
        Comment = grade.Comment;
        Type = grade.Type;
        IssuedAt = grade.IssuedAt;
        GradedAt = grade.GradedAt;
        IsPublished = grade.IsPublished;
        PublishedAt = grade.PublishedAt;
        StudentUid = grade.StudentUid;
        SubjectUid = grade.SubjectUid;
        TeacherUid = grade.TeacherUid;
        AssignmentUid = grade.AssignmentUid;

        // Обновляем связанные данные если они есть
        if (grade.Student != null)
        {
            StudentName = grade.Student.FullName;
            StudentFirstName = grade.Student.Person?.FirstName ?? string.Empty;
            StudentLastName = grade.Student.Person?.LastName ?? string.Empty;
        }

        if (grade.Subject != null)
        {
            SubjectName = grade.Subject.Name;
            SubjectCode = grade.Subject.Code;
        }

        if (grade.Teacher != null)
        {
            TeacherName = grade.Teacher.FullName;
            TeacherFirstName = grade.Teacher.Person?.FirstName ?? string.Empty;
            TeacherLastName = grade.Teacher.Person?.LastName ?? string.Empty;
        }

        if (grade.Assignment != null)
        {
            AssignmentTitle = grade.Assignment.Title;
            AssignmentDueDate = grade.Assignment.DueDate;
        }
    }

    /// <summary>
    /// Создает копию ViewModel
    /// </summary>
    public GradeViewModel Clone()
    {
        return new GradeViewModel
        {
            Uid = Uid,
            Value = Value,
            Description = Description,
            Comment = Comment,
            Type = Type,
            IssuedAt = IssuedAt,
            GradedAt = GradedAt,
            IsPublished = IsPublished,
            PublishedAt = PublishedAt,
            StudentUid = StudentUid,
            StudentName = StudentName,
            StudentFirstName = StudentFirstName,
            StudentLastName = StudentLastName,
            StudentGroup = StudentGroup,
            SubjectUid = SubjectUid,
            SubjectName = SubjectName,
            SubjectCode = SubjectCode,
            TeacherUid = TeacherUid,
            TeacherName = TeacherName,
            TeacherFirstName = TeacherFirstName,
            TeacherLastName = TeacherLastName,
            AssignmentUid = AssignmentUid,
            AssignmentTitle = AssignmentTitle,
            AssignmentDueDate = AssignmentDueDate,
            IsSelected = IsSelected,
            IsExpanded = IsExpanded,
            IsLoading = IsLoading
        };
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Создает ViewModel из Grade
    /// </summary>
    public static GradeViewModel FromGrade(Grade grade)
    {
        var viewModel = new GradeViewModel();
        viewModel.UpdateFromGrade(grade);
        return viewModel;
    }

    /// <summary>
    /// Создает пустую ViewModel для новой оценки
    /// </summary>
    public static GradeViewModel CreateNew(Guid studentUid, Guid subjectUid, Guid teacherUid)
    {
        return new GradeViewModel
        {
            Uid = Guid.NewGuid(),
            StudentUid = studentUid,
            SubjectUid = subjectUid,
            TeacherUid = teacherUid,
            IssuedAt = DateTime.Now,
            GradedAt = DateTime.Now,
            Type = GradeType.Homework,
            Value = 0
        };
    }

    #endregion

    #region Equality and Comparison

    public override bool Equals(object? obj)
    {
        return obj is GradeViewModel other && Uid.Equals(other.Uid);
    }

    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    public override string ToString()
    {
        return $"{StudentName} - {SubjectName}: {FormattedValue} ({TypeDisplayName})";
    }

    #endregion

    public void Dispose()
    {
        _disposables.Dispose();
    }
} 