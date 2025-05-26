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
    /// ViewModel для управления оценками
    /// </summary>
    public class GradesViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment => "grades";
        public IScreen HostScreen { get; }

        private readonly IGradeService _gradeService;
        private readonly ICourseService _courseService;
        private readonly IGroupService _groupService;
        private readonly IStudentService _studentService;
        private readonly IAssignmentService _assignmentService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;
        private readonly IExportService _exportService;

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
        
        public ReactiveCommand<Unit, Unit> LoadGradesCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateGradeCommand { get; }
        public ReactiveCommand<GradeViewModel, Unit> EditGradeCommand { get; }
        public ReactiveCommand<GradeViewModel, Unit> DeleteGradeCommand { get; }
        public ReactiveCommand<GradeViewModel, Unit> ViewGradeDetailsCommand { get; }
        public ReactiveCommand<GradeViewModel, Unit> AddCommentCommand { get; }
        public ReactiveCommand<Unit, Unit> BulkGradingCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportReportCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportToExcelCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateAnalyticsReportCommand { get; }
        public ReactiveCommand<Unit, Unit> NotifyParentsCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
        public ReactiveCommand<int, Unit> GoToPageCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public GradesViewModel(
            IGradeService gradeService,
            ICourseService courseService,
            IGroupService groupService,
            IStudentService studentService,
            IAssignmentService assignmentService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService,
            IExportService exportService,
            IScreen hostScreen)
        {
            _gradeService = gradeService;
            _courseService = courseService;
            _groupService = groupService;
            _studentService = studentService;
            _assignmentService = assignmentService;
            _dialogService = dialogService;
            _statusService = statusService;
            _notificationService = notificationService;
            _exportService = exportService;
            HostScreen = hostScreen;

            // === ИНИЦИАЛИЗАЦИЯ КОМАНД ===

            LoadGradesCommand = ReactiveCommand.CreateFromTask(LoadGradesAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
            CreateGradeCommand = ReactiveCommand.CreateFromTask(CreateGradeAsync);
            EditGradeCommand = ReactiveCommand.CreateFromTask<GradeViewModel>(EditGradeAsync);
            DeleteGradeCommand = ReactiveCommand.CreateFromTask<GradeViewModel>(DeleteGradeAsync);
            ViewGradeDetailsCommand = ReactiveCommand.CreateFromTask<GradeViewModel>(ViewGradeDetailsAsync);
            AddCommentCommand = ReactiveCommand.CreateFromTask<GradeViewModel>(AddCommentAsync);
            BulkGradingCommand = ReactiveCommand.CreateFromTask(BulkGradingAsync);
            ExportReportCommand = ReactiveCommand.CreateFromTask(ExportReportAsync);
            ExportToExcelCommand = ReactiveCommand.CreateFromTask(ExportToExcelAsync);
            GenerateAnalyticsReportCommand = ReactiveCommand.CreateFromTask(GenerateAnalyticsReportAsync);
            NotifyParentsCommand = ReactiveCommand.CreateFromTask(NotifyParentsAsync);
            SearchCommand = ReactiveCommand.CreateFromTask<string>(SearchGradesAsync);
            ApplyFiltersCommand = ReactiveCommand.CreateFromTask(ApplyFiltersAsync);
            ClearFiltersCommand = ReactiveCommand.CreateFromTask(ClearFiltersAsync);
            GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
            NextPageCommand = ReactiveCommand.CreateFromTask(() => NextPageAsync(), this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total));
            PreviousPageCommand = ReactiveCommand.CreateFromTask(() => PreviousPageAsync(), this.WhenAnyValue(x => x.CurrentPage, current => current > 1));

            // === ПОДПИСКИ ===

            // Автоматический поиск при изменении текста
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand);

            // Обновление computed properties
            this.WhenAnyValue(x => x.SelectedGrade)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGrade)));

            this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages)
                .Subscribe(_ => 
                {
                    this.RaisePropertyChanged(nameof(CanGoToPreviousPage));
                    this.RaisePropertyChanged(nameof(CanGoToNextPage));
                });

            // Автоматическое применение фильтров
            this.WhenAnyValue(x => x.SelectedCourseFilter, x => x.SelectedGroupFilter, x => x.GradeRangeFilter, x => x.PeriodFilter)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => Unit.Default)
                .InvokeCommand(ApplyFiltersCommand);

            // Инициализация
            InitializeAsync();
        }

        // === МЕТОДЫ ИНИЦИАЛИЗАЦИИ ===

        private async void InitializeAsync()
        {
            await LoadFiltersDataAsync();
            await LoadGradesAsync();
        }

        private async Task LoadFiltersDataAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var groups = await _groupService.GetAllGroupsAsync();

                Courses.Clear();
                Groups.Clear();

                foreach (var course in courses)
                    Courses.Add(course);

                foreach (var group in groups)
                    Groups.Add(group);
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки данных фильтров: {ex.Message}", "Оценки");
            }
        }

        // === МЕТОДЫ КОМАНД ===

        private async Task LoadGradesAsync()
        {
            try
            {
                IsLoading = true;
                
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

                _statusService.ShowSuccess($"Загружено {grades.Count()} оценок", "Оценки");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки оценок: {ex.Message}", "Оценки");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки статистики: {ex.Message}", "Оценки");
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadGradesAsync();
                _statusService.ShowSuccess("Данные обновлены", "Оценки");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка обновления: {ex.Message}", "Оценки");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task CreateGradeAsync()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                var assignments = await _assignmentService.GetAllAssignmentsAsync();
                
                var result = await _dialogService.ShowGradeEditDialogAsync(new Grade(), students, assignments);
                if (result != null)
                {
                    await _gradeService.AddGradeAsync(result);
                    await RefreshAsync();
                    _statusService.ShowSuccess("Оценка добавлена", "Оценки");
                    
                    await _notificationService.SendNotificationAsync(
                        "Новая оценка",
                        $"Добавлена оценка {result.Value} для студента {result.Student?.FullName}",
                        Domain.Models.System.NotificationType.Info);
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания оценки: {ex.Message}", "Оценки");
            }
        }

        private async Task EditGradeAsync(GradeViewModel gradeViewModel)
        {
            try
            {
                var grade = await _gradeService.GetGradeAsync(gradeViewModel.Uid);
                if (grade == null)
                {
                    _statusService.ShowError("Оценка не найдена", "Оценки");
                    return;
                }

                var students = await _studentService.GetAllStudentsAsync();
                var assignments = await _assignmentService.GetAllAssignmentsAsync();

                var result = await _dialogService.ShowGradeEditDialogAsync(grade, students, assignments);
                if (result != null)
                {
                    var success = await _gradeService.UpdateGradeAsync(result);
                    if (success)
                    {
                        await RefreshAsync();
                        _statusService.ShowSuccess("Оценка обновлена", "Оценки");
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось обновить оценку", "Оценки");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка редактирования оценки: {ex.Message}", "Оценки");
            }
        }

        private async Task DeleteGradeAsync(GradeViewModel gradeViewModel)
        {
            try
            {
                var confirmation = await _dialogService.ShowConfirmationDialogAsync(
                    "Удаление оценки",
                    $"Вы уверены, что хотите удалить оценку {gradeViewModel.GradeDisplay} для студента {gradeViewModel.StudentName}?\n\nЭто действие нельзя отменить.",
                    "Удалить",
                    "Отмена");

                if (confirmation)
                {
                    var success = await _gradeService.DeleteGradeAsync(gradeViewModel.Uid);
                    if (success)
                    {
                        await RefreshAsync();
                        _statusService.ShowSuccess("Оценка удалена", "Оценки");
                        
                        await _notificationService.SendNotificationAsync(
                            "Оценка удалена",
                            $"Удалена оценка {gradeViewModel.GradeDisplay} для студента {gradeViewModel.StudentName}",
                            Domain.Models.System.NotificationType.Warning);
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось удалить оценку", "Оценки");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка удаления оценки: {ex.Message}", "Оценки");
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
                _statusService.ShowError($"Ошибка загрузки деталей оценки: {ex.Message}", "Оценки");
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
                        _statusService.ShowSuccess("Комментарий добавлен", "Оценки");
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось добавить комментарий", "Оценки");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка добавления комментария: {ex.Message}", "Оценки");
            }
        }

        private async Task BulkGradingAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var assignments = await _assignmentService.GetAllAssignmentsAsync();
                
                var result = await _dialogService.ShowBulkGradingDialogAsync(courses, assignments);
                if (result != null)
                {
                    var success = await _gradeService.BulkAddGradesAsync(result);
                    if (success)
                    {
                        await RefreshAsync();
                        var count = result.Count();
                        _statusService.ShowSuccess($"Добавлено {count} оценок", "Массовое оценивание");
                        
                        await _notificationService.SendNotificationAsync(
                            "Массовое оценивание",
                            $"Добавлено {count} оценок",
                            Domain.Models.System.NotificationType.Info);
                    }
                    else
                    {
                        _statusService.ShowError("Не удалось выполнить массовое оценивание", "Оценки");
                    }
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка массового оценивания: {ex.Message}", "Оценки");
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

                var filePath = await _exportService.ExportGradesToPdfAsync(grades, "Отчет по оценкам");
                if (!string.IsNullOrEmpty(filePath))
                {
                    _statusService.ShowSuccess($"Отчет экспортирован: {filePath}", "Экспорт");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка экспорта отчета: {ex.Message}", "Экспорт");
            }
        }

        private async Task ExportToExcelAsync()
        {
            try
            {
                var grades = await _gradeService.GetAllGradesAsync(
                    SelectedCourseFilter?.Uid,
                    SelectedGroupFilter?.Uid,
                    ParseGradeRangeFilter(),
                    ParsePeriodFilter());

                var filePath = await _exportService.ExportGradesToExcelAsync(grades, "Оценки");
                if (!string.IsNullOrEmpty(filePath))
                {
                    _statusService.ShowSuccess($"Данные экспортированы в Excel: {filePath}", "Экспорт");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка экспорта в Excel: {ex.Message}", "Экспорт");
            }
        }

        private async Task GenerateAnalyticsReportAsync()
        {
            try
            {
                var analytics = await _gradeService.GenerateAnalyticsReportAsync(
                    SelectedCourseFilter?.Uid,
                    SelectedGroupFilter?.Uid,
                    ParsePeriodFilter());

                var filePath = await _exportService.ExportAnalyticsReportAsync(analytics, "Аналитический отчет по оценкам");
                if (!string.IsNullOrEmpty(filePath))
                {
                    _statusService.ShowSuccess($"Аналитический отчет создан: {filePath}", "Аналитика");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания аналитического отчета: {ex.Message}", "Аналитика");
            }
        }

        private async Task NotifyParentsAsync()
        {
            try
            {
                var recentGrades = await _gradeService.GetRecentGradesAsync(7); // За последние 7 дней
                await _notificationService.NotifyParentsAboutGradesAsync(recentGrades);
                
                _statusService.ShowSuccess($"Уведомления отправлены родителям", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отправки уведомлений: {ex.Message}", "Уведомления");
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