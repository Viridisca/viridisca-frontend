using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Студент
/// </summary>
public class Student : ViewModelBase
{
    private Guid _personUid;
    private string _studentCode = string.Empty;
    private DateTime _enrollmentDate;
    private DateTime? _graduationDate;
    private StudentStatus _status = StudentStatus.Active;
    private bool _isActive = true;
    private string _emergencyContactName = string.Empty;
    private string _emergencyContactPhone = string.Empty;
    private string _medicalInformation = string.Empty;
    private string _address = string.Empty;
    private decimal _gpa;

    private Guid? _groupUid;
    private Guid? _curriculumUid;

    private Person? _person;
    private Group? _group;
    private Curriculum? _curriculum;

    private ObservableCollection<Enrollment> _enrollments = [];
    private ObservableCollection<Grade> _grades = [];
    private ObservableCollection<Attendance> _attendances = [];

    /// <summary>
    /// ID связанного человека
    /// </summary>
    public Guid PersonUid
    {
        get => _personUid;
        set => this.RaiseAndSetIfChanged(ref _personUid, value);
    }

    /// <summary>
    /// Студенческий код
    /// </summary>
    public string StudentCode
    {
        get => _studentCode;
        set => this.RaiseAndSetIfChanged(ref _studentCode, value);
    }

    /// <summary>
    /// Дата поступления
    /// </summary>
    public DateTime EnrollmentDate
    {
        get => _enrollmentDate;
        set => this.RaiseAndSetIfChanged(ref _enrollmentDate, value);
    }

    /// <summary>
    /// Дата выпуска
    /// </summary>
    public DateTime? GraduationDate
    {
        get => _graduationDate;
        set => this.RaiseAndSetIfChanged(ref _graduationDate, value);
    }

    /// <summary>
    /// Статус студента
    /// </summary>
    public StudentStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    /// <summary>
    /// Активен ли студент
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Контактное лицо для экстренных случаев
    /// </summary>
    public string EmergencyContactName
    {
        get => _emergencyContactName;
        set => this.RaiseAndSetIfChanged(ref _emergencyContactName, value);
    }

    /// <summary>
    /// Телефон экстренного контакта
    /// </summary>
    public string EmergencyContactPhone
    {
        get => _emergencyContactPhone;
        set => this.RaiseAndSetIfChanged(ref _emergencyContactPhone, value);
    }

    /// <summary>
    /// Медицинская информация
    /// </summary>
    public string MedicalInformation
    {
        get => _medicalInformation;
        set => this.RaiseAndSetIfChanged(ref _medicalInformation, value);
    }

    /// <summary>
    /// Адрес
    /// </summary>
    public string Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    /// <summary>
    /// Средний балл
    /// </summary>
    public decimal GPA
    {
        get => _gpa;
        set => this.RaiseAndSetIfChanged(ref _gpa, value);
    }

    /// <summary>
    /// ID группы
    /// </summary>
    public Guid? GroupUid
    {
        get => _groupUid;
        set => this.RaiseAndSetIfChanged(ref _groupUid, value);
    }

    /// <summary>
    /// ID учебного плана
    /// </summary>
    public Guid? CurriculumUid
    {
        get => _curriculumUid;
        set => this.RaiseAndSetIfChanged(ref _curriculumUid, value);
    }

    /// <summary>
    /// Связанный человек
    /// </summary>
    public Person? Person
    {
        get => _person;
        set => this.RaiseAndSetIfChanged(ref _person, value);
    }

    /// <summary>
    /// Группа
    /// </summary>
    public Group? Group
    {
        get => _group;
        set => this.RaiseAndSetIfChanged(ref _group, value);
    }

    /// <summary>
    /// Учебный план
    /// </summary>
    public Curriculum? Curriculum
    {
        get => _curriculum;
        set => this.RaiseAndSetIfChanged(ref _curriculum, value);
    }

    /// <summary>
    /// Записи на курсы
    /// </summary>
    public ObservableCollection<Enrollment> Enrollments
    {
        get => _enrollments;
        set => this.RaiseAndSetIfChanged(ref _enrollments, value);
    }

    /// <summary>
    /// Оценки
    /// </summary>
    public ObservableCollection<Grade> Grades
    {
        get => _grades;
        set => this.RaiseAndSetIfChanged(ref _grades, value);
    }

    /// <summary>
    /// Посещаемость
    /// </summary>
    public ObservableCollection<Attendance> Attendances
    {
        get => _attendances;
        set => this.RaiseAndSetIfChanged(ref _attendances, value);
    }

    /// <summary>
    /// Полное имя студента
    /// </summary>
    public string FullName => Person?.FullName ?? "Неизвестный студент";

    /// <summary>
    /// Email студента
    /// </summary>
    public string Email => Person?.Email ?? string.Empty;

    public Student()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        EnrollmentDate = DateTime.UtcNow;
    }
}