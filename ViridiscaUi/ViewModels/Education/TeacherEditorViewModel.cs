using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для создания и редактирования преподавателей
/// </summary>
[Route("teacher-editor", DisplayName = "Редактор преподавателей", IconKey = "👨‍🏫", Order = 202, RequiredRoles = new[] { "Admin" })]
public class TeacherEditorViewModel : RoutableViewModelBase
{
    private readonly ITeacherService _teacherService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IDialogService _dialogService;

    #region Properties

    /// <summary>
    /// Флаг режима редактирования (true) или создания (false)
    /// </summary>
    [Reactive] public bool IsEditMode { get; set; }

    /// <summary>
    /// Текущий редактируемый преподаватель
    /// </summary>
    [Reactive] public Teacher? CurrentTeacher { get; set; }

    /// <summary>
    /// Идентификатор преподавателя для редактирования
    /// </summary>
    [Reactive] public Guid? TeacherId { get; set; }

    // Поля для редактирования
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string MiddleName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string PhoneNumber { get; set; } = string.Empty;
    [Reactive] public string Position { get; set; } = string.Empty;
    [Reactive] public string AcademicDegree { get; set; } = string.Empty;
    [Reactive] public string AcademicTitle { get; set; } = string.Empty;
    [Reactive] public int Experience { get; set; } = 0;
    [Reactive] public string Specialization { get; set; } = string.Empty;
    [Reactive] public string Biography { get; set; } = string.Empty;

    /// <summary>
    /// Доступные департаменты для выбора
    /// </summary>
    [Reactive] public ObservableCollection<string> AvailableDepartments { get; set; } = new();

    /// <summary>
    /// Предопределенные должности
    /// </summary>
    [Reactive] public ObservableCollection<string> AvailablePositions { get; set; } = new();

    /// <summary>
    /// Предопределенные ученые степени
    /// </summary>
    [Reactive] public ObservableCollection<string> AvailableDegrees { get; set; } = new();

    /// <summary>
    /// Предопределенные ученые звания
    /// </summary>
    [Reactive] public ObservableCollection<string> AvailableTitles { get; set; } = new();

    /// <summary>
    /// Флаг процесса сохранения
    /// </summary>
    [Reactive] public bool IsSaving { get; set; }

    /// <summary>
    /// Заголовок формы
    /// </summary>
    [Reactive] public string FormTitle { get; set; } = "Создание преподавателя";

    [Reactive] public ObservableCollection<string> Departments { get; set; } = new();
    [Reactive] public string? SelectedDepartment { get; set; }

    // Additional properties for dialogs
    [Reactive] public string EmployeeCode { get; set; } = string.Empty;
    [Reactive] public string Phone { get; set; } = string.Empty;
    [Reactive] public DateTime? BirthDate { get; set; }
    [Reactive] public string OfficeNumber { get; set; } = string.Empty;
    [Reactive] public string Address { get; set; } = string.Empty;
    [Reactive] public DateTime HireDate { get; set; } = DateTime.Today;
    [Reactive] public DateTime? TerminationDate { get; set; }
    [Reactive] public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    [Reactive] public decimal HourlyRate { get; set; } = 0;
    [Reactive] public string DepartmentName { get; set; } = string.Empty;

    // Computed properties for details dialog
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    public bool IsTerminated => Status == TeacherStatus.Terminated;
    
    // Statistics properties for details dialog
    [Reactive] public int CoursesCount { get; set; } = 0;
    [Reactive] public int GroupsCount { get; set; } = 0;
    [Reactive] public int StudentsCount { get; set; } = 0;
    [Reactive] public double AverageRating { get; set; } = 0.0;
    [Reactive] public string WorkExperience { get; set; } = "0 лет";
    [Reactive] public int ActiveCoursesCount { get; set; } = 0;
    [Reactive] public int TotalStudentsCount { get; set; } = 0;
    [Reactive] public double AverageCourseRating { get; set; } = 0.0;
    [Reactive] public int CompletedCoursesCount { get; set; } = 0;
    [Reactive] public int PublicationsCount { get; set; } = 0;
    
    // Collections for details dialog
    [Reactive] public ObservableCollection<CourseInstance> CourseInstances { get; set; } = new();
    [Reactive] public ObservableCollection<Group> CuratedGroups { get; set; } = new();
    
    // Computed properties for collections
    public bool HasCourses => CourseInstances.Any();
    public bool HasGroups => CuratedGroups.Any();

    #endregion

    #region Commands

    /// <summary>
    /// Команда сохранения преподавателя
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; } = null!;

    /// <summary>
    /// Команда отмены
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; } = null!;

    /// <summary>
    /// Команда удаления преподавателя (только в режиме редактирования)
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; } = null!;

    /// <summary>
    /// Команда создания нового преподавателя
    /// </summary>
    public ReactiveCommand<Unit, Unit> CreateNewCommand { get; set; } = null!;

    /// <summary>
    /// Команда редактирования (для диалога деталей)
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCommand { get; set; } = null!;

    /// <summary>
    /// Команда закрытия (для диалога деталей)
    /// </summary>
    public ReactiveCommand<Unit, Unit> CloseCommand { get; set; } = null!;

    /// <summary>
    /// Команда генерации кода сотрудника
    /// </summary>
    public ReactiveCommand<Unit, Unit> GenerateEmployeeCodeCommand { get; set; } = null!;

    /// <summary>
    /// Команда управления курсами
    /// </summary>
    public ReactiveCommand<Unit, Unit> ManageCoursesCommand { get; set; } = null!;

    /// <summary>
    /// Команда управления группами
    /// </summary>
    public ReactiveCommand<Unit, Unit> ManageGroupsCommand { get; set; } = null!;

    /// <summary>
    /// Команда просмотра статистики
    /// </summary>
    public ReactiveCommand<Unit, Unit> ViewStatisticsCommand { get; set; } = null!;

    /// <summary>
    /// Команда отправки сообщения
    /// </summary>
    public ReactiveCommand<Unit, Unit> SendMessageCommand { get; set; } = null!;

    #endregion

    /// <summary>
    /// Конструктор
    /// </summary>
    public TeacherEditorViewModel(
        IScreen hostScreen,
        ITeacherService teacherService,
        IUnifiedNavigationService navigationService,
        IDialogService dialogService) : base(hostScreen)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        InitializeCommands();
        InitializePredefinedValues();
    }

    /// <summary>
    /// Конструктор для диалогов с упрощенным набором зависимостей
    /// </summary>
    public TeacherEditorViewModel(ITeacherService teacherService, Teacher? teacher = null)
        : base(hostScreen: null!)  // Для диалогов hostScreen не нужен
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _navigationService = null!; // Для диалогов навигация не нужна
        _dialogService = null!; // Для диалогов DialogService может не понадобиться

        InitializeCommands();
        InitializePredefinedValues();

        if (teacher != null)
        {
            CurrentTeacher = teacher;
            IsEditMode = true;
            PopulateForm(teacher);
            FormTitle = "Редактирование преподавателя";
        }
        else
        {
            SetupForCreation();
            FormTitle = "Создание преподавателя";
        }
    }

    #region Lifecycle Methods

    /// <summary>
    /// Вызывается при первой загрузке ViewModel
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        
        if (CurrentTeacher != null)
        {
            await LoadTeacherDataAsync(CurrentTeacher.Uid);
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
            x => x.SelectedDepartment,
            x => x.Position,
            x => x.IsSaving,
            (firstName, lastName, email, department, position, isSaving) =>
                !string.IsNullOrWhiteSpace(firstName) &&
                !string.IsNullOrWhiteSpace(lastName) &&
                !string.IsNullOrWhiteSpace(email) &&
                department != null &&
                !string.IsNullOrWhiteSpace(position) &&
                !isSaving);

        SaveCommand = CreateCommand(SaveAsync, canSave, "Ошибка при сохранении преподавателя");
        CancelCommand = CreateCommand(CancelAsync, null, "Ошибка при отмене");
        
        var canDelete = this.WhenAnyValue(x => x.IsEditMode, x => x.IsSaving, 
            (isEdit, isSaving) => isEdit && !isSaving);
        DeleteCommand = CreateCommand(DeleteAsync, canDelete, "Ошибка при удалении преподавателя");
        
        CreateNewCommand = CreateCommand(CreateNewAsync, null, "Ошибка при создании нового преподавателя");
        
        // Additional commands for dialogs
        EditCommand = CreateCommand(EditAsync, null, "Ошибка при редактировании");
        CloseCommand = CreateCommand(CloseAsync, null, "Ошибка при закрытии");
        GenerateEmployeeCodeCommand = CreateCommand(GenerateEmployeeCodeAsync, null, "Ошибка при генерации кода");
        ManageCoursesCommand = CreateCommand(ManageCoursesAsync, null, "Ошибка при управлении курсами");
        ManageGroupsCommand = CreateCommand(ManageGroupsAsync, null, "Ошибка при управлении группами");
        ViewStatisticsCommand = CreateCommand(ViewStatisticsAsync, null, "Ошибка при просмотре статистики");
        SendMessageCommand = CreateCommand(SendMessageAsync, null, "Ошибка при отправке сообщения");
    }

    private void InitializePredefinedValues()
    {
        // Должности
        var positions = new[]
        {
            "Ассистент",
            "Преподаватель", 
            "Старший преподаватель",
            "Доцент",
            "Профессор",
            "Заведующий кафедрой"
        };
        AvailablePositions.Clear();
        foreach (var position in positions)
        {
            AvailablePositions.Add(position);
        }

        // Ученые степени
        var degrees = new[]
        {
            "",
            "Кандидат технических наук",
            "Кандидат физико-математических наук",
            "Кандидат экономических наук",
            "Кандидат педагогических наук",
            "Доктор технических наук",
            "Доктор физико-математических наук",
            "Доктор экономических наук",
            "Доктор педагогических наук"
        };
        AvailableDegrees.Clear();
        foreach (var degree in degrees)
        {
            AvailableDegrees.Add(degree);
        }

        // Ученые звания
        var titles = new[]
        {
            "",
            "Доцент",
            "Профессор"
        };
        AvailableTitles.Clear();
        foreach (var title in titles)
        {
            AvailableTitles.Add(title);
        }
    }

    private async Task LoadTeacherDataAsync(Guid teacherId)
    {
        try
        {
            ShowInfo("Загрузка данных преподавателя...");
            
            var teacher = await _teacherService.GetTeacherAsync(teacherId);
            if (teacher == null)
            {
                SetError("Преподаватель не найден");
                await _navigationService.GoBackAsync();
                return;
            }

            CurrentTeacher = teacher;
            PopulateForm(teacher);
            
            ShowSuccess("Данные преподавателя загружены");
            LogInfo("Loaded teacher: {TeacherName}", $"{teacher.LastName} {teacher.FirstName}");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке преподавателя", ex);
        }
    }

    public void PopulateForm(Teacher teacher)
    {
        FirstName = teacher.FirstName ?? string.Empty;
        LastName = teacher.LastName ?? string.Empty;
        MiddleName = teacher.MiddleName ?? string.Empty;
        EmployeeCode = teacher.EmployeeCode;
        Specialization = teacher.Specialization ?? string.Empty;
        AcademicDegree = teacher.AcademicDegree ?? string.Empty;
        AcademicTitle = teacher.AcademicTitle ?? string.Empty;
        HireDate = teacher.HireDate;
        HourlyRate = teacher.HourlyRate ?? 0;
        
        // Для работы в диалогах добавляем дополнительные поля
        // TODO: Эти поля нужно будет добавить в модель Teacher при необходимости
        // Phone = teacher.PhoneNumber ?? string.Empty;
        // BirthDate = teacher.BirthDate;
        // OfficeNumber = teacher.OfficeNumber ?? string.Empty;
        // Address = teacher.Address ?? string.Empty;
        // TerminationDate = teacher.TerminationDate;
        // Biography = teacher.Biography ?? string.Empty;
        
        // Устанавливаем статистику (для диалога деталей)
        // TODO: Получать реальную статистику из сервиса
        CoursesCount = 5;
        GroupsCount = 2;
        StudentsCount = 45;
        AverageRating = 4.8;
        WorkExperience = $"{DateTime.Now.Year - teacher.HireDate.Year} лет";
        ActiveCoursesCount = 3;
        TotalStudentsCount = 45;
        AverageCourseRating = 4.7;
        CompletedCoursesCount = 12;
        PublicationsCount = 8;
        
        DepartmentName = "Кафедра информационных технологий"; // TODO: Получать из департамента
    }

    private void SetupForCreation()
    {
        CurrentTeacher = null;
        ClearForm();
    }

    private void ClearForm()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        MiddleName = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        SelectedDepartment = null;
        Position = string.Empty;
        AcademicDegree = string.Empty;
        AcademicTitle = string.Empty;
        Experience = 0;
        Specialization = string.Empty;
        Biography = string.Empty;
    }

    private async Task SaveAsync()
    {
        try
        {
            IsSaving = true;
            ClearError();

            if (IsEditMode && CurrentTeacher != null)
            {
                await UpdateTeacherAsync();
            }
            else
            {
                await CreateTeacherAsync();
            }

            ShowSuccess(IsEditMode ? "Преподаватель обновлен" : "Преподаватель создан");
            
            // Для диалогов не используем навигацию
            if (_navigationService != null)
            {
                await _navigationService.NavigateToAsync("teachers");
            }
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

    private async Task UpdateTeacherAsync()
    {
        if (CurrentTeacher == null) return;

        // Обновляем данные Person
        CurrentTeacher.Person.FirstName = FirstName.Trim();
        CurrentTeacher.Person.LastName = LastName.Trim();
        CurrentTeacher.Person.MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim();
        CurrentTeacher.Person.Email = Email.Trim();
        CurrentTeacher.Person.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim();
        CurrentTeacher.Person.DateOfBirth = BirthDate ?? DateTime.MinValue;

        await _teacherService.UpdateTeacherAsync(CurrentTeacher);
        LogInfo("Updated teacher: {TeacherName}", $"{CurrentTeacher.Person.LastName} {CurrentTeacher.Person.FirstName}");
    }

    private async Task CreateTeacherAsync()
    {
        var newPerson = new Person
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
            Email = Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            DateOfBirth = BirthDate ?? DateTime.MinValue
        };

        var newTeacher = new Teacher
        {
            Uid = Guid.NewGuid(),
            PersonUid = newPerson.Uid,
            Person = newPerson,
            EmployeeCode = $"EMP{DateTime.Now.Year % 100:D2}{new Random().Next(1000, 9999)}",
            Specialization = string.IsNullOrWhiteSpace(Specialization) ? null : Specialization.Trim(),
            DepartmentUid = null, // SelectedDepartment это строка, нужно найти Department по имени
            HireDate = HireDate,
            HourlyRate = HourlyRate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        await _teacherService.CreateTeacherAsync(newTeacher);
        LogInfo("Created teacher: {TeacherName}", $"{newTeacher.Person.LastName} {newTeacher.Person.FirstName}");
    }

    private async Task DeleteAsync()
    {
        if (CurrentTeacher == null) return;

        try
        {
            IsSaving = true;
            
            // Здесь можно добавить диалог подтверждения
            await _teacherService.DeleteTeacherAsync(CurrentTeacher.Uid);
            
            ShowSuccess("Преподаватель удален");
            LogInfo("Deleted teacher: {TeacherName}", $"{CurrentTeacher.Person.LastName} {CurrentTeacher.Person.FirstName}");
            
            await _navigationService.NavigateToAsync("teachers");
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
        FormTitle = "Создание преподавателя";
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

    private async Task GenerateEmployeeCodeAsync()
    {
        try
        {
            EmployeeCode = $"EMP{DateTime.Now.Year % 100:D2}{new Random().Next(1000, 9999)}";
            ShowInfo("Код сотрудника сгенерирован");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при генерации кода", ex);
        }
    }

    private async Task ManageCoursesAsync()
    {
        if (CurrentTeacher == null)
        {
            ShowError("Преподаватель не выбран");
            return;
        }

        var allCourseInstances = new List<CourseInstance>(); // await _courseInstanceService.GetAllCourseInstancesAsync();
        await _dialogService.ShowTeacherCoursesManagementDialogAsync(CurrentTeacher, allCourseInstances);
    }

    private async Task ManageGroupsAsync()
    {
        if (CurrentTeacher == null) return;

        try
        {
            // TODO: Получить все доступные группы
            var allGroups = new List<Group>(); // await _groupService.GetAllGroupsAsync();
            await _dialogService.ShowTeacherGroupsManagementDialogAsync(CurrentTeacher, allGroups);
        }
        catch (Exception ex)
        {
            SetError("Ошибка при управлении группами", ex);
        }
    }

    private async Task ViewStatisticsAsync()
    {
        if (CurrentTeacher == null) return;

        try
        {
            // TODO: Получить статистику преподавателя
            var statistics = new
            {
                CoursesCount = CoursesCount,
                GroupsCount = GroupsCount,
                StudentsCount = StudentsCount,
                AverageRating = AverageRating,
                WorkExperience = WorkExperience
            };

            await _dialogService.ShowTeacherStatisticsDialogAsync(FullName, statistics);
        }
        catch (Exception ex)
        {
            SetError("Ошибка при просмотре статистики", ex);
        }
    }

    private async Task SendMessageAsync()
    {
        if (CurrentTeacher == null) return;

        try
        {
            // TODO: Реализовать отправку сообщения преподавателю
            ShowInfo($"Отправка сообщения преподавателю {FullName}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            SetError("Ошибка при отправке сообщения", ex);
        }
    }

    #endregion
} 