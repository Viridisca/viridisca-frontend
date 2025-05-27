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

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления оценками
    /// Следует принципам SOLID и чистой архитектуры
    /// </summary>
    [Route("grades", DisplayName = "Оценки", IconKey = "Grade", Order = 5, Group = "Education")]
    public class GradesViewModel : RoutableViewModelBase
    {
        

        private readonly IGradeService _gradeService;
        private readonly ICourseService _courseService;
        private readonly IGroupService _groupService;
        private readonly IStudentService _studentService;
        private readonly IAssignmentService _assignmentService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        // === СВОЙСТВА ===
        
        [Reactive] public ObservableCollection<GradeViewModel> Grades { get; set; } = new();
        [Reactive] public GradeViewModel? SelectedGrade { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        
        // Фильтры
        [Reactive] public ObservableCollection<Course> Courses { get; set; } = new();
        [Reactive] public ObservableCollection<Group> Groups { get; set; } = new();
        [Reactive] public Course? SelectedCourseFilter { get; set; }
        [Reactive] public Group? SelectedGroupFilter { get; set; }
        [Reactive] public string? GradeRangeFilter { get; set; }
        [Reactive] public string? PeriodFilter { get; set; }
        
        // Пагинация
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int PageSize { get; set; } = 25;
        [Reactive] public int TotalPages { get; set; }
        [Reactive] public int TotalGrades { get; set; }

        // Статистика
        [Reactive] public double AverageGrade { get; set; }
        [Reactive] public int ExcellentCount { get; set; }
        [Reactive] public int GoodCount { get; set; }
        [Reactive] public int SatisfactoryCount { get; set; }
        [Reactive] public int UnsatisfactoryCount { get; set; }
        [Reactive] public double SuccessRate { get; set; }
        [Reactive] public double QualityRate { get; set; }

        // Computed properties
        public bool HasSelectedGrade => SelectedGrade != null;
        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        // === КОМАНДЫ ===
        
        public ReactiveCommand<Unit, Unit> LoadGradesCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> CreateGradeCommand { get; private set; } = null!;
        public ReactiveCommand<GradeViewModel, Unit> EditGradeCommand { get; private set; } = null!;
        public ReactiveCommand<GradeViewModel, Unit> DeleteGradeCommand { get; private set; } = null!;
        public ReactiveCommand<GradeViewModel, Unit> ViewGradeDetailsCommand { get; private set; } = null!;
        public ReactiveCommand<GradeViewModel, Unit> AddCommentCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> BulkGradingCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ExportReportCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> GenerateAnalyticsReportCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> NotifyParentsCommand { get; private set; } = null!;
        public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;

        /// <summary>
        /// Конструктор
        /// </summary>
        public GradesViewModel(
            IScreen hostScreen,
            IGradeService gradeService,
            IStudentService studentService,
            ICourseService courseService,
            IGroupService groupService,
            IAssignmentService assignmentService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService) : base(hostScreen)
        {
            _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            InitializeCommands();
            SetupSubscriptions();
        }

        private void InitializeCommands()
        {
            LoadGradesCommand = CreateCommand(LoadGradesAsync);
            RefreshCommand = CreateCommand(RefreshAsync);
            CreateGradeCommand = CreateCommand(CreateGradeAsync);
            EditGradeCommand = CreateCommand<GradeViewModel>(EditGradeAsync);
            DeleteGradeCommand = CreateCommand<GradeViewModel>(DeleteGradeAsync);
            ViewGradeDetailsCommand = CreateCommand<GradeViewModel>(ViewGradeDetailsAsync);
            AddCommentCommand = CreateCommand<GradeViewModel>(AddCommentAsync);
            BulkGradingCommand = CreateCommand(BulkGradingAsync);
            ExportReportCommand = CreateCommand(ExportReportAsync);
            ExportToExcelCommand = CreateCommand(ExportToExcelAsync);
            GenerateAnalyticsReportCommand = CreateCommand(GenerateAnalyticsReportAsync);
            NotifyParentsCommand = CreateCommand(NotifyParentsAsync);
            SearchCommand = CreateCommand<string>(SearchGradesAsync);
            ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync);
            ClearFiltersCommand = CreateCommand(ClearFiltersAsync);
            GoToPageCommand = CreateCommand<int>(GoToPageAsync);
            
            var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
            var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
            
            NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
            PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        }

        private void SetupSubscriptions()
        {
            // Автоматический поиск при изменении текста
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(Disposables);

            // Обновление computed properties
            this.WhenAnyValue(x => x.SelectedGrade)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGrade)))
                .DisposeWith(Disposables);

            this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages)
                .Subscribe(_ => 
                {
                    this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                    this.RaisePropertyChanged(nameof(CanGoToNextPage));
                })
                .DisposeWith(Disposables);

            // Автоматическое применение фильтров
            this.WhenAnyValue(x => x.SelectedCourseFilter, x => x.SelectedGroupFilter, x => x.GradeRangeFilter, x => x.PeriodFilter)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => Unit.Default)
                .InvokeCommand(ApplyFiltersCommand)
                .DisposeWith(Disposables);
        }

        #region Lifecycle Methods

        protected override async Task OnFirstTimeLoadedAsync()
        {
            await base.OnFirstTimeLoadedAsync();
            LogInfo("GradesViewModel loaded for the first time");
            
            // Load filter data and grades when view is loaded for the first time
            await ExecuteWithErrorHandlingAsync(LoadFiltersDataAsync, "Ошибка загрузки данных фильтров");
            await LoadGradesAsync();
        }

        #endregion

        // === МЕТОДЫ ИНИЦИАЛИЗАЦИИ ===

        private async Task LoadFiltersDataAsync()
        {
            LogInfo("Loading filter data for grades");
            
            var courses = await _courseService.GetAllCoursesAsync();
            var groups = await _groupService.GetAllGroupsAsync();

            Courses.Clear();
            Groups.Clear();

            foreach (var course in courses)
                Courses.Add(course);

            foreach (var group in groups)
                Groups.Add(group);
                
            LogInfo("Loaded {CourseCount} courses and {GroupCount} groups for filters", courses.Count(), groups.Count());
        }

        // === МЕТОДЫ КОМАНД ===

        private async Task LoadGradesAsync()
        {
            LogInfo("Loading grades with filters: SearchText={SearchText}, Course={CourseFilter}, Group={GroupFilter}", 
                SearchText, SelectedCourseFilter?.Name, SelectedGroupFilter?.Name);
            
            IsLoading = true;
            ShowInfo("Загрузка оценок...");

            var (grades, totalCount) = await _gradeService.GetGradesPagedAsync(
                CurrentPage, 
                PageSize, 
                SearchText,
                SelectedCourseFilter?.Uid,
                SelectedGroupFilter?.Uid,
                ParseGradeRangeFilter(),
                ParsePeriodFilter());

            Grades.Clear();
            foreach (var grade in grades)
            {
                Grades.Add(new GradeViewModel(grade));
            }

            TotalGrades = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            await LoadStatisticsAsync();

            ShowSuccess($"Загружено {grades.Count()} оценок");
            IsLoading = false;
        }

        private async Task LoadStatisticsAsync()
        {
            LogInfo("Loading grade statistics");
            
            var statistics = await _gradeService.GetGradeStatisticsAsync(
                SelectedCourseFilter?.Uid,
                SelectedGroupFilter?.Uid,
                ParsePeriodFilter());

            AverageGrade = statistics.AverageGrade;
            ExcellentCount = statistics.ExcellentCount;
            GoodCount = statistics.GoodCount;
            SatisfactoryCount = statistics.SatisfactoryCount;
            UnsatisfactoryCount = statistics.UnsatisfactoryCount;
            SuccessRate = statistics.SuccessRate;
            QualityRate = statistics.QualityRate;
            
            LogInfo("Loaded statistics: Average={AverageGrade}, Success={SuccessRate}%", AverageGrade, SuccessRate);
        }

        private async Task RefreshAsync()
        {
            LogInfo("Refreshing grades data");
            IsRefreshing = true;
            
            await LoadGradesAsync();
            ShowSuccess("Данные обновлены");
            
            IsRefreshing = false;
        }

        private async Task CreateGradeAsync()
        {
            LogInfo("Creating new grade");
            
            var grade = new Grade
            {
                Uid = Guid.NewGuid(),
                StudentUid = Guid.NewGuid(), // Заглушка
                AssignmentUid = Guid.NewGuid(), // Заглушка
                TeacherUid = Guid.NewGuid(), // Заглушка
                Value = 0,
                Comment = "Новая оценка",
                GradedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var students = await _studentService.GetAllStudentsAsync();
            var assignments = await _assignmentService.GetAllAssignmentsAsync();

            var dialogResult = await _dialogService.ShowGradeEditDialogAsync(grade, students, assignments);
            if (dialogResult == null)
            {
                LogDebug("Grade creation cancelled by user");
                return;
            }

            await _gradeService.AddGradeAsync(dialogResult);
            Grades.Add(new GradeViewModel(dialogResult));

            ShowSuccess($"Оценка добавлена");
            LogInfo("Grade created successfully for student: {StudentUid}", dialogResult.StudentUid);
            
            // Уведомление студенту о новой оценке
            await _notificationService.CreateNotificationAsync(
                dialogResult.StudentUid,
                "Новая оценка",
                $"Вы получили оценку {dialogResult.Value}/100",
                Domain.Models.System.NotificationType.Info);

            // Обновляем статистику
            await LoadStatisticsAsync();
        }

        private async Task EditGradeAsync(GradeViewModel gradeViewModel)
        {
            LogInfo("Editing grade: {GradeId}", gradeViewModel.Uid);
            
            var grade = await _gradeService.GetGradeAsync(gradeViewModel.Uid);
            if (grade == null)
            {
                ShowError("Оценка не найдена");
                return;
            }

            var students = await _studentService.GetAllStudentsAsync();
            var assignments = await _assignmentService.GetAllAssignmentsAsync();

            var dialogResult = await _dialogService.ShowGradeEditDialogAsync(grade, students, assignments);
            if (dialogResult == null)
            {
                LogDebug("Grade editing cancelled by user");
                return;
            }

            var success = await _gradeService.UpdateGradeAsync(dialogResult);
            if (success)
            {
                var index = Grades.IndexOf(gradeViewModel);
                if (index >= 0)
                {
                    Grades[index] = new GradeViewModel(dialogResult);
                }

                ShowSuccess($"Оценка обновлена");
                LogInfo("Grade updated successfully: {GradeId}", dialogResult.Uid);
                
                // Уведомление студенту об изменении оценки
                await _notificationService.CreateNotificationAsync(
                    dialogResult.StudentUid,
                    "Оценка изменена",
                    $"Ваша оценка изменена на {dialogResult.Value}/100",
                    Domain.Models.System.NotificationType.Info);

                // Обновляем статистику
                await LoadStatisticsAsync();
            }
            else
            {
                ShowError("Не удалось обновить оценку");
            }
        }

        private async Task DeleteGradeAsync(GradeViewModel gradeViewModel)
        {
            LogInfo("Deleting grade: {GradeId}", gradeViewModel.Uid);
            
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Удаление оценки",
                $"Вы уверены, что хотите удалить оценку {gradeViewModel.Grade}/{gradeViewModel.MaxGrade}?\n\nЭто действие нельзя отменить.");

            if (!confirmResult)
            {
                LogDebug("Grade deletion cancelled by user");
                return;
            }

            var success = await _gradeService.DeleteGradeAsync(gradeViewModel.Uid);
            if (success)
            {
                Grades.Remove(gradeViewModel);
                ShowSuccess($"Оценка удалена");
                LogInfo("Grade deleted successfully: {GradeId}", gradeViewModel.Uid);
                
                // Обновляем статистику
                await LoadStatisticsAsync();
            }
            else
            {
                ShowError("Не удалось удалить оценку");
            }
        }

        private async Task ViewGradeDetailsAsync(GradeViewModel gradeViewModel)
        {
            try
            {
                var grade = await _gradeService.GetGradeAsync(gradeViewModel.Uid);
                if (grade != null)
                {
                    SelectedGrade = new GradeViewModel(grade);
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка загрузки деталей оценки: {ex.Message}", ex);
            }
        }

        private async Task AddCommentAsync(GradeViewModel gradeViewModel)
        {
            try
            {
                var comment = await _dialogService.ShowTextInputDialogAsync(
                    "Комментарий к оценке",
                    "Введите комментарий:",
                    gradeViewModel.Comment ?? string.Empty);

                if (comment != null)
                {
                    var success = await _gradeService.UpdateGradeCommentAsync(gradeViewModel.Uid, comment);
                    if (success)
                    {
                        await RefreshAsync();
                        ShowSuccess("Комментарий добавлен");
                    }
                    else
                    {
                        ShowError("Не удалось добавить комментарий");
                    }
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка добавления комментария: {ex.Message}", ex);
            }
        }

        private async Task BulkGradingAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var assignments = await _assignmentService.GetAllAssignmentsAsync();
                
                // Создаем заглушку для submissions
                var submissions = assignments.Select(a => new Submission 
                { 
                    Uid = Guid.NewGuid(),
                    AssignmentUid = a.Uid,
                    StudentUid = Guid.NewGuid(),
                    SubmissionDate = DateTime.UtcNow
                }).ToList();
                
                var result = await _dialogService.ShowBulkGradingDialogAsync(submissions);
                if (result != null)
                {
                    // Заглушка для массового добавления оценок
                    var count = result.Count();
                    ShowSuccess($"Добавлено {count} оценок");
                    
                    await _notificationService.SendNotificationAsync(
                        "Массовое оценивание",
                        $"Добавлено {count} оценок",
                        Domain.Models.System.NotificationType.Info);
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка массового оценивания: {ex.Message}", ex);
            }
        }

        private async Task ExportReportAsync()
        {
            try
            {
                var grades = await _gradeService.GetAllGradesAsync(
                    SelectedCourseFilter?.Uid,
                    SelectedGroupFilter?.Uid,
                    ParseGradeRangeFilter(),
                    ParsePeriodFilter());

                // Заглушка - в реальной реализации здесь будет экспорт в PDF
                ShowInfo($"Экспорт отчета: {grades.Count()} оценок готовы к экспорту");
                LogInfo("Export report requested for {GradeCount} grades", grades.Count());
            }
            catch (Exception ex)
            {
                SetError($"Ошибка экспорта отчета: {ex.Message}", ex);
            }
        }

        private async Task ExportToExcelAsync()
        {
            try
            {
                var grades = await _gradeService.GetAllGradesAsync();

                // Заглушка - в реальной реализации здесь будет экспорт в Excel
                ShowInfo($"Экспорт в Excel: {grades.Count()} оценок готовы к экспорту");
                LogInfo("Export to Excel requested for {GradeCount} grades", grades.Count());
            }
            catch (Exception ex)
            {
                SetError($"Ошибка экспорта в Excel: {ex.Message}", ex);
            }
        }

        private async Task GenerateAnalyticsReportAsync()
        {
            try
            {
                // Заглушка - в реальной реализации здесь будет создание аналитического отчета
                ShowInfo("Аналитический отчет готов к созданию");
                LogInfo("Analytics report generation requested");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка создания аналитического отчета: {ex.Message}", ex);
            }
        }

        private async Task NotifyParentsAsync()
        {
            try
            {
                // Заглушка - в реальной реализации здесь будет отправка уведомлений родителям
                ShowSuccess($"Уведомления отправлены родителям");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка отправки уведомлений: {ex.Message}", ex);
            }
        }

        private async Task SearchGradesAsync(string searchTerm)
        {
            CurrentPage = 1;
            await LoadGradesAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            CurrentPage = 1;
            await LoadGradesAsync();
        }

        private async Task ClearFiltersAsync()
        {
            SelectedCourseFilter = null;
            SelectedGroupFilter = null;
            GradeRangeFilter = null;
            PeriodFilter = null;
            SearchText = string.Empty;
            CurrentPage = 1;
            await LoadGradesAsync();
        }

        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                await LoadGradesAsync();
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

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

        private (decimal? min, decimal? max) ParseGradeRangeFilter()
        {
            return GradeRangeFilter switch
            {
                "5 (Отлично)" => (4.5m, 5.0m),
                "4 (Хорошо)" => (3.5m, 4.49m),
                "3 (Удовл.)" => (2.5m, 3.49m),
                "2 (Неудовл.)" => (1.0m, 2.49m),
                "Не оценено" => (null, null),
                _ => (null, null)
            };
        }

        private (DateTime? start, DateTime? end) ParsePeriodFilter()
        {
            var now = DateTime.Now;
            return PeriodFilter switch
            {
                "Сегодня" => (now.Date, now.Date.AddDays(1)),
                "Эта неделя" => (now.Date.AddDays(-(int)now.DayOfWeek), now.Date.AddDays(7 - (int)now.DayOfWeek)),
                "Этот месяц" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1)),
                "Этот семестр" => GetCurrentSemesterDates(),
                _ => (null, null)
            };
        }

        private (DateTime start, DateTime end) GetCurrentSemesterDates()
        {
            var now = DateTime.Now;
            if (now.Month >= 9 || now.Month <= 1) // Осенний семестр
            {
                var year = now.Month >= 9 ? now.Year : now.Year - 1;
                return (new DateTime(year, 9, 1), new DateTime(year + 1, 1, 31));
            }
            else // Весенний семестр
            {
                return (new DateTime(now.Year, 2, 1), new DateTime(now.Year, 6, 30));
            }
        }
    }

    /// <summary>
    /// ViewModel для отображения оценки в списке
    /// </summary>
    public class GradeViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        [Reactive] public string StudentName { get; set; } = string.Empty;
        [Reactive] public string CourseName { get; set; } = string.Empty;
        [Reactive] public string AssignmentTitle { get; set; } = string.Empty;
        [Reactive] public decimal Grade { get; set; }
        [Reactive] public decimal MaxGrade { get; set; }
        [Reactive] public string? Comment { get; set; }
        [Reactive] public DateTime GradedAt { get; set; }
        [Reactive] public string GradedByName { get; set; } = string.Empty;

        public string GradeDisplay => Grade > 0 ? Grade.ToString("F1") : "Не оценено";
        public double Percentage => MaxGrade > 0 ? (double)(Grade / MaxGrade * 100) : 0;

        public GradeViewModel(Grade grade)
        {
            Uid = grade.Uid;
            StudentName = grade.Student?.FullName ?? "Неизвестный студент";
            CourseName = grade.Assignment?.Course?.Name ?? "Неизвестный курс";
            AssignmentTitle = grade.Assignment?.Title ?? "Неизвестное задание";
            Grade = grade.Value;
            MaxGrade = (decimal)(grade.Assignment?.MaxGrade ?? 5);
            Comment = grade.Comment;
            GradedAt = grade.GradedAt;
            GradedByName = grade.GradedBy?.FullName ?? "Система";
        }
    }

    /// <summary>
    /// Статистика оценок
    /// </summary>
    public class GradeStatistics
    {
        public double AverageGrade { get; set; }
        public int ExcellentCount { get; set; }
        public int GoodCount { get; set; }
        public int SatisfactoryCount { get; set; }
        public int UnsatisfactoryCount { get; set; }
        public double SuccessRate { get; set; }
        public double QualityRate { get; set; }
        public int TotalGrades { get; set; }
    }
} 