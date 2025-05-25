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
using ViridiscaUi.Services;

namespace ViridiscaUi.ViewModels.Students
{
    public class StudentEditorViewModel : ViewModelBase
    {
        private readonly IGroupService _groupService;
        private readonly SourceList<Group> _groupsSource = new();
        private readonly ReadOnlyObservableCollection<Group> _groups;

        public ReadOnlyObservableCollection<Group> Groups => _groups;

        [Reactive] public string FirstName { get; set; } = string.Empty;
        [Reactive] public string LastName { get; set; } = string.Empty;
        [Reactive] public string? MiddleName { get; set; }
        [Reactive] public string Email { get; set; } = string.Empty;
        [Reactive] public string? PhoneNumber { get; set; }
        [Reactive] public DateTime? BirthDate { get; set; }
        [Reactive] public string? Address { get; set; }
        [Reactive] public Guid? GroupUid { get; set; }
        [Reactive] public Group? SelectedGroup { get; set; }
        [Reactive] public StudentStatus Status { get; set; } = StudentStatus.Active;

        [ObservableAsProperty] public bool IsLoading { get; }
        [ObservableAsProperty] public bool IsValid { get; }

        public string Title => Student == null ? "Добавить студента" : "Редактировать студента";

        private Student? Student { get; }

        public ReactiveCommand<Unit, Student?> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public StudentEditorViewModel(IGroupService groupService, Student? student = null)
        {
            _groupService = groupService;
            Student = student;

            // Initialize properties from existing student if editing
            if (student != null)
            {
                FirstName = student.FirstName;
                LastName = student.LastName;
                MiddleName = student.MiddleName;
                Email = student.Email;
                PhoneNumber = student.PhoneNumber;
                BirthDate = student.BirthDate;
                Address = student.Address;
                GroupUid = student.GroupUid;
                Status = student.Status;
            }

            // Sync GroupUid and SelectedGroup
            this.WhenAnyValue(x => x.SelectedGroup)
                .Subscribe(group => GroupUid = group?.Uid);

            // Load groups
            var loadGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            loadGroupsCommand.IsExecuting.ToPropertyEx(this, x => x.IsLoading);
            loadGroupsCommand.Execute().Subscribe();

            // Bind groups to observable collection
            _groupsSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _groups)
                .Subscribe();

            // Validation
            var canSave = this.WhenAnyValue(
                x => x.FirstName,
                x => x.LastName,
                x => x.Email,
                (firstName, lastName, email) =>
                    !string.IsNullOrWhiteSpace(firstName) &&
                    !string.IsNullOrWhiteSpace(lastName) &&
                    !string.IsNullOrWhiteSpace(email) &&
                    email.Contains("@")
            );

            canSave.ToPropertyEx(this, x => x.IsValid);

            // Commands
            SaveCommand = ReactiveCommand.CreateFromTask(
                SaveAsync,
                canSave
            );

            CancelCommand = ReactiveCommand.Create(() => Unit.Default);
        }

        private async Task LoadGroupsAsync()
        {
            var groups = await _groupService.GetGroupsAsync();
            _groupsSource.Clear();
            _groupsSource.AddRange(groups);
            
            // Set selected group based on GroupUid
            if (GroupUid.HasValue)
            {
                SelectedGroup = Groups.FirstOrDefault(g => g.Uid == GroupUid.Value);
            }
        }

        private async Task<Student?> SaveAsync()
        {
            var student = Student ?? new Student();
            
            student.FirstName = FirstName;
            student.LastName = LastName;
            student.MiddleName = MiddleName ?? string.Empty;
            student.Email = Email;
            student.PhoneNumber = PhoneNumber ?? string.Empty;
            student.BirthDate = BirthDate ?? DateTime.MinValue;
            student.Address = Address ?? string.Empty;
            student.GroupUid = GroupUid;
            student.Status = Status;

            return student;
        }
    }
} 