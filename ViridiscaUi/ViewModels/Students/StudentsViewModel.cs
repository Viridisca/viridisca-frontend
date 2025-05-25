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

namespace ViridiscaUi.ViewModels.Students
{
    public class StudentsViewModel : RoutableViewModelBase
    {
        private readonly IScreen _screen;
        private readonly IStudentService _studentService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly SourceList<Student> _studentsSource;

        public override string UrlPathSegment => "students";

        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public string SearchTerm { get; set; } = string.Empty;
        [Reactive] public Student? SelectedStudent { get; set; }

        public ReadOnlyObservableCollection<Student> Students { get; }
        public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddStudentCommand { get; }
        public ReactiveCommand<Unit, Unit> EditStudentCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteStudentCommand { get; }

        [ObservableAsProperty] public bool CanAddStudent { get; }
        [ObservableAsProperty] public bool CanEditStudent { get; }
        [ObservableAsProperty] public bool CanDeleteStudent { get; }

        public StudentsViewModel(
            IScreen screen,
            IStudentService studentService,
            INavigationService navigationService,
            IDialogService dialogService,
            IAuthService authService) : base(screen)
        {
            _screen = screen;
            _studentService = studentService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _authService = authService;
            _studentsSource = new SourceList<Student>();

            // Инициализация коллекции студентов с фильтрацией
            _studentsSource
                .Connect()
                .Filter(this.WhenAnyValue(x => x.SearchTerm)
                    .Select(term => new Func<Student, bool>(student =>
                        string.IsNullOrWhiteSpace(term) ||
                        student.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        student.LastName.Contains(term, StringComparison.OrdinalIgnoreCase))))
                .Bind(out var students)
                .Subscribe();

            Students = students;

            // Инициализация команд
            LoadStudentsCommand = ReactiveCommand.CreateFromTask(LoadStudentsAsync);

            var canAdd = Observable.FromAsync(async () => 
            {
                var user = await _authService.GetCurrentUserAsync();
                return user != null && await _authService.HasPermissionAsync(user.Uid, "Students.Create");
            });

            var canEdit = Observable.FromAsync(async () => 
            {
                var user = await _authService.GetCurrentUserAsync();
                return user != null && await _authService.HasPermissionAsync(user.Uid, "Students.Edit");
            });

            var canDelete = Observable.FromAsync(async () => 
            {
                var user = await _authService.GetCurrentUserAsync();
                return user != null && await _authService.HasPermissionAsync(user.Uid, "Students.Delete");
            });

            canAdd.ToPropertyEx(this, x => x.CanAddStudent);
            canEdit.ToPropertyEx(this, x => x.CanEditStudent);
            canDelete.ToPropertyEx(this, x => x.CanDeleteStudent);

            AddStudentCommand = ReactiveCommand.CreateFromTask(AddStudentAsync, canAdd);
            EditStudentCommand = ReactiveCommand.CreateFromTask(EditStudentAsync, 
                this.WhenAnyValue(x => x.SelectedStudent, x => x.CanEditStudent)
                    .Select(tuple => tuple.Item1 != null && tuple.Item2));
            DeleteStudentCommand = ReactiveCommand.CreateFromTask(DeleteStudentAsync,
                this.WhenAnyValue(x => x.SelectedStudent, x => x.CanDeleteStudent)
                    .Select(tuple => tuple.Item1 != null && tuple.Item2));

            // Загрузка данных при инициализации
            LoadStudentsCommand.Execute().Subscribe();
        }

        private async Task LoadStudentsAsync()
        {
            try
            {
                IsLoading = true;
                var students = await _studentService.GetAllStudentsAsync();
                _studentsSource.Clear();
                _studentsSource.AddRange(students);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка загрузки студентов", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddStudentAsync()
        {
            try
            {
                var student = new Student
                {
                    Uid = Guid.NewGuid(),
                    FirstName = string.Empty,
                    LastName = string.Empty
                };

                var firstName = await _dialogService.ShowInputDialogAsync("Добавление студента", "Введите имя студента:", "");
                if (string.IsNullOrWhiteSpace(firstName)) return;

                var lastName = await _dialogService.ShowInputDialogAsync("Добавление студента", "Введите фамилию студента:", "");
                if (string.IsNullOrWhiteSpace(lastName)) return;

                student.FirstName = firstName;
                student.LastName = lastName;

                await _studentService.AddStudentAsync(student);
                await LoadStudentsAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка добавления студента", ex.Message);
            }
        }

        private async Task EditStudentAsync()
        {
            if (SelectedStudent == null) return;

            try
            {
                var firstName = await _dialogService.ShowInputDialogAsync(
                    "Редактирование студента", 
                    "Введите имя студента:", 
                    SelectedStudent.FirstName);
                if (string.IsNullOrWhiteSpace(firstName)) return;

                var lastName = await _dialogService.ShowInputDialogAsync(
                    "Редактирование студента", 
                    "Введите фамилию студента:", 
                    SelectedStudent.LastName);
                if (string.IsNullOrWhiteSpace(lastName)) return;

                SelectedStudent.FirstName = firstName;
                SelectedStudent.LastName = lastName;

                await _studentService.UpdateStudentAsync(SelectedStudent);
                await LoadStudentsAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка редактирования студента", ex.Message);
            }
        }

        private async Task DeleteStudentAsync()
        {
            if (SelectedStudent == null) return;

            try
            {
                var confirm = await _dialogService.ShowConfirmationAsync(
                    "Подтверждение удаления",
                    $"Вы уверены, что хотите удалить студента {SelectedStudent.FirstName} {SelectedStudent.LastName}?");

                if (confirm)
                {
                    await _studentService.DeleteStudentAsync(SelectedStudent.Uid);
                    await LoadStudentsAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка удаления студента", ex.Message);
            }
        }
    }
} 