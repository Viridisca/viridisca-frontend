using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CourseStatus = ViridiscaUi.Domain.Models.Education.CourseStatus;
using NotificationType = ViridiscaUi.Domain.Models.System.NotificationType;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.Education
{
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
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        // === СВОЙСТВА ===
        
        [Reactive] public ObservableCollection<CourseViewModel> Courses { get; set; } = new();
        [Reactive] public CourseViewModel? SelectedCourse { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public CourseStatistics? SelectedCourseStatistics { get; set; }
        
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
        public bool HasSelectedCourse => SelectedCourse != null;
        public bool HasSelectedCourseStatistics => SelectedCourseStatistics != null;
        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        // === КОМАНДЫ ===
        
        public ReactiveCommand<Unit, Unit> LoadCoursesCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> CreateCourseCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> EditCourseCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> DeleteCourseCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> ViewCourseDetailsCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> LoadCourseStatisticsCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> PublishCourseCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> ArchiveCourseCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> ManageEnrollmentsCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> BulkEnrollGroupCommand { get; private set; } = null!;
        public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> ManageContentCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> ManageStudentsCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> ViewStatisticsCommand { get; private set; } = null!;
        public ReactiveCommand<CourseViewModel, Unit> CloneCourseCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ImportCoursesCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ExportReportCommand { get; private set; } = null!;

        /// <summary>
        /// Конструктор
        /// </summary>
        public CoursesViewModel(
            IScreen hostScreen,
            ICourseService courseService,
            ITeacherService teacherService,
            IStudentService studentService,
            IGroupService groupService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService) : base(hostScreen)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
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
            LoadCoursesCommand = CreateCommand(LoadCoursesAsync, null, "Ошибка загрузки курсов");
            RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
            CreateCourseCommand = CreateCommand(CreateCourseAsync, null, "Ошибка создания курса");
            EditCourseCommand = CreateCommand<CourseViewModel>(EditCourseAsync, null, "Ошибка редактирования курса");
            DeleteCourseCommand = CreateCommand<CourseViewModel>(DeleteCourseAsync, null, "Ошибка удаления курса");
            ViewCourseDetailsCommand = CreateCommand<CourseViewModel>(ViewCourseDetailsAsync, null, "Ошибка просмотра деталей курса");
            LoadCourseStatisticsCommand = CreateCommand<CourseViewModel>(LoadCourseStatisticsAsync, null, "Ошибка загрузки статистики");
            PublishCourseCommand = CreateCommand<CourseViewModel>(PublishCourseAsync, null, "Ошибка публикации курса");
            ArchiveCourseCommand = CreateCommand<CourseViewModel>(ArchiveCourseAsync, null, "Ошибка архивирования курса");
            ManageEnrollmentsCommand = CreateCommand<CourseViewModel>(ManageEnrollmentsAsync, null, "Ошибка управления записями");
            BulkEnrollGroupCommand = CreateCommand<CourseViewModel>(BulkEnrollGroupAsync, null, "Ошибка массовой записи группы");
            SearchCommand = CreateCommand<string>(SearchCoursesAsync, null, "Ошибка поиска курсов");
            ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync, null, "Ошибка применения фильтров");
            ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");
            GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
            
            var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
            var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
            
            NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
            PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
            ManageContentCommand = CreateCommand<CourseViewModel>(ManageContentAsync, null, "Ошибка управления контентом");
            ManageStudentsCommand = CreateCommand<CourseViewModel>(ManageStudentsAsync, null, "Ошибка управления студентами");
            ViewStatisticsCommand = CreateCommand<CourseViewModel>(ViewStatisticsAsync, null, "Ошибка просмотра статистики");
            CloneCourseCommand = CreateCommand<CourseViewModel>(CloneCourseAsync, null, "Ошибка клонирования курса");
            ImportCoursesCommand = CreateCommand(ImportCoursesAsync, null, "Ошибка импорта курсов");
            ExportReportCommand = CreateCommand(ExportReportAsync, null, "Ошибка экспорта отчета");
            FirstPageCommand = CreateCommand(FirstPageAsync, null, "Ошибка перехода на первую страницу");
            LastPageCommand = CreateCommand(LastPageAsync, null, "Ошибка перехода на последнюю страницу");
        }

        /// <summary>
        /// Настраивает подписки на изменения свойств
        /// </summary>
        private void SetupSubscriptions()
        {
            // Автопоиск при изменении текста поиска - исправляем вложенную подписку
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
                        await SearchCoursesAsync(searchText);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Ошибка поиска курсов");
                        ShowError("Ошибка поиска курсов");
                    }
                })
                .DisposeWith(Disposables);

            // Загрузка статистики при выборе курса - добавляем обработку ошибок
            this.WhenAnyValue(x => x.SelectedCourse)
                .Where(course => course != null && !IsLoading)
                .Select(course => course!)
                .Subscribe(async course =>
                {
                    try
                    {
                        await LoadCourseStatisticsAsync(course);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Ошибка загрузки статистики курса");
                        ShowError("Ошибка загрузки статистики курса");
                    }
                })
                .DisposeWith(Disposables);

            // Применение фильтров при изменении - добавляем обработку ошибок
            this.WhenAnyValue(x => x.CategoryFilter, x => x.StatusFilter, x => x.DifficultyFilter, x => x.SelectedTeacherFilter)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(_ => !IsLoading) // Предотвращаем выполнение во время загрузки
                .Subscribe(async _ =>
                {
                    try
                    {
                        await ApplyFiltersAsync();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Ошибка применения фильтров");
                        ShowError("Ошибка применения фильтров");
                    }
                })
                .DisposeWith(Disposables);

            // Уведомления об изменении computed properties - добавляем обработку ошибок
            this.WhenAnyValue(x => x.SelectedCourse)
                .Subscribe(_ => 
                {
                    try
                    {
                        this.RaisePropertyChanged(nameof(HasSelectedCourse));
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Ошибка обновления HasSelectedCourse");
                    }
                })
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.SelectedCourseStatistics)
                .Subscribe(_ => 
                {
                    try
                    {
                        this.RaisePropertyChanged(nameof(HasSelectedCourseStatistics));
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Ошибка обновления HasSelectedCourseStatistics");
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

        private async Task LoadCoursesAsync()
        {
            LogInfo("Loading courses with filters: SearchText={SearchText}, CategoryFilter={CategoryFilter}, StatusFilter={StatusFilter}", SearchText ?? string.Empty, CategoryFilter ?? string.Empty, StatusFilter ?? string.Empty);
            
            IsLoading = true;
            ShowInfo("Загрузка курсов...");

            // Используем новый универсальный метод пагинации
            var (courses, totalCount) = await _courseService.GetPagedAsync(
                CurrentPage, PageSize, SearchText);
            
            Courses.Clear();
            foreach (var course in courses)
            {
                Courses.Add(new CourseViewModel(course));
            }

            TotalCourses = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ActiveCourses = courses.Count(c => c.Status == CourseStatus.Active);

            ShowSuccess($"Загружено {Courses.Count} курсов");
            LogInfo("Loaded {CourseCount} courses, total: {TotalCount}", Courses.Count, totalCount);
            
            IsLoading = false;
        }

        private async Task LoadTeachersAsync()
        {
            LogInfo("Loading teachers for filter");
            
            // Используем новый универсальный метод получения всех записей
            var teachers = await _teacherService.GetAllAsync();
            Teachers.Clear();
            foreach (var teacher in teachers)
            {
                Teachers.Add(new TeacherViewModel(teacher));
            }
            
            LogInfo("Loaded {TeacherCount} teachers for filter", teachers.Count());
        }

        private async Task RefreshAsync()
        {
            LogInfo("Refreshing courses data");
            IsRefreshing = true;
            
            await LoadCoursesAsync();
            ShowSuccess("Данные обновлены");
            
            IsRefreshing = false;
        }

        private async Task CreateCourseAsync()
        {
            LogInfo("Creating new course");
            
            var newCourse = new Course
            {
                Uid = Guid.NewGuid(),
                Name = string.Empty,
                Description = string.Empty,
                Status = CourseStatus.Draft,
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
            var createdCourse = await _courseService.CreateAsync(dialogResult);
            Courses.Add(new CourseViewModel(createdCourse));

            ShowSuccess($"Курс '{createdCourse.Name}' создан");
            LogInfo("Course created successfully: {CourseName}", createdCourse.Name);
            
            // Уведомление преподавателю
            if (createdCourse.TeacherUid.HasValue)
            {
                await _notificationService.CreateNotificationAsync(
                    createdCourse.TeacherUid.Value,
                    "Назначение на курс",
                    $"Вы назначены преподавателем курса '{createdCourse.Name}'",
                    Domain.Models.System.NotificationType.Info);
            }
        }

        private async Task EditCourseAsync(CourseViewModel courseViewModel)
        {
            LogInfo("Editing course: {CourseId}", courseViewModel.Uid);
            
            // Получаем актуальные данные курса
            var course = await _courseService.GetByUidAsync(courseViewModel.Uid);
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
            var success = await _courseService.UpdateAsync(dialogResult);
            if (success)
            {
                var index = Courses.IndexOf(courseViewModel);
                if (index >= 0)
                {
                    Courses[index] = new CourseViewModel(dialogResult);
                }

                ShowSuccess($"Курс '{dialogResult.Name}' обновлен");
                LogInfo("Course updated successfully: {CourseName}", dialogResult.Name);
            }
            else
            {
                ShowError("Не удалось обновить курс");
            }
        }

        private async Task DeleteCourseAsync(CourseViewModel courseViewModel)
        {
            LogInfo("Deleting course: {CourseId}", courseViewModel.Uid);
            
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Удаление курса",
                $"Вы уверены, что хотите удалить курс '{courseViewModel.Name}'?\nВсе связанные данные будут утеряны.");

            if (!confirmResult)
            {
                LogDebug("Course deletion cancelled by user");
                return;
            }

            // Используем новый универсальный метод удаления
            var success = await _courseService.DeleteAsync(courseViewModel.Uid);
            if (success)
            {
                Courses.Remove(courseViewModel);
                ShowSuccess($"Курс '{courseViewModel.Name}' удален");
                LogInfo("Course deleted successfully: {CourseName}", courseViewModel.Name);
            }
            else
            {
                ShowError("Не удалось удалить курс");
            }
        }

        private async Task ViewCourseDetailsAsync(CourseViewModel courseViewModel)
        {
            LogInfo("Viewing course details: {CourseId}", courseViewModel.Uid);
            
            SelectedCourse = courseViewModel;
            await LoadCourseStatisticsAsync(courseViewModel);
            
            ShowInfo($"Просмотр курса '{courseViewModel.Name}'");
        }

        private async Task LoadCourseStatisticsAsync(CourseViewModel courseViewModel)
        {
            try
            {
                SelectedCourseStatistics = await _courseService.GetCourseStatisticsAsync(courseViewModel.Uid);
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить статистику курса: {ex.Message}", "Курсы");
            }
        }

        private async Task PublishCourseAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var success = await _courseService.PublishCourseAsync(courseViewModel.Uid);
                if (success)
                {
                    courseViewModel.Status = CourseStatus.Published;
                    _statusService.ShowSuccess($"Курс '{courseViewModel.Name}' опубликован", "Курсы");
                    
                    // Уведомление всем студентам о новом курсе
                    await _notificationService.SendNotificationToRoleAsync(
                        "Student",
                        "Новый курс доступен",
                        $"Опубликован новый курс '{courseViewModel.Name}'. Вы можете зарегистрироваться на него.",
                        Domain.Models.System.NotificationType.Info);
                }
                else
                {
                    _statusService.ShowError("Не удалось опубликовать курс", "Курсы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка публикации курса: {ex.Message}", "Курсы");
            }
        }

        private async Task ArchiveCourseAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var confirmResult = await _dialogService.ShowConfirmationAsync(
                    "Архивирование курса",
                    $"Вы уверены, что хотите архивировать курс '{courseViewModel.Name}'?");

                if (!confirmResult) return;

                var success = await _courseService.ArchiveCourseAsync(courseViewModel.Uid);
                if (success)
                {
                    courseViewModel.Status = CourseStatus.Archived;
                    _statusService.ShowSuccess($"Курс '{courseViewModel.Name}' архивирован", "Курсы");
                }
                else
                {
                    _statusService.ShowError("Не удалось архивировать курс", "Курсы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка архивирования курса: {ex.Message}", "Курсы");
            }
        }

        private async Task ManageEnrollmentsAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var allStudents = await _studentService.GetStudentsAsync();
                var enrolledStudents = await _courseService.GetCoursesByStudentAsync(courseViewModel.Uid); // TODO: исправить логику
                
                var result = await _dialogService.ShowCourseEnrollmentDialogAsync(courseViewModel.ToCourse(), allStudents);
                if (result != null)
                {
                    await RefreshAsync();
                    _statusService.ShowSuccess($"Записи на курс '{courseViewModel.Name}' обновлены", "Курсы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка управления записями: {ex.Message}", "Курсы");
            }
        }

        private async Task BulkEnrollGroupAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var groups = await _groupService.GetGroupsAsync();
                var selectedGroup = await _dialogService.ShowGroupSelectionDialogAsync(groups);
                
                if (selectedGroup == null) return;

                var result = await _courseService.BulkEnrollGroupAsync(courseViewModel.Uid, selectedGroup.Uid);
                
                if (result.SuccessfulEnrollments > 0)
                {
                    _statusService.ShowSuccess(
                        $"Записано студентов: {result.SuccessfulEnrollments}, " +
                        $"ошибок: {result.FailedEnrollments}", "Курсы");
                    
                    // Уведомление всем записанным студентам
                    foreach (var studentUid in result.EnrolledStudentUids)
                    {
                        await _notificationService.CreateNotificationAsync(
                            studentUid,
                            "Запись на курс",
                            $"Вы записаны на курс '{courseViewModel.Name}'",
                            Domain.Models.System.NotificationType.Info);
                    }
                }
                else
                {
                    _statusService.ShowWarning("Не удалось записать ни одного студента", "Курсы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка массовой записи: {ex.Message}", "Курсы");
            }
        }

        private async Task SearchCoursesAsync(string searchText)
        {
            SearchText = searchText;
            CurrentPage = 1;
            await LoadCoursesAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            CurrentPage = 1;
            await LoadCoursesAsync();
        }

        private async Task ClearFiltersAsync()
        {
            CategoryFilter = null;
            StatusFilter = null;
            DifficultyFilter = null;
            SelectedTeacherFilter = null;
            SearchText = string.Empty;
            CurrentPage = 1;
            await LoadCoursesAsync();
        }

        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                await LoadCoursesAsync();
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

        private async Task ManageContentAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var course = await _courseService.GetCourseAsync(courseViewModel.Uid);
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

        private async Task ManageStudentsAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var course = await _courseService.GetCourseAsync(courseViewModel.Uid);
                if (course == null)
                {
                    _statusService.ShowError("Курс не найден", "Курсы");
                    return;
                }

                var allStudents = await _studentService.GetAllStudentsAsync();
                var result = await _dialogService.ShowCourseStudentsManagementDialogAsync(course, allStudents);
                
                if (result != null)
                {
                    await RefreshAsync();
                    _statusService.ShowSuccess("Список студентов курса обновлен", "Курсы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка управления студентами: {ex.Message}", "Курсы");
            }
        }

        private async Task ViewStatisticsAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var statistics = await _courseService.GetCourseStatisticsAsync(courseViewModel.Uid);
                await _dialogService.ShowCourseStatisticsDialogAsync(courseViewModel.Name, statistics);
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки статистики: {ex.Message}", "Курсы");
            }
        }

        private async Task CloneCourseAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var confirmation = await _dialogService.ShowConfirmationDialogAsync(
                    "Клонирование курса",
                    $"Создать копию курса '{courseViewModel.Name}'?\n\nБудет создан новый курс со всем содержимым, но без записанных студентов.",
                    "Клонировать",
                    "Отмена");

                if (confirmation)
                {
                    var newCourseName = await _dialogService.ShowTextInputDialogAsync(
                        "Название нового курса",
                        "Введите название для копии курса:",
                        $"{courseViewModel.Name} (копия)");

                    if (!string.IsNullOrEmpty(newCourseName))
                    {
                        var clonedCourse = await _courseService.CloneCourseAsync(courseViewModel.Uid, newCourseName);
                        if (clonedCourse != null)
                        {
                            await RefreshAsync();
                            _statusService.ShowSuccess($"Курс '{newCourseName}' создан как копия", "Курсы");
                            
                            await _notificationService.SendNotificationAsync(
                                "Курс клонирован",
                                $"Создана копия курса: {newCourseName}",
                                Domain.Models.System.NotificationType.Info);
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
                        Domain.Models.System.NotificationType.Info);
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
                var courses = await _courseService.GetAllCoursesAsync(
                    CategoryFilter,
                    StatusFilter,
                    DifficultyFilter);

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
            await LoadCoursesAsync();
        }

        #endregion
    }
}
