using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления преподавателями
    /// </summary>
    public class TeachersViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment => "teachers";
        public IScreen HostScreen { get; }

        private readonly ITeacherService _teacherService;
        private readonly ICourseService _courseService;
        private readonly IGroupService _groupService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;
        private readonly IServiceProvider _serviceProvider;

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
        
        public ReactiveCommand<Unit, Unit> LoadTeachersCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateTeacherCommand { get; }
        public ReactiveCommand<TeacherViewModel, Unit> EditTeacherCommand { get; }
        public ReactiveCommand<TeacherViewModel, Unit> DeleteTeacherCommand { get; }
        public ReactiveCommand<TeacherViewModel, Unit> ViewTeacherDetailsCommand { get; }
        public ReactiveCommand<TeacherViewModel, Unit> ViewStatisticsCommand { get; }
        public ReactiveCommand<TeacherViewModel, Unit> ManageCoursesCommand { get; }
        public ReactiveCommand<TeacherViewModel, Unit> ManageGroupsCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
        public ReactiveCommand<int, Unit> GoToPageCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public TeachersViewModel(
            ITeacherService teacherService,
            ICourseService courseService,
            IGroupService groupService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService,
            IServiceProvider serviceProvider,
            IScreen hostScreen)
        {
            _teacherService = teacherService;
            _courseService = courseService;
            _groupService = groupService;
            _dialogService = dialogService;
            _statusService = statusService;
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;
            HostScreen = hostScreen;

            // === ИНИЦИАЛИЗАЦИЯ КОМАНД ===

            LoadTeachersCommand = ReactiveCommand.CreateFromTask(LoadTeachersAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
            CreateTeacherCommand = ReactiveCommand.CreateFromTask(CreateTeacherAsync);
            EditTeacherCommand = ReactiveCommand.CreateFromTask<TeacherViewModel>(EditTeacherAsync);
            DeleteTeacherCommand = ReactiveCommand.CreateFromTask<TeacherViewModel>(DeleteTeacherAsync);
            ViewTeacherDetailsCommand = ReactiveCommand.CreateFromTask<TeacherViewModel>(ViewTeacherDetailsAsync);
            ViewStatisticsCommand = ReactiveCommand.CreateFromTask<TeacherViewModel>(ViewStatisticsAsync);
            ManageCoursesCommand = ReactiveCommand.CreateFromTask<TeacherViewModel>(ManageCoursesAsync);
            ManageGroupsCommand = ReactiveCommand.CreateFromTask<TeacherViewModel>(ManageGroupsAsync);
            SearchCommand = ReactiveCommand.CreateFromTask<string>(SearchTeachersAsync);
            ApplyFiltersCommand = ReactiveCommand.CreateFromTask(ApplyFiltersAsync);
            ClearFiltersCommand = ReactiveCommand.CreateFromTask(ClearFiltersAsync);
            GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
            NextPageCommand = ReactiveCommand.CreateFromTask(NextPageAsync, this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total));
            PreviousPageCommand = ReactiveCommand.CreateFromTask(PreviousPageAsync, this.WhenAnyValue(x => x.CurrentPage, current => current > 1));

            // === ПОДПИСКИ ===

            // Автоматический поиск при изменении текста
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand);

            // Обновление computed properties
            this.WhenAnyValue(x => x.SelectedTeacher)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedTeacher)));

            this.WhenAnyValue(x => x.SelectedTeacherStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedTeacherStatistics)));

            this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages)
                .Subscribe(_ => 
                {
                    this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                    this.RaisePropertyChanged(nameof(CanGoToNextPage));
                });

            // Загрузка статистики при выборе преподавателя
            this.WhenAnyValue(x => x.SelectedTeacher)
                .Where(teacher => teacher != null)
                .SelectMany(teacher => ViewStatisticsCommand.Execute(teacher!))
                .Subscribe();

            // Инициализация
            LoadTeachersCommand.Execute().Subscribe();
        }

        // === МЕТОДЫ КОМАНД ===

        private async Task LoadTeachersAsync()
        {
            try
            {
                IsLoading = true;
                
                var (teachers, totalCount) = await _teacherService.GetTeachersPagedAsync(
                    CurrentPage, 
                    PageSize, 
                    SearchText,
                    SpecializationFilter,
                    StatusFilter);

                Teachers.Clear();
                foreach (var teacher in teachers)
                {
                    Teachers.Add(new TeacherViewModel(teacher));
                }

                TotalTeachers = totalCount;
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                _statusService.ShowSuccess($"Загружено {teachers.Count()} преподавателей", "Преподаватели");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки преподавателей: {ex.Message}", "Преподаватели");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadTeachersAsync();
                _statusService.ShowSuccess("Данные обновлены", "Преподаватели");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка обновления: {ex.Message}", "Преподаватели");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task CreateTeacherAsync()
        {
            try
            {
                var result = await _dialogService.ShowTeacherEditDialogAsync(new Teacher());
                if (result != null)
                {
                    await _teacherService.AddTeacherAsync(result);
                    await RefreshAsync();
                    _statusService.ShowSuccess($"Преподаватель '{result.FullName}' добавлен", "Преподаватели");
                    
                    await _notificationService.SendNotificationAsync(
                        "Новый преподаватель",
                        $"В систему добавлен новый преподаватель: {result.FullName}",
                        Domain.Models.System.NotificationType.Info);
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания преподавателя: {ex.Message}", "Преподаватели");
            }
        }

        private async Task EditTeacherAsync(TeacherViewModel teacherViewModel)
        {
            try
            {
                var teacher = await _teacherService.GetTeacherAsync(teacherViewModel.Uid);
                if (teacher == null)
                {
                    _statusService.ShowError("Преподаватель не найден", "Преподаватели");
                    return;
                }

                var result = await _dialogService.ShowTeacherEditDialogAsync(teacher);
                if (result != null)
                {
                    var success = await _teacherService.UpdateTeacherAsync(result);
                    if (success)
                    {
                        await RefreshAsync();
                        _statusService.ShowSuccess($"Преподаватель '{result.FullName}' обновлен", "Преподаватели");
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось обновить преподавателя", "Преподаватели");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка редактирования преподавателя: {ex.Message}", "Преподаватели");
            }
        }

        private async Task DeleteTeacherAsync(TeacherViewModel teacherViewModel)
        {
            try
            {
                var confirmation = await _dialogService.ShowConfirmationDialogAsync(
                    "Удаление преподавателя",
                    $"Вы уверены, что хотите удалить преподавателя '{teacherViewModel.FullName}'?\n\nЭто действие нельзя отменить.",
                    "Удалить",
                    "Отмена");

                if (confirmation)
                {
                    var success = await _teacherService.DeleteTeacherAsync(teacherViewModel.Uid);
                    if (success)
                    {
                        await RefreshAsync();
                        _statusService.ShowSuccess($"Преподаватель '{teacherViewModel.FullName}' удален", "Преподаватели");
                        
                        await _notificationService.SendNotificationAsync(
                            "Преподаватель удален",
                            $"Преподаватель {teacherViewModel.FullName} был удален из системы",
                            Domain.Models.System.NotificationType.Warning);
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось удалить преподавателя", "Преподаватели");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка удаления преподавателя: {ex.Message}", "Преподаватели");
            }
        }

        private async Task ViewTeacherDetailsAsync(TeacherViewModel teacherViewModel)
        {
            try
            {
                var teacher = await _teacherService.GetTeacherAsync(teacherViewModel.Uid);
                if (teacher != null)
                {
                    SelectedTeacher = new TeacherViewModel(teacher);
                    await ViewStatisticsAsync(SelectedTeacher);
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки деталей преподавателя: {ex.Message}", "Преподаватели");
            }
        }

        private async Task ViewStatisticsAsync(TeacherViewModel teacherViewModel)
        {
            try
            {
                var statistics = await _teacherService.GetTeacherStatisticsAsync(teacherViewModel.Uid);
                SelectedTeacherStatistics = statistics as TeacherStatistics;
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки статистики: {ex.Message}", "Преподаватели");
            }
        }

        private async Task ManageCoursesAsync(TeacherViewModel teacherViewModel)
        {
            try
            {
                var teacher = await _teacherService.GetTeacherAsync(teacherViewModel.Uid);
                if (teacher == null)
                {
                    _statusService.ShowError("Преподаватель не найден", "Преподаватели");
                    return;
                }

                var allCourses = await _courseService.GetAllCoursesAsync();
                var result = await _dialogService.ShowTeacherCoursesManagementDialogAsync(teacher, allCourses);
                
                if (result != null)
                {
                    await RefreshAsync();
                    _statusService.ShowSuccess("Курсы преподавателя обновлены", "Преподаватели");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка управления курсами: {ex.Message}", "Преподаватели");
            }
        }

        private async Task ManageGroupsAsync(TeacherViewModel teacherViewModel)
        {
            try
            {
                var teacher = await _teacherService.GetTeacherAsync(teacherViewModel.Uid);
                if (teacher == null)
                {
                    _statusService.ShowError("Преподаватель не найден", "Преподаватели");
                    return;
                }

                var allGroups = await _groupService.GetAllGroupsAsync();
                var result = await _dialogService.ShowTeacherGroupsManagementDialogAsync(teacher, allGroups);
                
                if (result != null)
                {
                    await RefreshAsync();
                    _statusService.ShowSuccess("Группы преподавателя обновлены", "Преподаватели");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка управления группами: {ex.Message}", "Преподаватели");
            }
        }

        private async Task SearchTeachersAsync(string searchTerm)
        {
            CurrentPage = 1;
            await LoadTeachersAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            CurrentPage = 1;
            await LoadTeachersAsync();
        }

        private async Task ClearFiltersAsync()
        {
            SpecializationFilter = null;
            StatusFilter = null;
            SearchText = string.Empty;
            CurrentPage = 1;
            await LoadTeachersAsync();
        }

        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                await LoadTeachersAsync();
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
    }

    /// <summary>
    /// ViewModel для отображения преподавателя в списке
    /// </summary>
    public class TeacherViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        [Reactive] public string FirstName { get; set; } = string.Empty;
        [Reactive] public string LastName { get; set; } = string.Empty;
        [Reactive] public string? MiddleName { get; set; }
        [Reactive] public string Email { get; set; } = string.Empty;
        [Reactive] public string? Phone { get; set; }
        [Reactive] public string? Bio { get; set; }
        [Reactive] public string? Specialization { get; set; }
        [Reactive] public int CoursesCount { get; set; }
        [Reactive] public int GroupsCount { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }
        [Reactive] public DateTime LastModifiedAt { get; set; }
        [Reactive] public string Status { get; set; } = string.Empty;
        [Reactive] public DateTime HireDate { get; set; }
        [Reactive] public string AcademicDegree { get; set; } = string.Empty;
        [Reactive] public string AcademicTitle { get; set; } = string.Empty;

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        public TeacherViewModel(Teacher teacher)
        {
            Uid = teacher.Uid;
            FirstName = teacher.FirstName;
            LastName = teacher.LastName;
            MiddleName = teacher.MiddleName;
            Email = teacher.Email;
            Phone = teacher.Phone;
            Specialization = teacher.Specialization;
            CoursesCount = teacher.Courses?.Count ?? 0;
            GroupsCount = teacher.CuratedGroups?.Count ?? 0;
            Status = teacher.Status.ToString();
            HireDate = teacher.HireDate;
            AcademicDegree = teacher.AcademicDegree ?? string.Empty;
            AcademicTitle = teacher.AcademicTitle ?? string.Empty;
            Bio = teacher.Bio ?? string.Empty;
            CreatedAt = teacher.CreatedAt;
            LastModifiedAt = teacher.LastModifiedAt ?? DateTime.UtcNow;
        }

        public Teacher ToTeacher()
        {
            return new Teacher
            {
                Uid = Uid,
                FirstName = FirstName,
                LastName = LastName,
                MiddleName = MiddleName ?? string.Empty,
                Phone = Phone ?? string.Empty,
                Specialization = Specialization ?? string.Empty,
                AcademicDegree = AcademicDegree ?? string.Empty,
                AcademicTitle = AcademicTitle ?? string.Empty,
                Bio = Bio ?? string.Empty,
                Status = Enum.Parse<TeacherStatus>(Status),
                HireDate = HireDate,
                CreatedAt = CreatedAt,
                LastModifiedAt = LastModifiedAt
            };
        }
    }

    /// <summary>
    /// Статистика преподавателя
    /// </summary>
    public class TeacherStatistics
    {
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalStudents { get; set; }
        public int CuratedGroups { get; set; }
        public int TotalAssignments { get; set; }
        public int PendingGrading { get; set; }
        public double AverageGrade { get; set; }
        public int TotalLessons { get; set; }
        public DateTime LastActivity { get; set; }
    }
} 