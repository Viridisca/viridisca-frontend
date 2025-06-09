using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.System.Enums;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.Domain.Models.Base;
using Microsoft.EntityFrameworkCore;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using static ViridiscaUi.Services.Interfaces.IGroupService;
using DomainValidationResult = ViridiscaUi.Domain.Models.Base.ValidationResult;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления группами
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("groups", 
    DisplayName = "Группы", 
    IconKey = "AccountMultiple", 
    Order = 3,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление учебными группами")]
public class GroupsViewModel : RoutableViewModelBase
{
    private readonly IGroupService _groupService;
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly IDialogService _dialogService;
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;

    // === СВОЙСТВА ===
    
    [Reactive] public ObservableCollection<GroupViewModel> Groups { get; set; } = new();
    [Reactive] public GroupViewModel? SelectedGroup { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public bool HasErrors { get; set; }
    [Reactive] public GroupStatistics? SelectedGroupStatistics { get; set; }
    
    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalGroups { get; set; }

    // Computed properties for UI binding
    public bool HasSelectedGroup => SelectedGroup != null;
    public bool HasSelectedGroupStatistics => SelectedGroupStatistics != null;

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> EditGroupCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeleteGroupCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> ViewGroupDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> LoadGroupStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> AssignCuratorCommand { get; private set; } = null!;
    public ReactiveCommand<GroupViewModel, Unit> ManageStudentsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;

    public GroupsViewModel(
        IScreen hostScreen,
        IGroupService groupService,
        IStudentService studentService,
        ITeacherService teacherService,
        IDialogService dialogService,
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService) : base(hostScreen)
    {
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        InitializeCommands();
        SetupSubscriptions();
        
        LogInfo("GroupsViewModel initialized");
    }

    #region Private Methods

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        LoadGroupsCommand = CreateCommand(LoadGroupsAsync, null, "Ошибка загрузки групп");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        CreateGroupCommand = CreateCommand(CreateGroupAsync, null, "Ошибка создания группы");
        EditGroupCommand = ReactiveCommand.CreateFromTask(EditGroupAsync);
        DeleteGroupCommand = ReactiveCommand.CreateFromTask(DeleteGroupAsync);
        ViewGroupDetailsCommand = CreateCommand<GroupViewModel>(ViewGroupDetailsAsync, null, "Ошибка просмотра деталей группы");
        LoadGroupStatisticsCommand = CreateCommand<GroupViewModel>(LoadGroupStatisticsAsync, null, "Ошибка загрузки статистики группы");
        AssignCuratorCommand = CreateCommand<GroupViewModel>(AssignCuratorAsync, null, "Ошибка назначения куратора");
        ManageStudentsCommand = CreateCommand<GroupViewModel>(ManageStudentsAsync, null, "Ошибка управления студентами");
        SearchCommand = CreateCommand<string>(SearchGroupsAsync, null, "Ошибка поиска групп");
        GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");

        var canGoFirst = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        var canGoLast = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        
        FirstPageCommand = CreateCommand(FirstPageAsync, canGoFirst, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(LastPageAsync, canGoLast, "Ошибка перехода на последнюю страницу");
    }

    /// <summary>
    /// Настраивает подписки на изменения свойств
    /// </summary>
    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchText => SearchCommand.Execute(searchText ?? string.Empty).Subscribe())
            .DisposeWith(Disposables);

        // Загрузка статистики при выборе группы
        this.WhenAnyValue(x => x.SelectedGroup)
            .Where(group => group != null)
            .Select(group => group!)
            .InvokeCommand(LoadGroupStatisticsCommand)
            .DisposeWith(Disposables);

        // Уведомления об изменении computed properties
        this.WhenAnyValue(x => x.SelectedGroup)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroup)))
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.SelectedGroupStatistics)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroupStatistics)))
            .DisposeWith(Disposables);
    }

    private async Task LoadGroupsAsync()
    {
        LogInfo("Loading groups with search text: {SearchText}", SearchText);
        
        IsLoading = true;
        ShowInfo("Загрузка групп...");

        try
        {
            // Используем новый универсальный метод пагинации
            var (groups, totalCount) = await _groupService.GetPagedAsync(CurrentPage, PageSize, SearchText);
            
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(new GroupViewModel(group));
            }

            TotalGroups = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            ShowSuccess($"Загружено {Groups.Count} групп");
            LogInfo("Loaded {GroupCount} groups, total: {TotalCount}", Groups.Count, totalCount);
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load groups");
            ShowError("Не удалось загрузить список групп");
            Groups.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing groups data");
        IsRefreshing = true;
        
        await LoadGroupsAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Создание новой группы
    /// </summary>
    private async Task CreateGroupAsync()
    {
        try
        {
            var newGroup = await _dialogService.ShowGroupEditDialogAsync(new Group());
            if (newGroup != null)
            {
                var createdGroup = await _groupService.CreateAsync(newGroup);
                if (createdGroup != null)
                {
                    LogInfo($"Группа '{createdGroup.Name}' успешно создана");
                    await LoadGroupsAsync();
                }
                else
                {
                    LogWarning("Не удалось создать группу");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при создании группы");
        }
    }

    /// <summary>
    /// Редактирование группы с optimistic locking
    /// </summary>
    private async Task EditGroupAsync()
    {
        if (SelectedGroup == null) return;

        try
        {
            // Используем реальный диалог редактирования
            var editedGroup = await _dialogService.ShowGroupEditDialogAsync(SelectedGroup.ToGroup());
            
            if (editedGroup != null)
            {
                var updateResult = await _groupService.UpdateAsync(editedGroup);
                if (updateResult)
                {
                    // Получаем обновленную группу
                    var updatedGroup = await _groupService.GetByUidAsync(editedGroup.Uid);
                    if (updatedGroup != null)
                    {
                        LogInfo($"Группа '{updatedGroup.Name}' успешно обновлена");
                        await LoadGroupsAsync();
                    }
                    else
                    {
                        LogWarning("Не удалось получить обновленную группу");
                    }
                }
                else
                {
                    LogWarning("Не удалось обновить группу");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при редактировании группы");
        }
    }

    /// <summary>
    /// Удаление группы с проверкой связанных данных
    /// </summary>
    private async Task DeleteGroupAsync()
    {
        if (SelectedGroup == null) return;

        try
        {
            var result = await _dialogService.ShowConfirmationDialogAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить группу '{SelectedGroup.Name}'?");

            if (result == true)
            {
                var success = await _groupService.DeleteAsync(SelectedGroup.Uid);
                if (success)
                {
                    LogInfo($"Группа '{SelectedGroup.Name}' успешно удалена");
                    await LoadGroupsAsync();
                }
                else
                {
                    LogWarning("Не удалось удалить группу");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при удалении группы");
        }
    }

    private async Task ViewGroupDetailsAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo("Viewing group details: {GroupName}", groupViewModel.Name);
        
        try
        {
            await NavigateToAsync($"group-details/{groupViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to group details");
            ShowError("Не удалось открыть детали группы");
        }
    }

    private async Task LoadGroupStatisticsAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo("Loading statistics for group: {GroupName}", groupViewModel.Name);
        
        try
        {
            var statistics = await _groupService.GetGroupStatisticsAsync(groupViewModel.Uid);
            SelectedGroupStatistics = statistics;
            
            LogDebug("Loaded statistics for group {GroupName}: {StudentsCount} students", 
                groupViewModel.Name, statistics?.StudentsCount ?? 0);
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load group statistics");
            SelectedGroupStatistics = null;
        }
    }

    /// <summary>
    /// Назначение куратора группе
    /// </summary>
    private async Task AssignCuratorAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo("Assigning curator to group: {GroupName}", groupViewModel.Name);

        try
        {
            // Получаем список доступных преподавателей
            var teachers = await _teacherService.GetAllTeachersAsync();
            
            // Показываем диалог выбора преподавателя
            var selectedTeacher = await _dialogService.ShowTeacherSelectionDialogAsync(teachers);
            
            if (selectedTeacher != null)
            {
                // Обновляем куратора группы
                var group = await _groupService.GetByUidAsync(groupViewModel.Uid);
                if (group != null)
                {
                    group.CuratorUid = selectedTeacher.Uid;
                    var updateResult = await _groupService.UpdateAsync(group);
                    
                    if (updateResult)
                    {
                        // Получаем обновленную группу
                        var updatedGroup = await _groupService.GetByUidAsync(group.Uid);
                        if (updatedGroup != null)
                        {
                            // Обновляем UI
                            var index = Groups.IndexOf(groupViewModel);
                            if (index >= 0)
                            {
                                Groups[index] = new GroupViewModel(updatedGroup);
                                if (SelectedGroup?.Uid == updatedGroup.Uid)
                                {
                                    SelectedGroup = Groups[index];
                                }
                            }
                            
                            ShowSuccess($"Куратор группы '{groupViewModel.Name}' назначен: {selectedTeacher.Person?.FirstName} {selectedTeacher.Person?.LastName}");
                            LogInfo("Curator assigned successfully to group: {GroupName}", groupViewModel.Name);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to assign curator");
            ShowError("Не удалось назначить куратора");
        }
    }

    private async Task ManageStudentsAsync(GroupViewModel groupViewModel)
    {
        if (groupViewModel == null) return;

        LogInfo("Managing students for group: {GroupName}", groupViewModel.Name);
        
        try
        {
            await NavigateToAsync($"group-students/{groupViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to group students management");
            ShowError("Не удалось открыть управление студентами группы");
        }
    }

    private async Task SearchGroupsAsync(string searchText)
    {
        LogInfo("Searching groups with text: {SearchText}", searchText);
        CurrentPage = 1; // Сброс на первую страницу при поиске
        await LoadGroupsAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        
        CurrentPage = page;
        await LoadGroupsAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadGroupsAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadGroupsAsync();
        }
    }

    private async Task FirstPageAsync()
    {
        CurrentPage = 1;
        await LoadGroupsAsync();
    }

    private async Task LastPageAsync()
    {
        CurrentPage = TotalPages;
        await LoadGroupsAsync();
    }

    /// <summary>
    /// Валидация данных группы
    /// </summary>
    private async Task<DomainValidationResult> ValidateGroupAsync(Group group)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (string.IsNullOrWhiteSpace(group.Code))
            errors.Add("Код группы обязателен");
        else if (group.Code.Length < 2 || group.Code.Length > 10)
            errors.Add("Код группы должен содержать от 2 до 10 символов");

        if (string.IsNullOrWhiteSpace(group.Name))
            errors.Add("Название группы обязательно");
        else if (group.Name.Length > 100)
            errors.Add("Название группы не должно превышать 100 символов");

        // Проверка уникальности кода
        if (!string.IsNullOrWhiteSpace(group.Code))
        {
            var existingGroup = await _groupService.GetByCodeAsync(group.Code);
            if (existingGroup != null && existingGroup.Uid != group.Uid)
                errors.Add($"Группа с кодом '{group.Code}' уже существует");
        }

        // Бизнес-правила валидации
        if (group.MaxStudents <= 0)
            errors.Add("Максимальное количество студентов должно быть больше 0");

        if (group.MaxStudents > 50)
            warnings.Add("Рекомендуется не превышать 50 студентов в группе");

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Проверка связанных данных перед удалением
    /// </summary>
    private async Task<GroupRelatedDataInfo> CheckRelatedDataAsync(Guid groupUid)
    {
        var info = new GroupRelatedDataInfo();
        
        try
        {
            // Проверяем студентов
            var studentsCount = await _studentService.GetStudentsCountByGroupAsync(groupUid);
            if (studentsCount > 0)
            {
                info.HasStudents = true;
                info.StudentsCount = studentsCount;
                info.RelatedDataDescriptions.Add($"• Студенты: {studentsCount}");
            }
            
            // Проверяем экземпляры курсов - используем альтернативный метод
            var courseInstances = await _groupService.GetGroupCourseInstancesAsync(groupUid);
            var courseInstancesCount = courseInstances?.Count() ?? 0;
            if (courseInstancesCount > 0)
            {
                info.HasCourseInstances = true;
                info.CourseInstancesCount = courseInstancesCount;
                info.RelatedDataDescriptions.Add($"• Экземпляры курсов: {courseInstancesCount}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Error checking related data for group: {GroupUid}", groupUid);
        }
        
        return info;
    }

    /// <summary>
    /// Проверка прав доступа
    /// </summary>
    private async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            return await _permissionService.HasPermissionAsync(permission);
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to check permission: {Permission}", permission);
            return false;
        }
    }

    /// <summary>
    /// Обновление статистики
    /// </summary>
    private async Task UpdateStatisticsAsync()
    {
        try
        {
            // Обновление общей статистики может быть реализовано здесь
            // Например, уведомление других ViewModels об изменениях
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update statistics");
        }
    }

    protected override async Task OnFirstTimeLoadedAsync()
    {
        LogInfo("GroupsViewModel first time loaded");
        await LoadGroupsAsync();
    }

    #endregion
}
