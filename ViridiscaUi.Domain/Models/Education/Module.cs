using System;
using System.Collections.Generic;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Модуль курса
/// </summary>
public class Module : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private Guid _courseUid;
    private Course? _course;
    private int _orderIndex;
    private bool _isPublished;

    /// <summary>
    /// Название модуля
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание модуля
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
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
    /// Курс, к которому принадлежит модуль
    /// </summary>
    public Course? Course
    {
        get => _course;
        set => this.RaiseAndSetIfChanged(ref _course, value);
    }

    /// <summary>
    /// Порядковый номер модуля в курсе
    /// </summary>
    public int OrderIndex
    {
        get => _orderIndex;
        set => this.RaiseAndSetIfChanged(ref _orderIndex, value);
    }

    /// <summary>
    /// Флаг публикации модуля
    /// </summary>
    public bool IsPublished
    {
        get => _isPublished;
        set => this.RaiseAndSetIfChanged(ref _isPublished, value);
    }

    /// <summary>
    /// Уроки модуля
    /// </summary>
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    /// <summary>
    /// Создает новый экземпляр модуля
    /// </summary>
    public Module()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр модуля с указанными параметрами
    /// </summary>
    public Module(string name, string description, Guid courseUid)
    {
        Uid = Guid.NewGuid();
        _name = name.Trim();
        _description = description;
        _courseUid = courseUid;
    }
} 