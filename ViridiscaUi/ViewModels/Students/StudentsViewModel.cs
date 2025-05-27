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
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Infrastructure.Navigation;

namespace ViridiscaUi.ViewModels.Students
{
    /// <summary>
    /// ViewModel для управления студентами
    /// </summary>
    [Route("students", DisplayName = "Студенты", IconKey = "Student", Order = 3, Group = "Education")]
    public class StudentsViewModel : RoutableViewModelBase
    { 
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly SourceList<StudentViewModel> _studentsSource;

        #region Properties

        [ObservableAsProperty] public bool IsLoading { get; }
        [Reactive] public string SearchTerm { get; set; } = string.Empty;
        [Reactive] public StudentViewModel? SelectedStudent { get; set; }
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int TotalPages { get; set; } = 1;

        /// <summary>
        /// Коллекция студентов для отображения
        /// </summary>
        public ReadOnlyObservableCollection<StudentViewModel> Students { get; private set; } = null!;

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> AddStudentCommand { get; private set; } = null!;
        public ReactiveCommand<StudentViewModel, Unit> EditStudentCommand { get; private set; } = null!;
        public ReactiveCommand<StudentViewModel, Unit> DeleteStudentCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
        public ReactiveCommand<StudentViewModel, Unit> ViewStudentDetailsCommand { get; private set; } = null!;
        public ReactiveCommand<StudentViewModel, Unit> LoadStudentStatisticsCommand { get; private set; } = null!;
        public ReactiveCommand<StudentViewModel, Unit> AssignToGroupCommand { get; private set; } = null!;
        public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
        public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;

        #endregion

        public StudentsViewModel(
            IScreen hostScreen,
            IStudentService studentService,
            IGroupService groupService,
            IDialogService dialogService,
            IStatusService statusService) : base(hostScreen)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
            
            _studentsSource = new SourceList<StudentViewModel>();

            SetupReactiveCollections();
            InitializeCommands();
            
            LogInfo("StudentsViewModel initialized");
        }

        #region Private Methods

        private void SetupReactiveCollections()
        {
            // Setup reactive collections with search filtering
            var searchFilter = this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Select(CreateSearchPredicate);

            _studentsSource.Connect()
                .Filter(searchFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var students)
                .DisposeMany()
                .Subscribe()
                .DisposeWith(Disposables);

            Students = students;
        }

        private void InitializeCommands()
        {
            // Используем стандартизированные методы создания команд из ViewModelBase
            LoadStudentsCommand = CreateCommand(LoadStudentsAsync, null, "Ошибка загрузки студентов");
            RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
            AddStudentCommand = CreateCommand(CreateStudentAsync, null, "Ошибка создания студента");
            EditStudentCommand = CreateCommand<StudentViewModel>(EditStudentAsync, null, "Ошибка редактирования студента");
            DeleteStudentCommand = CreateCommand<StudentViewModel>(DeleteStudentAsync, null, "Ошибка удаления студента");
            ViewStudentDetailsCommand = CreateCommand<StudentViewModel>(ViewStudentDetailsAsync, null, "Ошибка просмотра деталей студента");
            LoadStudentStatisticsCommand = CreateCommand<StudentViewModel>(LoadStudentStatisticsAsync, null, "Ошибка загрузки статистики студента");
            AssignToGroupCommand = CreateCommand<StudentViewModel>(AssignToGroupAsync, null, "Ошибка назначения в группу");
            SearchCommand = CreateCommand<string>(SearchStudentsAsync, null, "Ошибка поиска студентов");
            GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
            
            var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
            var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
            
            NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
            PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        }

        private bool HasPermission(Domain.Models.Auth.User? user, string permission)
        {
            // Simplified permission check - can be enhanced with proper permission system
            if (user?.Role?.Name == "Administrator") return true;
            if (user?.Role?.Name == "Teacher") return permission != "CanDeleteStudent";
            return false;
        }

        private Func<StudentViewModel, bool> CreateSearchPredicate(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _ => true;

            var term = searchTerm.ToLowerInvariant();
            return student =>
                student.FullName.ToLowerInvariant().Contains(term) ||
                (student.Email?.ToLowerInvariant().Contains(term) ?? false) ||
                (student.GroupName?.ToLowerInvariant().Contains(term) ?? false);
        }

        private async Task LoadStudentsAsync()
        {
            LogInfo("Loading students");
            var students = await _studentService.GetAllStudentsAsync();
            var studentViewModels = students.Select(s => new StudentViewModel(s)).ToList();
            
            _studentsSource.Clear();
            _studentsSource.AddRange(studentViewModels);
            
            LogInfo("Loaded {StudentCount} students", students.Count());
            ShowInfo($"Загружено студентов: {students.Count()}");
        }

        private async Task RefreshAsync()
        {
            await LoadStudentsAsync();
        }

        private async Task CreateStudentAsync()
        {
            LogInfo("Creating new student");
            var editorViewModel = new StudentEditorViewModel(_groupService);
            var result = await _dialogService.ShowDialogAsync<Student>(editorViewModel);
            
            if (result != null)
            {
                await _studentService.AddStudentAsync(result);
                _studentsSource.Add(new StudentViewModel(result));
                
                ShowSuccess($"Студент '{result.FirstName} {result.LastName}' создан");
                LogInfo("Student created successfully: {StudentName}", $"{result.FirstName} {result.LastName}");
            }
            else
            {
                LogDebug("Student creation cancelled by user");
            }
        }

        private async Task EditStudentAsync(StudentViewModel studentViewModel)
        {
            LogInfo("Editing student: {StudentId}", studentViewModel.Uid);
            
            var student = await _studentService.GetStudentAsync(studentViewModel.Uid);
            if (student == null)
            {
                ShowError("Студент не найден");
                return;
            }

            var editorViewModel = new StudentEditorViewModel(_groupService, student);
            var result = await _dialogService.ShowDialogAsync<Student>(editorViewModel);
            
            if (result != null)
            {
                var success = await _studentService.UpdateStudentAsync(result);
                if (success)
                {
                    var index = _studentsSource.Items.ToList().FindIndex(s => s.Uid == studentViewModel.Uid);
                    if (index >= 0)
                    {
                        _studentsSource.RemoveAt(index);
                        _studentsSource.Insert(index, new StudentViewModel(result));
                    }
                    
                    ShowSuccess($"Студент '{result.FirstName} {result.LastName}' обновлен");
                    LogInfo("Student updated successfully: {StudentName}", $"{result.FirstName} {result.LastName}");
                }
                else
                {
                    ShowError("Не удалось обновить студента");
                }
            }
            else
            {
                LogDebug("Student editing cancelled by user");
            }
        }

        private async Task DeleteStudentAsync(StudentViewModel studentViewModel)
        {
            LogInfo("Deleting student: {StudentId}", studentViewModel.Uid);
            
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Удаление студента",
                $"Вы уверены, что хотите удалить студента '{studentViewModel.FullName}'?\nВсе связанные данные будут удалены.");

            if (!confirmResult)
            {
                LogDebug("Student deletion cancelled by user");
                return;
            }

            var success = await _studentService.DeleteStudentAsync(studentViewModel.Uid);
            if (success)
            {
                _studentsSource.Remove(studentViewModel);
                ShowSuccess($"Студент '{studentViewModel.FullName}' удален");
                LogInfo("Student deleted successfully: {StudentName}", studentViewModel.FullName);
            }
            else
            {
                ShowError("Не удалось удалить студента");
            }
        }

        private async Task ViewStudentDetailsAsync(StudentViewModel studentViewModel)
        {
            LogInfo("Viewing student details: {StudentId}", studentViewModel.Uid);
            
            SelectedStudent = studentViewModel;
            await LoadStudentStatisticsAsync(studentViewModel);
            
            ShowInfo($"Просмотр студента '{studentViewModel.FullName}'");
        }

        private async Task LoadStudentStatisticsAsync(StudentViewModel studentViewModel)
        {
            try
            {
                // Load student statistics here
                LogInfo("Student statistics loaded for: {StudentName}", studentViewModel.FullName);
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить статистику студента: {ex.Message}");
                LogError(ex, "Failed to load student statistics for: {StudentName}", studentViewModel.FullName);
            }
        }

        private async Task AssignToGroupAsync(StudentViewModel studentViewModel)
        {
            LogInfo("Assigning student to group: {StudentId}", studentViewModel.Uid);
            
            var groups = await _groupService.GetAllGroupsAsync();
            var selectedGroup = await _dialogService.ShowGroupSelectionDialogAsync(groups);
            
            if (selectedGroup == null)
            {
                LogDebug("Group assignment cancelled by user");
                return;
            }

            var success = await _studentService.AssignToGroupAsync(studentViewModel.Uid, selectedGroup.Uid);
            if (success)
            {
                studentViewModel.GroupName = selectedGroup.Name;
                ShowSuccess($"Студент '{studentViewModel.FullName}' назначен в группу '{selectedGroup.Name}'");
                LogInfo("Student assigned to group {GroupName}: {StudentName}", selectedGroup.Name, studentViewModel.FullName);
            }
            else
            {
                ShowError("Не удалось назначить студента в группу");
            }
        }

        private async Task SearchStudentsAsync(string searchTerm)
        {
            SearchTerm = searchTerm;
            CurrentPage = 1;
            await LoadStudentsAsync();
        }

        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                await LoadStudentsAsync();
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

        #endregion

        #region Lifecycle Methods

        protected override async Task OnFirstTimeLoadedAsync()
        {
            await base.OnFirstTimeLoadedAsync();
            LogInfo("StudentsViewModel loaded for the first time");
            
            // Load students when view is loaded for the first time
            await LoadStudentsAsync();
        }

        #endregion

        public override void Dispose()
        {
            LogDebug("Disposing StudentsViewModel");
            _studentsSource?.Dispose();
            base.Dispose();
        }
    }
} 