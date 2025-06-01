using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure.Navigation;
using CourseStatus = ViridiscaUi.Domain.Models.Education.Enums.CourseStatus;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления курсами
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("courses", 
    DisplayName = "Курсы", 
    IconKey = "BookOpenPageVariant", 
    Order = 4,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление курсами и программами")]
public class CoursesViewModel : RoutableViewModelBase
{ 
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly IGroupService _groupService;
    private readonly IDialogService _dialogService;
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;

    // === СВОЙСТВА ===
    
    [Reactive] public ObservableCollection<CourseInstanceViewModel> CourseInstances { get; set; } = new();
    [Reactive] public CourseInstanceViewModel? SelectedCourseInstance { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public CourseInstanceStatistics? SelectedCourseInstanceStatistics { get; set; }
    
    // Фильтры
    [Reactive] public string? CategoryFilter { get; set; }
    [Reactive] public string? StatusFilter { get; set; }
    [Reactive] public string? DifficultyFilter { get; set; }
    [Reactive] public ObservableCollection<TeacherViewModel> Teachers { get; set; } = new();
    [Reactive] public TeacherViewModel? SelectedTeacherFilter { get; set; }
    
    // Добавляем коллекции для фильтров
    [Reactive] public ObservableCollection<string> Categories { get; set; } = new();
    [Reactive] public ObservableCollection<string> Difficulties { get; set; } = new();
    [Reactive] public ObservableCollection<string> Statuses { get; set; } = new();
    
    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 15;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalCourses { get; set; }
    [Reactive] public int ActiveCourses { get; set; }

    // Computed properties for UI binding
    public bool HasSelectedCourseInstance => SelectedCourseInstance != null;
    public bool HasSelectedCourseInstanceStatistics => SelectedCourseInstanceStatistics != null;
    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadCourseInstancesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> EditCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> DeleteCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ViewCourseInstanceDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ViewStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ManageEnrollmentsCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> ManageContentCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    public ReactiveCommand<CourseInstanceViewModel, Unit> CloneCourseInstanceCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ImportCoursesCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportReportCommand { get; private set; } = null!;

    /// <summary>
    /// Конструктор
    /// </summary>
    public CoursesViewModel(
        IScreen hostScreen,
        ICourseInstanceService courseInstanceService,
        ITeacherService teacherService,
        IStudentService studentService,
        IGroupService groupService,
        IDialogService dialogService,
        IStatusService statusService,
        INotificationService notificationService) : base(hostScreen)
    {
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        InitializeCommands();
        SetupSubscriptions();
        
        LogInfo("CoursesViewModel initialized");
    }

    #region Private Methods

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        LoadCourseInstancesCommand = CreateCommand(LoadCourseInstancesAsync, null, "Ошибка загрузки курсов");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        CreateCourseInstanceCommand = CreateCommand(CreateCourseInstanceAsync, null, "Ошибка создания курса");
        EditCourseInstanceCommand = CreateCommand<CourseInstanceViewModel>(EditCourseInstanceAsync, null, "Ошибка редактирования курса");
        DeleteCourseInstanceCommand = CreateCommand<CourseInstanceViewModel>(DeleteCourseInstanceAsync, null, "Ошибка удаления курса");
        ViewCourseInstanceDetailsCommand = CreateCommand<CourseInstanceViewModel>(ViewCourseInstanceDetailsAsync, null, "Ошибка просмотра деталей курса");
        ViewStatisticsCommand = CreateCommand<CourseInstanceViewModel>(ViewStatisticsAsync, null, "Ошибка просмотра статистики");
        ManageEnrollmentsCommand = CreateCommand<CourseInstanceViewModel>(ManageEnrollmentsAsync, null, "Ошибка управления записями");
        ManageContentCommand = CreateCommand<CourseInstanceViewModel>(ManageContentAsync, null, "Ошибка управления контентом");
        SearchCommand = CreateCommand<string>(SearchCoursesAsync, null, "Ошибка поиска курсов");
        ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync, null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");
        GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        FirstPageCommand = CreateCommand(FirstPageAsync, null, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(LastPageAsync, null, "Ошибка перехода на последнюю страницу");
        CloneCourseInstanceCommand = CreateCommand<CourseInstanceViewModel>(CloneCourseInstanceAsync, null, "Ошибка клонирования курса");
        ImportCoursesCommand = CreateCommand(ImportCoursesAsync, null, "Ошибка импорта курсов");
        ExportReportCommand = CreateCommand(ExportReportAsync, null, "Ошибка экспорта отчета");
    }

    /// <summary>
    /// Настраивает подписки на изменения свойств
    /// </summary>
    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска - используем безопасный подход без вложенных команд
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(searchText => searchText?.Trim() ?? string.Empty)
            .DistinctUntilChanged()
            .Where(_ => !IsLoading) // Предотвращаем выполнение во время загрузки
            .Subscribe(async searchText => 
            {
                try
                {
                    if (!string.IsNullOrEmpty(searchText) || CurrentPage > 1)
                    {
                        // Используем прямой вызов метода вместо команды
                        await SearchCoursesAsync(searchText);
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка автопоиска");
                }
            })
            .DisposeWith(Disposables);

        // Загрузка статистики при выборе курса - используем безопасный подход
        this.WhenAnyValue(x => x.SelectedCourseInstance)
            .Where(courseInstance => courseInstance != null && !IsLoading)
            .Select(courseInstance => courseInstance!)
            .Subscribe(async courseInstance => 
            {
                try
                {
                    // Используем прямой вызов метода вместо команды
                    await ViewStatisticsCommand.Execute(courseInstance);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка загрузки статистики курса");
                }
            })
            .DisposeWith(Disposables);

        // Применение фильтров при изменении - используем безопасный подход
        this.WhenAnyValue(x => x.CategoryFilter, x => x.StatusFilter, x => x.DifficultyFilter, x => x.SelectedTeacherFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(_ => !IsLoading) // Предотвращаем выполнение во время загрузки
            .Subscribe(async _ => 
            {
                try
                {
                    // Используем прямой вызов метода вместо команды
                    await ApplyFiltersAsync();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка применения фильтров");
                }
            })
            .DisposeWith(Disposables);

        // Уведомления об изменении computed properties - добавляем обработку ошибок
        this.WhenAnyValue(x => x.SelectedCourseInstance)
            .Subscribe(_ => 
            {
                try
                {
                    this.RaisePropertyChanged(nameof(HasSelectedCourseInstance));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка обновления HasSelectedCourseInstance");
                }
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.SelectedCourseInstanceStatistics)
            .Subscribe(_ => 
            {
                try
                {
                    this.RaisePropertyChanged(nameof(HasSelectedCourseInstanceStatistics));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка обновления HasSelectedCourseInstanceStatistics");
                }
            })
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages)
            .Subscribe(_ => 
            {
                try
                {
                    this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                    this.RaisePropertyChanged(nameof(CanGoToNextPage));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка обновления пагинации");
                }
            })
            .DisposeWith(Disposables);
    }

    private async Task LoadCourseInstancesAsync()
    {
        // Предотвращаем множественные одновременные вызовы
        if (IsLoading) return;
        
        LogInfo("Loading courses with filters: SearchText={SearchText}, CategoryFilter={CategoryFilter}, StatusFilter={StatusFilter}", SearchText ?? string.Empty, CategoryFilter ?? string.Empty, StatusFilter ?? string.Empty);
        
        IsLoading = true;
        ShowInfo("Загрузка курсов...");

        try
        {
            // Используем правильный метод из сервиса
            var (courses, totalCount) = await _courseInstanceService.GetCourseInstancesPagedAsync(
                CurrentPage,
                PageSize,
                SearchText);
            
            CourseInstances.Clear();
            foreach (var course in courses)
            {
                CourseInstances.Add(new CourseInstanceViewModel(course));
            }

            TotalCourses = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ActiveCourses = courses.Count(c => c.Status == CourseStatus.Active.ToString());

            ShowSuccess($"Загружено {CourseInstances.Count} курсов");
            LogInfo("Loaded {CourseCount} courses, total: {TotalCount}", CourseInstances.Count, totalCount);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки курсов");
            ShowError("Не удалось загрузить курсы");
            CourseInstances.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTeachersAsync()
    {
        LogInfo("Loading teachers for filter");
        
        try
        {
            // Используем правильный метод из сервиса
            var teachers = await _teacherService.GetAllTeachersAsync();
            Teachers.Clear();
            foreach (var teacher in teachers)
            {
                Teachers.Add(new TeacherViewModel(teacher));
            }
            
            LogInfo("Loaded {TeacherCount} teachers for filter", teachers.Count());
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки преподавателей");
            ShowError("Не удалось загрузить список преподавателей");
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing courses data");
        IsRefreshing = true;
        
        await LoadCourseInstancesAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    private async Task CreateCourseInstanceAsync()
    {
        LogInfo("Creating new course");
        
        var newCourse = new CourseInstance
        {
            Uid = Guid.NewGuid(),
            Name = string.Empty,
            Description = string.Empty,
            Status = CourseStatus.Draft.ToString(),
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(3),
            Credits = 3,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        var dialogResult = await _dialogService.ShowCourseEditDialogAsync(newCourse);
        if (dialogResult == null)
        {
            LogDebug("Course creation cancelled by user");
            return;
        }

        // Используем новый универсальный метод создания
        var createdCourse = await _courseInstanceService.CreateAsync(dialogResult);
        CourseInstances.Add(new CourseInstanceViewModel(createdCourse));

        ShowSuccess($"Курс '{createdCourse.Name}' создан");
        LogInfo("Course created successfully: {CourseName}", createdCourse.Name);
        
        // Уведомление преподавателю
        if (createdCourse.TeacherUid != Guid.Empty)
        {
            var teacher = await _teacherService.GetTeacherAsync(createdCourse.TeacherUid);
            if (teacher != null)
            {
                await _notificationService.CreateNotificationAsync(
                    createdCourse.TeacherUid,
                    "Назначение на курс",
                    $"Вы назначены преподавателем курса '{createdCourse.Name}'",
                    Domain.Models.System.Enums.NotificationType.Info);
            }
        }
    }

    private async Task EditCourseInstanceAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        LogInfo("Editing course: {CourseId}", courseInstanceViewModel.Uid);
        
        // Получаем актуальные данные курса
        var course = await _courseInstanceService.GetByUidAsync(courseInstanceViewModel.Uid);
        if (course == null)
        {
            ShowError("Курс не найден");
            return;
        }

        var dialogResult = await _dialogService.ShowCourseEditDialogAsync(course);
        if (dialogResult == null)
        {
            LogDebug("Course editing cancelled by user");
            return;
        }

        // Используем новый универсальный метод обновления
        var success = await _courseInstanceService.UpdateAsync(dialogResult);
        if (success)
        {
            var index = CourseInstances.IndexOf(courseInstanceViewModel);
            if (index >= 0)
            {
                CourseInstances[index] = new CourseInstanceViewModel(dialogResult);
            }

            ShowSuccess($"Курс '{dialogResult.Name}' обновлен");
            LogInfo("Course updated successfully: {CourseName}", dialogResult.Name);
        }
        else
        {
            ShowError("Не удалось обновить курс");
        }
    }

    private async Task DeleteCourseInstanceAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        LogInfo("Deleting course: {CourseId}", courseInstanceViewModel.Uid);
        
        var confirmResult = await _dialogService.ShowConfirmationAsync(
            "Удаление курса",
            $"Вы уверены, что хотите удалить курс '{courseInstanceViewModel.Name}'?\nВсе связанные данные будут утеряны.");

        if (!confirmResult)
        {
            LogDebug("Course deletion cancelled by user");
            return;
        }

        // Используем новый универсальный метод удаления
        var success = await _courseInstanceService.DeleteAsync(courseInstanceViewModel.Uid);
        if (success)
        {
            CourseInstances.Remove(courseInstanceViewModel);
            ShowSuccess($"Курс '{courseInstanceViewModel.Name}' удален");
            LogInfo("Course deleted successfully: {CourseName}", courseInstanceViewModel.Name);
        }
        else
        {
            ShowError("Не удалось удалить курс");
        }
    }

    private async Task ViewCourseInstanceDetailsAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        LogInfo("Viewing course details: {CourseId}", courseInstanceViewModel.Uid);
        
        SelectedCourseInstance = courseInstanceViewModel;
        await ViewStatisticsCommand.Execute(courseInstanceViewModel);
        
        ShowInfo($"Просмотр курса '{courseInstanceViewModel.Name}'");
    }

    private async Task ViewStatisticsAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            var statistics = await _courseInstanceService.GetCourseStatisticsAsync(courseInstanceViewModel.Uid);
            if (statistics is CourseInstanceStatistics courseStats)
            {
                await _dialogService.ShowCourseStatisticsDialogAsync(courseStats);
            }
            else
            {
                _statusService.ShowError("Не удалось получить статистику курса", "Курсы");
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка загрузки статистики: {ex.Message}", "Курсы");
        }
    }

    private async Task ManageEnrollmentsAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            var allStudents = await _studentService.GetStudentsAsync();
            var enrolledStudents = await _courseInstanceService.GetCoursesByStudentAsync(courseInstanceViewModel.Uid); // TODO: исправить логику
            
            var result = await _dialogService.ShowCourseEnrollmentDialogAsync(courseInstanceViewModel.ToCourseInstance(), allStudents);
            if (result != null)
            {
                await RefreshAsync();
                _statusService.ShowSuccess($"Записи на курс '{courseInstanceViewModel.Name}' обновлены", "Курсы");
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка управления записями: {ex.Message}", "Курсы");
        }
    }

    private async Task ManageContentAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            var course = await _courseInstanceService.GetCourseAsync(courseInstanceViewModel.Uid);
            if (course == null)
            {
                _statusService.ShowError("Курс не найден", "Курсы");
                return;
            }

            var result = await _dialogService.ShowCourseContentManagementDialogAsync(course);
            if (result != null)
            {
                await RefreshAsync();
                _statusService.ShowSuccess("Контент курса обновлен", "Курсы");
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка управления контентом: {ex.Message}", "Курсы");
        }
    }

    private async Task SearchCoursesAsync(string searchText)
    {
        SearchText = searchText;
        CurrentPage = 1;
        await LoadCourseInstancesAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        try
        {
            // Предотвращаем рекурсивные вызовы
            if (IsLoading) return;
            
            LogInfo("Applying filters: Category={CategoryFilter}, Status={StatusFilter}, Difficulty={DifficultyFilter}, Teacher={TeacherName}", 
                CategoryFilter ?? "null", StatusFilter ?? "null", DifficultyFilter ?? "null", 
                SelectedTeacherFilter?.FullName ?? "null");
            
            CurrentPage = 1;
            await LoadCourseInstancesAsync();
            
            LogInfo("Filters applied successfully. Loaded {CourseCount} courses", CourseInstances.Count);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка применения фильтров");
            ShowError("Ошибка применения фильтров");
        }
    }

    private async Task ClearFiltersAsync()
    {
        CategoryFilter = null;
        StatusFilter = null;
        DifficultyFilter = null;
        SelectedTeacherFilter = null;
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadCourseInstancesAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            await LoadCourseInstancesAsync();
        }
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            await GoToPageAsync(CurrentPage + 1);
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            await GoToPageAsync(CurrentPage - 1);
        }
    }

    private async Task FirstPageAsync()
    {
        if (CurrentPage > 1)
        {
            await GoToPageAsync(1);
        }
    }

    private async Task LastPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            await GoToPageAsync(TotalPages);
        }
    }

    private async Task CloneCourseInstanceAsync(CourseInstanceViewModel courseInstanceViewModel)
    {
        try
        {
            var confirmation = await _dialogService.ShowConfirmationDialogAsync(
                "Клонирование курса",
                $"Создать копию курса '{courseInstanceViewModel.Name}'?\n\nБудет создан новый курс со всем содержимым, но без записанных студентов.",
                "Клонировать",
                "Отмена");

            if (confirmation)
            {
                var newCourseName = await _dialogService.ShowTextInputDialogAsync(
                    "Название нового курса",
                    "Введите название для копии курса:",
                    $"{courseInstanceViewModel.Name} (копия)");

                if (!string.IsNullOrEmpty(newCourseName))
                {
                    var clonedCourse = await _courseInstanceService.CloneCourseAsync(courseInstanceViewModel.Uid);
                    if (clonedCourse != null)
                    {
                        // Обновляем название клонированного курса
                        clonedCourse.Name = newCourseName;
                        await _courseInstanceService.UpdateCourseInstanceAsync(clonedCourse);
                        
                        await RefreshAsync();
                        _statusService.ShowSuccess($"Курс '{newCourseName}' создан как копия", "Курсы");
                        
                        await _notificationService.SendNotificationAsync(
                            "Курс клонирован",
                            $"Создана копия курса: {newCourseName}",
                            Domain.Models.System.Enums.NotificationType.Info);
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось клонировать курс", "Курсы");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка клонирования курса: {ex.Message}", "Курсы");
        }
    }

    private async Task ImportCoursesAsync()
    {
        try
        {
            var filePath = await _dialogService.ShowFileOpenDialogAsync(
                "Импорт курсов",
                new[] { "*.xlsx", "*.csv", "*.json" });

            if (!string.IsNullOrEmpty(filePath))
            {
                // Заглушка - в реальной реализации здесь будет импорт курсов
                ShowInfo($"Импорт курсов из файла: {filePath}");
                LogInfo("Import courses requested from file: {FilePath}", filePath);
                
                await _notificationService.SendNotificationAsync(
                    "Импорт курсов",
                    "Функция импорта курсов будет реализована позже",
                    Domain.Models.System.Enums.NotificationType.Info);
            }
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка импорта курсов: {ex.Message}", "Импорт");
        }
    }

    private async Task ExportReportAsync()
    {
        try
        {
            var courses = await _courseInstanceService.GetAllCoursesAsync();

            // Заглушка - в реальной реализации здесь будет экспорт в Excel
            _statusService.ShowInfo($"Экспорт отчета: {courses.Count()} курсов готовы к экспорту", "Экспорт");
            LogInfo("Export report requested for {CourseCount} courses", courses.Count());
        }
        catch (Exception ex)
        {
            _statusService.ShowError($"Ошибка экспорта отчета: {ex.Message}", "Экспорт");
        }
    }

    private void InitializeFilters()
    {
        LogInfo("Initializing course filters");
        
        // Инициализируем категории
        Categories.Clear();
        Categories.Add("Все категории");
        Categories.Add("Математика");
        Categories.Add("Естественные науки");
        Categories.Add("Гуманитарные науки");
        Categories.Add("Информатика");
        Categories.Add("Языки");
        Categories.Add("Искусство");
        Categories.Add("Спорт");
        Categories.Add("Экономика");
        Categories.Add("Инженерия");
        
        // Инициализируем уровни сложности
        Difficulties.Clear();
        Difficulties.Add("Все");
        Difficulties.Add("Начальный");
        Difficulties.Add("Средний");
        Difficulties.Add("Продвинутый");
        Difficulties.Add("Экспертный");
        
        // Инициализируем статусы
        Statuses.Clear();
        Statuses.Add("Все");
        Statuses.Add("Активные");
        Statuses.Add("Опубликованные");
        Statuses.Add("Черновики");
        Statuses.Add("Архивированные");
        Statuses.Add("Приостановленные");
        
        LogInfo("Course filters initialized");
    }

    #endregion

    #region Lifecycle Methods

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        LogInfo("CoursesViewModel loaded for the first time");
        
        // Initialize filters first
        InitializeFilters();
        
        // Load teachers and courses when view is loaded for the first time
        await ExecuteWithErrorHandlingAsync(LoadTeachersAsync, "Ошибка загрузки списка преподавателей");
        await LoadCourseInstancesAsync();
    }

    #endregion
}
