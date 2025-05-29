using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.System;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Учебный предмет
/// </summary>
public class Subject : ViewModelBase
{
    private string _code = string.Empty;
    private string _name = string.Empty;
    private string _description = string.Empty;

    private int _credits;
    private int _lessonsPerWeek;

    private SubjectType _type;

    private bool _isActive;
    private ObservableCollection<TeacherSubject> _teacherSubjects = [];

    private Guid? _departmentUid;
    private Department? _department;

    /// <summary>
    /// Код предмета
    /// </summary>
    public string Code
    {
        get => _code;
        set => this.RaiseAndSetIfChanged(ref _code, value);
    }

    /// <summary>
    /// Название предмета
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание предмета
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Количество кредитов
    /// </summary>
    public int Credits
    {
        get => _credits;
        set => this.RaiseAndSetIfChanged(ref _credits, value);
    }

    /// <summary>
    /// Количество уроков в неделю
    /// </summary>
    public int LessonsPerWeek
    {
        get => _lessonsPerWeek;
        set => this.RaiseAndSetIfChanged(ref _lessonsPerWeek, value);
    }

    /// <summary>
    /// Тип предмета
    /// </summary>
    public SubjectType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    /// <summary>
    /// Идентификатор кафедры/отдела
    /// </summary>
    public Guid? DepartmentUid
    {
        get => _departmentUid;
        set => this.RaiseAndSetIfChanged(ref _departmentUid, value);
    }

    /// <summary>
    /// Кафедра/отдел
    /// </summary>
    public Department? Department
    {
        get => _department;
        set => this.RaiseAndSetIfChanged(ref _department, value);
    }

    /// <summary>
    /// Флаг активности предмета
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Преподаватели, связанные с предметом
    /// </summary>
    public ObservableCollection<TeacherSubject> TeacherSubjects
    {
        get => _teacherSubjects;
        set => this.RaiseAndSetIfChanged(ref _teacherSubjects, value);
    }

    /// <summary>
    /// Отображаемый тип предмета
    /// </summary>
    public string TypeDisplayName => Type.GetDisplayName();

    /// <summary>
    /// Создает новый экземпляр предмета
    /// </summary>
    public Subject()
    {
        _isActive = true;
    }

    /// <summary>
    /// Создает новый экземпляр предмета с указанными параметрами
    /// </summary>
    public Subject(
        string code,
        string name,
        string description,
        int credits,
        int lessonsPerWeek,
        SubjectType type,
        Guid departmentUid)
    {
        Uid = Guid.NewGuid();
        _code = code.Trim();
        _name = name.Trim();
        _description = description;
        _credits = credits;
        _lessonsPerWeek = lessonsPerWeek;
        _type = type;
        _departmentUid = departmentUid;
        _isActive = true;
    }

    /// <summary>
    /// Обновляет основную информацию о предмете
    /// </summary>
    public void UpdateDetails(
        string name,
        string description,
        int credits,
        int lessonsPerWeek,
        SubjectType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        if (credits <= 0 || lessonsPerWeek <= 0)
            return;

        Name = name.Trim();
        Description = description;
        Credits = credits;
        LessonsPerWeek = lessonsPerWeek;
        Type = type;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активирует предмет
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивирует предмет
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавляет преподавателя к предмету
    /// </summary>
    public void AddTeacher(TeacherSubject teacherSubject)
    {
        if (teacherSubject != null && !TeacherSubjects.Any(ts => ts.TeacherUid == teacherSubject.TeacherUid))
        {
            TeacherSubjects.Add(teacherSubject);
            this.RaisePropertyChanged(nameof(TeacherSubjects));
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Удаляет преподавателя из предмета
    /// </summary>
    public void RemoveTeacher(TeacherSubject teacherSubject)
    {
        if (teacherSubject != null && TeacherSubjects.Contains(teacherSubject))
        {
            TeacherSubjects.Remove(teacherSubject);
            this.RaisePropertyChanged(nameof(TeacherSubjects));
            LastModifiedAt = DateTime.UtcNow;
        }
    }
} 