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

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для редактирования оценки
    /// </summary>
    public class GradeEditorViewModel : ViewModelBase
    {
        private readonly IStudentService _studentService;
        private readonly IAssignmentService _assignmentService;
        private readonly ITeacherService _teacherService;
        private readonly SourceCache<Student, Guid> _studentsSource = new(s => s.Uid);
        private readonly SourceCache<Assignment, Guid> _assignmentsSource = new(a => a.Uid);
        private readonly SourceCache<Teacher, Guid> _teachersSource = new(t => t.Uid);
        private ReadOnlyObservableCollection<Student> _students;
        private ReadOnlyObservableCollection<Assignment> _assignments;
        private ReadOnlyObservableCollection<Teacher> _teachers;

        public ReadOnlyObservableCollection<Student> Students => _students;
        public ReadOnlyObservableCollection<Assignment> Assignments => _assignments;
        public ReadOnlyObservableCollection<Teacher> Teachers => _teachers;

        [Reactive] public decimal Value { get; set; } = 0;
        [Reactive] public string? Comment { get; set; }
        [Reactive] public DateTime GradedAt { get; set; } = DateTime.Now;
        [Reactive] public Guid StudentUid { get; set; }
        [Reactive] public Guid AssignmentUid { get; set; }
        [Reactive] public Guid TeacherUid { get; set; }
        [Reactive] public Student? SelectedStudent { get; set; }
        [Reactive] public Assignment? SelectedAssignment { get; set; }
        [Reactive] public Teacher? SelectedTeacher { get; set; }

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }
        [ObservableAsProperty] public string LetterGrade { get; }

        public string Title => Grade == null ? "Добавить оценку" : "Редактировать оценку";

        public Grade Grade { get; set; }

        public ReactiveCommand<Unit, Grade?> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        public GradeEditorViewModel(
            IStudentService studentService,
            IAssignmentService assignmentService,
            ITeacherService teacherService,
            Grade? grade = null)
        {
            _studentService = studentService;
            _assignmentService = assignmentService;
            _teacherService = teacherService;
            Grade = grade;

            // Инициализация из существующей оценки
            if (grade != null)
            {
                Value = grade.Value;
                Comment = grade.Comment ?? string.Empty;
                SelectedStudent = Students.FirstOrDefault(s => s.Uid == grade.StudentUid);
                SelectedAssignment = Assignments.FirstOrDefault(a => a.Uid == grade.AssignmentUid);
                SelectedTeacher = Teachers.FirstOrDefault(t => t.Uid == grade.TeacherUid);
            }

            // Команды
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            CancelCommand = ReactiveCommand.Create(() => { });

            // Вычисляемые свойства
            this.WhenAnyValue(x => x.Value, value => GetLetterGrade(value))
                .ToPropertyEx(this, x => x.LetterGrade);

            InitializeCommands();
            SetupSubscriptions();
            
            // Load data
            _ = LoadDataAsync();
        }

        /// <summary>
        /// Инициализирует команды
        /// </summary>
        private void InitializeCommands()
        {
            // Validation
            var canSave = this.WhenAnyValue(
                x => x.Value,
                x => x.StudentUid,
                x => x.AssignmentUid,
                x => x.TeacherUid,
                (value, studentUid, assignmentUid, teacherUid) =>
                    value >= 0 &&
                    studentUid != Guid.Empty &&
                    assignmentUid != Guid.Empty &&
                    teacherUid != Guid.Empty
            );

            canSave.ToPropertyEx(this, x => x.IsValid);
        }

        /// <summary>
        /// Настраивает подписки на изменения свойств
        /// </summary>
        private void SetupSubscriptions()
        {
            // Sync selected items with Uids
            this.WhenAnyValue(x => x.SelectedStudent)
                .Subscribe(student => StudentUid = student?.Uid ?? Guid.Empty);

            this.WhenAnyValue(x => x.SelectedAssignment)
                .Subscribe(assignment => 
                {
                    AssignmentUid = assignment?.Uid ?? Guid.Empty;
                });

            this.WhenAnyValue(x => x.SelectedTeacher)
                .Subscribe(teacher => TeacherUid = teacher?.Uid ?? Guid.Empty);

            // Bind collections
            _studentsSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _students);

            _assignmentsSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _assignments);

            _teachersSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _teachers);
        }

        /// <summary>
        /// Загружает данные для выпадающих списков
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                _studentsSource.AddOrUpdate(students);

                var assignments = await _assignmentService.GetAllAssignmentsAsync();
                _assignmentsSource.AddOrUpdate(assignments);

                var teachers = await _teacherService.GetAllTeachersAsync();
                _teachersSource.AddOrUpdate(teachers);
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при загрузке данных");
            }
        }

        private async Task<Grade?> SaveAsync()
        {
            try
            {
                if (Grade == null)
                {
                    Grade = new Grade();
                }

                Grade.Value = Value;
                Grade.Comment = Comment;
                Grade.StudentUid = SelectedStudent?.Uid ?? Guid.Empty;
                Grade.AssignmentUid = SelectedAssignment?.Uid;
                Grade.TeacherUid = SelectedTeacher?.Uid ?? Guid.Empty;
                Grade.GradedAt = DateTime.UtcNow;

                return Grade;
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при сохранении оценки");
                return null;
            }
        }

        /// <summary>
        /// Преобразует оценку в буквенную
        /// </summary>
        private static string GetLetterGrade(decimal value)
        {
            return value switch
            {
                >= 4.5m => "A",
                >= 3.5m => "B", 
                >= 2.5m => "C",
                >= 1.5m => "D",
                _ => "F"
            };
        }
    }
} 