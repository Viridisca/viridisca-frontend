using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Services;
using static ViridiscaUi.Services.Interfaces.IAssignmentService;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления заданиями
    /// </summary>
    public class AssignmentsViewModel : RoutableViewModelBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseService _courseService;
        private readonly ITeacherService _teacherService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        public override string UrlPathSegment => "assignments";

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
        
        public ReactiveCommand<Unit, Unit> LoadAssignmentsCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateAssignmentCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> EditAssignmentCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> DeleteAssignmentCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> ViewAssignmentDetailsCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> LoadAssignmentStatisticsCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> PublishAssignmentCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> ViewSubmissionsCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> SendReminderCommand { get; }
        public ReactiveCommand<AssignmentViewModel, Unit> BulkGradeCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadAnalyticsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowOverdueAssignmentsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowPendingGradingCommand { get; }
        public ReactiveCommand<int, Unit> GoToPageCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        public AssignmentsViewModel(
            IScreen hostScreen,
            IAssignmentService assignmentService,
            ICourseService courseService,
            ITeacherService teacherService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService) : base(hostScreen)
        {
            _assignmentService = assignmentService;
            _courseService = courseService;
            _teacherService = teacherService;
            _dialogService = dialogService;
            _statusService = statusService;
            _notificationService = notificationService;

            // === ИНИЦИАЛИЗАЦИЯ КОМАНД ===

            LoadAssignmentsCommand = ReactiveCommand.CreateFromTask(LoadAssignmentsAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
            CreateAssignmentCommand = ReactiveCommand.CreateFromTask(CreateAssignmentAsync);
            EditAssignmentCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(EditAssignmentAsync);
            DeleteAssignmentCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(DeleteAssignmentAsync);
            ViewAssignmentDetailsCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(ViewAssignmentDetailsAsync);
            LoadAssignmentStatisticsCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(LoadAssignmentStatisticsAsync);
            PublishAssignmentCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(PublishAssignmentAsync);
            ViewSubmissionsCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(ViewSubmissionsAsync);
            SendReminderCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(SendReminderAsync);
            BulkGradeCommand = ReactiveCommand.CreateFromTask<AssignmentViewModel>(BulkGradeAsync);
            SearchCommand = ReactiveCommand.CreateFromTask<string>(SearchAssignmentsAsync);
            ApplyFiltersCommand = ReactiveCommand.CreateFromTask(ApplyFiltersAsync);
            ClearFiltersCommand = ReactiveCommand.CreateFromTask(ClearFiltersAsync);
            LoadAnalyticsCommand = ReactiveCommand.CreateFromTask(LoadAnalyticsAsync);
            ShowOverdueAssignmentsCommand = ReactiveCommand.CreateFromTask(ShowOverdueAssignmentsAsync);
            ShowPendingGradingCommand = ReactiveCommand.CreateFromTask(ShowPendingGradingAsync);
            GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
            NextPageCommand = ReactiveCommand.CreateFromTask(NextPageAsync, this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total));
            PreviousPageCommand = ReactiveCommand.CreateFromTask(PreviousPageAsync, this.WhenAnyValue(x => x.CurrentPage, current => current > 1));

            // === ПОДПИСКИ ===

            // Автопоиск при изменении текста поиска
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand);

            // Загрузка статистики при выборе задания
            this.WhenAnyValue(x => x.SelectedAssignment)
                .Where(assignment => assignment != null)
                .Select(assignment => assignment!)
                .InvokeCommand(LoadAssignmentStatisticsCommand);

            // Применение фильтров при изменении
            this.WhenAnyValue(x => x.StatusFilter, x => x.SelectedCourseFilter, x => x.SelectedTeacherFilter, x => x.DueDateFrom, x => x.DueDateTo)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => ApplyFiltersCommand.Execute().Subscribe());

            // Уведомления об изменении computed properties
            this.WhenAnyValue(x => x.SelectedAssignment)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedAssignment)));
                
            this.WhenAnyValue(x => x.SelectedAssignmentStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedAssignmentStatistics)));
                
            this.WhenAnyValue(x => x.Analytics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasAnalytics)));

            // Первоначальная загрузка
            LoadCoursesAndTeachersAsync();
            LoadAssignmentsCommand.Execute().Subscribe();
            LoadAnalyticsCommand.Execute().Subscribe();
        }

        // === МЕТОДЫ КОМАНД ===

        private async Task LoadAssignmentsAsync()
        {
            try
            {
                IsLoading = true;
                _statusService.ShowInfo("Загрузка заданий...", "Задания");

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

                _statusService.ShowSuccess($"Загружено {Assignments.Count} заданий", "Задания");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки заданий: {ex.Message}", "Задания");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCoursesAndTeachersAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                Courses.Clear();
                foreach (var course in courses)
                {
                    Courses.Add(new CourseViewModel(course));
                }

                var teachers = await _teacherService.GetTeachersAsync();
                Teachers.Clear();
                foreach (var teacher in teachers)
                {
                    Teachers.Add(new TeacherViewModel(teacher));
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить фильтры: {ex.Message}", "Задания");
            }
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
            try
            {
                var newAssignment = new Assignment
                {
                    Uid = Guid.NewGuid(),
                    Title = "Новое задание",
                    Description = string.Empty,
                    Instructions = string.Empty,
                    DueDate = DateTime.Today.AddDays(7),
                    MaxScore = 100,
                    Type = AssignmentType.Homework,
                    Difficulty = AssignmentDifficulty.Medium
                };

                var dialogResult = await _dialogService.ShowAssignmentEditDialogAsync(newAssignment);
                if (dialogResult == null) return;

                await _assignmentService.AddAssignmentAsync(dialogResult);
                Assignments.Add(new AssignmentViewModel(dialogResult));

                _statusService.ShowSuccess($"Задание '{dialogResult.Title}' создано", "Задания");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания задания: {ex.Message}", "Задания");
            }
        }

        private async Task EditAssignmentAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                var dialogResult = await _dialogService.ShowAssignmentEditDialogAsync(assignmentViewModel.ToAssignment());
                if (dialogResult == null) return;

                var success = await _assignmentService.UpdateAssignmentAsync(dialogResult);
                if (success)
                {
                    var index = Assignments.IndexOf(assignmentViewModel);
                    if (index >= 0)
                    {
                        Assignments[index] = new AssignmentViewModel(dialogResult);
                    }

                    _statusService.ShowSuccess($"Задание '{dialogResult.Title}' обновлено", "Задания");
                }
                else
                {
                    _statusService.ShowError("Не удалось обновить задание", "Задания");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка обновления задания: {ex.Message}", "Задания");
            }
        }

        private async Task DeleteAssignmentAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                var confirmResult = await _dialogService.ShowConfirmationAsync(
                    "Удаление задания",
                    $"Вы уверены, что хотите удалить задание '{assignmentViewModel.Title}'?\nВсе сдачи будут утеряны.");

                if (!confirmResult) return;

                var success = await _assignmentService.DeleteAssignmentAsync(assignmentViewModel.Uid);
                if (success)
                {
                    Assignments.Remove(assignmentViewModel);
                    _statusService.ShowSuccess($"Задание '{assignmentViewModel.Title}' удалено", "Задания");
                }
                else
                {
                    _statusService.ShowError("Не удалось удалить задание", "Задания");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка удаления задания: {ex.Message}", "Задания");
            }
        }

        private async Task ViewAssignmentDetailsAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                SelectedAssignment = assignmentViewModel;
                await LoadAssignmentStatisticsAsync(assignmentViewModel);
                
                _statusService.ShowInfo($"Просмотр задания '{assignmentViewModel.Title}'", "Задания");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отображения деталей задания: {ex.Message}", "Задания");
            }
        }

        private async Task LoadAssignmentStatisticsAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                SelectedAssignmentStatistics = await _assignmentService.GetAssignmentStatisticsAsync(assignmentViewModel.Uid);
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить статистику задания: {ex.Message}", "Задания");
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
                    _statusService.ShowSuccess($"Задание '{assignmentViewModel.Title}' опубликовано", "Задания");
                }
                else
                {
                    _statusService.ShowError("Не удалось опубликовать задание", "Задания");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка публикации задания: {ex.Message}", "Задания");
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
                    _statusService.ShowSuccess("Сдачи обновлены", "Задания");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка просмотра сдач: {ex.Message}", "Задания");
            }
        }

        private async Task SendReminderAsync(AssignmentViewModel assignmentViewModel)
        {
            try
            {
                await _assignmentService.SendDueDateReminderAsync(assignmentViewModel.Uid);
                _statusService.ShowSuccess($"Напоминания отправлены для задания '{assignmentViewModel.Title}'", "Задания");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отправки напоминаний: {ex.Message}", "Задания");
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
                    _statusService.ShowInfo("Все сдачи уже оценены", "Задания");
                    return;
                }

                var result = await _dialogService.ShowBulkGradingDialogAsync(ungradedSubmissions);
                if (result != null && result.Any())
                {
                    var gradingRequests = result.Cast<GradingRequest>();
                    var bulkResult = await _assignmentService.BulkGradeSubmissionsAsync(gradingRequests);
                    _statusService.ShowSuccess(
                        $"Оценено: {bulkResult.SuccessfulGradings}, ошибок: {bulkResult.FailedGradings}", 
                        "Задания");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка массового оценивания: {ex.Message}", "Задания");
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
                _statusService.ShowWarning($"Не удалось загрузить аналитику: {ex.Message}", "Задания");
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
                _statusService.ShowInfo("Показаны просроченные задания", "Задания");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки просроченных заданий: {ex.Message}", "Задания");
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

                _statusService.ShowInfo($"Показано {Assignments.Count} заданий, требующих проверки", "Задания");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки заданий для проверки: {ex.Message}", "Задания");
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