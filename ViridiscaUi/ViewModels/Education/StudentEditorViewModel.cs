using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Bases.Navigations;
using Avalonia.Controls;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для создания и редактирования студентов
/// </summary>
[Route("student-editor", DisplayName = "Редактор студентов", IconKey = "👤", Order = 102, RequiredRoles = new[] { "Admin", "Teacher" })]
public class StudentEditorViewModel : RoutableViewModelBase
{
    private readonly IStudentService _studentService;
    private readonly IGroupService _groupService;
    private readonly IUnifiedNavigationService _navigationService;

    #region Properties

    /// <summary>
    /// Флаг режима редактирования (true) или создания (false)
    /// </summary>
    [Reactive] public bool IsEditMode { get; set; }

    /// <summary>
    /// Текущий редактируемый студент
    /// </summary>
    [Reactive] public Student? CurrentStudent { get; set; }

    /// <summary>
    /// Идентификатор студента для редактирования
    /// </summary>
    [Reactive] public Guid? StudentId { get; set; }

    // Поля для редактирования
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string MiddleName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string PhoneNumber { get; set; } = string.Empty;
    [Reactive] public string StudentCode { get; set; } = string.Empty;
    [Reactive] public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    [Reactive] public DateTime BirthDate { get; set; } = DateTime.Now;
    [Reactive] public Group? SelectedGroup { get; set; }
    [Reactive] public StudentStatus SelectedStatus { get; set; } = StudentStatus.Active;
    [Reactive] public int AcademicYear { get; set; } = 1;

    /// <summary>
    /// Адрес студента
    /// </summary>
    [Reactive] public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Имя экстренного контакта
    /// </summary>
    [Reactive] public string EmergencyContactName { get; set; } = string.Empty;

    /// <summary>
    /// Телефон экстренного контакта
    /// </summary>
    [Reactive] public string EmergencyContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// Медицинская информация
    /// </summary>
    [Reactive] public string MedicalInformation { get; set; } = string.Empty;

    /// <summary>
    /// Доступные группы для выбора
    /// </summary>
    [Reactive] public ObservableCollection<Group> AvailableGroups { get; set; } = new();

    /// <summary>
    /// Доступные статусы студента
    /// </summary>
    [Reactive] public ObservableCollection<StudentStatus> AvailableStatuses { get; set; } = new();

    /// <summary>
    /// Флаг процесса сохранения
    /// </summary>
    [Reactive] public bool IsSaving { get; set; }

    /// <summary>
    /// Заголовок формы
    /// </summary>
    [Reactive] public string FormTitle { get; set; } = "Создание студента";

    /// <summary>
    /// Полное имя студента для отображения
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Имя группы для отображения
    /// </summary>
    public string GroupName => SelectedGroup?.Name ?? "Не назначена";

    #endregion

    #region Commands

    /// <summary>
    /// Команда сохранения студента
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; } = null!;

    /// <summary>
    /// Команда отмены
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; } = null!;

    /// <summary>
    /// Команда удаления студента (только в режиме редактирования)
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; } = null!;

    /// <summary>
    /// Команда создания нового студента
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateNewCommand { get; set; } = null!;

    /// <summary>
    /// Команда редактирования из диалога деталей
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCommand { get; set; } = null!;

    /// <summary>
    /// Команда закрытия диалога деталей
    /// </summary>
    public ReactiveCommand<Unit, Unit> CloseCommand { get; set; } = null!;

    #endregion

    public StudentEditorViewModel(
        IStudentService studentService,
        IGroupService groupService,
        IUnifiedNavigationService navigationService,
        IScreen hostScreen) : base(hostScreen)
    {
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        InitializeCommands();
        InitializeStatuses();
    }

    /// <summary>
    /// Конструктор для диалогов с упрощенным набором зависимостей
    /// </summary>
    public StudentEditorViewModel(IStudentService studentService, IGroupService groupService, Student? student = null)
        : base(hostScreen: null!)  // Для диалогов hostScreen не нужен
    {
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _navigationService = null!; // Для диалогов навигация не нужна

        InitializeCommands();
        InitializeStatuses();

        if (student != null)
        {
            CurrentStudent = student;
            IsEditMode = true;
            PopulateForm(student);
            FormTitle = "Редактирование студента";
        }
        else
        {
            SetupForCreation();
            FormTitle = "Создание студента";
        }
    }

    #region Lifecycle Methods

    /// <summary>
    /// Вызывается при первой загрузке ViewModel
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        
        await LoadGroupsAsync();
        
        if (StudentId.HasValue)
        {
            await LoadStudentAsync(StudentId.Value);
        }
        else
        {
            SetupForCreation();
        }
    }

    #endregion

    #region Private Methods

    private void InitializeCommands()
    {
        // Проверка валидности формы
        var canSave = this.WhenAnyValue(
            x => x.FirstName,
            x => x.LastName,
            x => x.Email,
            x => x.StudentCode,
            x => x.SelectedGroup,
            x => x.IsSaving,
            (firstName, lastName, email, studentCode, group, isSaving) =>
                !string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName) &&
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(studentCode) &&
                group != null &&
                !isSaving);

        SaveCommand = CreateCommand(SaveAsync, canSave, "Ошибка при сохранении студента");
        CancelCommand = CreateCommand(CancelAsync, null, "Ошибка при отмене");
        
        var canDelete = this.WhenAnyValue(x => x.IsEditMode, x => x.IsSaving, 
            (isEdit, isSaving) => isEdit && !isSaving);
        DeleteCommand = CreateCommand(DeleteAsync, canDelete, "Ошибка при удалении студента");
        
        CreateNewCommand = CreateCommand(CreateNewAsync, null, "Ошибка при создании нового студента");
        
        EditCommand = CreateCommand(EditAsync, null, "Ошибка при редактировании студента");
        CloseCommand = CreateCommand(CloseAsync, null, "Ошибка при закрытии диалога");
    }

    private void InitializeStatuses()
    {
        AvailableStatuses.Clear();
        foreach (var status in Enum.GetValues<StudentStatus>())
        {
            AvailableStatuses.Add(status);
        }
    }

    private async Task LoadGroupsAsync()
    {
        try
        {
            ShowInfo("Загрузка групп...");
            var groups = await _groupService.GetAllGroupsAsync();
            
            AvailableGroups.Clear();
            foreach (var group in groups.OrderBy(g => g.Name))
            {
                AvailableGroups.Add(group);
            }
            
            LogInfo("Loaded {GroupCount} groups", groups.Count());
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке групп", ex);
        }
    }

    private async Task LoadStudentAsync(Guid studentId)
    {
        try
        {
            ShowInfo("Загрузка данных студента...");
            
            var student = await _studentService.GetStudentAsync(studentId);
            if (student == null)
            {
                SetError("Студент не найден");
                await _navigationService.GoBackAsync();
                return;
            }

            CurrentStudent = student;
            PopulateForm(student);
            
            ShowSuccess("Данные студента загружены");
            LogInfo("Loaded student: {StudentName}", $"{student.Person?.LastName} {student.Person?.FirstName}");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке студента", ex);
        }
    }

    private void PopulateForm(Student student)
    {
        // Основные свойства из Person
        FirstName = student.Person?.FirstName ?? string.Empty;
        LastName = student.Person?.LastName ?? string.Empty;
        MiddleName = student.Person?.MiddleName ?? string.Empty;
        Email = student.Person?.Email ?? string.Empty;
        PhoneNumber = student.Person?.PhoneNumber ?? string.Empty;
        BirthDate = student.Person?.DateOfBirth ?? DateTime.Now.AddYears(-18);

        // Свойства Student
        StudentCode = student.StudentCode;
        EnrollmentDate = student.EnrollmentDate;
        SelectedStatus = student.Status;

        // Примечание: EmergencyContactName, EmergencyContactPhone, MedicalInformation 
        // не существуют в модели Student - эти данные должны храниться отдельно
        
        // Выбираем группу из загруженного списка
        SelectedGroup = AvailableGroups.FirstOrDefault(g => g.Uid == student.GroupUid);
    }

    private void SetupForCreation()
    {
        CurrentStudent = null;
        ClearForm();
        GenerateStudentNumber();
    }

    private void ClearForm()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        MiddleName = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        StudentCode = string.Empty;
        EnrollmentDate = DateTime.Now;
        BirthDate = DateTime.Now;
        SelectedGroup = null;
        SelectedStatus = StudentStatus.Active;
        Address = string.Empty;
        EmergencyContactName = string.Empty;
        EmergencyContactPhone = string.Empty;
        MedicalInformation = string.Empty;
    }

    private void GenerateStudentNumber()
    {
        var year = DateTime.Now.Year;
        var random = new Random();
        StudentCode = $"ST{year}{random.Next(1000, 9999)}";
    }

    private async Task SaveAsync()
    {
        try
        {
            IsSaving = true;
            ClearError();

            if (IsEditMode && CurrentStudent != null)
            {
                await UpdateStudentAsync();
            }
            else
            {
                await CreateStudentAsync();
            }

            ShowSuccess(IsEditMode ? "Студент обновлен" : "Студент создан");
            
            // Для диалогов не используем навигацию
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("students");
            }
        }
        catch (Exception ex)
        {
            SetError("Ошибка при сохранении студента", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task UpdateStudentAsync()
    {
        if (CurrentStudent == null || SelectedGroup == null) return;

        if (CurrentStudent.Person != null)
        {
            // Обновляем данные Person
            CurrentStudent.Person.FirstName = FirstName;
            CurrentStudent.Person.LastName = LastName;
            CurrentStudent.Person.MiddleName = MiddleName;
            CurrentStudent.Person.Email = Email;
            CurrentStudent.Person.PhoneNumber = PhoneNumber;
            CurrentStudent.Person.DateOfBirth = BirthDate;

            // Обновляем данные Student
            CurrentStudent.StudentCode = StudentCode;
            CurrentStudent.EnrollmentDate = EnrollmentDate;
            CurrentStudent.Status = SelectedStatus;
            CurrentStudent.GroupUid = SelectedGroup.Uid;

            // Примечание: IsActive - read-only свойство, вычисляется автоматически
            // EmergencyContactName, EmergencyContactPhone, MedicalInformation не существуют

            var success = await _studentService.UpdateStudentAsync(CurrentStudent);
            if (success)
            {
                ShowSuccess($"Студент '{CurrentStudent.Person.FirstName} {CurrentStudent.Person.LastName}' обновлен");
            }
            else
            {
                SetError("Ошибка обновления студента");
            }
        }
    }

    private async Task CreateStudentAsync()
    {
        if (SelectedGroup == null) return;

        // Создаем нового студента
        var newStudent = new Student
        {
            StudentCode = StudentCode,
            EnrollmentDate = EnrollmentDate,
            Status = SelectedStatus,
            GroupUid = SelectedGroup.Uid,
            Person = new Person
            {
                FirstName = FirstName,
                LastName = LastName,
                MiddleName = MiddleName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                DateOfBirth = BirthDate
            }
        };

        var createdStudent = await _studentService.CreateStudentAsync(newStudent);
        if (createdStudent != null)
        {
            ShowSuccess($"Студент '{createdStudent.Person?.FirstName} {createdStudent.Person?.LastName}' создан");
        }
        else
        {
            SetError("Ошибка создания студента");
        }
    }

    private async Task DeleteAsync()
    {
        if (CurrentStudent == null) return;

        try
        {
            IsSaving = true;
            
            // Здесь можно добавить диалог подтверждения
            await _studentService.DeleteStudentAsync(CurrentStudent.Uid);
            
            ShowSuccess("Студент удален");
            LogInfo("Deleted student: {StudentName}", $"{CurrentStudent.Person?.LastName} {CurrentStudent.Person?.FirstName}");
            
            await _navigationService.NavigateToAsync("students");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при удалении: {ex.Message}", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CancelAsync()
    {
        // Для диалогов не используем навигацию
        if (_navigationService != null)
        {
            await _navigationService.GoBackAsync();
        }
    }

    private async Task CreateNewAsync()
    {
        SetupForCreation();
        IsEditMode = false;
        FormTitle = "Создание студента";
        ClearError();
    }

    private async Task EditAsync()
    {
        // Этот метод вызывается из диалога деталей для перехода к редактированию
        // Логика будет обработана в code-behind диалога
        await Task.CompletedTask;
    }

    private async Task CloseAsync()
    {
        // Этот метод вызывается для закрытия диалога деталей
        // Логика будет обработана в code-behind диалога
        await Task.CompletedTask;
    }

    #endregion
} 