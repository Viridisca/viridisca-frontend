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
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для редактирования задания
    /// </summary>
    public class AssignmentEditorViewModel : ViewModelBase
    {
        private readonly ICourseService _courseService;
        private readonly SourceCache<Course, Guid> _coursesSource = new(c => c.Uid);
        private ReadOnlyObservableCollection<Course> _courses;

        public ReadOnlyObservableCollection<Course> Courses => _courses;

        [Reactive] public string Title { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public string? Instructions { get; set; }
        [Reactive] public AssignmentType Type { get; set; } = AssignmentType.Homework;
        [Reactive] public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
        [Reactive] public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
        [Reactive] public DateTime? StartDate { get; set; } = DateTime.Today;
        [Reactive] public decimal MaxScore { get; set; } = 100;
        [Reactive] public int? TimeLimit { get; set; }
        [Reactive] public int MaxAttempts { get; set; } = 1;
        [Reactive] public bool AllowLateSubmission { get; set; } = true;
        [Reactive] public decimal? LatePenalty { get; set; }
        [Reactive] public Guid CourseId { get; set; }
        [Reactive] public Course? SelectedCourse { get; set; }
        [Reactive] public string? Resources { get; set; }
        [Reactive] public string? GradingCriteria { get; set; }
        [Reactive] public AssignmentDifficulty Difficulty { get; set; } = AssignmentDifficulty.Easy;

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }
        [ObservableAsProperty] public bool CanSave { get; }

        public string WindowTitle => Assignment == null ? "Добавить задание" : "Редактировать задание";

        private Assignment? Assignment { get; set; }
        public Assignment? Result { get; set; }

        public ReactiveCommand<Unit, Assignment?> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        // Предопределенные типы заданий
        public ObservableCollection<AssignmentType> AssignmentTypes { get; } = new()
        {
            AssignmentType.Homework,
            AssignmentType.Quiz,
            AssignmentType.Exam,
            AssignmentType.Project,
            AssignmentType.LabWork
        };

        public ObservableCollection<AssignmentDifficulty> DifficultyLevels { get; } = new()
        {
            AssignmentDifficulty.Easy,
            AssignmentDifficulty.Medium,
            AssignmentDifficulty.Hard
        };

        public ObservableCollection<AssignmentStatus> StatusOptions { get; } = new()
        {
            AssignmentStatus.Draft,
            AssignmentStatus.Published,
            AssignmentStatus.Closed,
            AssignmentStatus.Archived
        };

        public AssignmentEditorViewModel(ICourseService courseService, Assignment? assignment = null)
        {
            _courseService = courseService;
            Assignment = assignment;

            // Инициализация из существующего задания
            if (assignment != null)
            {
                Title = assignment.Title;
                Description = assignment.Description;
                Instructions = assignment.Instructions;
                DueDate = assignment.DueDate ?? DateTime.Today.AddDays(7);
                MaxScore = (decimal)assignment.MaxScore;
                Type = assignment.Type;
                Difficulty = assignment.Difficulty;
                Status = assignment.Status;
                CourseId = assignment.CourseUid;
                SelectedCourse = Courses.FirstOrDefault(c => c.Uid == assignment.CourseUid);
            }

            // Команды
            SaveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    LoadCourses();
                    
                    if (Assignment.Uid == Guid.Empty)
                    {
                        Assignment.Uid = Guid.NewGuid();
                        Assignment.CreatedAt = DateTime.UtcNow;
                        await _courseService.AddCourseAsync(new Course()); // Заглушка
                    }
                    else
                    {
                        Assignment.LastModifiedAt = DateTime.UtcNow;
                        // await _assignmentService.UpdateAssignmentAsync(Assignment); // Заглушка
                    }
                    
                    return Assignment;
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка при сохранении задания");
                    return null;
                }
            });
            CancelCommand = ReactiveCommand.Create(() => { });

            SetupSubscriptions();
            LoadDataAsync();
        }

        /// <summary>
        /// Настраивает подписки на изменения свойств
        /// </summary>
        private void SetupSubscriptions()
        {
            // Sync CourseId and SelectedCourse
            this.WhenAnyValue(x => x.SelectedCourse)
                .Subscribe(course => CourseId = course?.Uid ?? Guid.Empty);

            // Bind courses to observable collection
            _coursesSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _courses)
                .Subscribe();

            // Auto-set start date when due date changes
            this.WhenAnyValue(x => x.DueDate)
                .Where(dueDate => StartDate == null || StartDate >= dueDate)
                .Subscribe(dueDate => StartDate = dueDate.AddDays(-7));

            // Validation
            var canSave = this.WhenAnyValue(
                x => x.Title,
                x => x.Description,
                x => x.DueDate,
                x => x.MaxScore,
                x => x.SelectedCourse,
                (title, description, dueDate, maxScore, course) =>
                    !string.IsNullOrWhiteSpace(title) &&
                    !string.IsNullOrWhiteSpace(description) &&
                    dueDate > DateTime.Today &&
                    maxScore > 0 &&
                    course != null
            );

            canSave.ToPropertyEx(this, x => x.CanSave);
        }

        /// <summary>
        /// Загружает данные для выпадающих списков
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                await LoadCoursesAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при загрузке курсов");
            }
        }

        /// <summary>
        /// Загружает список курсов
        /// </summary>
        private async Task LoadCoursesAsync()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                _coursesSource.AddOrUpdate(courses);
                
                // Set selected course if editing existing assignment
                if (Assignment?.CourseUid != Guid.Empty)
                {
                    SelectedCourse = courses.FirstOrDefault(c => c.Uid == Assignment.CourseUid);
                }
            }
            catch (Exception ex)
            {
                SetError("Ошибка загрузки курсов", ex);
            }
        }

        private void LoadCourses()
        {
            try
            {
                // Заглушка для загрузки курсов - используем существующий SourceCache
                // _courses уже инициализирован через _coursesSource.Connect().Bind()
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при загрузке курсов");
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                if (Assignment == null)
                {
                    Assignment = new Assignment();
                }

                Assignment.Title = Title;
                Assignment.Description = Description;
                Assignment.Instructions = Instructions;
                Assignment.DueDate = DueDate;
                Assignment.MaxScore = (double)MaxScore;
                Assignment.Type = Type;
                Assignment.Difficulty = Difficulty;
                Assignment.Status = Status;
                Assignment.CourseUid = SelectedCourse?.Uid ?? Guid.Empty;

                Result = Assignment;
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при сохранении задания");
            }
        }
    }
} 