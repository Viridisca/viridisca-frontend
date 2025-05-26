using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;
using NotificationType = ViridiscaUi.Domain.Models.System.NotificationType;
using static ViridiscaUi.Services.Interfaces.IGroupService;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления группами
    /// </summary>
    public class GroupsViewModel : RoutableViewModelBase
    {
        private readonly IGroupService _groupService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        public override string UrlPathSegment => "groups";

        // === СВОЙСТВА ===
        
        [Reactive] public ObservableCollection<GroupViewModel> Groups { get; set; } = new();
        [Reactive] public GroupViewModel? SelectedGroup { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public GroupStatistics? SelectedGroupStatistics { get; set; }
        
        // Пагинация
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int PageSize { get; set; } = 20;
        [Reactive] public int TotalPages { get; set; }
        [Reactive] public int TotalGroups { get; set; }

        // Computed properties for UI binding
        public bool HasSelectedGroup => SelectedGroup != null;
        public bool HasSelectedGroupStatistics => SelectedGroupStatistics != null;

        // === КОМАНДЫ ===
        
        public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> EditGroupCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> DeleteGroupCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> ViewGroupDetailsCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> LoadGroupStatisticsCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> AssignCuratorCommand { get; }
        public ReactiveCommand<GroupViewModel, Unit> ManageStudentsCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<int, Unit> GoToPageCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        public GroupsViewModel(
            IScreen hostScreen,
            IGroupService groupService,
            IStudentService studentService,
            ITeacherService teacherService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService) : base(hostScreen)
        {
            _groupService = groupService;
            _studentService = studentService;
            _teacherService = teacherService;
            _dialogService = dialogService;
            _statusService = statusService;
            _notificationService = notificationService;

            // === ИНИЦИАЛИЗАЦИЯ КОМАНД ===

            LoadGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
            CreateGroupCommand = ReactiveCommand.CreateFromTask(CreateGroupAsync);
            EditGroupCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(EditGroupAsync);
            DeleteGroupCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(DeleteGroupAsync);
            ViewGroupDetailsCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(ViewGroupDetailsAsync);
            LoadGroupStatisticsCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(LoadGroupStatisticsAsync);
            AssignCuratorCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(AssignCuratorAsync);
            ManageStudentsCommand = ReactiveCommand.CreateFromTask<GroupViewModel>(ManageStudentsAsync);
            SearchCommand = ReactiveCommand.CreateFromTask<string>(SearchGroupsAsync);
            GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
            NextPageCommand = ReactiveCommand.CreateFromTask(NextPageAsync, this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total));
            PreviousPageCommand = ReactiveCommand.CreateFromTask(PreviousPageAsync, this.WhenAnyValue(x => x.CurrentPage, current => current > 1));

            // === ПОДПИСКИ ===

            // Автопоиск при изменении текста поиска
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand);

            // Загрузка статистики при выборе группы
            this.WhenAnyValue(x => x.SelectedGroup)
                .Where(group => group != null)
                .Select(group => group!)
                .InvokeCommand(LoadGroupStatisticsCommand);

            // Уведомления об изменении computed properties
            this.WhenAnyValue(x => x.SelectedGroup)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroup)));
                
            this.WhenAnyValue(x => x.SelectedGroupStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroupStatistics)));

            // Первоначальная загрузка
            LoadGroupsCommand.Execute().Subscribe();
        }

        // === МЕТОДЫ КОМАНД ===

        private async Task LoadGroupsAsync()
        {
            try
            {
                IsLoading = true;
                _statusService.ShowInfo("Загрузка групп...", "Группы");

                var (groups, totalCount) = await _groupService.GetGroupsPagedAsync(CurrentPage, PageSize, SearchText);
                
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(new GroupViewModel(group));
                }

                TotalGroups = totalCount;
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                _statusService.ShowSuccess($"Загружено {Groups.Count} групп", "Группы");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки групп: {ex.Message}", "Группы");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadGroupsAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task CreateGroupAsync()
        {
            try
            {
                var newGroup = new Group
                {
                    Uid = Guid.NewGuid(),
                    Name = string.Empty,
                    Description = string.Empty
                };

                var dialogResult = await _dialogService.ShowGroupEditDialogAsync(newGroup);
                if (dialogResult == null) return;

                var createdGroup = await _groupService.CreateGroupAsync(dialogResult);
                Groups.Add(new GroupViewModel(createdGroup));

                _statusService.ShowSuccess($"Группа '{createdGroup.Name}' создана", "Группы");
                
                // Уведомление куратору, если он назначен
                if (createdGroup.CuratorUid.HasValue)
                {
                    await _notificationService.CreateNotificationAsync(
                        createdGroup.CuratorUid.Value,
                        "Назначение куратором",
                        $"Вы назначены куратором группы '{createdGroup.Name}'",
                        NotificationType.Info);
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания группы: {ex.Message}", "Группы");
            }
        }

        private async Task EditGroupAsync(GroupViewModel groupViewModel)
        {
            try
            {
                var dialogResult = await _dialogService.ShowGroupEditDialogAsync(groupViewModel.ToGroup());
                if (dialogResult == null) return;

                var updatedGroup = await _groupService.UpdateGroupAsync(dialogResult);
                var index = Groups.IndexOf(groupViewModel);
                if (index >= 0)
                {
                    Groups[index] = new GroupViewModel(updatedGroup);
                }

                _statusService.ShowSuccess($"Группа '{updatedGroup.Name}' обновлена", "Группы");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка обновления группы: {ex.Message}", "Группы");
            }
        }

        private async Task DeleteGroupAsync(GroupViewModel groupViewModel)
        {
            try
            {
                var confirmResult = await _dialogService.ShowConfirmationAsync(
                    "Удаление группы",
                    $"Вы уверены, что хотите удалить группу '{groupViewModel.Name}'?");

                if (!confirmResult) return;

                await _groupService.DeleteGroupAsync(groupViewModel.Uid);
                Groups.Remove(groupViewModel);

                _statusService.ShowSuccess($"Группа '{groupViewModel.Name}' удалена", "Группы");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка удаления группы: {ex.Message}", "Группы");
            }
        }

        private async Task ViewGroupDetailsAsync(GroupViewModel groupViewModel)
        {
            try
            {
                SelectedGroup = groupViewModel;
                await LoadGroupStatisticsAsync(groupViewModel);
                
                // Здесь можно добавить навигацию к детальному представлению группы
                _statusService.ShowInfo($"Просмотр группы '{groupViewModel.Name}'", "Группы");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отображения деталей группы: {ex.Message}", "Группы");
            }
        }

        private async Task LoadGroupStatisticsAsync(GroupViewModel groupViewModel)
        {
            try
            {
                SelectedGroupStatistics = await _groupService.GetGroupStatisticsAsync(groupViewModel.Uid);
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить статистику группы: {ex.Message}", "Группы");
            }
        }

        private async Task AssignCuratorAsync(GroupViewModel groupViewModel)
        {
            try
            {
                var teachers = await _teacherService.GetTeachersAsync();
                var selectedTeacher = await _dialogService.ShowTeacherSelectionDialogAsync(teachers);
                
                if (selectedTeacher == null) return;

                var success = await _groupService.AssignCuratorAsync(groupViewModel.Uid, selectedTeacher.Uid);
                if (success)
                {
                    groupViewModel.CuratorName = $"{selectedTeacher.FirstName} {selectedTeacher.LastName}";
                    _statusService.ShowSuccess($"Куратор назначен группе '{groupViewModel.Name}'", "Группы");
                    
                    // Уведомление новому куратору
                    await _notificationService.CreateNotificationAsync(
                        selectedTeacher.UserUid,
                        "Назначение куратором",
                        $"Вы назначены куратором группы '{groupViewModel.Name}'",
                        NotificationType.Info);
                }
                else
                {
                    _statusService.ShowError("Не удалось назначить куратора", "Группы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка назначения куратора: {ex.Message}", "Группы");
            }
        }

        private async Task ManageStudentsAsync(GroupViewModel groupViewModel)
        {
            try
            {
                var allStudents = await _studentService.GetStudentsAsync();
                var groupStudents = allStudents.Where(s => s.GroupUid == groupViewModel.Uid).ToList();
                
                var result = await _dialogService.ShowGroupStudentsManagementDialogAsync(groupViewModel.ToGroup(), allStudents);
                if (result != null)
                {
                    await RefreshAsync();
                    _statusService.ShowSuccess($"Состав группы '{groupViewModel.Name}' обновлен", "Группы");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка управления студентами: {ex.Message}", "Группы");
            }
        }

        private async Task SearchGroupsAsync(string searchText)
        {
            SearchText = searchText;
            CurrentPage = 1;
            await LoadGroupsAsync();
        }

        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                await LoadGroupsAsync();
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
    }

    /// <summary>
    /// ViewModel для отображения группы в списке
    /// </summary>
    public class GroupViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string? Description { get; set; }
        [Reactive] public string? CuratorName { get; set; }
        [Reactive] public int StudentsCount { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }
        [Reactive] public DateTime LastModifiedAt { get; set; }
        [Reactive] public DateTime LastActivityDate { get; set; }

        public GroupViewModel(Group group)
        {
            Uid = group.Uid;
            Name = group.Name;
            Description = group.Description;
            CuratorName = group.Curator != null ? $"{group.Curator.FirstName} {group.Curator.LastName}" : null;
            StudentsCount = group.Students?.Count ?? 0;
            CreatedAt = group.CreatedAt;
            LastModifiedAt = group.LastModifiedAt ?? DateTime.UtcNow;
            LastActivityDate = group.LastActivityDate ?? DateTime.MinValue;
        }

        public Group ToGroup()
        {
            return new Group
            {
                Uid = Uid,
                Name = Name,
                Description = Description,
                CreatedAt = CreatedAt,
                LastModifiedAt = LastModifiedAt,
                LastActivityDate = LastActivityDate
            };
        }
    }
} 