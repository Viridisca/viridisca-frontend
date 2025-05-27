using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Services;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using static ViridiscaUi.Services.Interfaces.IAssignmentService;
using ViridiscaUi.Domain.Models.System;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления заданиями
    /// Следует принципам SOLID и чистой архитектуры
    /// </summary>
    [Route("assignments", DisplayName = "Задания", IconKey = "📝", Order = 6, Group = "Education")]
    public class AssignmentsViewModel : RoutableViewModelBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseService _courseService;
        private readonly ITeacherService _teacherService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        

        // === СВОЙСТВА ===
        
        [Reactive] public ObservableCollection<AssignmentViewModel> Assignments { get; set; } = new();
        [Reactive] public AssignmentViewModel? SelectedAssignment { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public AssignmentStatistics? SelectedAssignmentStatistics { get; set; }
        [Reactive] public AssignmentAnalytics? Analytics { get; set; }
        
        // Фильтры
        [Reactive] public AssignmentStatus? StatusFilter { get; set; }
        [Reactive] public ObservableCollection<CourseViewModel> Courses { get; set; } = new();
        [Reactive] public CourseViewModel? SelectedCourseFilter { get; set; }
        [Reactive] public ObservableCollection<TeacherViewModel> Teachers { get; set; } = new();
        [Reactive] public TeacherViewModel? SelectedTeacherFilter { get; set; }
        [Reactive] public DateTime? DueDateFrom { get; set; }
        [Reactive] public DateTime? DueDateTo { get; set; }
        
        // Пагинация
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int PageSize { get; set; } = 20;
        [Reactive] public int TotalPages { get; set; }
        [Reactive] public int TotalAssignments { get; set; }
        
        // Computed properties for UI binding
        public bool HasSelectedAssignment => SelectedAssignment != null;
        public bool HasSelectedAssignmentStatistics => SelectedAssignmentStatistics != null;
        public bool HasAnalytics => Analytics != null;

        // === КОМАНДЫ ===
        
        public ReactiveCommand<Unit, Unit> LoadAssignmentsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> CreateAssignmentCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> EditAssignmentCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> DeleteAssignmentCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> ViewAssignmentDetailsCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> LoadAssignmentStatisticsCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> PublishAssignmentCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> ViewSubmissionsCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> SendReminderCommand { get; private set; } = null!;
        public ReactiveCommand<AssignmentViewModel, Unit> BulkGradeCommand { get; private set; } = null!;
        public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> LoadAnalyticsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ShowOverdueAssignmentsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ShowPendingGradingCommand { get; private set; } = null!;
        public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;

        public AssignmentsViewModel(
            IScreen hostScreen,
            IAssignmentService assignmentService,
            ICourseService courseService,
            ITeacherService teacherService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService) : base(hostScreen)
        {
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            InitializeCommands();
            SetupSubscriptions();
        }

        private void InitializeCommands()
        {
            LoadAssignmentsCommand = CreateCommand(LoadAssignmentsAsync);
            RefreshCommand = CreateCommand(RefreshAsync);
            CreateAssignmentCommand = CreateCommand(CreateAssignmentAsync);
            EditAssignmentCommand = CreateCommand<AssignmentViewModel>(EditAssignmentAsync);
            DeleteAssignmentCommand = CreateCommand<AssignmentViewModel>(DeleteAssignmentAsync);
            ViewAssignmentDetailsCommand = CreateCommand<AssignmentViewModel>(ViewAssignmentDetailsAsync);
            LoadAssignmentStatisticsCommand = CreateCommand<AssignmentViewModel>(LoadAssignmentStatisticsAsync);
            PublishAssignmentCommand = CreateCommand<AssignmentViewModel>(PublishAssignmentAsync);
            ViewSubmissionsCommand = CreateCommand<AssignmentViewModel>(ViewSubmissionsAsync);
            SendReminderCommand = CreateCommand<AssignmentViewModel>(SendReminderAsync);
            BulkGradeCommand = CreateCommand<AssignmentViewModel>(BulkGradeAsync);
            SearchCommand = CreateCommand<string>(SearchAssignmentsAsync);
            ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync);
            ClearFiltersCommand = CreateCommand(ClearFiltersAsync);
            LoadAnalyticsCommand = CreateCommand(LoadAnalyticsAsync);
            ShowOverdueAssignmentsCommand = CreateCommand(ShowOverdueAssignmentsAsync);
            ShowPendingGradingCommand = CreateCommand(ShowPendingGradingAsync);
            GoToPageCommand = CreateCommand<int>(GoToPageAsync);
            
            var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
            var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
            
            NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
            PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        }

        private void SetupSubscriptions()
        {
            // Автопоиск при изменении текста поиска
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(Disposables);

            // Загрузка статистики при выборе задания
            this.WhenAnyValue(x => x.SelectedAssignment)
                .Where(assignment => assignment != null)
                .Select(assignment => assignment!)
                .InvokeCommand(LoadAssignmentStatisticsCommand)
                .DisposeWith(Disposables);

            // Применение фильтров при изменении
            this.WhenAnyValue(x => x.StatusFilter, x => x.SelectedCourseFilter, x => x.SelectedTeacherFilter, x => x.DueDateFrom, x => x.DueDateTo)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => Unit.Default)
                .InvokeCommand(ApplyFiltersCommand)
                .DisposeWith(Disposables);

            // Уведомления об изменении computed properties
            this.WhenAnyValue(x => x.SelectedAssignment)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedAssignment)))
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.SelectedAssignmentStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedAssignmentStatistics)))
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.Analytics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasAnalytics)))
                .DisposeWith(Disposables);
        }

        #region Lifecycle Methods

        protected override async Task OnFirstTimeLoadedAsync()
        {
            await base.OnFirstTimeLoadedAsync();
            LogInfo("AssignmentsViewModel loaded for the first time");
            
            // Load filter data and assignments when view is loaded for the first time
            await ExecuteWithErrorHandlingAsync(LoadCoursesAndTeachersAsync, "Ошибка загрузки данных фильтров");
            await LoadAssignmentsAsync();
            await ExecuteWithErrorHandlingAsync(LoadAnalyticsAsync, "Ошибка загрузки аналитики");
        }

        #endregion

        // === МЕТОДЫ КОМАНД ===

        private async Task LoadAssignmentsAsync()
        {
            LogInfo("Loading assignments with filters: SearchText={SearchText}, Status={StatusFilter}, Course={CourseFilter}", 
                SearchText, StatusFilter, SelectedCourseFilter?.Name);
            
            IsLoading = true;
            ShowInfo("Загрузка заданий...");

            var courseFilter = SelectedCourseFilter?.Uid;
            var teacherFilter = SelectedTeacherFilter?.Uid;
            
            var (assignments, totalCount) = await _assignmentService.GetAssignmentsPagedAsync(
                CurrentPage, PageSize, SearchText, StatusFilter, courseFilter, teacherFilter, DueDateFrom, DueDateTo);
            
            Assignments.Clear();
            foreach (var assignment in assignments)
            {
                Assignments.Add(new AssignmentViewModel(assignment));
            }

            TotalAssignments = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            ShowSuccess($"Загружено {Assignments.Count} заданий");
            IsLoading = false;
        }

        private async Task LoadCoursesAndTeachersAsync()
        {
            LogInfo("Loading courses and teachers for filters");
            
            var courses = await _courseService.GetAllCoursesAsync();
            var teachers = await _teacherService.GetAllTeachersAsync();

            Courses.Clear();
            Teachers.Clear();

            foreach (var course in courses)
                Courses.Add(new CourseViewModel(course));

            foreach (var teacher in teachers)
                Teachers.Add(new TeacherViewModel(teacher));
                
            LogInfo("Loaded {CourseCount} courses and {TeacherCount} teachers for filters", courses.Count(), teachers.Count());
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadAssignmentsAsync();
                await LoadAnalyticsAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task CreateAssignmentAsync()
        {
            LogInfo("Creating new assignment");
            
            var newAssignment = new Assignment
            {
                Uid = Guid.NewGuid(),
                Title = string.Empty,
                Description = string.Empty,
                Instructions = string.Empty,
                DueDate = DateTime.Today.AddDays(7),
                MaxScore = 100,
                Type = AssignmentType.Homework,
                Difficulty = AssignmentDifficulty.Medium,
                Status = AssignmentStatus.Draft,
                CreatedAt = DateTime.Now
            };

            var dialogResult = await _dialogService.ShowAssignmentEditDialogAsync(newAssignment);
            if (dialogResult == null)
            {
                LogDebug("Assignment creation cancelled by user");
                return;
            }

            await _assignmentService.AddAssignmentAsync(dialogResult);
            Assignments.Add(new AssignmentViewModel(dialogResult));

            ShowSuccess($"Задание '{dialogResult.Title}' создано");
            LogInfo("Assignment created successfully: {AssignmentTitle}", dialogResult.Title);
            
            // Уведомление о создании нового задания
            if (dialogResult.CourseUid != Guid.Empty)
            {
                await _notificationService.CreateNotificationAsync(
                    Guid.NewGuid(), // Заглушка для recipientUid
                    "Новое задание",
                    $"Добавлено новое задание: {dialogResult.Title}",
                    Domain.Models.System.NotificationType.Info
                );
            }
        }

        private async Task EditAssignmentAsync(AssignmentViewModel assignmentViewModel)
        {
            LogInfo("Editing assignment: {AssignmentId}", assignmentViewModel.Uid);
            
            var assignment = await _assignmentService.GetAssignmentAsync(assignmentViewModel.Uid);
            if (assignment == null)
            {
                ShowError("Задание не найдено");
                return;
            }

            var dialogResult = await _dialogService.ShowAssignmentEditDialogAsync(assignment);
            if (dialogResult == null)
            {
                LogDebug("Assignment editing cancelled by user");
                return;
            }

            var success = await _assignmentService.UpdateAssignmentAsync(dialogResult);
            if (success)
            {
                var index = Assignments.IndexOf(assignmentViewModel);
                if (index >= 0)
                {
                    Assignments[index] = new AssignmentViewModel(dialogResult);
                }

                ShowSuccess($"Задание '{dialogResult.Title}' обновлено");
                LogInfo("Assignment updated successfully: {AssignmentTitle}", dialogResult.Title);
                
                // Уведомление об изменении задания
                if (dialogResult.CourseUid != Guid.Empty)
                {
                    await _notificationService.CreateNotificationAsync(
                        Guid.NewGuid(), // Заглушка для recipientUid
                        "Задание изменено",
                        $"Задание '{dialogResult.Title}' было изменено",
                        Domain.Models.System.NotificationType.Info
                    );
                }
            }
            else
            {
                ShowError("Не удалось обновить задание");
            }
        }

        private async Task DeleteAssignmentAsync(AssignmentViewModel assignmentViewModel)
        {
            LogInfo("Deleting assignment: {AssignmentId}", assignmentViewModel.Uid);
            
            // Проверяем, есть ли сдачи по этому заданию
            var submissions = await _assignmentService.GetSubmissionsByAssignmentAsync(assignmentViewModel.Uid);
            var hasSubmissions = submissions.Any();
            
            string warningMessage = $"Вы уверены, что хотите удалить задание '{assignmentViewModel.Title}'?";
            
            if (hasSubmissions)
            {
                warningMessage += $"\n\nВНИМАНИЕ: У задания есть {submissions.Count()} сдач, которые будут удалены!";
            }

            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Удаление задания", warningMessage);

            if (!confirmResult)
            {
                LogDebug("Assignment deletion cancelled by user");
                return;
            }

            var success = await _assignmentService.DeleteAssignmentAsync(assignmentViewModel.Uid);
            if (success)
            {
                Assignments.Remove(assignmentViewModel);
                ShowSuccess($"Задание '{assignmentViewModel.Title}' удалено");
                LogInfo("Assignment deleted successfully: {AssignmentTitle}", assignmentViewModel.Title);
                
                // Уведомление об удалении задания
                if (assignmentViewModel.CourseName != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        Guid.NewGuid(), // Заглушка для recipientUid
                        "Задание удалено",
                        $"Задание '{assignmentViewModel.Title}' было удалено",
                        Domain.Models.System.NotificationType.Warning);
                }
            }
            else
            {
                ShowError("Не удалось удалить задание");
            }
        }

        private async Task ViewAssignmentDetailsAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                SelectedAssignment = assignmentViewModel;
                await LoadAssignmentStatisticsAsync(assignmentViewModel);
                
                ShowInfo($"Просмотр задания '{assignmentViewModel.Title}'");
                LogInfo("Viewing assignment details: {AssignmentTitle}", assignmentViewModel.Title);
            }
            catch (Exception ex)
            {
                SetError($"Ошибка отображения деталей задания: {ex.Message}", ex);
            }
        }

        private async Task LoadAssignmentStatisticsAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                SelectedAssignmentStatistics = await _assignmentService.GetAssignmentStatisticsAsync(assignmentViewModel.Uid);
                LogInfo("Assignment statistics loaded for: {AssignmentTitle}", assignmentViewModel.Title);
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить статистику задания: {ex.Message}");
                LogError(ex, "Failed to load assignment statistics for: {AssignmentTitle}", assignmentViewModel.Title);
            }
        }

        private async Task PublishAssignmentAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                var success = await _assignmentService.PublishAssignmentAsync(assignmentViewModel.Uid);
                if (success)
                {
                    assignmentViewModel.Status = AssignmentStatus.Published;
                    ShowSuccess($"Задание '{assignmentViewModel.Title}' опубликовано");
                    LogInfo("Assignment published successfully: {AssignmentTitle}", assignmentViewModel.Title);
                }
                else
                {
                    ShowError("Не удалось опубликовать задание");
                    LogWarning("Failed to publish assignment: {AssignmentTitle}", assignmentViewModel.Title);
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка публикации задания: {ex.Message}", ex);
            }
        }

        private async Task ViewSubmissionsAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                var submissions = await _assignmentService.GetSubmissionsByAssignmentAsync(assignmentViewModel.Uid);
                var result = await _dialogService.ShowSubmissionsViewDialogAsync(assignmentViewModel.ToAssignment(), submissions);
                
                if (result != null)
                {
                    await RefreshAsync();
                    ShowSuccess("Сдачи обновлены");
                    LogInfo("Submissions updated for assignment: {AssignmentTitle}", assignmentViewModel.Title);
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка просмотра сдач: {ex.Message}", ex);
            }
        }

        private async Task SendReminderAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                await _assignmentService.SendDueDateReminderAsync(assignmentViewModel.Uid);
                ShowSuccess($"Напоминания отправлены для задания '{assignmentViewModel.Title}'");
                LogInfo("Reminders sent for assignment: {AssignmentTitle}", assignmentViewModel.Title);
            }
            catch (Exception ex)
            {
                SetError($"Ошибка отправки напоминаний: {ex.Message}", ex);
            }
        }

        private async Task BulkGradeAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                var submissions = await _assignmentService.GetSubmissionsByAssignmentAsync(assignmentViewModel.Uid);
                var ungradedSubmissions = submissions.Where(s => !s.Score.HasValue).ToList();
                
                if (!ungradedSubmissions.Any())
                {
                    ShowInfo("Все сдачи уже оценены");
                    LogInfo("All submissions already graded for assignment: {AssignmentTitle}", assignmentViewModel.Title);
                    return;
                }

                var result = await _dialogService.ShowBulkGradingDialogAsync(ungradedSubmissions);
                if (result != null && result.Any())
                {
                    var gradingRequests = result.Cast<GradingRequest>();
                    var bulkResult = await _assignmentService.BulkGradeSubmissionsAsync(gradingRequests);
                    ShowSuccess($"Оценено: {bulkResult.SuccessfulGradings}, ошибок: {bulkResult.FailedGradings}");
                    LogInfo("Bulk grading completed for assignment {AssignmentTitle}: {SuccessCount} successful, {FailCount} failed", 
                        assignmentViewModel.Title, bulkResult.SuccessfulGradings, bulkResult.FailedGradings);
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка массового оценивания: {ex.Message}", ex);
            }
        }

        private async Task LoadAnalyticsAsync()
        {
            try
            {
                var courseFilter = SelectedCourseFilter?.Uid;
                Analytics = await _assignmentService.GetAssignmentAnalyticsAsync(courseFilter);
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить аналитику: {ex.Message}");
            }
        }

        private async Task ShowOverdueAssignmentsAsync()
        {
            try
            {
                StatusFilter = null;
                DueDateTo = DateTime.Now;
                SearchText = string.Empty;
                CurrentPage = 1;
                await LoadAssignmentsAsync();
                ShowInfo("Показаны просроченные задания");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка загрузки просроченных заданий: {ex.Message}", ex);
            }
        }

        private async Task ShowPendingGradingAsync()
        {
            try
            {
                var teacherFilter = SelectedTeacherFilter?.Uid;
                var pendingAssignments = await _assignmentService.GetAssignmentsPendingGradingAsync(teacherFilter);
                
                Assignments.Clear();
                foreach (var assignment in pendingAssignments)
                {
                    Assignments.Add(new AssignmentViewModel(assignment));
                }

                ShowInfo($"Показано {Assignments.Count} заданий, требующих проверки");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка загрузки заданий для проверки: {ex.Message}", ex);
            }
        }

        private async Task SearchAssignmentsAsync(string searchText)
        {
            SearchText = searchText;
            CurrentPage = 1;
            await LoadAssignmentsAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            CurrentPage = 1;
            await LoadAssignmentsAsync();
            await LoadAnalyticsAsync();
        }

        private async Task ClearFiltersAsync()
        {
            StatusFilter = null;
            SelectedCourseFilter = null;
            SelectedTeacherFilter = null;
            DueDateFrom = null;
            DueDateTo = null;
            SearchText = string.Empty;
            CurrentPage = 1;
            await LoadAssignmentsAsync();
            await LoadAnalyticsAsync();
        }

        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                await LoadAssignmentsAsync();
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
    /// ViewModel для отображения задания в списке
    /// </summary>
    public class AssignmentViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        [Reactive] public string Title { get; set; } = string.Empty;
        [Reactive] public string? Description { get; set; }
        [Reactive] public string? Instructions { get; set; }
        [Reactive] public DateTime DueDate { get; set; } = DateTime.MinValue;
        [Reactive] public double MaxScore { get; set; }
        [Reactive] public string Type { get; set; } = string.Empty;
        [Reactive] public string Difficulty { get; set; } = string.Empty;
        [Reactive] public AssignmentStatus Status { get; set; }
        [Reactive] public string? CourseName { get; set; }
        [Reactive] public string? TeacherName { get; set; }
        [Reactive] public int SubmissionsCount { get; set; }
        [Reactive] public int GradedCount { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }
        [Reactive] public DateTime LastModifiedAt { get; set; }

        // Computed properties
        public string StatusText => Status switch
        {
            AssignmentStatus.Draft => "📝 Черновик",
            AssignmentStatus.Published => "✅ Опубликовано",
            AssignmentStatus.Closed => "🔒 Закрыто",
            AssignmentStatus.Archived => "📦 Архивировано",
            _ => Status.ToString()
        };

        public string DueDateText => DueDate.ToString("dd.MM.yyyy HH:mm");
        
        public bool IsOverdue => DateTime.Now > DueDate && Status == AssignmentStatus.Published;
        
        public string OverdueText => IsOverdue ? "⚠️ Просрочено" : "";

        public string ProgressText => SubmissionsCount > 0 ? $"{GradedCount}/{SubmissionsCount}" : "0/0";

        public AssignmentViewModel(Assignment assignment)
        {
            Uid = assignment.Uid;
            Title = assignment.Title;
            Description = assignment.Description;
            Instructions = assignment.Instructions;
            DueDate = assignment.DueDate ?? DateTime.MinValue;
            MaxScore = assignment.MaxScore;
            Type = assignment.Type.ToString();
            Difficulty = assignment.Difficulty.ToString();
            Status = assignment.Status;
            CourseName = assignment.Course?.Name;
            TeacherName = assignment.Course?.Teacher != null ? 
                $"{assignment.Course.Teacher.FirstName} {assignment.Course.Teacher.LastName}" : null;
            SubmissionsCount = assignment.Submissions?.Count ?? 0;
            GradedCount = assignment.Submissions?.Count(s => s.Score.HasValue) ?? 0;
            CreatedAt = assignment.CreatedAt;
            LastModifiedAt = assignment.LastModifiedAt ?? DateTime.UtcNow;
        }

        public Assignment ToAssignment()
        {
            return new Assignment
            {
                Uid = Uid,
                Title = Title,
                Description = Description,
                Instructions = Instructions,
                DueDate = DueDate,
                MaxScore = MaxScore,
                Type = Enum.TryParse<AssignmentType>(Type, out var type) ? type : AssignmentType.Homework,
                Difficulty = Enum.TryParse<AssignmentDifficulty>(Difficulty, out var difficulty) ? difficulty : AssignmentDifficulty.Medium,
                Status = Status,
                CreatedAt = CreatedAt,
                LastModifiedAt = LastModifiedAt
            };
        }
    }
} 