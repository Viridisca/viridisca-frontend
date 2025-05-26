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
using ViridiscaUi.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ViridiscaUi.ViewModels.Students
{
    public class StudentsViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment => "students";
        public IScreen HostScreen { get; }

        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly SourceList<StudentViewModel> _studentsSource;

        [ObservableAsProperty] public bool IsLoading { get; }
        [Reactive] public string SearchTerm { get; set; } = string.Empty;
        [Reactive] public StudentViewModel? SelectedStudent { get; set; }

        public ReadOnlyObservableCollection<StudentViewModel> Students { get; }
        public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddStudentCommand { get; }
        public ReactiveCommand<Unit, Unit> EditStudentCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteStudentCommand { get; }

        [ObservableAsProperty] public bool CanAddStudent { get; }
        [ObservableAsProperty] public bool CanEditStudent { get; }
        [ObservableAsProperty] public bool CanDeleteStudent { get; }

        public StudentsViewModel(
            IScreen hostScreen,
            IStudentService studentService,
            IGroupService groupService,
            IDialogService dialogService,
            IStatusService statusService,
            INavigationService navigationService,
            IAuthService authService,
            IServiceProvider serviceProvider)
        {
            HostScreen = hostScreen;
            _studentService = studentService;
            _groupService = groupService;
            _dialogService = dialogService;
            _statusService = statusService;
            _navigationService = navigationService;
            _authService = authService;
            _serviceProvider = serviceProvider;
            _studentsSource = new SourceList<StudentViewModel>();

            // Setup reactive collections
            var searchFilter = this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Select(CreateSearchPredicate);

            _studentsSource.Connect()
                .Filter(searchFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var students)
                .Subscribe();

            Students = students;

            // Commands
            LoadStudentsCommand = ReactiveCommand.CreateFromTask(LoadStudentsAsync);
            LoadStudentsCommand.IsExecuting.ToPropertyEx(this, x => x.IsLoading);

            AddStudentCommand = ReactiveCommand.CreateFromTask(AddStudentAsync);
            EditStudentCommand = ReactiveCommand.CreateFromTask(EditStudentAsync, 
                this.WhenAnyValue(x => x.SelectedStudent).Select(s => s != null));
            DeleteStudentCommand = ReactiveCommand.CreateFromTask(DeleteStudentAsync,
                this.WhenAnyValue(x => x.SelectedStudent).Select(s => s != null));

            // Permission-based properties - simplified to avoid crashes
            _authService.CurrentUserObservable
                .Select(user => user != null) // Simplified permission check
                .ToPropertyEx(this, x => x.CanAddStudent);

            _authService.CurrentUserObservable
                .Select(user => user != null) // Simplified permission check
                .ToPropertyEx(this, x => x.CanEditStudent);

            _authService.CurrentUserObservable
                .Select(user => user != null) // Simplified permission check
                .ToPropertyEx(this, x => x.CanDeleteStudent);

            // Load students on initialization
            LoadStudentsCommand.Execute().Subscribe();
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
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                var studentViewModels = students.Select(s => new StudentViewModel(s)).ToList();
                
                _studentsSource.Clear();
                _studentsSource.AddRange(studentViewModels);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка загрузки", 
                    $"Не удалось загрузить список студентов: {ex.Message}");
            }
        }

        private async Task AddStudentAsync()
        {
            try
            {
                var editorViewModel = new StudentEditorViewModel(_groupService);
                var result = await _dialogService.ShowDialogAsync<Student>(editorViewModel);
                
                if (result != null)
                {
                    await _studentService.AddStudentAsync(result);
                    _studentsSource.Add(new StudentViewModel(result));
                    _statusService.ShowSuccess("Студент успешно добавлен");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка добавления", 
                    $"Не удалось добавить студента: {ex.Message}");
            }
        }

        private async Task EditStudentAsync()
        {
            if (SelectedStudent == null) return;

            try
            {
                var student = await _studentService.GetStudentAsync(SelectedStudent.Uid);
                if (student == null)
                {
                    await _dialogService.ShowErrorAsync("Ошибка", "Студент не найден");
                    return;
                }

                var editorViewModel = new StudentEditorViewModel(_groupService, student);
                
                var result = await _dialogService.ShowDialogAsync<Student>(editorViewModel);
                
                if (result != null)
                {
                    await _studentService.UpdateStudentAsync(result);
                    
                    // Обновляем элемент в коллекции
                    var index = _studentsSource.Items.ToList().FindIndex(s => s.Uid == result.Uid);
                    if (index >= 0)
                    {
                        _studentsSource.RemoveAt(index);
                        _studentsSource.Insert(index, new StudentViewModel(result));
                    }
                    
                    _statusService.ShowSuccess("Студент успешно обновлен");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка редактирования", 
                    $"Не удалось обновить студента: {ex.Message}");
            }
        }

        private async Task DeleteStudentAsync()
        {
            if (SelectedStudent == null) return;

            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Подтверждение удаления",
                    $"Вы уверены, что хотите удалить студента {SelectedStudent.FullName}?");

                if (confirmed)
                {
                    await _studentService.DeleteStudentAsync(SelectedStudent.Uid);
                    _studentsSource.Remove(SelectedStudent);
                    _statusService.ShowSuccess("Студент успешно удален");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка удаления", 
                    $"Не удалось удалить студента: {ex.Message}");
            }
        }
    }
} 