using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.ViewModels.Students
{
    /// <summary>
    /// ViewModel для редактирования студента
    /// </summary>
    public class StudentEditorViewModel : ViewModelBase
    {
        private readonly IGroupService _groupService;
        private readonly SourceCache<Group, Guid> _groupsSource = new(g => g.Uid);
        private ReadOnlyObservableCollection<Group> _groups;

        public ReadOnlyObservableCollection<Group> Groups => _groups;

        [Reactive] public string FirstName { get; set; } = string.Empty;
        [Reactive] public string LastName { get; set; } = string.Empty;
        [Reactive] public string? MiddleName { get; set; }
        [Reactive] public string Email { get; set; } = string.Empty;
        [Reactive] public string? PhoneNumber { get; set; }
        [Reactive] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);
        [Reactive] public string StudentNumber { get; set; } = string.Empty;
        [Reactive] public DateTime EnrollmentDate { get; set; } = DateTime.Today;
        [Reactive] public StudentStatus Status { get; set; } = StudentStatus.Active;
        [Reactive] public Group? SelectedGroup { get; set; }
        [Reactive] public string? Address { get; set; }
        [Reactive] public string? EmergencyContact { get; set; }
        [Reactive] public string? EmergencyPhone { get; set; }
        [Reactive] public string? Notes { get; set; }
        [Reactive] public Guid? GroupUid { get; set; }

        public string Title => IsEditing ? $"Редактирование студента: {FirstName} {LastName}" : "Создание нового студента";
        public bool IsEditing { get; }
        public Guid? StudentUid { get; }

        public ReactiveCommand<Unit, Student> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public StudentEditorViewModel(IGroupService groupService, Student? student = null)
        {
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            
            IsEditing = student != null;
            StudentUid = student?.Uid;

            // Настройка источника данных для групп
            _groupsSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _groups)
                .Subscribe();

            // Инициализация данных студента
            if (student != null)
            {
                FirstName = student.FirstName;
                LastName = student.LastName;
                MiddleName = student.MiddleName;
                Email = student.Email;
                PhoneNumber = student.PhoneNumber;
                DateOfBirth = student.BirthDate;
                StudentNumber = student.StudentCode;
                EnrollmentDate = student.EnrollmentDate;
                Status = student.Status;
                Address = student.Address;
                EmergencyContact = student.EmergencyContactName;
                EmergencyPhone = student.EmergencyContactPhone;
                Notes = student.MedicalInformation;
                GroupUid = student.GroupUid;
            }
            else
            {
                // Генерируем номер студента для нового студента
                StudentNumber = GenerateStudentNumber();
            }

            // Команды
            var canSave = this.WhenAnyValue(
                x => x.FirstName,
                x => x.LastName,
                x => x.Email,
                x => x.StudentNumber,
                (firstName, lastName, email, studentNumber) =>
                    !string.IsNullOrWhiteSpace(firstName) &&
                    !string.IsNullOrWhiteSpace(lastName) &&
                    !string.IsNullOrWhiteSpace(email) &&
                    !string.IsNullOrWhiteSpace(studentNumber) &&
                    IsValidEmail(email));

            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, canSave);
            CancelCommand = ReactiveCommand.Create(() => { });

            // Загружаем группы
            LoadGroupsAsync();
        }

        private async Task<Student> SaveAsync()
        {
            var student = new Student
            {
                Uid = StudentUid ?? Guid.NewGuid(),
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
                Email = Email.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                BirthDate = DateOfBirth,
                StudentCode = StudentNumber.Trim(),
                EnrollmentDate = EnrollmentDate,
                Status = Status,
                GroupUid = SelectedGroup?.Uid,
                Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                EmergencyContactName = string.IsNullOrWhiteSpace(EmergencyContact) ? null : EmergencyContact.Trim(),
                EmergencyContactPhone = string.IsNullOrWhiteSpace(EmergencyPhone) ? null : EmergencyPhone.Trim(),
                MedicalInformation = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                LastModifiedAt = DateTime.Now
            };

            if (!IsEditing)
            {
                student.CreatedAt = DateTime.Now;
            }

            return student;
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var groups = await _groupService.GetAllGroupsAsync();
                _groupsSource.AddOrUpdate(groups);

                // Если редактируем существующего студента, находим его группу
                if (IsEditing && StudentUid.HasValue)
                {
                    // Пока просто оставляем группу пустой - в реальной реализации здесь будет поиск группы студента
                    SelectedGroup = null;
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем работу
                Debug.WriteLine($"Error loading groups: {ex.Message}");
            }
        }

        private static string GenerateStudentNumber()
        {
            var year = DateTime.Now.Year;
            var random = new Random();
            var number = random.Next(1000, 9999);
            return $"ST{year}{number}";
        }

        private static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Contains(".");
        }
    }
} 