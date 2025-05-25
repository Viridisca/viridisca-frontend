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
    private int _order;
    private Guid _courseUid;
    private Course? _course;

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
    /// Порядок модуля в курсе
    /// </summary>
    public int Order
    {
        get => _order;
        set => this.RaiseAndSetIfChanged(ref _order, value);
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
    public Module(string name, string description, int order, Guid courseUid)
    {
        Uid = Guid.NewGuid();
        _name = name.Trim();
        _description = description;
        _order = order;
        _courseUid = courseUid;
    }
} 