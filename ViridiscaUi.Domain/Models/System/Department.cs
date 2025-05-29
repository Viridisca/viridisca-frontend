using ViridiscaUi.Domain.Models.Education;
using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Департамент/Кафедра учебного заведения
/// </summary>
public class Department : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _code = string.Empty;

    private Guid? _headOfDepartmentUid;
    private Teacher? _headOfDepartment;
    
    private bool _isActive;

    private ObservableCollection<Teacher> _teachers = [];
    private ObservableCollection<Subject> _subjects = [];
    private ObservableCollection<Group> _groups = [];

    /// <summary>
    /// Название департамента
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание департамента
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Код департамента
    /// </summary>
    public string Code
    {
        get => _code;
        set => this.RaiseAndSetIfChanged(ref _code, value);
    }

    /// <summary>
    /// Идентификатор заведующего кафедрой
    /// </summary>
    public Guid? HeadOfDepartmentUid
    {
        get => _headOfDepartmentUid;
        set => this.RaiseAndSetIfChanged(ref _headOfDepartmentUid, value);
    }

    /// <summary>
    /// Заведующий кафедрой
    /// </summary>
    public Teacher? HeadOfDepartment
    {
        get => _headOfDepartment;
        set => this.RaiseAndSetIfChanged(ref _headOfDepartment, value);
    }

    /// <summary>
    /// Флаг активности департамента
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Предметы департамента
    /// </summary>
    public ObservableCollection<Subject> Subjects
    {
        get => _subjects;
        set => this.RaiseAndSetIfChanged(ref _subjects, value);
    }

    /// <summary>
    /// Группы департамента
    /// </summary>
    public ObservableCollection<Group> Groups
    {
        get => _groups;
        set => this.RaiseAndSetIfChanged(ref _groups, value);
    }

    /// <summary>
    /// Преподаватели департамента
    /// </summary>
    public ObservableCollection<Teacher> Teachers
    {
        get => _teachers;
        set => this.RaiseAndSetIfChanged(ref _teachers, value);
    }

    /// <summary>
    /// Создает новый экземпляр департамента
    /// </summary>
    public Department()
    {
        _isActive = true;
    }

    /// <summary>
    /// Создает новый экземпляр департамента с указанными параметрами
    /// </summary>
    public Department(string name, string code, string description = "")
    {
        Uid = Guid.NewGuid();
        _name = name;
        _code = code;
        _description = description;
        _isActive = true;
    }
} 