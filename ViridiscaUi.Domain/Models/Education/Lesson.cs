using System;
using System.Collections.Generic;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Урок в рамках модуля курса
/// </summary>
public class Lesson : ViewModelBase
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _content = string.Empty;
    private Guid _moduleUid;
    private Module? _module;
    private int _orderIndex;
    private TimeSpan? _duration;
    private LessonType _type = LessonType.Lecture;
    private bool _isPublished;
    private string _topic = string.Empty;
    private Guid? _subjectUid;
    private Guid? _teacherUid;
    private Guid? _groupUid;

    /// <summary>
    /// Название урока
    /// </summary>
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    /// <summary>
    /// Тема урока (синоним для Title)
    /// </summary>
    public string Topic
    {
        get => string.IsNullOrEmpty(_topic) ? _title : _topic;
        set => this.RaiseAndSetIfChanged(ref _topic, value);
    }

    /// <summary>
    /// Описание урока
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Содержимое урока
    /// </summary>
    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    /// <summary>
    /// Идентификатор модуля
    /// </summary>
    public Guid ModuleUid
    {
        get => _moduleUid;
        set => this.RaiseAndSetIfChanged(ref _moduleUid, value);
    }

    /// <summary>
    /// Модуль, к которому принадлежит урок
    /// </summary>
    public Module? Module
    {
        get => _module;
        set => this.RaiseAndSetIfChanged(ref _module, value);
    }

    /// <summary>
    /// Порядковый номер урока в модуле
    /// </summary>
    public int OrderIndex
    {
        get => _orderIndex;
        set => this.RaiseAndSetIfChanged(ref _orderIndex, value);
    }

    /// <summary>
    /// Продолжительность урока
    /// </summary>
    public TimeSpan? Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    /// <summary>
    /// Тип урока
    /// </summary>
    public LessonType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid? SubjectUid
    {
        get => _subjectUid;
        set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
    }

    /// <summary>
    /// Идентификатор преподавателя
    /// </summary>
    public Guid? TeacherUid
    {
        get => _teacherUid;
        set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
    }

    /// <summary>
    /// Идентификатор группы
    /// </summary>
    public Guid? GroupUid
    {
        get => _groupUid;
        set => this.RaiseAndSetIfChanged(ref _groupUid, value);
    }

    /// <summary>
    /// Флаг публикации урока
    /// </summary>
    public bool IsPublished
    {
        get => _isPublished;
        set => this.RaiseAndSetIfChanged(ref _isPublished, value);
    }

    /// <summary>
    /// Прогресс студентов по уроку
    /// </summary>
    public ICollection<LessonProgress> LessonProgress { get; set; } = new List<LessonProgress>();

    /// <summary>
    /// Создает новый экземпляр урока
    /// </summary>
    public Lesson()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр урока с указанными параметрами
    /// </summary>
    public Lesson(string title, string description, Guid moduleUid)
    {
        Uid = Guid.NewGuid();
        _title = title.Trim();
        _description = description;
        _moduleUid = moduleUid;
    }
}

/// <summary>
/// Тип урока
/// </summary>
public enum LessonType
{
    /// <summary>
    /// Лекция
    /// </summary>
    Lecture,
    
    /// <summary>
    /// Практическое занятие
    /// </summary>
    Practice,
    
    /// <summary>
    /// Лабораторная работа
    /// </summary>
    Lab,
    
    /// <summary>
    /// Семинар
    /// </summary>
    Seminar,
    
    /// <summary>
    /// Видео урок
    /// </summary>
    Video,
    
    /// <summary>
    /// Тест
    /// </summary>
    Quiz
}

/// <summary>
/// Прогресс студента по уроку
/// </summary>
public class LessonProgress : ViewModelBase
{
    private Guid _studentUid;
    private Guid _lessonUid;
    private bool _isCompleted;
    private DateTime? _completedAt;
    private TimeSpan? _timeSpent;
    private Student? _student;
    private Lesson? _lesson;

    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid
    {
        get => _studentUid;
        set => this.RaiseAndSetIfChanged(ref _studentUid, value);
    }

    /// <summary>
    /// Идентификатор урока
    /// </summary>
    public Guid LessonUid
    {
        get => _lessonUid;
        set => this.RaiseAndSetIfChanged(ref _lessonUid, value);
    }

    /// <summary>
    /// Флаг завершения урока
    /// </summary>
    public bool IsCompleted
    {
        get => _isCompleted;
        set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
    }

    /// <summary>
    /// Дата завершения урока
    /// </summary>
    public DateTime? CompletedAt
    {
        get => _completedAt;
        set => this.RaiseAndSetIfChanged(ref _completedAt, value);
    }

    /// <summary>
    /// Время, потраченное на урок
    /// </summary>
    public TimeSpan? TimeSpent
    {
        get => _timeSpent;
        set => this.RaiseAndSetIfChanged(ref _timeSpent, value);
    }

    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student
    {
        get => _student;
        set => this.RaiseAndSetIfChanged(ref _student, value);
    }

    /// <summary>
    /// Урок
    /// </summary>
    public Lesson? Lesson
    {
        get => _lesson;
        set => this.RaiseAndSetIfChanged(ref _lesson, value);
    }

    /// <summary>
    /// Создает новый экземпляр прогресса урока
    /// </summary>
    public LessonProgress()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр прогресса урока с указанными параметрами
    /// </summary>
    public LessonProgress(Guid studentUid, Guid lessonUid)
    {
        Uid = Guid.NewGuid();
        _studentUid = studentUid;
        _lessonUid = lessonUid;
    }
} 