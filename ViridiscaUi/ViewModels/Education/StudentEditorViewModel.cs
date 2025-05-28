using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.Students;

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
            LogInfo("Loaded student: {StudentName}", $"{student.LastName} {student.FirstName}");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке студента", ex);
        }
    }

    private void PopulateForm(Student student)
    {
        FirstName = student.FirstName;
        LastName = student.LastName;
        MiddleName = student.MiddleName ?? string.Empty;
        Email = student.Email;
        PhoneNumber = student.PhoneNumber ?? string.Empty;
        StudentCode = student.StudentCode;
        EnrollmentDate = student.EnrollmentDate;
        BirthDate = student.BirthDate;
        SelectedStatus = student.Status;
        
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
            await _navigationService.NavigateToAsync("students");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при сохранении: {ex.Message}", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task UpdateStudentAsync()
    {
        if (CurrentStudent == null || SelectedGroup == null) return;

        var updatedStudent = new Student
        {
            Uid = CurrentStudent.Uid,
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
            Email = Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            StudentCode = StudentCode.Trim(),
            EnrollmentDate = EnrollmentDate,
            BirthDate = BirthDate,
            GroupUid = SelectedGroup.Uid,
            Status = SelectedStatus,
            IsActive = SelectedStatus == StudentStatus.Active,
            CreatedAt = CurrentStudent.CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        await _studentService.UpdateStudentAsync(updatedStudent);
        LogInfo("Updated student: {StudentName}", $"{updatedStudent.LastName} {updatedStudent.FirstName}");
    }

    private async Task CreateStudentAsync()
    {
        if (SelectedGroup == null) return;

        var newStudent = new Student
        {
            Uid = Guid.NewGuid(),
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
            Email = Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            StudentCode = StudentCode.Trim(),
            EnrollmentDate = EnrollmentDate,
            BirthDate = BirthDate,
            GroupUid = SelectedGroup.Uid,
            Status = SelectedStatus,
            IsActive = SelectedStatus == StudentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        await _studentService.CreateStudentAsync(newStudent);
        LogInfo("Created student: {StudentName}", $"{newStudent.LastName} {newStudent.FirstName}");
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
            LogInfo("Deleted student: {StudentName}", $"{CurrentStudent.LastName} {CurrentStudent.FirstName}");
            
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
        await _navigationService.GoBackAsync();
    }

    private async Task CreateNewAsync()
    {
        SetupForCreation();
        IsEditMode = false;
        FormTitle = "Создание студента";
        ClearError();
    }

    #endregion
} 