using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Преподаватель
/// </summary>
public class Teacher : ViewModelBase
{
    private string _employeeCode = string.Empty;
    private Guid _userUid;
    private DateTime _hireDate;
    private DateTime? _terminationDate;
    private TeacherStatus _status;
    private string _academicDegree = string.Empty;
    private string _academicTitle = string.Empty;
    private string _specialization = string.Empty;
    private decimal _hourlyRate;
    private string _bio = string.Empty;
    private ObservableCollection<TeacherSubject> _subjects = new();
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _middleName = string.Empty;

    /// <summary>
    /// Код сотрудника
    /// </summary>
    public string EmployeeCode
    {
        get => _employeeCode;
        set => this.RaiseAndSetIfChanged(ref _employeeCode, value);
    }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserUid
    {
        get => _userUid;
        set => this.RaiseAndSetIfChanged(ref _userUid, value);
    }

    /// <summary>
    /// Имя преподавателя
    /// </summary>
    public string FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    /// <summary>
    /// Фамилия преподавателя
    /// </summary>
    public string LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    /// <summary>
    /// Отчество преподавателя
    /// </summary>
    public string MiddleName
    {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
    }

    /// <summary>
    /// Дата найма
    /// </summary>
    public DateTime HireDate
    {
        get => _hireDate;
        set => this.RaiseAndSetIfChanged(ref _hireDate, value);
    }

    /// <summary>
    /// Дата увольнения
    /// </summary>
    public DateTime? TerminationDate
    {
        get => _terminationDate;
        set => this.RaiseAndSetIfChanged(ref _terminationDate, value);
    }

    /// <summary>
    /// Статус преподавателя
    /// </summary>
    public TeacherStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Ученая степень
    /// </summary>
    public string AcademicDegree
    {
        get => _academicDegree;
        set => this.RaiseAndSetIfChanged(ref _academicDegree, value);
    }

    /// <summary>
    /// Ученое звание
    /// </summary>
    public string AcademicTitle
    {
        get => _academicTitle;
        set => this.RaiseAndSetIfChanged(ref _academicTitle, value);
    }

    /// <summary>
    /// Специализация
    /// </summary>
    public string Specialization
    {
        get => _specialization;
        set => this.RaiseAndSetIfChanged(ref _specialization, value);
    }

    /// <summary>
    /// Почасовая ставка
    /// </summary>
    public decimal HourlyRate
    {
        get => _hourlyRate;
        set => this.RaiseAndSetIfChanged(ref _hourlyRate, value);
    }

    /// <summary>
    /// Биография
    /// </summary>
    public string Bio
    {
        get => _bio;
        set => this.RaiseAndSetIfChanged(ref _bio, value);
    }

    /// <summary>
    /// Предметы, которые ведет преподаватель
    /// </summary>
    public ObservableCollection<TeacherSubject> Subjects
    {
        get => _subjects;
        set => this.RaiseAndSetIfChanged(ref _subjects, value);
    }

    /// <summary>
    /// Отображаемый статус преподавателя
    /// </summary>
    public string StatusDisplayName => Status.GetDisplayName();

    /// <summary>
    /// Полное имя преподавателя (Фамилия Имя Отчество)
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Создает новый экземпляр преподавателя
    /// </summary>
    public Teacher()
    {
        _status = TeacherStatus.Active;
    }

    /// <summary>
    /// Создает новый экземпляр преподавателя с указанными параметрами
    /// </summary>
    public Teacher(
        string employeeCode,
        Guid userUid,
        DateTime hireDate,
        string specialization,
        decimal hourlyRate,
        string lastName,
        string firstName,
        string? middleName = null,
        string? academicDegree = null,
        string? academicTitle = null,
        string? bio = null)
    {
        Uid = Guid.NewGuid();
        _employeeCode = employeeCode;
        _userUid = userUid;
        _hireDate = hireDate;
        _specialization = specialization;
        _hourlyRate = hourlyRate;
        _lastName = lastName;
        _firstName = firstName;
        _middleName = middleName ?? string.Empty;
        _academicDegree = academicDegree ?? string.Empty;
        _academicTitle = academicTitle ?? string.Empty;
        _bio = bio ?? string.Empty;
        _status = TeacherStatus.Active;
    }

    /// <summary>
    /// Обновляет основную информацию о преподавателе
    /// </summary>
    public void UpdatePersonalInfo(string lastName, string firstName, string middleName)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
        LastModifiedAt = DateTime.UtcNow;
        this.RaisePropertyChanged(nameof(FullName));
    }

    /// <summary>
    /// Назначает предмет преподавателю
    /// </summary>
    public void AssignSubject(TeacherSubject subject)
    {
        if (subject != null && !Subjects.Any(s => s.SubjectUid == subject.SubjectUid))
        {
            Subjects.Add(subject);
            this.RaisePropertyChanged(nameof(Subjects));
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Отзывает предмет у преподавателя
    /// </summary>
    public void RemoveSubject(TeacherSubject subject)
    {
        if (subject != null && Subjects.Contains(subject))
        {
            Subjects.Remove(subject);
            this.RaisePropertyChanged(nameof(Subjects));
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Обновляет статус преподавателя
    /// </summary>
    public void UpdateStatus(TeacherStatus status)
    {
        Status = status;
        
        if (status == TeacherStatus.Terminated && !TerminationDate.HasValue)
        {
            TerminationDate = DateTime.UtcNow;
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет данные преподавателя
    /// </summary>
    public void UpdateDetails(
        string specialization,
        decimal hourlyRate,
        string? academicDegree,
        string? academicTitle,
        string? bio)
    {
        Specialization = specialization;
        HourlyRate = hourlyRate;
        AcademicDegree = academicDegree ?? string.Empty;
        AcademicTitle = academicTitle ?? string.Empty;
        Bio = bio ?? string.Empty;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Устанавливает дату увольнения
    /// </summary>
    public void SetTerminationDate(DateTime terminationDate)
    {
        if (terminationDate <= HireDate)
            return;

        TerminationDate = terminationDate;
        Status = TeacherStatus.Terminated;
        LastModifiedAt = DateTime.UtcNow;
    }
}