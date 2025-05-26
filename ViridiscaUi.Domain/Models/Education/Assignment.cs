using System;
using System.Collections.Generic;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Задание для студентов
/// </summary>
public class Assignment : ViewModelBase
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private DateTime? _dueDate;
    private double _maxScore = 100.0;
    private AssignmentType _type = AssignmentType.Homework;
    private Guid _courseUid;
    private Guid? _lessonUid;
    private Course? _course;
    private Lesson? _lesson;
    private string _instructions = string.Empty;
    private AssignmentDifficulty _difficulty = AssignmentDifficulty.Medium;
    private AssignmentStatus _status = AssignmentStatus.Draft;
    private Guid? _moduleUid;

    /// <summary>
    /// Название задания
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Описание задания
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Срок сдачи задания
    /// </summary>
    public DateTime? DueDate
    {
        get => _dueDate;
        set => this.RaiseAndSetIfChanged(ref _dueDate, value);
    }

    /// <summary>
    /// Максимальный балл за задание
    /// </summary>
    public double MaxScore
    {
        get => _maxScore;
        set => this.RaiseAndSetIfChanged(ref _maxScore, value);
    }

    /// <summary>
    /// Максимальная оценка за задание (синоним для MaxScore)
    /// </summary>
    public double MaxGrade
    {
        get => _maxScore;
        set => this.RaiseAndSetIfChanged(ref _maxScore, value);
    }

    /// <summary>
    /// Тип задания
    /// </summary>
    public AssignmentType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Идентификатор курса
    /// </summary>
    public Guid CourseUid
    {
        get => _courseUid;
        set => this.RaiseAndSetIfChanged(ref _courseUid, value);
    }

    /// <summary>
    /// Идентификатор курса (синоним для CourseUid)
    /// </summary>
    public Guid CourseId
    {
        get => _courseUid;
        set => this.RaiseAndSetIfChanged(ref _courseUid, value);
    }

    /// <summary>
    /// Идентификатор урока (опционально)
    /// </summary>
    public Guid? LessonUid
    {
        get => _lessonUid;
        set => this.RaiseAndSetIfChanged(ref _lessonUid, value);
    }

    /// <summary>
    /// Курс, к которому принадлежит задание
    /// </summary>
    public Course? Course
    {
        get => _course;
        set => this.RaiseAndSetIfChanged(ref _course, value);
    }

    /// <summary>
    /// Урок, к которому принадлежит задание (опционально)
    /// </summary>
    public Lesson? Lesson
    {
        get => _lesson;
        set => this.RaiseAndSetIfChanged(ref _lesson, value);
    }

    /// <summary>
    /// Инструкции к заданию
    /// </summary>
    public string Instructions
    {
        get => _instructions;
        set => this.RaiseAndSetIfChanged(ref _instructions, value);
    }

    /// <summary>
    /// Сложность задания
    /// </summary>
    public AssignmentDifficulty Difficulty
    {
        get => _difficulty;
        set => this.RaiseAndSetIfChanged(ref _difficulty, value);
    }

    /// <summary>
    /// Статус задания
    /// </summary>
    public AssignmentStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Идентификатор модуля (опционально)
    /// </summary>
    public Guid? ModuleUid
    {
        get => _moduleUid;
        set => this.RaiseAndSetIfChanged(ref _moduleUid, value);
    }

    /// <summary>
    /// Идентификатор модуля (синоним для ModuleUid)
    /// </summary>
    public Guid? ModuleId
    {
        get => _moduleUid;
        set => this.RaiseAndSetIfChanged(ref _moduleUid, value);
    }

    /// <summary>
    /// Сданные работы по заданию
    /// </summary>
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    /// <summary>
    /// Создает новый экземпляр задания
    /// </summary>
    public Assignment()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр задания с указанными параметрами
    /// </summary>
    public Assignment(string title, string description, Guid courseUid, AssignmentType type = AssignmentType.Homework)
    {
        Uid = Guid.NewGuid();
        _title = title.Trim();
        _description = description;
        _courseUid = courseUid;
        _type = type;
    }

    /// <summary>
    /// Проверяет, просрочено ли задание
    /// </summary>
    public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value;

    /// <summary>
    /// Отображаемый тип задания
    /// </summary>
    public string TypeDisplayName => Type switch
    {
        AssignmentType.Homework => "Домашнее задание",
        AssignmentType.Quiz => "Тест",
        AssignmentType.Exam => "Экзамен",
        AssignmentType.Project => "Проект",
        AssignmentType.LabWork => "Лабораторная работа",
        _ => "Неизвестный тип"
    };
}

/// <summary>
/// Тип задания
/// </summary>
public enum AssignmentType
{
    /// <summary>
    /// Домашнее задание
    /// </summary>
    Homework,
    
    /// <summary>
    /// Тест
    /// </summary>
    Quiz,
    
    /// <summary>
    /// Экзамен
    /// </summary>
    Exam,
    
    /// <summary>
    /// Проект
    /// </summary>
    Project,
    
    /// <summary>
    /// Лабораторная работа
    /// </summary>
    LabWork
}

/// <summary>
/// Сложность задания
/// </summary>
public enum AssignmentDifficulty
{
    /// <summary>
    /// Легкая
    /// </summary>
    Easy,
    
    /// <summary>
    /// Средняя
    /// </summary>
    Medium,
    
    /// <summary>
    /// Сложная
    /// </summary>
    Hard
}

/// <summary>
/// Статус задания
/// </summary>
public enum AssignmentStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    Draft,
    
    /// <summary>
    /// Опубликовано
    /// </summary>
    Published,
    
    /// <summary>
    /// Закрыто для сдачи
    /// </summary>
    Closed,
    
    /// <summary>
    /// Архивировано
    /// </summary>
    Archived
} 