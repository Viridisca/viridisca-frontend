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
    /// ViewModel для редактирования курса
    /// </summary>
    public class CourseEditorViewModel : ViewModelBase
    {
        private readonly ITeacherService _teacherService;
        private readonly SourceCache<Teacher, Guid> _teachersSource = new(t => t.Uid);
        private ReadOnlyObservableCollection<Teacher> _teachers;

        public ReadOnlyObservableCollection<Teacher> Teachers => _teachers;

        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Code { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public string Category { get; set; } = string.Empty;
        [Reactive] public CourseStatus Status { get; set; } = CourseStatus.Draft;
        [Reactive] public DateTime StartDate { get; set; } = DateTime.Today;
        [Reactive] public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);
        [Reactive] public int Credits { get; set; } = 3;
        [Reactive] public Guid? TeacherUid { get; set; }
        [Reactive] public Teacher? SelectedTeacher { get; set; }
        [Reactive] public string? Prerequisites { get; set; }
        [Reactive] public string? LearningOutcomes { get; set; }
        [Reactive] public int MaxEnrollments { get; set; } = 50;

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }
        [ObservableAsProperty] public bool CanSave { get; }

        public string Title => Course == null ? "Добавить курс" : "Редактировать курс";

        private Course? Course { get; set; }
        public Course? Result { get; set; }

        public ReactiveCommand<Unit, Course?> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        // Предопределенные категории курсов
        public ObservableCollection<string> Categories { get; } = new()
        {
            "Программирование",
            "Веб-разработка",
            "Базы данных",
            "Математика",
            "Физика",
            "Иностранные языки",
            "Гуманитарные науки",
            "Экономика",
            "Менеджмент"
        };

        public CourseEditorViewModel(ITeacherService teacherService, Course? course = null)
        {
            _teacherService = teacherService;
            Course = course;

            // Инициализация из существующего курса
            if (course != null)
            {
                Name = course.Name;
                Code = course.Code;
                Description = course.Description;
                Category = course.Category;
                StartDate = course.StartDate ?? DateTime.Today;
                EndDate = course.EndDate ?? DateTime.Today.AddMonths(3);
                Credits = course.Credits;
                Status = course.Status;
                SelectedTeacher = Teachers.FirstOrDefault(t => t.Uid == course.TeacherUid);
            }

            // Команды
            SaveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    LoadTeachers();
                    
                    if (Course.Uid == Guid.Empty)
                    {
                        Course.Uid = Guid.NewGuid();
                        Course.CreatedAt = DateTime.UtcNow;
                        await _teacherService.AddTeacherAsync(new Teacher()); // Заглушка
                    }
                    else
                    {
                        Course.LastModifiedAt = DateTime.UtcNow;
                        // await _courseService.UpdateCourseAsync(Course); // Заглушка
                    }
                    
                    return Course;
                }
                catch (Exception ex)
                {
                    LogError(ex, "Ошибка при сохранении курса");
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
            // Sync TeacherUid and SelectedTeacher
            this.WhenAnyValue(x => x.SelectedTeacher)
                .Subscribe(teacher => TeacherUid = teacher?.Uid);

            // Bind teachers to observable collection
            _teachersSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _teachers)
                .Subscribe();

            // Validation
            var canSave = this.WhenAnyValue(
                x => x.Name,
                x => x.Code,
                x => x.Description,
                x => x.StartDate,
                x => x.EndDate,
                x => x.Credits,
                (name, code, description, startDate, endDate, credits) =>
                    !string.IsNullOrWhiteSpace(name) &&
                    !string.IsNullOrWhiteSpace(code) &&
                    !string.IsNullOrWhiteSpace(description) &&
                    startDate < endDate &&
                    credits > 0
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
                await LoadTeachersAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при загрузке данных");
            }
        }

        /// <summary>
        /// Загружает список преподавателей
        /// </summary>
        private async Task LoadTeachersAsync()
        {
            try
            {
                var teachers = await _teacherService.GetAllTeachersAsync();
                _teachersSource.AddOrUpdate(teachers);
                
                // Set selected teacher if editing existing course
                if (Course?.TeacherUid != null)
                {
                    SelectedTeacher = teachers.FirstOrDefault(t => t.Uid == Course.TeacherUid);
                }
            }
            catch (Exception ex)
            {
                SetError("Ошибка загрузки преподавателей", ex);
            }
        }

        private void LoadTeachers()
        {
            // Implementation of LoadTeachers method
        }
    }
} 