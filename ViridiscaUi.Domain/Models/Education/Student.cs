using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Студент
/// </summary>
public class Student : ViewModelBase
{
    private Guid _userUid;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _middleName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    private DateTime _birthDate;
    private Guid? _groupUid;
    private bool _isActive;
    private string _studentCode = string.Empty;
    private DateTime _enrollmentDate;
    private StudentStatus _status;
    private string _emergencyContactName = string.Empty;
    private string _emergencyContactPhone = string.Empty;
    private string _medicalInformation = string.Empty;
    private DateTime? _graduationDate;
    private ObservableCollection<StudentParent> _parents = new();
    private Group? _group;

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserUid
    {
        get => _userUid;
        set => this.RaiseAndSetIfChanged(ref _userUid, value);
    }

    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    /// <summary>
    /// Отчество
    /// </summary>
    public string MiddleName
    {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
    }

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
    }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime BirthDate
    {
        get => _birthDate;
        set => this.RaiseAndSetIfChanged(ref _birthDate, value);
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
    /// Группа студента
    /// </summary>
    public Group? Group
    {
        get => _group;
        set => this.RaiseAndSetIfChanged(ref _group, value);
    }

    /// <summary>
    /// Флаг активности учетной записи
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    /// <summary>
    /// Код студента
    /// </summary>
    public string StudentCode
    {
        get => _studentCode;
        set => this.RaiseAndSetIfChanged(ref _studentCode, value);
    }

    /// <summary>
    /// Дата зачисления
    /// </summary>
    public DateTime EnrollmentDate
    {
        get => _enrollmentDate;
        set => this.RaiseAndSetIfChanged(ref _enrollmentDate, value);
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
    /// Имя контакта для экстренных случаев
    /// </summary>
    public string EmergencyContactName
    {
        get => _emergencyContactName;
        set => this.RaiseAndSetIfChanged(ref _emergencyContactName, value);
    }

    /// <summary>
    /// Телефон контакта для экстренных случаев
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
    /// Дата выпуска
    /// </summary>
    public DateTime? GraduationDate
    {
        get => _graduationDate;
        set => this.RaiseAndSetIfChanged(ref _graduationDate, value);
    }

    /// <summary>
    /// Родители/опекуны студента
    /// </summary>
    public ObservableCollection<StudentParent> Parents
    {
        get => _parents;
        set => this.RaiseAndSetIfChanged(ref _parents, value);
    }

    /// <summary>
    /// Полное имя студента (Фамилия Имя Отчество)
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Отображаемый статус студента
    /// </summary>
    public string StatusDisplayName => Status.GetDisplayName();
    
    /// <summary>
    /// Возраст студента
    /// </summary>
    public int Age => CalculateAge(BirthDate);

    /// <summary>
    /// Имя группы, в которой учится студент
    /// </summary>
    public string GroupName => Group?.Name ?? "Не назначена";

    /// <summary>
    /// Создает новый экземпляр студента
    /// </summary>
    public Student()
    {
        _status = StudentStatus.Active;
        _isActive = true;
        _enrollmentDate = DateTime.UtcNow;
        _studentCode = GenerateStudentCode();
    }

    /// <summary>
    /// Создает новый экземпляр студента с указанными параметрами
    /// </summary>
    public Student(
        Guid userUid,
        string firstName,
        string lastName,
        string email,
        DateTime birthDate,
        string? middleName = null,
        string? phoneNumber = null,
        Guid? groupUid = null)
    {
        Uid = Guid.NewGuid();
        _userUid = userUid;
        _firstName = firstName;
        _lastName = lastName;
        _middleName = middleName ?? string.Empty;
        _email = email;
        _phoneNumber = phoneNumber ?? string.Empty;
        _birthDate = birthDate;
        _groupUid = groupUid;
        _isActive = true;
        _status = StudentStatus.Active;
        _enrollmentDate = DateTime.UtcNow;
        _studentCode = GenerateStudentCode();
    }

    /// <summary>
    /// Генерирует код студента
    /// </summary>
    private string GenerateStudentCode()
    {
        return $"ST-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    /// <summary>
    /// Обновляет информацию о студенте
    /// </summary>
    public void Update(
        string firstName,
        string lastName,
        string middleName,
        string email,
        string phoneNumber,
        DateTime birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Email = email;
        PhoneNumber = phoneNumber;
        BirthDate = birthDate;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет контактную информацию для экстренных случаев
    /// </summary>
    public void UpdateEmergencyContact(string name, string phone)
    {
        EmergencyContactName = name;
        EmergencyContactPhone = phone;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет медицинскую информацию
    /// </summary>
    public void UpdateMedicalInformation(string medicalInfo)
    {
        MedicalInformation = medicalInfo;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет код студента
    /// </summary>
    public void UpdateStudentCode(string studentCode)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
            return;
            
        StudentCode = studentCode;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет дату зачисления
    /// </summary>
    public void UpdateEnrollmentDate(DateTime enrollmentDate)
    {
        EnrollmentDate = enrollmentDate;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Назначает студента в группу
    /// </summary>
    public void AssignToGroup(Guid groupUid, Group group)
    {
        GroupUid = groupUid;
        Group = group;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Удаляет студента из группы
    /// </summary>
    public void RemoveFromGroup()
    {
        GroupUid = null;
        Group = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивирует учетную запись студента
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        Status = StudentStatus.Inactive;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активирует учетную запись студента
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        Status = StudentStatus.Active;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Помечает студента как выпускника
    /// </summary>
    public void Graduate(DateTime graduationDate)
    {
        GraduationDate = graduationDate;
        Status = StudentStatus.Graduated;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавляет родителя/опекуна студенту
    /// </summary>
    public void AddParent(StudentParent parent)
    {
        if (!Parents.Any(p => p.Uid == parent.Uid))
        {
            Parents.Add(parent);
            this.RaisePropertyChanged(nameof(Parents));
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Удаляет родителя/опекуна у студента
    /// </summary>
    public void RemoveParent(StudentParent parent)
    {
        var existingParent = Parents.FirstOrDefault(p => p.Uid == parent.Uid);
        if (existingParent != null)
        {
            Parents.Remove(existingParent);
            this.RaisePropertyChanged(nameof(Parents));
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Рассчитывает возраст на основе даты рождения
    /// </summary>
    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        
        if (birthDate.Date > today.AddYears(-age))
            age--;
            
        return age;
    }
} 