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
using System.Collections.Generic;
using DynamicData;
using DynamicData.Binding;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for managing students with full CRUD functionality
/// Follows SOLID principles and clean architecture with ReactiveUI
/// </summary>
[Route("students", 
    DisplayName = "Студенты", 
    IconKey = "AccountGroup", 
    Order = 1,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление студентами и их данными")]
public class StudentsViewModel : RoutableViewModelBase
{
    private readonly IStudentService _studentService;
    private readonly IGroupService _groupService;
    private readonly IDialogService _dialogService;
    private readonly IStatusService _statusService;
    private readonly IExportService _exportService;
    private readonly IImportService _importService;

    // Source collections for reactive data management
    private readonly SourceList<StudentViewModel> _studentsSource = new();
    private readonly SourceList<Group> _groupsSource = new();
    private ReadOnlyObservableCollection<StudentViewModel> _students;
    private ReadOnlyObservableCollection<Group> _groups;

    // === PROPERTIES ===
    
    public ReadOnlyObservableCollection<StudentViewModel> Students => _students;
    public ReadOnlyObservableCollection<Group> Groups => _groups;
    
    [Reactive] public StudentViewModel? SelectedStudent { get; set; }
    [Reactive] public ObservableCollection<StudentViewModel> SelectedStudents { get; set; } = new();
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    
    // Enhanced Filters
    [Reactive] public Group? GroupFilter { get; set; }
    [Reactive] public StudentStatus? StatusFilter { get; set; }
    [Reactive] public string? YearFilter { get; set; }
    
    // Pagination
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 25;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalStudents { get; set; }
    [Reactive] public int ActiveStudents { get; set; }

    // Computed properties using ObservableAsProperty
    [ObservableAsProperty] public bool HasSelectedStudent { get; }
    [ObservableAsProperty] public bool HasSelectedStudents { get; }
    [ObservableAsProperty] public int SelectedStudentsCount { get; }
    [ObservableAsProperty] public bool CanGoToPreviousPage { get; }
    [ObservableAsProperty] public bool CanGoToNextPage { get; }
    [ObservableAsProperty] public bool CanGoToFirstPage { get; }
    [ObservableAsProperty] public bool CanGoToLastPage { get; }
    [ObservableAsProperty] public bool HasPages { get; }
    [ObservableAsProperty] public string PaginationInfo { get; }

    // === COMMANDS ===
    
    // Basic CRUD Commands
    public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateStudentCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> EditStudentCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> DeleteStudentCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> ViewStudentDetailsCommand { get; private set; } = null!;
    
    // Search and Filter Commands
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    
    // Pagination Commands
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; } = null!;
    
    // Navigation Commands
    public ReactiveCommand<StudentViewModel, Unit> ViewCoursesCommand { get; private set; } = null!;
    public ReactiveCommand<StudentViewModel, Unit> ViewGradesCommand { get; private set; } = null!;
    
    // Import/Export Commands
    public ReactiveCommand<Unit, Unit> ImportStudentsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportReportCommand { get; private set; } = null!;
    
    // Bulk Operations Commands
    public ReactiveCommand<Unit, Unit> BulkEditCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkDeleteCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkExportCommand { get; private set; } = null!;
    
    // Selection Commands
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> DeselectAllCommand { get; private set; } = null!;

    /// <summary>
    /// Constructor with enhanced dependency injection
    /// </summary>
    public StudentsViewModel(
        IScreen hostScreen,
        IUnifiedNavigationService navigationService,
        IStudentService studentService,
        IGroupService groupService,
        IDialogService dialogService,
        IStatusService statusService,
        IExportService exportService,
        IImportService importService) : base(hostScreen)
    {
        var _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));

        SetupReactiveCollections();
        InitializeCommands();
        SetupComputedProperties();
        SetupSubscriptions();
        
        LogInfo("Enhanced StudentsViewModel initialized with full CRUD functionality");
    }

    #region Reactive Collections Setup

    /// <summary>
    /// Sets up reactive collections with filtering and sorting
    /// </summary>
    private void SetupReactiveCollections()
    {
        // Setup students collection with advanced filtering
        var studentsFilter = this.WhenAnyValue(
                x => x.SearchText,
                x => x.GroupFilter,
                x => x.StatusFilter,
                x => x.YearFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Select(CreateStudentFilter);

        _studentsSource.Connect()
            .Filter(studentsFilter)
            .Sort(SortExpressionComparer<StudentViewModel>.Ascending(s => s.LastName)
                .ThenByAscending(s => s.FirstName))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _students)
            .Subscribe()
            .DisposeWith(Disposables);

        // Setup groups collection
        _groupsSource.Connect()
            .Sort(SortExpressionComparer<Group>.Ascending(g => g.Name))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _groups)
            .Subscribe()
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Creates a filter function for students based on current filter criteria
    /// </summary>
    private Func<StudentViewModel, bool> CreateStudentFilter((string? searchText, Group? groupFilter, StudentStatus? statusFilter, string? yearFilter) criteria)
    {
        return student =>
        {
            // Search text filter
            if (!string.IsNullOrWhiteSpace(criteria.searchText))
            {
                var searchLower = criteria.searchText.ToLowerInvariant();
                if (!student.FullName.ToLowerInvariant().Contains(searchLower) &&
                    !student.Email.ToLowerInvariant().Contains(searchLower) &&
                    !student.StudentCode.ToLowerInvariant().Contains(searchLower) &&
                    !(student.GroupName?.ToLowerInvariant().Contains(searchLower) ?? false))
                {
                    return false;
                }
            }

            // Group filter
            if (criteria.groupFilter != null && student.GroupUid != criteria.groupFilter.Uid)
            {
                return false;
            }

            // Status filter
            if (criteria.statusFilter.HasValue && student.Status != criteria.statusFilter.Value)
            {
                return false;
            }

            // Year filter
            if (!string.IsNullOrWhiteSpace(criteria.yearFilter) && criteria.yearFilter != "Все")
            {
                var yearNumber = criteria.yearFilter.Replace(" курс", "").Trim();
                if (student.AcademicYear.ToString() != yearNumber)
                {
                    return false;
                }
            }

            return true;
        };
    }

    #endregion

    #region Commands Initialization

    /// <summary>
    /// Initializes all reactive commands with proper error handling
    /// </summary>
    private void InitializeCommands()
    {
        // Basic CRUD Commands
        LoadStudentsCommand = CreateCommand(LoadStudentsAsync, null, "Ошибка загрузки студентов");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        CreateStudentCommand = CreateCommand(CreateStudentAsync, null, "Ошибка создания студента");
        EditStudentCommand = CreateCommand<StudentViewModel>(EditStudentAsync, null, "Ошибка редактирования студента");
        DeleteStudentCommand = CreateCommand<StudentViewModel>(DeleteStudentAsync, null, "Ошибка удаления студента");
        ViewStudentDetailsCommand = CreateCommand<StudentViewModel>(ViewStudentDetailsAsync, null, "Ошибка просмотра деталей студента");

        // Search and Filter Commands
        SearchCommand = CreateCommand<string>(SearchStudentsAsync, null, "Ошибка поиска студентов");
        ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync, null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");

        // Pagination Commands
        GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        FirstPageCommand = CreateCommand(FirstPageAsync, canGoPrevious, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(LastPageAsync, canGoNext, "Ошибка перехода на последнюю страницу");

        // Navigation Commands
        ViewCoursesCommand = CreateCommand<StudentViewModel>(ViewCoursesAsync, null, "Ошибка просмотра курсов студента");
        ViewGradesCommand = CreateCommand<StudentViewModel>(ViewGradesAsync, null, "Ошибка просмотра оценок студента");

        // Import/Export Commands
        ImportStudentsCommand = CreateCommand(ImportStudentsAsync, null, "Ошибка импорта студентов");
        ExportReportCommand = CreateCommand(ExportReportAsync, null, "Ошибка экспорта отчета");

        // Bulk Operations Commands
        var hasSelectedStudents = this.WhenAnyValue(x => x.SelectedStudentsCount, count => count > 0);
        BulkEditCommand = CreateCommand(BulkEditAsync, hasSelectedStudents, "Ошибка массового редактирования");
        BulkDeleteCommand = CreateCommand(BulkDeleteAsync, hasSelectedStudents, "Ошибка массового удаления");
        BulkExportCommand = CreateCommand(BulkExportAsync, hasSelectedStudents, "Ошибка экспорта выбранных студентов");

        // Selection Commands
        var hasStudents = this.WhenAnyValue(x => x.Students.Count, count => count > 0);
        SelectAllCommand = CreateCommand(SelectAllAsync, hasStudents, "Ошибка выбора всех студентов");
        DeselectAllCommand = CreateCommand(DeselectAllAsync, hasSelectedStudents, "Ошибка снятия выделения");
    }

    #endregion

    #region Computed Properties Setup

    /// <summary>
    /// Sets up computed properties using ObservableAsProperty pattern
    /// </summary>
    private void SetupComputedProperties()
    {
        // Selection properties
        this.WhenAnyValue(x => x.SelectedStudent)
            .Select(student => student != null)
            .ToPropertyEx(this, x => x.HasSelectedStudent)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.SelectedStudents.Count)
            .Select(count => count > 0)
            .ToPropertyEx(this, x => x.HasSelectedStudents)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.SelectedStudents.Count)
            .ToPropertyEx(this, x => x.SelectedStudentsCount)
            .DisposeWith(Disposables);

        // Pagination properties
        this.WhenAnyValue(x => x.CurrentPage)
            .Select(page => page > 1)
            .ToPropertyEx(this, x => x.CanGoToPreviousPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total)
            .ToPropertyEx(this, x => x.CanGoToNextPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage)
            .Select(page => page > 1)
            .ToPropertyEx(this, x => x.CanGoToFirstPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total)
            .ToPropertyEx(this, x => x.CanGoToLastPage)
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.TotalPages)
            .Select(pages => pages > 1)
            .ToPropertyEx(this, x => x.HasPages)
            .DisposeWith(Disposables);

        // Pagination info
        this.WhenAnyValue(x => x.Students.Count, x => x.TotalStudents, (shown, total) => 
                $"Показано {shown} из {total} студентов")
            .ToPropertyEx(this, x => x.PaginationInfo)
            .DisposeWith(Disposables);
    }

    #endregion

    #region Subscriptions Setup

    /// <summary>
    /// Sets up reactive subscriptions for automatic data updates
    /// </summary>
    private void SetupSubscriptions()
    {
        // Auto-search when search text changes
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(searchText => searchText?.Trim() ?? string.Empty)
            .DistinctUntilChanged()
            .Where(_ => !IsLoading)
            .Subscribe(async searchText =>
            {
                try
                {
                    await SearchStudentsAsync(searchText);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка автоматического поиска студентов");
                    ShowError("Ошибка поиска студентов");
                }
            })
            .DisposeWith(Disposables);

        // Auto-apply filters when filter criteria change
        this.WhenAnyValue(x => x.GroupFilter, x => x.StatusFilter, x => x.YearFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(_ => !IsLoading)
            .Subscribe(async _ =>
            {
                try
                {
                    await ApplyFiltersAsync();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка автоматического применения фильтров");
                    ShowError("Ошибка применения фильтров");
                }
            })
            .DisposeWith(Disposables);

        // Update statistics when students collection changes
        this.WhenAnyValue(x => x.Students.Count)
            .Subscribe(count =>
            {
                TotalStudents = count;
                ActiveStudents = Students.Count(s => s.Status == StudentStatus.Active);
            })
            .DisposeWith(Disposables);

        // Handle selection changes
        SelectedStudents.ToObservableChangeSet()
            .Subscribe(_ =>
            {
                // Update selection state in individual student view models
                foreach (var student in Students)
                {
                    student.IsSelected = SelectedStudents.Contains(student);
                }
            })
            .DisposeWith(Disposables);
    }

    #endregion

    #region CRUD Operations

    /// <summary>
    /// Loads students with pagination and filtering
    /// </summary>
    private async Task LoadStudentsAsync()
    {
        try
        {
            IsLoading = true;
            LogInfo("Loading students...");
            
            // Load groups first for filtering
            await LoadGroupsAsync();
            
            // Load students with pagination
            var (students, totalCount) = await _studentService.GetPagedAsync(
                CurrentPage, PageSize, SearchText);
            
            // Convert to ViewModels
            var studentViewModels = students.Select(s => new StudentViewModel(s)).ToList();
            
            // Update collections
            _studentsSource.Clear();
            _studentsSource.AddRange(studentViewModels);
            
            // Update pagination info
            TotalStudents = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
            ActiveStudents = studentViewModels.Count(s => s.Status == StudentStatus.Active);
            
            LogInfo($"Loaded {studentViewModels.Count} students (page {CurrentPage} of {TotalPages})");
            ShowSuccess($"Загружено {studentViewModels.Count} студентов");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load students");
            ShowError("Ошибка загрузки студентов");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads groups for filtering
    /// </summary>
    private async Task LoadGroupsAsync()
    {
        try
        {
            var groups = await _groupService.GetAllAsync();
            _groupsSource.Clear();
            _groupsSource.AddRange(groups);
            LogInfo($"Loaded {groups.Count()} groups for filtering");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load groups");
        }
    }

    /// <summary>
    /// Refreshes all data
    /// </summary>
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadStudentsAsync();
            ShowSuccess("Данные обновлены");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Creates a new student
    /// </summary>
    private async Task CreateStudentAsync()
    {
        try
        {
            LogInfo("Opening create student dialog");
            
            var result = await _dialogService.ShowStudentEditorDialogAsync(null);
            if (result != null)
            {
                var createdStudent = await _studentService.CreateAsync(result);
                var studentViewModel = new StudentViewModel(createdStudent);
                
                _studentsSource.Add(studentViewModel);
                SelectedStudent = studentViewModel;
                
                ShowSuccess($"Студент {createdStudent.FullName} успешно создан");
                LogInfo($"Created student: {createdStudent.FullName}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create student");
            ShowError("Ошибка при создании студента");
        }
    }

    /// <summary>
    /// Edits an existing student
    /// </summary>
    private async Task EditStudentAsync(StudentViewModel studentViewModel)
    {
        try
        {
            LogInfo($"Opening edit dialog for student: {studentViewModel.FullName}");
            
            var student = await _studentService.GetByUidAsync(studentViewModel.Uid);
            if (student == null)
            {
                ShowError("Студент не найден");
                return;
            }
            
            var result = await _dialogService.ShowStudentEditDialogAsync(student);
            if (result != null)
            {
                var success = await _studentService.UpdateAsync(result);
                if (success)
                {
                    // Update the view model
                    var index = _studentsSource.Items.ToList().FindIndex(s => s.Uid == studentViewModel.Uid);
                    if (index >= 0)
                    {
                        var updatedViewModel = new StudentViewModel(result);
                        _studentsSource.RemoveAt(index);
                        _studentsSource.Insert(index, updatedViewModel);
                        SelectedStudent = updatedViewModel;
                    }
                    
                    ShowSuccess($"Студент {result.FullName} успешно обновлен");
                    LogInfo($"Updated student: {result.FullName}");
                }
                else
                {
                    ShowError("Ошибка при обновлении студента");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to edit student: {studentViewModel.FullName}");
            ShowError("Ошибка при редактировании студента");
        }
    }

    /// <summary>
    /// Deletes a student with confirmation
    /// </summary>
    private async Task DeleteStudentAsync(StudentViewModel studentViewModel)
    {
        try
        {
            var confirmed = await _dialogService.ShowConfirmationDialogAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить студента {studentViewModel.FullName}?\n\nЭто действие нельзя отменить.",
                "Удалить",
                "Отмена");
                
            if (confirmed)
            {
                var success = await _studentService.DeleteAsync(studentViewModel.Uid);
                if (success)
                {
                    _studentsSource.Remove(studentViewModel);
                    if (SelectedStudent == studentViewModel)
                    {
                        SelectedStudent = null;
                    }
                    
                    ShowSuccess($"Студент {studentViewModel.FullName} успешно удален");
                    LogInfo($"Deleted student: {studentViewModel.FullName}");
                }
                else
                {
                    ShowError("Ошибка при удалении студента");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to delete student: {studentViewModel.FullName}");
            ShowError("Ошибка при удалении студента");
        }
    }

    /// <summary>
    /// Shows detailed view of a student
    /// </summary>
    private async Task ViewStudentDetailsAsync(StudentViewModel studentViewModel)
    {
        try
        {
            LogInfo($"Viewing details for student: {studentViewModel.FullName}");
            
            var student = await _studentService.GetByUidAsync(studentViewModel.Uid);
            if (student != null)
            {
                await _dialogService.ShowStudentDetailsDialogAsync(student);
            }
            else
            {
                ShowError("Студент не найден");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to view student details: {studentViewModel.FullName}");
            ShowError("Ошибка при просмотре деталей студента");
        }
    }

    #endregion

    #region Search and Filtering

    /// <summary>
    /// Performs search with the given search text
    /// </summary>
    private async Task SearchStudentsAsync(string searchText)
    {
        try
        {
            LogInfo($"Searching students with text: '{searchText}'");
            
            // Reset to first page when searching
            CurrentPage = 1;
            
            // Reload data with search filter
            await LoadStudentsAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to search students with text: '{searchText}'");
            ShowError("Ошибка поиска студентов");
        }
    }

    /// <summary>
    /// Applies current filter criteria
    /// </summary>
    private async Task ApplyFiltersAsync()
    {
        try
        {
            LogInfo("Applying filters to students");
            
            // Reset to first page when applying filters
            CurrentPage = 1;
            
            // Reload data with filters
            await LoadStudentsAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to apply filters");
            ShowError("Ошибка применения фильтров");
        }
    }

    /// <summary>
    /// Clears all filters and search criteria
    /// </summary>
    private async Task ClearFiltersAsync()
    {
        try
        {
            LogInfo("Clearing all filters");
            
            SearchText = string.Empty;
            GroupFilter = null;
            StatusFilter = null;
            YearFilter = null;
            CurrentPage = 1;
            
            await LoadStudentsAsync();
            ShowSuccess("Фильтры очищены");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to clear filters");
            ShowError("Ошибка очистки фильтров");
        }
    }

    #endregion

    #region Pagination

    /// <summary>
    /// Navigates to a specific page
    /// </summary>
    private async Task GoToPageAsync(int page)
    {
        if (page >= 1 && page <= TotalPages && page != CurrentPage)
        {
            CurrentPage = page;
            await LoadStudentsAsync();
        }
    }

    /// <summary>
    /// Navigates to the next page
    /// </summary>
    private async Task NextPageAsync()
    {
        await GoToPageAsync(CurrentPage + 1);
    }

    /// <summary>
    /// Navigates to the previous page
    /// </summary>
    private async Task PreviousPageAsync()
    {
        await GoToPageAsync(CurrentPage - 1);
    }

    /// <summary>
    /// Navigates to the first page
    /// </summary>
    private async Task FirstPageAsync()
    {
        await GoToPageAsync(1);
    }

    /// <summary>
    /// Navigates to the last page
    /// </summary>
    private async Task LastPageAsync()
    {
        await GoToPageAsync(TotalPages);
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Navigates to courses view for the selected student
    /// </summary>
    private async Task ViewCoursesAsync(StudentViewModel studentViewModel)
    {
        try
        {
            LogInfo($"Navigating to courses for student: {studentViewModel.FullName}");
            await NavigateToAsync($"courses?studentId={studentViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to navigate to courses for student: {studentViewModel.FullName}");
            ShowError("Ошибка навигации к курсам студента");
        }
    }

    /// <summary>
    /// Navigates to grades view for the selected student
    /// </summary>
    private async Task ViewGradesAsync(StudentViewModel studentViewModel)
    {
        try
        {
            LogInfo($"Navigating to grades for student: {studentViewModel.FullName}");
            await NavigateToAsync($"grades?studentId={studentViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to navigate to grades for student: {studentViewModel.FullName}");
            ShowError("Ошибка навигации к оценкам студента");
        }
    }

    #endregion

    #region Import/Export

    /// <summary>
    /// Imports students from file
    /// </summary>
    private async Task ImportStudentsAsync()
    {
        try
        {
            LogInfo("Starting student import process");
            
            var result = await _dialogService.ShowFileOpenDialogAsync("Импорт студентов", new[] { "*.xlsx", "*.csv" });
            if (result != null)
            {
                var importedCount = await _importService.ImportStudentsAsync(result);
                await RefreshAsync();
                
                ShowSuccess($"Импортировано {importedCount} студентов");
                LogInfo($"Imported {importedCount} students");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to import students");
            ShowError("Ошибка импорта студентов");
        }
    }

    /// <summary>
    /// Exports students report
    /// </summary>
    private async Task ExportReportAsync()
    {
        try
        {
            LogInfo("Starting student export process");
            
            var students = Students.Select(vm => vm.ToStudent()).ToList();
            var filePath = await _exportService.ExportStudentsToExcelAsync(students, "Отчет по студентам");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                ShowSuccess($"Отчет экспортирован: {filePath}");
                LogInfo($"Exported students report to: {filePath}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to export students report");
            ShowError("Ошибка экспорта отчета");
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Performs bulk edit on selected students
    /// </summary>
    private async Task BulkEditAsync()
    {
        try
        {
            LogInfo($"Starting bulk edit for {SelectedStudentsCount} students");
            
            // For now, just show a message that bulk edit is not implemented
            ShowInfo($"Массовое редактирование {SelectedStudentsCount} студентов пока не реализовано");
            
            // TODO: Implement bulk edit dialog and logic
            /*
            var result = await _dialogService.ShowBulkEditDialogAsync(SelectedStudents.ToList());
            if (result != null)
            {
                var updatedCount = 0;
                foreach (var student in SelectedStudents.ToList())
                {
                    // Apply bulk changes
                    var studentEntity = student.ToStudent();
                    // Apply bulk edit properties from result
                    
                    var success = await _studentService.UpdateAsync(studentEntity);
                    if (success) updatedCount++;
                }
                
                await RefreshAsync();
                ShowSuccess($"Обновлено {updatedCount} студентов");
                LogInfo($"Bulk edited {updatedCount} students");
            }
            */
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to perform bulk edit");
            ShowError("Ошибка массового редактирования");
        }
    }

    /// <summary>
    /// Performs bulk delete on selected students
    /// </summary>
    private async Task BulkDeleteAsync()
    {
        try
        {
            var confirmed = await _dialogService.ShowConfirmationDialogAsync(
                "Подтверждение массового удаления",
                $"Вы уверены, что хотите удалить {SelectedStudentsCount} студентов?\n\nЭто действие нельзя отменить.",
                "Удалить всех",
                "Отмена");
                
            if (confirmed)
            {
                LogInfo($"Starting bulk delete for {SelectedStudentsCount} students");
                
                var deletedCount = 0;
                var studentsToDelete = SelectedStudents.ToList();
                
                foreach (var student in studentsToDelete)
                {
                    var success = await _studentService.DeleteAsync(student.Uid);
                    if (success)
                    {
                        _studentsSource.Remove(student);
                        deletedCount++;
                    }
                }
                
                SelectedStudents.Clear();
                ShowSuccess($"Удалено {deletedCount} студентов");
                LogInfo($"Bulk deleted {deletedCount} students");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to perform bulk delete");
            ShowError("Ошибка массового удаления");
        }
    }

    /// <summary>
    /// Exports selected students
    /// </summary>
    private async Task BulkExportAsync()
    {
        try
        {
            LogInfo($"Starting bulk export for {SelectedStudentsCount} students");
            
            var students = SelectedStudents.Select(vm => vm.ToStudent()).ToList();
            var filePath = await _exportService.ExportStudentsToExcelAsync(students, "Отчет по студентам");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                ShowSuccess($"Экспортировано {SelectedStudentsCount} студентов: {filePath}");
                LogInfo($"Bulk exported {SelectedStudentsCount} students to: {filePath}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to perform bulk export");
            ShowError("Ошибка экспорта выбранных студентов");
        }
    }

    #endregion

    #region Selection Management

    /// <summary>
    /// Selects all visible students
    /// </summary>
    private async Task SelectAllAsync()
    {
        try
        {
            SelectedStudents.Clear();
            foreach (var student in Students)
            {
                SelectedStudents.Add(student);
                student.IsSelected = true;
            }
            
            LogInfo($"Selected all {Students.Count} visible students");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to select all students");
            ShowError("Ошибка выбора всех студентов");
        }
    }

    /// <summary>
    /// Deselects all students
    /// </summary>
    private async Task DeselectAllAsync()
    {
        try
        {
            foreach (var student in Students)
            {
                student.IsSelected = false;
            }
            SelectedStudents.Clear();
            
            LogInfo("Deselected all students");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to deselect all students");
            ShowError("Ошибка снятия выделения");
        }
    }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Called when the ViewModel is first loaded
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        try
        {
            LogInfo("StudentsViewModel first time loading...");
            await LoadStudentsAsync();
            LogInfo("StudentsViewModel first time loading completed");
        }
        catch (Exception ex)
        {
            LogError(ex, "Error during first time loading");
            ShowError("Ошибка при первоначальной загрузке данных");
        }
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public override void Dispose()
    {
        _studentsSource?.Dispose();
        _groupsSource?.Dispose();
        base.Dispose();
        LogInfo("StudentsViewModel disposed");
    }

    #endregion
} 