using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using NotificationType = ViridiscaUi.Domain.Models.System.NotificationType;
using static ViridiscaUi.Services.Interfaces.IGroupService;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для управления группами
    /// Следует принципам SOLID и чистой архитектуры
    /// </summary>
    [Route("groups", DisplayName = "Группы", IconKey = "👥", Order = 3, Group = "Education")]
    public class GroupsViewModel : RoutableViewModelBase
    {
        private readonly IGroupService _groupService;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly INotificationService _notificationService;

        

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
        
        public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; private set; } = null!;
        public ReactiveCommand<GroupViewModel, Unit> EditGroupCommand { get; private set; } = null!;
        public ReactiveCommand<GroupViewModel, Unit> DeleteGroupCommand { get; private set; } = null!;
        public ReactiveCommand<GroupViewModel, Unit> ViewGroupDetailsCommand { get; private set; } = null!;
        public ReactiveCommand<GroupViewModel, Unit> LoadGroupStatisticsCommand { get; private set; } = null!;
        public ReactiveCommand<GroupViewModel, Unit> AssignCuratorCommand { get; private set; } = null!;
        public ReactiveCommand<GroupViewModel, Unit> ManageStudentsCommand { get; private set; } = null!;
        public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
        public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;

        public GroupsViewModel(
            IScreen hostScreen,
            IGroupService groupService,
            IStudentService studentService,
            ITeacherService teacherService,
            IDialogService dialogService,
            IStatusService statusService,
            INotificationService notificationService) : base(hostScreen)
        {
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            InitializeCommands();
            SetupSubscriptions();
            
            LogInfo("GroupsViewModel initialized");
        }

        #region Private Methods

        /// <summary>
        /// Инициализирует команды
        /// </summary>
        private void InitializeCommands()
        {
            // Используем стандартизированные методы создания команд из ViewModelBase
            LoadGroupsCommand = CreateCommand(LoadGroupsAsync, null, "Ошибка загрузки групп");
            RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
            CreateGroupCommand = CreateCommand(CreateGroupAsync, null, "Ошибка создания группы");
            EditGroupCommand = CreateCommand<GroupViewModel>(EditGroupAsync, null, "Ошибка редактирования группы");
            DeleteGroupCommand = CreateCommand<GroupViewModel>(DeleteGroupAsync, null, "Ошибка удаления группы");
            ViewGroupDetailsCommand = CreateCommand<GroupViewModel>(ViewGroupDetailsAsync, null, "Ошибка просмотра деталей группы");
            LoadGroupStatisticsCommand = CreateCommand<GroupViewModel>(LoadGroupStatisticsAsync, null, "Ошибка загрузки статистики группы");
            AssignCuratorCommand = CreateCommand<GroupViewModel>(AssignCuratorAsync, null, "Ошибка назначения куратора");
            ManageStudentsCommand = CreateCommand<GroupViewModel>(ManageStudentsAsync, null, "Ошибка управления студентами");
            SearchCommand = CreateCommand<string>(SearchGroupsAsync, null, "Ошибка поиска групп");
            GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
            
            var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
            var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
            
            NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
            PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        }

        /// <summary>
        /// Настраивает подписки на изменения свойств
        /// </summary>
        private void SetupSubscriptions()
        {
            // Автопоиск при изменении текста поиска
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(Disposables);

            // Загрузка статистики при выборе группы
            this.WhenAnyValue(x => x.SelectedGroup)
                .Where(group => group != null)
                .Select(group => group!)
                .InvokeCommand(LoadGroupStatisticsCommand)
                .DisposeWith(Disposables);

            // Уведомления об изменении computed properties
            this.WhenAnyValue(x => x.SelectedGroup)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroup)))
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.SelectedGroupStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedGroupStatistics)))
                .DisposeWith(Disposables);
        }

        private async Task LoadGroupsAsync()
        {
            LogInfo("Loading groups with search text: {SearchText}", SearchText);
            
            IsLoading = true;
            ShowInfo("Загрузка групп...");

            var (groups, totalCount) = await _groupService.GetGroupsPagedAsync(CurrentPage, PageSize, SearchText);
            
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(new GroupViewModel(group));
            }

            TotalGroups = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            ShowSuccess($"Загружено {Groups.Count} групп");
            LogInfo("Loaded {GroupCount} groups, total: {TotalCount}", Groups.Count, totalCount);
            
            IsLoading = false;
        }

        private async Task RefreshAsync()
        {
            LogInfo("Refreshing groups data");
            IsRefreshing = true;
            
            await LoadGroupsAsync();
            ShowSuccess("Данные обновлены");
            
            IsRefreshing = false;
        }

        private async Task CreateGroupAsync()
        {
            LogInfo("Creating new group");
            
            var newGroup = new Group
            {
                Uid = Guid.NewGuid(),
                Name = string.Empty,
                Description = string.Empty
            };

            var dialogResult = await _dialogService.ShowGroupEditDialogAsync(newGroup);
            if (dialogResult == null)
            {
                LogDebug("Group creation cancelled by user");
                return;
            }

            await _groupService.CreateGroupAsync(dialogResult);
            Groups.Add(new GroupViewModel(dialogResult));

            ShowSuccess($"Группа '{dialogResult.Name}' создана");
            LogInfo("Group created successfully: {GroupName}", dialogResult.Name);
            
            // Уведомление куратору, если он назначен
            if (dialogResult.CuratorUid.HasValue)
            {
                await _notificationService.CreateNotificationAsync(
                    dialogResult.CuratorUid.Value,
                    "Назначение куратором",
                    $"Вы назначены куратором группы '{dialogResult.Name}'",
                    Domain.Models.System.NotificationType.Info);
            }
        }

        private async Task EditGroupAsync(GroupViewModel groupViewModel)
        {
            LogInfo("Editing group: {GroupId}", groupViewModel.Uid);
            
            var dialogResult = await _dialogService.ShowGroupEditDialogAsync(groupViewModel.ToGroup());
            if (dialogResult == null)
            {
                LogDebug("Group editing cancelled by user");
                return;
            }

            var updatedGroup = await _groupService.UpdateGroupAsync(dialogResult);
            var index = Groups.IndexOf(groupViewModel);
            if (index >= 0)
            {
                Groups[index] = new GroupViewModel(updatedGroup);
            }

            ShowSuccess($"Группа '{updatedGroup.Name}' обновлена");
            LogInfo("Group updated successfully: {GroupName}", updatedGroup.Name);
        }

        private async Task DeleteGroupAsync(GroupViewModel groupViewModel)
        {
            LogInfo("Deleting group: {GroupId}", groupViewModel.Uid);
            
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Удаление группы",
                $"Вы уверены, что хотите удалить группу '{groupViewModel.Name}'?\nВсе студенты будут исключены из группы.");

            if (!confirmResult)
            {
                LogDebug("Group deletion cancelled by user");
                return;
            }

            await _groupService.DeleteGroupAsync(groupViewModel.Uid);
            Groups.Remove(groupViewModel);
            ShowSuccess($"Группа '{groupViewModel.Name}' удалена");
            LogInfo("Group deleted successfully: {GroupName}", groupViewModel.Name);
        }

        private async Task ViewGroupDetailsAsync(GroupViewModel groupViewModel)
        {
            LogInfo("Viewing group details: {GroupId}", groupViewModel.Uid);
            
            SelectedGroup = groupViewModel;
            await LoadGroupStatisticsAsync(groupViewModel);
            
            ShowInfo($"Просмотр группы '{groupViewModel.Name}'");
        }

        private async Task LoadGroupStatisticsAsync(GroupViewModel groupViewModel)
        {
            try
            {
                var statistics = await _groupService.GetGroupStatisticsAsync(groupViewModel.Uid);
                SelectedGroupStatistics = statistics;
                LogInfo("Group statistics loaded for: {GroupName}", groupViewModel.Name);
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить статистику группы: {ex.Message}");
                LogError(ex, "Failed to load group statistics for: {GroupName}", groupViewModel.Name);
            }
        }

        private async Task AssignCuratorAsync(GroupViewModel groupViewModel)
        {
            LogInfo("Assigning curator to group: {GroupId}", groupViewModel.Uid);
            
            var teachers = await _teacherService.GetAllTeachersAsync();
            var selectedTeacher = await _dialogService.ShowTeacherSelectionDialogAsync(teachers);
            
            if (selectedTeacher == null)
            {
                LogDebug("Curator assignment cancelled by user");
                return;
            }

            var success = await _groupService.AssignCuratorAsync(groupViewModel.Uid, selectedTeacher.Uid);
            if (success)
            {
                groupViewModel.CuratorName = $"{selectedTeacher.FirstName} {selectedTeacher.LastName}";
                ShowSuccess($"Куратор назначен для группы '{groupViewModel.Name}'");
                LogInfo("Curator assigned to group {GroupName}: {CuratorName}", groupViewModel.Name, groupViewModel.CuratorName);
                
                // Уведомление куратору
                await _notificationService.CreateNotificationAsync(
                    selectedTeacher.Uid,
                    "Назначение куратором",
                    $"Вы назначены куратором группы '{groupViewModel.Name}'",
                    Domain.Models.System.NotificationType.Info);
            }
            else
            {
                ShowError("Не удалось назначить куратора");
            }
        }

        private async Task ManageStudentsAsync(GroupViewModel groupViewModel)
        {
            LogInfo("Managing students for group: {GroupId}", groupViewModel.Uid);
            
            var allStudents = await _studentService.GetAllStudentsAsync();
            var result = await _dialogService.ShowGroupStudentsManagementDialogAsync(groupViewModel.ToGroup(), allStudents);
            
            if (result != null)
            {
                await RefreshAsync();
                ShowSuccess($"Список студентов группы '{groupViewModel.Name}' обновлен");
                LogInfo("Students list updated for group: {GroupName}", groupViewModel.Name);
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

        #endregion

        #region Lifecycle Methods

        protected override async Task OnFirstTimeLoadedAsync()
        {
            await base.OnFirstTimeLoadedAsync();
            LogInfo("GroupsViewModel loaded for the first time");
            
            // Load groups when view is loaded for the first time
            await LoadGroupsAsync();
        }

        #endregion
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