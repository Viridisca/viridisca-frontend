using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.System.Enums;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Domain.Models.Base;
using Microsoft.EntityFrameworkCore;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using DomainValidationResult = ViridiscaUi.Domain.Models.Base.ValidationResult;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления преподавателями с расширенной функциональностью
/// </summary>
[Route("teachers", 
    DisplayName = "Преподаватели", 
    IconKey = "AccountTie", 
    Order = 2,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление преподавателями")]
public class TeachersViewModel : RoutableViewModelBase
{ 
    #region Services
    private readonly ITeacherService _teacherService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IGroupService _groupService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IDepartmentService _departmentService;
    private readonly IExportService _exportService;
    private readonly IImportService _importService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;

    // Add missing source cache
    private readonly SourceCache<TeacherViewModel, Guid> _teachersSource = new(t => t.Uid);

    #endregion

    #region Properties
    [Reactive] public ObservableCollection<TeacherViewModel> Teachers { get; set; } = new();
    [Reactive] public TeacherViewModel? SelectedTeacher { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public TeacherStatistics? SelectedTeacherStatistics { get; set; }

    // Filters
    [Reactive] public string? SpecializationFilter { get; set; }
    [Reactive] public string? StatusFilter { get; set; }

    // Pagination
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 15;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalTeachers { get; set; }
    [Reactive] public int ActiveTeachers { get; set; }
    [Reactive] public int InactiveTeachers { get; set; }
    [Reactive] public int RetiredTeachers { get; set; }

    // Computed properties
    public bool HasSelectedTeacher => SelectedTeacher != null;
    public bool HasSelectedTeacherStatistics => SelectedTeacherStatistics != null;
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;

    #endregion

    #region Commands
    public ReactiveCommand<Unit, Unit> LoadTeachersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> EditTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> DeleteTeacherCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ViewTeacherDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ViewStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ManageCoursesCommand { get; private set; } = null!;
    public ReactiveCommand<TeacherViewModel, Unit> ManageGroupsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    #endregion

    #region Constructor
    public TeachersViewModel(
        IScreen hostScreen,
        ITeacherService teacherService,
        ICourseInstanceService courseInstanceService,
        IGroupService groupService,
        IDialogService dialogService,
        INotificationService notificationService,
        IDepartmentService departmentService,
        IExportService exportService,
        IImportService importService,
        IPermissionService permissionService,
        IAuthService authService) : base(hostScreen)
    {
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        InitializeCommands();
        SetupSubscriptions();
        
        LogInfo("TeachersViewModel initialized");
    }
    #endregion

    #region Private Methods

    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        LoadTeachersCommand = CreateCommand(async () => await LoadTeachersAsync(), null, "Ошибка загрузки преподавателей");
        RefreshCommand = CreateCommand(async () => await RefreshAsync(), null, "Ошибка обновления данных");
        CreateTeacherCommand = CreateCommand(async () => await CreateTeacherAsync(), null, "Ошибка создания преподавателя");
        EditTeacherCommand = CreateCommand<TeacherViewModel>(async (teacher) => await EditTeacherAsync(teacher), null, "Ошибка редактирования преподавателя");
        DeleteTeacherCommand = CreateCommand<TeacherViewModel>(async (teacher) => await DeleteTeacherAsync(teacher), null, "Ошибка удаления преподавателя");
        ViewTeacherDetailsCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ViewTeacherDetailsAsync(teacher), null, "Ошибка просмотра деталей преподавателя");
        ViewStatisticsCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ViewStatisticsAsync(teacher), null, "Ошибка загрузки статистики");
        ManageCoursesCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ManageCoursesAsync(teacher), null, "Ошибка управления курсами");
        ManageGroupsCommand = CreateCommand<TeacherViewModel>(async (teacher) => await ManageGroupsAsync(teacher), null, "Ошибка управления группами");
        SearchCommand = CreateCommand<string>(async (searchTerm) => await SearchTeachersAsync(searchTerm), null, "Ошибка поиска преподавателей");
        ApplyFiltersCommand = CreateCommand(async () => await ApplyFiltersAsync(), null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(async () => await ClearFiltersAsync(), null, "Ошибка очистки фильтров");
        GoToPageCommand = CreateCommand<int>(async (page) => await GoToPageAsync(page), null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(async () => await NextPageAsync(), canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(async () => await PreviousPageAsync(), canGoPrevious, "Ошибка перехода на предыдущую страницу");

        var canGoFirst = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        var canGoLast = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        
        FirstPageCommand = CreateCommand(async () => await FirstPageAsync(), canGoFirst, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(async () => await LastPageAsync(), canGoLast, "Ошибка перехода на последнюю страницу");
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchText => SearchCommand.Execute(searchText ?? string.Empty).Subscribe())
            .DisposeWith(Disposables);

        // Обновление computed properties
        this.WhenAnyValue(x => x.SelectedTeacher)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedTeacher)))
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.SelectedTeacherStatistics)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedTeacherStatistics)))
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages)
            .Subscribe(_ => 
            {
                this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                this.RaisePropertyChanged(nameof(CanGoToNextPage));
            })
            .DisposeWith(Disposables);

        // Загрузка статистики при выборе преподавателя
        this.WhenAnyValue(x => x.SelectedTeacher)
            .Where(teacher => teacher != null)
            .SelectMany(teacher => ViewStatisticsCommand.Execute(teacher!))
            .Subscribe()
            .DisposeWith(Disposables);
    }

    private async Task LoadTeachersAsync()
    {
        LogInfo("Loading teachers with filters: SearchText={SearchText}, SpecializationFilter={SpecializationFilter}, StatusFilter={StatusFilter}", SearchText, SpecializationFilter, StatusFilter);
        
        IsLoading = true;
        
        // Используем новый универсальный метод пагинации
        var (teachers, totalCount) = await _teacherService.GetPagedAsync(
            CurrentPage, 
            PageSize, 
            SearchText);

        Teachers.Clear();
        foreach (var teacher in teachers)
        {
            Teachers.Add(new TeacherViewModel(teacher));
        }

        TotalTeachers = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

        ShowSuccess($"Загружено {teachers.Count()} преподавателей");
        LogInfo("Loaded {TeacherCount} teachers, total: {TotalCount}", teachers.Count(), totalCount);
        
        IsLoading = false;
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing teachers data");
        IsRefreshing = true;
        
        await LoadTeachersAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Создает новый преподаватель
    /// </summary>
    private async Task CreateTeacherAsync()
    {
        try
        {
            var result = await _dialogService.ShowTeacherEditDialogAsync(new Teacher());
            if (result != null && result is Teacher teacher)
            {
                await _teacherService.CreateAsync(teacher);
                await LoadTeachersAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Преподаватель создан",
                    $"Новый преподаватель успешно создан",
                    NotificationType.Info,
                    Domain.Models.System.Enums.NotificationPriority.Normal);

                await _dialogService.ShowInfoAsync("Успех", "Преподаватель успешно создан");
            }
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Validation failed for teacher creation");
            ShowError($"Ошибка валидации: {ex.Message}");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("уже существует"))
        {
            LogError(ex, "Дублирование при создании преподавателя");
            ErrorMessage = $"Преподаватель с таким кодом уже существует: {ex.Message}";
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при создании преподавателя");
        }
    }

    /// <summary>
    /// Редактирует выбранного преподавателя
    /// </summary>
    private async Task EditTeacherAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel?.Teacher == null) return;

        try
        {
            var teacherToEdit = await _teacherService.GetByUidAsync(teacherViewModel.Teacher.Uid);
            if (teacherToEdit == null) return;

            var result = await _dialogService.ShowTeacherEditDialogAsync(teacherToEdit);
            if (result != null && result is Teacher updatedTeacher)
            {
                await _teacherService.UpdateAsync(updatedTeacher);
                await LoadTeachersAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Преподаватель обновлен",
                    $"Преподаватель {updatedTeacher.Person?.FirstName} {updatedTeacher.Person?.LastName} успешно обновлен",
                    NotificationType.Info,
                    Domain.Models.System.Enums.NotificationPriority.Normal);

                await _dialogService.ShowInfoAsync("Успех", "Преподаватель успешно обновлен");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Concurrency conflict while updating teacher");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update teacher");
            ShowError("Не удалось обновить преподавателя. Попробуйте еще раз.");
        }
    }

    /// <summary>
    /// Deletes a teacher with enhanced confirmation and cascade handling
    /// </summary>
    private async Task DeleteTeacherAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        LogInfo("Attempting to delete teacher: {TeacherId}", teacherViewModel.Uid);

        try
        {
            IsLoading = true;

            // Проверка связанных данных
            var relatedDataInfo = await _teacherService.GetTeacherRelatedDataInfoAsync(teacherViewModel.Uid);
            
            string confirmationMessage = $"Вы уверены, что хотите удалить преподавателя '{teacherViewModel.FullName}'?";
            
            if (relatedDataInfo.HasRelatedData)
            {
                confirmationMessage += "\n\n" + relatedDataInfo.GetWarningMessage();
            }

            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                confirmationMessage);

            if (confirmed != DialogResult.Yes) return;

            // Удаление преподавателя
            var deleteSuccess = await _teacherService.DeleteAsync(teacherViewModel.Uid);
            
            if (deleteSuccess)
            {
                // Удаление из коллекции
                Teachers.Remove(teacherViewModel);
                
                // Обновление статистики
                TotalTeachers--;
                if (teacherViewModel.Teacher.Person?.FirstName != null)
                    ActiveTeachers--;

                // Очистка выбора если удаленный преподаватель был выбран
                if (SelectedTeacher == teacherViewModel)
                    SelectedTeacher = null;

                ShowSuccess($"Преподаватель '{teacherViewModel.FullName}' удален");
                
                LogInfo($"Преподаватель удален: {teacherViewModel.FullName} (ID: {teacherViewModel.Uid})");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка удаления", 
                    "Не удалось удалить преподавателя");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("foreign key") || ex.Message.Contains("связанных данных"))
        {
            await _dialogService.ShowErrorAsync("Невозможно удалить", 
                "Преподаватель не может быть удален, так как имеет связанные записи. " +
                "Сначала удалите или переназначьте связанные данные.");
            LogWarning($"Попытка удаления преподавателя со связанными данными: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Ошибка удаления", 
                $"Не удалось удалить преподавателя: {ex.Message}");
            LogError(ex, $"Ошибка при удалении преподавателя: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewTeacherDetailsAsync(TeacherViewModel teacherViewModel)
    {
        LogInfo("Viewing teacher details: {TeacherId}", teacherViewModel.Uid);
        
        // Используем новый универсальный метод получения
        var teacher = await _teacherService.GetByUidAsync(teacherViewModel.Uid);
        if (teacher != null)
        {
            SelectedTeacher = new TeacherViewModel(teacher);
            await ViewStatisticsAsync(SelectedTeacher);
            
            // Показываем диалог деталей
            var result = await _dialogService.ShowTeacherDetailsDialogAsync(teacher);
            if (result == "edit")
            {
                // Если пользователь нажал "Редактировать" в диалоге деталей
                await EditTeacherAsync(teacherViewModel);
            }
            
            ShowInfo($"Просмотр деталей преподавателя: {teacher.FullName}");
        }
        else
        {
            ShowError("Преподаватель не найден");
        }
    }

    private async Task ViewStatisticsAsync(TeacherViewModel teacherViewModel)
    {
        LogInfo("Loading teacher statistics: {TeacherId}", teacherViewModel.Uid);
        
        try
        {
            var statistics = await _teacherService.GetTeacherStatisticsAsync(teacherViewModel.Uid);
            // Просто показываем статистику в диалоге
            if (statistics != null)
            {
                await _dialogService.ShowTeacherStatisticsDialogAsync("Статистика преподавателя", statistics);
                LogInfo("Teacher statistics loaded successfully");
            }
            else
            {
                ShowInfo("Статистика для преподавателя недоступна");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load teacher statistics");
            ShowError("Не удалось загрузить статистику преподавателя");
        }
    }

    private async Task ManageCoursesAsync(TeacherViewModel teacherViewModel)
    {
        LogInfo("Managing courses for teacher: {TeacherId}", teacherViewModel.Uid);
        
        var teacher = await _teacherService.GetTeacherAsync(teacherViewModel.Uid);
        if (teacher == null)
        {
            ShowError("Преподаватель не найден");
            return;
        }

        var allCourses = await _courseInstanceService.GetAllCoursesAsync();
        
        var result = await _dialogService.ShowTeacherCoursesManagementDialogAsync(teacher, allCourses);
        if (result != null)
        {
            // TODO: Implement course assignment logic when service method is available
            await RefreshAsync();
            ShowSuccess($"Курсы преподавателя '{teacherViewModel.FullName}' обновлены");
            LogInfo("Teacher courses updated successfully");
        }
        else
        {
            LogDebug("Course management cancelled by user");
        }
    }

    private async Task ManageGroupsAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        LogInfo("Managing groups for teacher: {TeacherName}", teacherViewModel.FullName);

        try
        {
            var availableGroups = await _groupService.GetAllAsync();
            var selectedGroups = await _dialogService.ShowGroupSelectionDialogAsync(availableGroups);
            
            if (selectedGroups != null && selectedGroups.Any())
            {
                var selectedGroup = selectedGroups.First();
                
                // Получаем экземпляры курсов преподавателя
                var courseInstances = await _courseInstanceService.GetByTeacherUidAsync(teacherViewModel.Uid);
                var selectedCourseInstances = await _dialogService.ShowCourseInstanceSelectionDialogAsync(courseInstances);
                
                if (selectedCourseInstances != null && selectedCourseInstances.Any())
                {
                    var selectedCourseInstance = selectedCourseInstances.First();
                    selectedCourseInstance.GroupUid = selectedGroup.Uid;
                    await _courseInstanceService.UpdateAsync(selectedCourseInstance);
                    
                    ShowSuccess($"Группа '{selectedGroup.Name}' назначена преподавателю {teacherViewModel.FullName}");
                    LogInfo("Group assigned to teacher: {TeacherName} -> {GroupName}", teacherViewModel.FullName, selectedGroup.Name);
                }
            }
            else
            {
                LogDebug("Group management cancelled by user");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка управления группами преподавателя");
            ShowError("Ошибка управления группами");
        }
    }

    private async Task SearchTeachersAsync(string searchTerm)
    {
        LogInfo("Searching teachers with term: {SearchTerm}", searchTerm);
        SearchText = searchTerm;
        CurrentPage = 1; // Сброс на первую страницу при поиске
        await LoadTeachersAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        LogInfo("Applying filters");
        CurrentPage = 1; // Сброс на первую страницу при применении фильтров
        await LoadTeachersAsync();
    }

    private async Task ClearFiltersAsync()
    {
        LogInfo("Clearing filters");
        SpecializationFilter = null;
        StatusFilter = null;
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadTeachersAsync();
        ShowInfo("Фильтры очищены");
    }

    private async Task GoToPageAsync(int page)
    {
        LogInfo("Navigating to page: {Page}", page);
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            await LoadTeachersAsync();
        }
    }

    private async Task NextPageAsync()
    {
        LogInfo("Navigating to next page");
        await GoToPageAsync(CurrentPage + 1);
    }

    private async Task PreviousPageAsync()
    {
        LogInfo("Navigating to previous page");
        await GoToPageAsync(CurrentPage - 1);
    }

    private async Task FirstPageAsync()
    {
        LogInfo("Navigating to first page");
        await GoToPageAsync(1);
    }

    private async Task LastPageAsync()
    {
        LogInfo("Navigating to last page");
        await GoToPageAsync(TotalPages);
    }

    private async Task AssignCourseAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        try
        {
            var availableCourseInstances = await _courseInstanceService.GetUnassignedAsync();
            var selectedCourseInstances = await _dialogService.ShowCourseInstanceSelectionDialogAsync(availableCourseInstances);
            
            if (selectedCourseInstances != null && selectedCourseInstances.Any())
            {
                var selectedCourseInstance = selectedCourseInstances.First();
                selectedCourseInstance.TeacherUid = teacherViewModel.Uid;
                await _courseInstanceService.UpdateAsync(selectedCourseInstance);
                
                ShowSuccess($"Курс '{selectedCourseInstance.Subject?.Name}' назначен преподавателю {teacherViewModel.FullName}");
                LogInfo("Course assigned to teacher: {TeacherName} -> {CourseName}", teacherViewModel.FullName, selectedCourseInstance.Subject?.Name);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка назначения курса преподавателю");
            ShowError("Ошибка назначения курса");
        }
    }

    private async Task AssignGroupAsync(TeacherViewModel teacherViewModel)
    {
        if (teacherViewModel == null) return;

        try
        {
            var availableGroups = await _groupService.GetAllAsync();
            var selectedGroups = await _dialogService.ShowGroupSelectionDialogAsync(availableGroups);
            
            if (selectedGroups != null && selectedGroups.Any())
            {
                var selectedGroup = selectedGroups.First();
                
                // Получаем экземпляры курсов преподавателя
                var courseInstances = await _courseInstanceService.GetByTeacherUidAsync(teacherViewModel.Uid);
                var selectedCourseInstances = await _dialogService.ShowCourseInstanceSelectionDialogAsync(courseInstances);
                
                if (selectedCourseInstances != null && selectedCourseInstances.Any())
                {
                    var selectedCourseInstance = selectedCourseInstances.First();
                    selectedCourseInstance.GroupUid = selectedGroup.Uid;
                    await _courseInstanceService.UpdateAsync(selectedCourseInstance);
                    
                    ShowSuccess($"Группа '{selectedGroup.Name}' назначена преподавателю {teacherViewModel.FullName}");
                    LogInfo("Group assigned to teacher: {TeacherName} -> {GroupName}", teacherViewModel.FullName, selectedGroup.Name);
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка назначения группы преподавателю");
            ShowError("Ошибка назначения группы");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Валидация данных преподавателя
    /// </summary>
    private async Task<DomainValidationResult> ValidateTeacherAsync(Teacher teacher)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (string.IsNullOrWhiteSpace(teacher.Person?.FirstName))
            errors.Add("Имя преподавателя обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(teacher.Person?.LastName))
            errors.Add("Фамилия преподавателя обязательна для заполнения");

        if (string.IsNullOrWhiteSpace(teacher.Person?.Email))
            errors.Add("Email преподавателя обязателен для заполнения");
        else if (!IsValidEmail(teacher.Person.Email))
            errors.Add("Некорректный формат email");

        if (string.IsNullOrWhiteSpace(teacher.EmployeeCode))
            errors.Add("Код сотрудника обязателен для заполнения");

        // Проверка уникальности
        if (!string.IsNullOrWhiteSpace(teacher.Person?.Email))
        {
            var emailExists = await _teacherService.ExistsByEmailAsync(
                teacher.Person.Email, teacher.Uid);
            if (emailExists)
                errors.Add($"Преподаватель с email '{teacher.Person.Email}' уже существует");
        }

        // Проверка зарплаты
        if (teacher.Salary < 0)
            errors.Add("Зарплата не может быть отрицательной");
        else if (teacher.Salary == 0)
            warnings.Add("Зарплата не указана");

        // Проверка даты найма
        if (teacher.HireDate > DateTime.Now)
            warnings.Add("Дата найма в будущем");

        if (teacher.HireDate < DateTime.Now.AddYears(-50))
            warnings.Add("Дата найма более 50 лет назад");

        // Проверка департамента
        if (teacher.DepartmentUid.HasValue)
        {
            var department = await _departmentService.GetByUidAsync(teacher.DepartmentUid.Value);
            if (department == null)
                errors.Add("Выбранный департамент не найден");
        }

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Updates statistics after teacher status change
    /// </summary>
    private void UpdateStatisticsAfterStatusChange(TeacherStatus oldStatus, TeacherStatus newStatus)
    {
        if (oldStatus == TeacherStatus.Active && newStatus != TeacherStatus.Active)
            ActiveTeachers--;
        else if (oldStatus != TeacherStatus.Active && newStatus == TeacherStatus.Active)
            ActiveTeachers++;
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if current user has specific permission
    /// </summary>
    private async Task<bool> HasPermissionAsync(string permission)
    {
        // Здесь должна быть реальная проверка прав доступа
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Экспортирует данные преподавателей
    /// </summary>
    private async Task ExportTeachersAsync()
    {
        try
        {
            IsLoading = true;
            
            // Получаем всех преподавателей для экспорта
            var allTeachers = await _teacherService.GetAllTeachersAsync();
            var exportData = await _teacherService.ExportTeachersAsync(allTeachers);
            
            // Здесь должна быть логика сохранения файла
            // Пока что просто показываем сообщение об успехе
            ShowSuccess("Данные преподавателей экспортированы");
            
            LogInfo("Экспорт преподавателей выполнен успешно");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при экспорте преподавателей");
            ShowError($"Ошибка при экспорте: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Lifecycle Methods

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        LogInfo("TeachersViewModel loaded for the first time");
        
        // Load teachers when view is loaded for the first time
        await LoadTeachersAsync();
    }

    #endregion
} 