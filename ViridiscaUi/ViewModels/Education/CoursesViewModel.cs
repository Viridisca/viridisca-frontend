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
using CourseStatus = ViridiscaUi.Domain.Models.Education.CourseStatus;
using NotificationType = ViridiscaUi.Domain.Models.System.NotificationType;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления курсами
    /// </summary>
    public class CoursesViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment => "courses";
        public IScreen HostScreen { get; }

        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IGroupService _groupService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;
        private readonly IServiceProvider _serviceProvider;

        // === СВОЙСТВА ===
        
        [Reactive] public ObservableCollection<CourseViewModel> Courses { get; set; } = new();
        [Reactive] public CourseViewModel? SelectedCourse { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public CourseStatistics? SelectedCourseStatistics { get; set; }
        
        // Фильтры
        [Reactive] public CourseStatus? StatusFilter { get; set; }
        [Reactive] public ObservableCollection<TeacherViewModel> Teachers { get; set; } = new();
        [Reactive] public TeacherViewModel? SelectedTeacherFilter { get; set; }
        
        // Пагинация
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int PageSize { get; set; } = 15;
        [Reactive] public int TotalPages { get; set; }
        [Reactive] public int TotalCourses { get; set; }

        // Computed properties for UI binding
        public bool HasSelectedCourse => SelectedCourse != null;
        public bool HasSelectedCourseStatistics => SelectedCourseStatistics != null;

        // === КОМАНДЫ ===
        
        public ReactiveCommand<Unit, Unit> LoadCoursesCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateCourseCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> EditCourseCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> DeleteCourseCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> ViewCourseDetailsCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> LoadCourseStatisticsCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> PublishCourseCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> ArchiveCourseCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> ManageEnrollmentsCommand { get; }
        public ReactiveCommand<CourseViewModel, Unit> BulkEnrollGroupCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
        public ReactiveCommand<int, Unit> GoToPageCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public CoursesViewModel(
            ICourseService courseService,
            IStudentService studentService,
            ITeacherService teacherService,
            IGroupService groupService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService,
            IServiceProvider serviceProvider,
            IScreen hostScreen)
        {
            _courseService = courseService;
            _studentService = studentService;
            _teacherService = teacherService;
            _groupService = groupService;
            _dialogService = dialogService;
            _statusService = statusService;
            _notificationService = notificationService;
            _serviceProvider = serviceProvider;
            HostScreen = hostScreen;

            // === ИНИЦИАЛИЗАЦИЯ КОМАНД ===

            LoadCoursesCommand = ReactiveCommand.CreateFromTask(LoadCoursesAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
            CreateCourseCommand = ReactiveCommand.CreateFromTask(CreateCourseAsync);
            EditCourseCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(EditCourseAsync);
            DeleteCourseCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(DeleteCourseAsync);
            ViewCourseDetailsCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(ViewCourseDetailsAsync);
            LoadCourseStatisticsCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(LoadCourseStatisticsAsync);
            PublishCourseCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(PublishCourseAsync);
            ArchiveCourseCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(ArchiveCourseAsync);
            ManageEnrollmentsCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(ManageEnrollmentsAsync);
            BulkEnrollGroupCommand = ReactiveCommand.CreateFromTask<CourseViewModel>(BulkEnrollGroupAsync);
            SearchCommand = ReactiveCommand.CreateFromTask<string>(SearchCoursesAsync);
            ApplyFiltersCommand = ReactiveCommand.CreateFromTask(ApplyFiltersAsync);
            ClearFiltersCommand = ReactiveCommand.CreateFromTask(ClearFiltersAsync);
            GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
            NextPageCommand = ReactiveCommand.CreateFromTask(NextPageAsync, this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total));
            PreviousPageCommand = ReactiveCommand.CreateFromTask(PreviousPageAsync, this.WhenAnyValue(x => x.CurrentPage, current => current > 1));

            // === ПОДПИСКИ ===

            // Автопоиск при изменении текста поиска
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand);

            // Загрузка статистики при выборе курса
            this.WhenAnyValue(x => x.SelectedCourse)
                .Where(course => course != null)
                .Select(course => course!)
                .InvokeCommand(LoadCourseStatisticsCommand);

            // Применение фильтров при изменении
            this.WhenAnyValue(x => x.StatusFilter, x => x.SelectedTeacherFilter)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => ApplyFiltersCommand.Execute().Subscribe());

            // Уведомления об изменении computed properties
            this.WhenAnyValue(x => x.SelectedCourse)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedCourse)));
                
            this.WhenAnyValue(x => x.SelectedCourseStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedCourseStatistics)));

            // Первоначальная загрузка
            LoadTeachersAsync();
            LoadCoursesCommand.Execute().Subscribe();
        }

        // === МЕТОДЫ КОМАНД ===

        private async Task LoadCoursesAsync()
        {
            try
            {
                IsLoading = true;
                _statusService.ShowInfo("Загрузка курсов...", "Курсы");

                var teacherFilter = SelectedTeacherFilter?.Uid;
                var (courses, totalCount) = await _courseService.GetCoursesPagedAsync(
                    CurrentPage, PageSize, SearchText, StatusFilter, teacherFilter);
                
                Courses.Clear();
                foreach (var course in courses)
                {
                    Courses.Add(new CourseViewModel(course));
                }

                TotalCourses = totalCount;
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                _statusService.ShowSuccess($"Загружено {Courses.Count} курсов", "Курсы");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки курсов: {ex.Message}", "Курсы");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTeachersAsync()
        {
            try
            {
                var teachers = await _teacherService.GetTeachersAsync();
                Teachers.Clear();
                foreach (var teacher in teachers)
                {
                    Teachers.Add(new TeacherViewModel(teacher));
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить список преподавателей: {ex.Message}", "Курсы");
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadCoursesAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task CreateCourseAsync()
        {
            try
            {
                var newCourse = new Course
                {
                    Uid = Guid.NewGuid(),
                    Name = string.Empty,
                    Description = string.Empty,
                    Status = CourseStatus.Draft,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(3),
                    Credits = 3
                };

                var dialogResult = await _dialogService.ShowCourseEditDialogAsync(newCourse);
                if (dialogResult == null) return;

                await _courseService.AddCourseAsync(dialogResult);
                Courses.Add(new CourseViewModel(dialogResult));

                _statusService.ShowSuccess($"Курс '{dialogResult.Name}' создан", "Курсы");
                
                // Уведомление преподавателю
                if (dialogResult.TeacherUid.HasValue)
                {
                    await _notificationService.CreateNotificationAsync(
                        dialogResult.TeacherUid.Value,
                        "Назначение на курс",
                        $"Вы назначены преподавателем курса '{dialogResult.Name}'",
                        NotificationType.Info);
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания курса: {ex.Message}", "Курсы");
            }
        }

        private async Task EditCourseAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var dialogResult = await _dialogService.ShowCourseEditDialogAsync(courseViewModel.ToCourse());
                if (dialogResult == null) return;

                var success = await _courseService.UpdateCourseAsync(dialogResult);
                if (success)
                {
                    var index = Courses.IndexOf(courseViewModel);
                    if (index >= 0)
                    {
                        Courses[index] = new CourseViewModel(dialogResult);
                    }

                    _statusService.ShowSuccess($"Курс '{dialogResult.Name}' обновлен", "Курсы");
                }
                else
                {
                    _statusService.ShowError("Не удалось обновить курс", "Курсы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка обновления курса: {ex.Message}", "Курсы");
            }
        }

        private async Task DeleteCourseAsync(CourseViewModel courseViewModel)
        {
            try
            {
                var confirmResult = await _dialogService.ShowConfirmationAsync(
                    "Удаление курса",
                    $"Вы уверены, что хотите удалить курс '{courseViewModel.Name}'?\nВсе связанные данные будут утеряны.");

                if (!confirmResult) return;

                var success = await _courseService.DeleteCourseAsync(courseViewModel.Uid);
                if (success)
                {
                    Courses.Remove(courseViewModel);
                    _statusService.ShowSuccess($"Курс '{courseViewModel.Name}' удален", "Курсы");
                }
                else
                {
                    _statusService.ShowError("Не удалось удалить курс", "Курсы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка удаления курса: {ex.Message}", "Курсы");
            }
        }

        private async Task ViewCourseDetailsAsync(CourseViewModel courseViewModel)
        {
            try
            {
                SelectedCourse = courseViewModel;
                await LoadCourseStatisticsAsync(courseViewModel);
                
                _statusService.ShowInfo($"Просмотр курса '{courseViewModel.Name}'", "Курсы");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отображения деталей курса: {ex.Message}", "Курсы");
            }
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
                        NotificationType.Info);
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
                            NotificationType.Info);
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
            StatusFilter = null;
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
    }

    /// <summary>
    /// ViewModel для отображения курса в списке
    /// </summary>
    public class CourseViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string? Description { get; set; }
        [Reactive] public string? TeacherName { get; set; }
        [Reactive] public Guid? TeacherUid { get; set; }
        [Reactive] public CourseStatus Status { get; set; }
        [Reactive] public DateTime StartDate { get; set; }
        [Reactive] public DateTime EndDate { get; set; }
        [Reactive] public int Credits { get; set; }
        [Reactive] public int EnrolledStudents { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }
        [Reactive] public DateTime LastModifiedAt { get; set; }

        // Computed properties
        public string StatusText => Status switch
        {
            CourseStatus.Draft => "📝 Черновик",
            CourseStatus.Published => "✅ Опубликован",
            CourseStatus.Archived => "📦 Архивирован",
            CourseStatus.Suspended => "⏸️ Приостановлен",
            _ => Status.ToString()
        };

        public string DurationText => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";

        public CourseViewModel(Course course)
        {
            Uid = course.Uid;
            Name = course.Name;
            Description = course.Description;
            TeacherName = course.Teacher != null ? $"{course.Teacher.FirstName} {course.Teacher.LastName}" : null;
            TeacherUid = course.TeacherUid;
            Status = course.Status;
            StartDate = course.StartDate ?? DateTime.MinValue;
            EndDate = course.EndDate ?? DateTime.MinValue;
            Credits = course.Credits;
            EnrolledStudents = course.Enrollments?.Count ?? 0;
            CreatedAt = course.CreatedAt;
            LastModifiedAt = course.LastModifiedAt ?? DateTime.UtcNow;
        }

        public Course ToCourse()
        {
            return new Course
            {
                Uid = Uid,
                Name = Name,
                Description = Description,
                TeacherUid = TeacherUid,
                Status = Status,
                StartDate = StartDate,
                EndDate = EndDate,
                Credits = Credits,
                CreatedAt = CreatedAt,
                LastModifiedAt = LastModifiedAt
            };
        }
    }

    /// <summary>
    /// ViewModel для отображения преподавателя в фильтре
    /// </summary>
    public class TeacherViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        public string FullName { get; }

        public TeacherViewModel(Teacher teacher)
        {
            Uid = teacher.Uid;
            FullName = $"{teacher.FirstName} {teacher.LastName}";
        }
    }
} 