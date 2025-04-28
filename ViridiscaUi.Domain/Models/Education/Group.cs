using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Учебная группа
/// </summary>
public class Group : ViewModelBase
{
    private string _code = string.Empty;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private int _year;
    private DateTime _startDate;
    private DateTime? _endDate;
    private int _maxStudents;
    private GroupStatus _status;
    private Guid? _curatorUid;
    private Guid _departmentUid;
    private ObservableCollection<Student> _students = new();

    /// <summary>
    /// Код группы
    /// </summary>
    public string Code
    {
        get => _code;
        set => this.RaiseAndSetIfChanged(ref _code, value);
    }

    /// <summary>
    /// Название группы
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание группы
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Год обучения
    /// </summary>
    public int Year
    {
        get => _year;
        set => this.RaiseAndSetIfChanged(ref _year, value);
    }

    /// <summary>
    /// Дата начала обучения
    /// </summary>
    public DateTime StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    /// <summary>
    /// Дата окончания обучения
    /// </summary>
    public DateTime? EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    /// <summary>
    /// Максимальное количество студентов
    /// </summary>
    public int MaxStudents
    {
        get => _maxStudents;
        set => this.RaiseAndSetIfChanged(ref _maxStudents, value);
    }

    /// <summary>
    /// Статус группы
    /// </summary>
    public GroupStatus Status
    {
        get => _status;
        private set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Идентификатор куратора
    /// </summary>
    public Guid? CuratorUid
    {
        get => _curatorUid;
        set => this.RaiseAndSetIfChanged(ref _curatorUid, value);
    }

    /// <summary>
    /// Идентификатор департамента
    /// </summary>
    public Guid DepartmentUid
    {
        get => _departmentUid;
        set => this.RaiseAndSetIfChanged(ref _departmentUid, value);
    }

    /// <summary>
    /// Отображаемый статус группы
    /// </summary>
    public string StatusDisplayName => Status.GetDisplayName();

    /// <summary>
    /// Студенты группы
    /// </summary>
    public ObservableCollection<Student> Students
    {
        get => _students;
        set => this.RaiseAndSetIfChanged(ref _students, value);
    }

    /// <summary>
    /// Количество студентов в группе
    /// </summary>
    public int StudentCount => Students?.Count ?? 0;

    /// <summary>
    /// Full name of the group (Code - Name)
    /// </summary>
    public string FullName => $"{Code} - {Name}";

    /// <summary>
    /// Создает новый экземпляр группы
    /// </summary>
    public Group()
    {
        _status = GroupStatus.Forming;
    }

    /// <summary>
    /// Создает новый экземпляр группы с указанными параметрами
    /// </summary>
    public Group(
        string code,
        string name,
        string description,
        int year,
        DateTime startDate,
        int maxStudents,
        Guid departmentUid,
        Guid? curatorUid = null)
    {
        Uid = Guid.NewGuid();
        _code = code;
        _name = name;
        _description = description;
        _year = year;
        _startDate = startDate;
        _maxStudents = maxStudents;
        _departmentUid = departmentUid;
        _curatorUid = curatorUid;
        _status = GroupStatus.Forming;
    }

    /// <summary>
    /// Обновляет детали группы
    /// </summary>
    public void UpdateDetails(string name, string description, int maxStudents)
    {
        Name = name;
        Description = description;
        MaxStudents = maxStudents;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Устанавливает куратора группы
    /// </summary>
    public void SetCurator(Guid? curatorUid)
    {
        CuratorUid = curatorUid;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет статус группы
    /// </summary>
    public void UpdateStatus(GroupStatus status)
    {
        Status = status;
        
        // Автоматическое обновление дат при изменении статуса
        if (status == GroupStatus.Completed || status == GroupStatus.Archived)
        {
            if (!EndDate.HasValue)
            {
                EndDate = DateTime.UtcNow;
            }
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Устанавливает дату окончания группы
    /// </summary>
    public void SetEndDate(DateTime endDate)
    {
        if (endDate <= StartDate)
            return;
                
        EndDate = endDate;
        
        // Автоматическое обновление статуса
        if (Status != GroupStatus.Completed && Status != GroupStatus.Archived)
        {
            if (endDate <= DateTime.UtcNow)
            {
                Status = GroupStatus.Completed;
            }
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавляет студента в группу
    /// </summary>
    public void AddStudent(Student student)
    {
        if (student != null && !Students.Any(s => s.Uid == student.Uid))
        {
            Students.Add(student);
            student.GroupUid = this.Uid;
            student.Group = this;
            this.RaisePropertyChanged(nameof(Students));
            this.RaisePropertyChanged(nameof(StudentCount));
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Удаляет студента из группы
    /// </summary>
    public void RemoveStudent(Student student)
    {
        if (student != null && Students.Contains(student))
        {
            Students.Remove(student);
            if (student.GroupUid == this.Uid)
            {
                student.GroupUid = null;
                student.Group = null;
            }
            this.RaisePropertyChanged(nameof(Students));
            this.RaisePropertyChanged(nameof(StudentCount));
            LastModifiedAt = DateTime.UtcNow;
        }
    }
} 