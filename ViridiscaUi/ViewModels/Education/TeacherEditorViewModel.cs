using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для редактирования преподавателя
    /// </summary>
    public class TeacherEditorViewModel : ViewModelBase
    {
        [Reactive] public string FirstName { get; set; } = string.Empty;
        [Reactive] public string LastName { get; set; } = string.Empty;
        [Reactive] public string? MiddleName { get; set; }
        [Reactive] public string Email { get; set; } = string.Empty;
        [Reactive] public string? PhoneNumber { get; set; }
        [Reactive] public string? Specialization { get; set; }
        [Reactive] public string? AcademicTitle { get; set; }
        [Reactive] public string? AcademicDegree { get; set; }
        [Reactive] public decimal? HourlyRate { get; set; }
        [Reactive] public string? Bio { get; set; }
        [Reactive] public TeacherStatus Status { get; set; } = TeacherStatus.Active;
        [Reactive] public DateTime HireDate { get; set; } = DateTime.Today;

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }
        [ObservableAsProperty] public bool CanSave { get; }

        public string Title => Teacher == null ? "Добавить преподавателя" : "Редактировать преподавателя";

        private Teacher? Teacher { get; set; }
        public Teacher? Result { get; set; }

        public ReactiveCommand<Unit, Teacher?> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        // Предопределенные специализации
        public ObservableCollection<string> Specializations { get; } = new()
        {
            "Программирование",
            "Веб-разработка",
            "Базы данных",
            "Математика",
            "Физика",
            "Иностранные языки",
            "Гуманитарные науки",
            "Экономика",
            "Менеджмент",
            "Информационные технологии",
            "Кибербезопасность"
        };

        // Предопределенные академические звания
        public ObservableCollection<string> AcademicTitles { get; } = new()
        {
            "Ассистент",
            "Преподаватель",
            "Старший преподаватель",
            "Доцент",
            "Профессор"
        };

        // Предопределенные академические степени
        public ObservableCollection<string> AcademicDegrees { get; } = new()
        {
            "Бакалавр",
            "Магистр",
            "Кандидат наук",
            "Доктор наук"
        };

        public TeacherEditorViewModel(Teacher? teacher = null)
        {
            Teacher = teacher;

            // Инициализация полей из модели
            if (teacher != null)
            {
                FirstName = teacher.FirstName;
                LastName = teacher.LastName;
                MiddleName = teacher.MiddleName;
                Email = teacher.Email;
                PhoneNumber = teacher.PhoneNumber;
                Specialization = teacher.Specialization;
                AcademicTitle = teacher.AcademicTitle;
                AcademicDegree = teacher.AcademicDegree;
                HourlyRate = teacher.HourlyRate;
                Bio = teacher.Bio;
                Status = teacher.Status;
                HireDate = teacher.HireDate;
            }

            // Команды
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, this.WhenAnyValue(x => x.CanSave));
            CancelCommand = ReactiveCommand.Create(() => { });

            InitializeCommands();
        }

        /// <summary>
        /// Инициализирует команды
        /// </summary>
        private void InitializeCommands()
        {
            // Validation
            var canSave = this.WhenAnyValue(
                x => x.FirstName,
                x => x.LastName,
                x => x.Email,
                x => x.HireDate,
                (firstName, lastName, email, hireDate) =>
                    !string.IsNullOrWhiteSpace(firstName) &&
                    !string.IsNullOrWhiteSpace(lastName) &&
                    !string.IsNullOrWhiteSpace(email) &&
                    email.Contains("@") &&
                    hireDate <= DateTime.Today
            );

            canSave.ToPropertyEx(this, x => x.CanSave);
        }

        private async Task<Teacher?> SaveAsync()
        {
            try
            {
                if (Teacher == null)
                {
                    Teacher = new Teacher
                    {
                        Uid = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow
                    };
                }

                Teacher.FirstName = FirstName;
                Teacher.LastName = LastName;
                Teacher.MiddleName = MiddleName;
                Teacher.Phone = PhoneNumber;
                Teacher.AcademicDegree = AcademicDegree;
                Teacher.AcademicTitle = AcademicTitle;
                Teacher.Specialization = Specialization;
                Teacher.HourlyRate = HourlyRate ?? 0;
                Teacher.Bio = Bio;
                Teacher.HireDate = HireDate;
                Teacher.Status = Status;
                Teacher.LastModifiedAt = DateTime.UtcNow;

                if (Teacher.User == null)
                {
                    Teacher.User = new User
                    {
                        Uid = Teacher.Uid,
                        Email = Email,
                        PhoneNumber = PhoneNumber,
                        FirstName = FirstName,
                        LastName = LastName,
                        MiddleName = MiddleName,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                }
                else
                {
                    Teacher.User.Email = Email;
                    Teacher.User.PhoneNumber = PhoneNumber;
                    Teacher.User.FirstName = FirstName;
                    Teacher.User.LastName = LastName;
                    Teacher.User.MiddleName = MiddleName;
                    Teacher.User.LastModifiedAt = DateTime.UtcNow;
                }

                Result = Teacher;
                return Teacher;
            }
            catch (Exception ex)
            {
                LogError(ex, "Ошибка при сохранении преподавателя");
                return null;
            }
        }
    }
} 