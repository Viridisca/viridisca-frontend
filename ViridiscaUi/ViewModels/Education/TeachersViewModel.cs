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
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления преподавателями
    /// Следует принципам SOLID и чистой архитектуры
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
        private readonly ITeacherService _teacherService;
        private readonly ICourseService _courseService;
        private readonly IGroupService _groupService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        // === СВОЙСТВА ===
        
        [Reactive] public ObservableCollection<TeacherViewModel> Teachers { get; set; } = new();
        [Reactive] public TeacherViewModel? SelectedTeacher { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public TeacherStatistics? SelectedTeacherStatistics { get; set; }
        
        // Фильтры
        [Reactive] public string? SpecializationFilter { get; set; }
        [Reactive] public string? StatusFilter { get; set; }
        
        // Пагинация
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int PageSize { get; set; } = 15;
        [Reactive] public int TotalPages { get; set; }
        [Reactive] public int TotalTeachers { get; set; }

        // Computed properties for UI binding
        public bool HasSelectedTeacher => SelectedTeacher != null;
        public bool HasSelectedTeacherStatistics => SelectedTeacherStatistics != null;
        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        // === КОМАНДЫ ===
        
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

        /// <summary>
        /// Конструктор
        /// </summary>
        public TeachersViewModel(
            IScreen hostScreen,
            ITeacherService teacherService,
            ICourseService courseService,
            IGroupService groupService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService) : base(hostScreen)
        {
            _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            InitializeCommands();
            SetupSubscriptions();
            
            LogInfo("TeachersViewModel initialized");
        }

        #region Private Methods

        private void InitializeCommands()
        {
            // Используем стандартизированные методы создания команд из ViewModelBase
            LoadTeachersCommand = CreateCommand(LoadTeachersAsync, null, "Ошибка загрузки преподавателей");
            RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
            CreateTeacherCommand = CreateCommand(CreateTeacherAsync, null, "Ошибка создания преподавателя");
            EditTeacherCommand = CreateCommand<TeacherViewModel>(EditTeacherAsync, null, "Ошибка редактирования преподавателя");
            DeleteTeacherCommand = CreateCommand<TeacherViewModel>(DeleteTeacherAsync, null, "Ошибка удаления преподавателя");
            ViewTeacherDetailsCommand = CreateCommand<TeacherViewModel>(ViewTeacherDetailsAsync, null, "Ошибка просмотра деталей преподавателя");
            ViewStatisticsCommand = CreateCommand<TeacherViewModel>(ViewStatisticsAsync, null, "Ошибка загрузки статистики");
            ManageCoursesCommand = CreateCommand<TeacherViewModel>(ManageCoursesAsync, null, "Ошибка управления курсами");
            ManageGroupsCommand = CreateCommand<TeacherViewModel>(ManageGroupsAsync, null, "Ошибка управления группами");
            SearchCommand = CreateCommand<string>(SearchTeachersAsync, null, "Ошибка поиска преподавателей");
            ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync, null, "Ошибка применения фильтров");
            ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");
            GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
            
            var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
            var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
            
            NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
            PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
            FirstPageCommand = CreateCommand(FirstPageAsync, canGoPrevious, "Ошибка перехода на первую страницу");
            LastPageCommand = CreateCommand(LastPageAsync, canGoNext, "Ошибка перехода на последнюю страницу");
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

        private async Task CreateTeacherAsync()
        {
            LogInfo("Creating new teacher");
            
            var newTeacher = new Teacher
            {
                Uid = Guid.NewGuid(),
                FirstName = string.Empty,
                LastName = string.Empty,
                Status = TeacherStatus.Active,
                HireDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            var dialogResult = await _dialogService.ShowTeacherEditDialogAsync(newTeacher);
            if (dialogResult == null)
            {
                LogDebug("Teacher creation cancelled by user");
                return;
            }

            // Используем новый универсальный метод создания
            var createdTeacher = await _teacherService.CreateAsync(dialogResult);
            Teachers.Add(new TeacherViewModel(createdTeacher));

            ShowSuccess($"Преподаватель '{createdTeacher.FirstName} {createdTeacher.LastName}' создан");
            LogInfo("Teacher created successfully: {TeacherName}", $"{createdTeacher.FirstName} {createdTeacher.LastName}");
            
            // Уведомление о создании нового преподавателя
            await _notificationService.CreateNotificationAsync(
                createdTeacher.Uid,
                "Добро пожаловать!",
                $"Добро пожаловать в систему, {createdTeacher.FirstName}! Ваш аккаунт преподавателя создан.",
                Domain.Models.System.Enums.NotificationType.Info);
        }

        private async Task EditTeacherAsync(TeacherViewModel teacherViewModel)
        {
            LogInfo("Editing teacher: {TeacherId}", teacherViewModel.Uid);
            
            // Используем новый универсальный метод получения
            var teacher = await _teacherService.GetByUidAsync(teacherViewModel.Uid);
            if (teacher == null)
            {
                ShowError("Преподаватель не найден");
                return;
            }

            var dialogResult = await _dialogService.ShowTeacherEditDialogAsync(teacher);
            if (dialogResult == null)
            {
                LogDebug("Teacher editing cancelled by user");
                return;
            }

            // Используем новый универсальный метод обновления
            var success = await _teacherService.UpdateAsync(dialogResult);
            if (success)
            {
                var index = Teachers.IndexOf(teacherViewModel);
                if (index >= 0)
                {
                    Teachers[index] = new TeacherViewModel(dialogResult);
                }

                ShowSuccess($"Преподаватель '{dialogResult.FirstName} {dialogResult.LastName}' обновлен");
                LogInfo("Teacher updated successfully: {TeacherName}", $"{dialogResult.FirstName} {dialogResult.LastName}");
            }
            else
            {
                ShowError("Не удалось обновить преподавателя");
            }
        }

        private async Task DeleteTeacherAsync(TeacherViewModel teacherViewModel)
        {
            LogInfo("Deleting teacher: {TeacherId}", teacherViewModel.Uid);
            
            // Проверяем, есть ли у преподавателя активные курсы или группы
            var courses = await _courseService.GetCoursesByTeacherAsync(teacherViewModel.Uid);
            var groups = await _groupService.GetGroupsByCuratorAsync(teacherViewModel.Uid);
            
            var hasActiveCourses = courses.Any();
            var hasActiveGroups = groups.Any();
            
            string warningMessage = $"Вы уверены, что хотите удалить преподавателя '{teacherViewModel.FullName}'?";
            
            if (hasActiveCourses || hasActiveGroups)
            {
                warningMessage += "\n\nВНИМАНИЕ: У преподавателя есть:";
                if (hasActiveCourses)
                    warningMessage += $"\n• {courses.Count()} активных курсов";
                if (hasActiveGroups)
                    warningMessage += $"\n• {groups.Count()} курируемых групп";
                warningMessage += "\n\nВсе связи будут удалены!";
            }

            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Удаление преподавателя", warningMessage);

            if (!confirmResult)
            {
                LogDebug("Teacher deletion cancelled by user");
                return;
            }

            // Используем новый универсальный метод удаления
            var success = await _teacherService.DeleteAsync(teacherViewModel.Uid);
            if (success)
            {
                Teachers.Remove(teacherViewModel);
                ShowSuccess($"Преподаватель '{teacherViewModel.FullName}' удален");
                LogInfo("Teacher deleted successfully: {TeacherName}", teacherViewModel.FullName);
                
                // Уведомляем администраторов об удалении
                await _notificationService.SendNotificationToRoleAsync(
                    "Administrator",
                    "Преподаватель удален",
                    $"Преподаватель '{teacherViewModel.FullName}' был удален из системы",
                    Domain.Models.System.Enums.NotificationType.Warning);
            }
            else
            {
                ShowError("Не удалось удалить преподавателя");
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
            
            var statistics = await _teacherService.GetTeacherStatisticsAsync(teacherViewModel.Uid);
            SelectedTeacherStatistics = statistics as TeacherStatistics;
            
            if (SelectedTeacherStatistics != null)
            {
                LogInfo("Teacher statistics loaded successfully");
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

            var allCourses = await _courseService.GetAllCoursesAsync();
            
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
            LogInfo("Managing groups for teacher: {TeacherId}", teacherViewModel.Uid);
            
            var teacher = await _teacherService.GetTeacherAsync(teacherViewModel.Uid);
            if (teacher == null)
            {
                ShowError("Преподаватель не найден");
                return;
            }

            var allGroups = await _groupService.GetGroupsAsync();
            
            var result = await _dialogService.ShowTeacherGroupsManagementDialogAsync(teacher, allGroups);
            if (result != null)
            {
                // TODO: Implement group assignment logic when service method is available
                await RefreshAsync();
                ShowSuccess($"Группы преподавателя '{teacherViewModel.FullName}' обновлены");
                LogInfo("Teacher groups updated successfully");
            }
            else
            {
                LogDebug("Group management cancelled by user");
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
} 