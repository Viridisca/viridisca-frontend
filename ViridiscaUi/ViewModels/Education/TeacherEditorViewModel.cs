using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Bases.Navigations;

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

    private void PopulateForm(Teacher teacher)
    {
        FirstName = teacher.FirstName;
        LastName = teacher.LastName;
        MiddleName = teacher.MiddleName;
        // Email и PhoneNumber - read-only свойства из User
        // Position - read-only свойство
        AcademicDegree = teacher.AcademicDegree ?? string.Empty;
        AcademicTitle = teacher.AcademicTitle ?? string.Empty;
        Specialization = teacher.Specialization ?? string.Empty;
        // Не используем несуществующие свойства
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
            await _navigationService.NavigateToAsync("teachers");
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

        // Обновляем только существующие settable свойства
        CurrentTeacher.FirstName = FirstName.Trim();
        CurrentTeacher.LastName = LastName.Trim();
        CurrentTeacher.MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? string.Empty : MiddleName.Trim();
        CurrentTeacher.AcademicDegree = string.IsNullOrWhiteSpace(AcademicDegree) ? string.Empty : AcademicDegree.Trim();
        CurrentTeacher.AcademicTitle = string.IsNullOrWhiteSpace(AcademicTitle) ? string.Empty : AcademicTitle.Trim();
        CurrentTeacher.Specialization = string.IsNullOrWhiteSpace(Specialization) ? string.Empty : Specialization.Trim();

        await _teacherService.UpdateTeacherAsync(CurrentTeacher);
        LogInfo("Updated teacher: {TeacherName}", $"{CurrentTeacher.LastName} {CurrentTeacher.FirstName}");
    }

    private async Task CreateTeacherAsync()
    {
        var newTeacher = new Teacher(
            employeeCode: $"T{DateTime.Now:yyyyMMddHHmmss}",
            userUid: Guid.Empty, // Temporary
            hireDate: DateTime.Now,
            specialization: Specialization.Trim(),
            hourlyRate: 1000m,
            lastName: LastName.Trim(),
            firstName: FirstName.Trim(),
            middleName: string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
            academicDegree: string.IsNullOrWhiteSpace(AcademicDegree) ? null : AcademicDegree.Trim(),
            academicTitle: string.IsNullOrWhiteSpace(AcademicTitle) ? null : AcademicTitle.Trim()
        );

        await _teacherService.CreateTeacherAsync(newTeacher);
        LogInfo("Created teacher: {TeacherName}", $"{newTeacher.LastName} {newTeacher.FirstName}");
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
            LogInfo("Deleted teacher: {TeacherName}", $"{CurrentTeacher.LastName} {CurrentTeacher.FirstName}");
            
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
        await _navigationService.GoBackAsync();
    }

    private async Task CreateNewAsync()
    {
        SetupForCreation();
        IsEditMode = false;
        FormTitle = "Создание преподавателя";
        ClearError();
    }

    #endregion
} 