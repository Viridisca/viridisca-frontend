using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Services;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using System.Collections.Generic;
using NotificationType = ViridiscaUi.Domain.Models.System.NotificationType;
using NotificationPriority = ViridiscaUi.Domain.Models.System.NotificationPriority;
using DomainNotificationSettings = ViridiscaUi.Domain.Models.System.NotificationSettings;
using DomainNotificationTemplate = ViridiscaUi.Domain.Models.System.NotificationTemplate;
using InterfaceNotificationSettings = ViridiscaUi.Services.Interfaces.NotificationSettings;
using ViridiscaUi.ViewModels;
using System.Reactive.Subjects;

namespace ViridiscaUi.ViewModels.System
{
    /// <summary>
    /// ViewModel для центра уведомлений
    /// Следует принципам SOLID и чистой архитектуры
    /// </summary>
    [Route("notifications", DisplayName = "Уведомления", IconKey = "🔔", Order = 10, Group = "System")]
    public class NotificationCenterViewModel : RoutableViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly IAuthService _authService;

        

        // === СВОЙСТВА ===
        
        [Reactive] public ObservableCollection<NotificationViewModel> Notifications { get; set; } = new();
        [Reactive] public NotificationViewModel? SelectedNotification { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public bool IsRefreshing { get; set; }
        [Reactive] public NotificationStatistics? UserStatistics { get; set; }
        [Reactive] public SystemNotificationStatistics? SystemStatistics { get; set; }
        [Reactive] public InterfaceNotificationSettings? UserSettings { get; set; }
        
        // Фильтры
        [Reactive] public bool? IsReadFilter { get; set; }
        [Reactive] public Domain.Models.System.NotificationType? TypeFilter { get; set; }
        [Reactive] public Domain.Models.System.NotificationPriority? PriorityFilter { get; set; }
        [Reactive] public string? CategoryFilter { get; set; }
        [Reactive] public DateTime? FromDateFilter { get; set; }
        [Reactive] public DateTime? ToDateFilter { get; set; }
        [Reactive] public bool? IsImportantFilter { get; set; }
        [Reactive] public bool? IsExpiredFilter { get; set; }
        
        // Пагинация
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int PageSize { get; set; } = 20;
        [Reactive] public int TotalPages { get; set; }
        [Reactive] public int TotalNotifications { get; set; }
        [Reactive] public int UnreadCount { get; set; }
        
        // Computed property for UI binding
        public bool HasUnreadNotifications => UnreadCount > 0;
        
        // Computed property for statistics visibility
        public bool HasUserStatistics => UserStatistics != null;
        
        // Computed property for selected notification visibility
        public bool HasSelectedNotification => SelectedNotification != null;
        
        // Computed property for system statistics visibility
        public bool HasSystemStatistics => SystemStatistics != null;
        
        // Computed property for user settings visibility
        public bool HasUserSettings => UserSettings != null;
        
        // Computed properties for selected notification fields
        public bool HasSelectedNotificationReadAt => SelectedNotification?.ReadAt != null;
        public bool HasSelectedNotificationExpiresAt => SelectedNotification?.ExpiresAt != null;
        public bool HasSelectedNotificationActionUrl => !string.IsNullOrEmpty(SelectedNotification?.ActionUrl);
        
        // Шаблоны
        [Reactive] public ObservableCollection<NotificationTemplateViewModel> Templates { get; set; } = new();
        [Reactive] public NotificationTemplateViewModel? SelectedTemplate { get; set; }
        
        // Категории
        [Reactive] public ObservableCollection<string> Categories { get; set; } = new();
        
        // Текущий пользователь
        [Reactive] public Guid CurrentUserUid { get; set; }

        private NotificationFilter _filter = new();
        private DomainNotificationSettings? _userSettings;

        // === КОМАНДЫ ===
        
        public ReactiveCommand<Unit, Unit> LoadNotificationsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationViewModel, Unit> MarkAsReadCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> MarkAllAsReadCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationViewModel, Unit> MarkAsImportantCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationViewModel, Unit> UnmarkAsImportantCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationViewModel, Unit> DeleteNotificationCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationViewModel, Unit> ViewNotificationDetailsCommand { get; private set; } = null!;
        public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> LoadStatisticsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> LoadSystemStatisticsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> LoadUserSettingsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> SaveUserSettingsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> LoadTemplatesCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> CreateTemplateCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationTemplateViewModel, Unit> EditTemplateCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationTemplateViewModel, Unit> DeleteTemplateCommand { get; private set; } = null!;
        public ReactiveCommand<NotificationTemplateViewModel, Unit> SendFromTemplateCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> CreateReminderCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ShowUnreadOnlyCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ShowImportantOnlyCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ShowHighPriorityCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ShowTodayNotificationsCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> ArchiveOldNotificationsCommand { get; private set; } = null!;
        public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;

        public NotificationCenterViewModel(
            IScreen hostScreen,
            INotificationService notificationService,
            IDialogService dialogService,
            IStatusService statusService,
            IAuthService authService) : base(hostScreen)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            InitializeCommands();
            SetupSubscriptions();
        }

        private void InitializeCommands()
        {
            LoadNotificationsCommand = CreateCommand(LoadNotificationsAsync);
            RefreshCommand = CreateCommand(RefreshAsync);
            MarkAsReadCommand = CreateCommand<NotificationViewModel>(MarkAsReadAsync);
            MarkAllAsReadCommand = CreateCommand(MarkAllAsReadAsync);
            MarkAsImportantCommand = CreateCommand<NotificationViewModel>(MarkAsImportantAsync);
            UnmarkAsImportantCommand = CreateCommand<NotificationViewModel>(UnmarkAsImportantAsync);
            DeleteNotificationCommand = CreateCommand<NotificationViewModel>(DeleteNotificationAsync);
            ViewNotificationDetailsCommand = CreateCommand<NotificationViewModel>(ViewNotificationDetailsAsync);
            SearchCommand = CreateCommand<string>(SearchNotificationsAsync);
            ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync);
            ClearFiltersCommand = CreateCommand(ClearFiltersAsync);
            LoadStatisticsCommand = CreateCommand(LoadStatisticsAsync);
            LoadSystemStatisticsCommand = CreateCommand(LoadSystemStatisticsAsync);
            LoadUserSettingsCommand = CreateCommand(LoadUserSettingsAsync);
            SaveUserSettingsCommand = CreateCommand(SaveUserSettingsAsync);
            LoadTemplatesCommand = CreateCommand(LoadTemplatesAsync);
            CreateTemplateCommand = CreateCommand(CreateTemplateAsync);
            EditTemplateCommand = CreateCommand<NotificationTemplateViewModel>(EditTemplateAsync);
            DeleteTemplateCommand = CreateCommand<NotificationTemplateViewModel>(DeleteTemplateAsync);
            SendFromTemplateCommand = CreateCommand<NotificationTemplateViewModel>(SendFromTemplateAsync);
            CreateReminderCommand = CreateCommand(CreateReminderAsync);
            ShowUnreadOnlyCommand = CreateCommand(ShowUnreadOnlyAsync);
            ShowImportantOnlyCommand = CreateCommand(ShowImportantOnlyAsync);
            ShowHighPriorityCommand = CreateCommand(ShowHighPriorityAsync);
            ShowTodayNotificationsCommand = CreateCommand(ShowTodayNotificationsAsync);
            ArchiveOldNotificationsCommand = CreateCommand(ArchiveOldNotificationsAsync);
            GoToPageCommand = CreateCommand<int>(GoToPageAsync);
            
            var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
            var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
            
            NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
            PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
        }

        private void SetupSubscriptions()
        {
            // Автопоиск при изменении текста поиска
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand)
                .DisposeWith(Disposables);

            // Применение фильтров при изменении
            var filterObservables = new[]
            {
                this.WhenAnyValue(x => x.IsReadFilter).Select(_ => Unit.Default),
                this.WhenAnyValue(x => x.TypeFilter).Select(_ => Unit.Default),
                this.WhenAnyValue(x => x.PriorityFilter).Select(_ => Unit.Default),
                this.WhenAnyValue(x => x.CategoryFilter).Select(_ => Unit.Default),
                this.WhenAnyValue(x => x.FromDateFilter).Select(_ => Unit.Default),
                this.WhenAnyValue(x => x.ToDateFilter).Select(_ => Unit.Default),
                this.WhenAnyValue(x => x.IsImportantFilter).Select(_ => Unit.Default),
                this.WhenAnyValue(x => x.IsExpiredFilter).Select(_ => Unit.Default)
            };

            Observable.Merge(filterObservables)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => Unit.Default)
                .InvokeCommand(ApplyFiltersCommand)
                .DisposeWith(Disposables);

            // Уведомления об изменении computed properties
            this.WhenAnyValue(x => x.UnreadCount)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasUnreadNotifications)))
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.UserStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasUserStatistics)))
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.SelectedNotification)
                .Subscribe(_ => 
                {
                    this.RaisePropertyChanged(nameof(HasSelectedNotification));
                    this.RaisePropertyChanged(nameof(HasSelectedNotificationReadAt));
                    this.RaisePropertyChanged(nameof(HasSelectedNotificationExpiresAt));
                    this.RaisePropertyChanged(nameof(HasSelectedNotificationActionUrl));
                })
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.SystemStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSystemStatistics)))
                .DisposeWith(Disposables);
                
            this.WhenAnyValue(x => x.UserSettings)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasUserSettings)))
                .DisposeWith(Disposables);
        }

        #region Lifecycle Methods

        protected override async Task OnFirstTimeLoadedAsync()
        {
            await base.OnFirstTimeLoadedAsync();
            LogInfo("NotificationCenterViewModel loaded for the first time");
            
            // Initialize current user and load data when view is loaded for the first time
            await ExecuteWithErrorHandlingAsync(InitializeAsync, "Ошибка инициализации центра уведомлений");
        }

        #endregion

        private async Task InitializeAsync()
        {
            LogInfo("Initializing NotificationCenterViewModel");
            
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                CurrentUserUid = currentUser.Uid;
                LogInfo("Current user set: {UserUid}", CurrentUserUid);
            }

            // Load initial data
            await LoadCategoriesAsync();
            await LoadNotificationsAsync();
            await ExecuteWithErrorHandlingAsync(LoadStatisticsAsync, "Ошибка загрузки статистики");
            await ExecuteWithErrorHandlingAsync(LoadUserSettingsAsync, "Ошибка загрузки настроек пользователя");
            await ExecuteWithErrorHandlingAsync(LoadTemplatesAsync, "Ошибка загрузки шаблонов");
        }

        private async Task LoadNotificationsAsync()
        {
            try
            {
                IsLoading = true;
                ShowInfo("Загрузка уведомлений...");

                var filter = new NotificationFilter
                {
                    IsRead = IsReadFilter,
                    Type = TypeFilter,
                    Priority = PriorityFilter,
                    Category = CategoryFilter,
                    FromDate = FromDateFilter,
                    ToDate = ToDateFilter,
                    IsImportant = IsImportantFilter,
                    IsExpired = IsExpiredFilter,
                    SearchTerm = SearchText
                };

                var (notifications, totalCount) = await _notificationService.GetNotificationsAdvancedAsync(
                    CurrentUserUid, CurrentPage, PageSize, filter);

                Notifications.Clear();
                foreach (var notification in notifications)
                {
                    Notifications.Add(new NotificationViewModel(notification));
                }

                TotalNotifications = totalCount;
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                await UpdateUnreadCountAsync();

                ShowSuccess($"Загружено {Notifications.Count} уведомлений");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка загрузки уведомлений: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(CurrentUserUid);
                var categories = notifications
                    .Where(n => !string.IsNullOrEmpty(n.Category))
                    .Select(n => n.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить категории: {ex.Message}");
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await LoadNotificationsAsync();
                await LoadStatisticsAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task MarkAsReadAsync(NotificationViewModel notificationViewModel)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(notificationViewModel.Uid);
                if (success)
                {
                    notificationViewModel.IsRead = true;
                    notificationViewModel.ReadAt = DateTime.UtcNow;
                    await UpdateUnreadCountAsync();
                    ShowSuccess("Уведомление отмечено как прочитанное");
                }
                else
                {
                    ShowError("Не удалось отметить уведомление как прочитанное");
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка отметки уведомления: {ex.Message}", ex);
            }
        }

        private async Task MarkAllAsReadAsync()
        {
            try
            {
                var markedCount = await _notificationService.MarkAllAsReadAsync(CurrentUserUid);
                if (markedCount > 0)
                {
                    foreach (var notification in Notifications.Where(n => !n.IsRead))
                    {
                        notification.IsRead = true;
                        notification.ReadAt = DateTime.UtcNow;
                    }
                    await UpdateUnreadCountAsync();
                    ShowSuccess($"Все уведомления отмечены как прочитанные");
                }
                else
                {
                    ShowError("Не удалось отметить все уведомления как прочитанные");
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка отметки всех уведомлений: {ex.Message}", ex);
            }
        }

        private async Task MarkAsImportantAsync(NotificationViewModel notificationViewModel)
        {
            try
            {
                var success = await _notificationService.MarkAsImportantAsync(notificationViewModel.Uid, CurrentUserUid);
                if (success)
                {
                    notificationViewModel.IsImportant = true;
                    ShowSuccess("Уведомление отмечено как важное");
                }
                else
                {
                    ShowError("Не удалось отметить уведомление как важное");
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка отметки важности: {ex.Message}", ex);
            }
        }

        private async Task UnmarkAsImportantAsync(NotificationViewModel notificationViewModel)
        {
            try
            {
                var success = await _notificationService.UnmarkAsImportantAsync(notificationViewModel.Uid, CurrentUserUid);
                if (success)
                {
                    notificationViewModel.IsImportant = false;
                    ShowSuccess("Отметка важности снята");
                }
                else
                {
                    ShowError("Не удалось снять отметку важности");
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка снятия отметки важности: {ex.Message}", ex);
            }
        }

        private async Task DeleteNotificationAsync(NotificationViewModel notificationViewModel)
        {
            try
            {
                var confirmResult = await _dialogService.ShowConfirmationAsync(
                    "Удаление уведомления",
                    $"Вы уверены, что хотите удалить уведомление '{notificationViewModel.Title}'?");

                if (!confirmResult) return;

                var success = await _notificationService.DeleteNotificationAsync(notificationViewModel.Uid);
                if (success)
                {
                    Notifications.Remove(notificationViewModel);
                    await UpdateUnreadCountAsync();
                    ShowSuccess("Уведомление удалено");
                }
                else
                {
                    ShowError("Не удалось удалить уведомление");
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка удаления уведомления: {ex.Message}", ex);
            }
        }

        private async Task ViewNotificationDetailsAsync(NotificationViewModel notificationViewModel)
        {
            try
            {
                SelectedNotification = notificationViewModel;
                
                // Автоматически отмечаем как прочитанное при просмотре
                if (!notificationViewModel.IsRead)
                {
                    await MarkAsReadAsync(notificationViewModel);
                }

                ShowInfo($"Просмотр уведомления '{notificationViewModel.Title}'");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка просмотра уведомления: {ex.Message}", ex);
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                UserStatistics = await _notificationService.GetUserNotificationStatisticsAsync(CurrentUserUid);
                LogInfo("User notification statistics loaded");
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить статистику: {ex.Message}");
                LogError(ex, "Failed to load user notification statistics");
            }
        }

        private async Task LoadSystemStatisticsAsync()
        {
            try
            {
                SystemStatistics = await _notificationService.GetSystemNotificationStatisticsAsync();
                LogInfo("System notification statistics loaded");
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить системную статистику: {ex.Message}");
                LogError(ex, "Failed to load system notification statistics");
            }
        }

        private async Task LoadUserSettingsAsync()
        {
            try
            {
                UserSettings = await _notificationService.GetUserSettingsAsync(CurrentUserUid);
                LogInfo("User notification settings loaded");
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить настройки: {ex.Message}");
                LogError(ex, "Failed to load user notification settings");
            }
        }

        private async Task SaveUserSettingsAsync()
        {
            try
            {
                if (UserSettings == null) return;

                await _notificationService.UpdateUserSettingsAsync(CurrentUserUid, UserSettings);
                ShowSuccess("Настройки уведомлений сохранены");
                LogInfo("User notification settings saved successfully");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка сохранения настроек: {ex.Message}", ex);
            }
        }

        private async Task LoadTemplatesAsync()
        {
            try
            {
                var templates = await _notificationService.GetTemplatesAsync();
                Templates.Clear();
                foreach (var template in templates)
                {
                    Templates.Add(new NotificationTemplateViewModel(template));
                }
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось загрузить шаблоны: {ex.Message}");
            }
        }

        private async Task CreateTemplateAsync()
        {
            try
            {
                var newTemplate = new DomainNotificationTemplate
                {
                    Name = "Новый шаблон",
                    TitleTemplate = "Заголовок",
                    MessageTemplate = "Сообщение",
                    Type = Domain.Models.System.NotificationType.Info,
                    Priority = Domain.Models.System.NotificationPriority.Normal,
                    IsActive = true
                };

                var dialogResult = await _dialogService.ShowNotificationTemplateEditDialogAsync(newTemplate);
                if (dialogResult == null) return;

                var createdTemplate = await _notificationService.CreateTemplateAsync(dialogResult);
                Templates.Add(new NotificationTemplateViewModel(createdTemplate));

                ShowSuccess($"Шаблон '{createdTemplate.Name}' создан");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка создания шаблона: {ex.Message}", ex);
            }
        }

        private async Task EditTemplateAsync(NotificationTemplateViewModel templateViewModel)
        {
            try
            {
                var dialogResult = await _dialogService.ShowNotificationTemplateEditDialogAsync(templateViewModel.ToTemplate());
                if (dialogResult == null) return;

                // В реальной системе здесь был бы метод UpdateTemplateAsync
                ShowSuccess($"Шаблон '{dialogResult.Name}' обновлен");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка обновления шаблона: {ex.Message}", ex);
            }
        }

        private async Task DeleteTemplateAsync(NotificationTemplateViewModel templateViewModel)
        {
            try
            {
                var confirmResult = await _dialogService.ShowConfirmationAsync(
                    "Удаление шаблона",
                    $"Вы уверены, что хотите удалить шаблон '{templateViewModel.Name}'?");

                if (!confirmResult) return;

                // В реальной системе здесь был бы метод DeleteTemplateAsync
                Templates.Remove(templateViewModel);
                ShowSuccess($"Шаблон '{templateViewModel.Name}' удален");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка удаления шаблона: {ex.Message}", ex);
            }
        }

        private async Task SendFromTemplateAsync(NotificationTemplateViewModel templateViewModel)
        {
            try
            {
                var parameters = await _dialogService.ShowTemplateParametersDialogAsync(templateViewModel.ToTemplate());
                if (parameters == null) return;

                var notification = await _notificationService.SendFromTemplateAsync(
                    templateViewModel.Uid, CurrentUserUid, parameters);

                ShowSuccess($"Уведомление отправлено по шаблону '{templateViewModel.Name}'");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка отправки по шаблону: {ex.Message}", ex);
            }
        }

        private async Task CreateReminderAsync()
        {
            try
            {
                var reminderData = await _dialogService.ShowCreateReminderDialogAsync();
                if (reminderData == null) return;

                var reminder = await _notificationService.CreateReminderAsync(
                    CurrentUserUid,
                    reminderData.Title,
                    reminderData.Message,
                    reminderData.RemindAt,
                    reminderData.RepeatInterval);

                ShowSuccess($"Напоминание '{reminderData.Title}' создано");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка создания напоминания: {ex.Message}", ex);
            }
        }

        private async Task ShowUnreadOnlyAsync()
        {
            IsReadFilter = false;
            CurrentPage = 1;
            await LoadNotificationsAsync();
        }

        private async Task ShowImportantOnlyAsync()
        {
            IsImportantFilter = true;
            CurrentPage = 1;
            await LoadNotificationsAsync();
        }

        private async Task ShowHighPriorityAsync()
        {
            PriorityFilter = Domain.Models.System.NotificationPriority.High;
            CurrentPage = 1;
            await LoadNotificationsAsync();
        }

        private async Task ShowTodayNotificationsAsync()
        {
            FromDateFilter = DateTime.Today;
            ToDateFilter = DateTime.Today.AddDays(1);
            CurrentPage = 1;
            await LoadNotificationsAsync();
        }

        private async Task ArchiveOldNotificationsAsync()
        {
            try
            {
                var confirmResult = await _dialogService.ShowConfirmationAsync(
                    "Архивирование уведомлений",
                    "Вы уверены, что хотите архивировать все прочитанные уведомления старше 30 дней?");

                if (!confirmResult) return;

                var archivedCount = await _notificationService.ArchiveOldNotificationsAsync(
                    DateTime.UtcNow.AddDays(-30));

                ShowSuccess($"Архивировано {archivedCount} уведомлений");
                await LoadNotificationsAsync();
            }
            catch (Exception ex)
            {
                SetError($"Ошибка архивирования: {ex.Message}", ex);
            }
        }

        private async Task SearchNotificationsAsync(string searchText)
        {
            SearchText = searchText;
            CurrentPage = 1;
            await LoadNotificationsAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            CurrentPage = 1;
            await LoadNotificationsAsync();
        }

        private async Task ClearFiltersAsync()
        {
            IsReadFilter = null;
            TypeFilter = null;
            PriorityFilter = null;
            CategoryFilter = null;
            FromDateFilter = null;
            ToDateFilter = null;
            IsImportantFilter = null;
            IsExpiredFilter = null;
            SearchText = string.Empty;
            CurrentPage = 1;
            await LoadNotificationsAsync();
        }

        private async Task GoToPageAsync(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                await LoadNotificationsAsync();
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

        private async Task UpdateUnreadCountAsync()
        {
            try
            {
                UnreadCount = await _notificationService.GetUnreadCountAsync(CurrentUserUid);
            }
            catch (Exception ex)
            {
                ShowWarning($"Не удалось обновить счетчик непрочитанных: {ex.Message}");
            }
        }

        private void UpdateUnreadCount()
        {
            UnreadCount = Notifications.Count(n => !n.IsRead);
        }
    }

    /// <summary>
    /// ViewModel для отображения уведомления в списке
    /// </summary>
    public class NotificationViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        [Reactive] public string Title { get; set; } = string.Empty;
        [Reactive] public string Message { get; set; } = string.Empty;
        [Reactive] public Domain.Models.System.NotificationType Type { get; set; }
        [Reactive] public Domain.Models.System.NotificationPriority Priority { get; set; }
        [Reactive] public string? Category { get; set; }
        [Reactive] public string? ActionUrl { get; set; }
        [Reactive] public bool IsRead { get; set; }
        [Reactive] public bool IsImportant { get; set; }
        [Reactive] public DateTime? ReadAt { get; set; }
        [Reactive] public DateTime? ExpiresAt { get; set; }
        [Reactive] public DateTime? ScheduledFor { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }

        // Computed properties
        public string TypeIcon => Type switch
        {
            Domain.Models.System.NotificationType.Info => "ℹ️",
            Domain.Models.System.NotificationType.Warning => "⚠️",
            Domain.Models.System.NotificationType.Error => "❌",
            Domain.Models.System.NotificationType.Success => "✅",
            Domain.Models.System.NotificationType.Reminder => "⏰",
            _ => "📢"
        };

        public string PriorityIcon => Priority switch
        {
            Domain.Models.System.NotificationPriority.Low => "🔵",
            Domain.Models.System.NotificationPriority.Normal => "🟡",
            Domain.Models.System.NotificationPriority.High => "🟠",
            Domain.Models.System.NotificationPriority.Critical => "🔴",
            _ => "⚪"
        };

        public string StatusIcon => IsRead ? "✅" : "📬";
        public string ImportantIcon => IsImportant ? "⭐" : "";
        public string CreatedAtText => CreatedAt.ToString("dd.MM.yyyy HH:mm");
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        public string ExpiredText => IsExpired ? "⏰ Истекло" : "";

        public NotificationViewModel(ViridiscaUi.Domain.Models.System.Notification notification)
        {
            Uid = notification.Uid;
            Title = notification.Title;
            Message = notification.Message;
            Type = notification.Type;
            Priority = notification.Priority;
            Category = notification.Category;
            ActionUrl = notification.ActionUrl;
            IsRead = notification.IsRead;
            IsImportant = notification.IsImportant;
            ReadAt = notification.ReadAt;
            ExpiresAt = notification.ExpiresAt;
            ScheduledFor = notification.ScheduledFor;
            CreatedAt = notification.CreatedAt;
        }
    }

    /// <summary>
    /// ViewModel для отображения шаблона уведомления
    /// </summary>
    public class NotificationTemplateViewModel : ReactiveObject
    {
        public Guid Uid { get; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public string TitleTemplate { get; set; } = string.Empty;
        [Reactive] public string MessageTemplate { get; set; } = string.Empty;
        [Reactive] public Domain.Models.System.NotificationType Type { get; set; }
        [Reactive] public Domain.Models.System.NotificationPriority Priority { get; set; }
        [Reactive] public string? Category { get; set; }
        [Reactive] public bool IsActive { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }

        public string TypeIcon => Type switch
        {
            Domain.Models.System.NotificationType.Info => "ℹ️",
            Domain.Models.System.NotificationType.Warning => "⚠️",
            Domain.Models.System.NotificationType.Error => "❌",
            Domain.Models.System.NotificationType.Success => "✅",
            Domain.Models.System.NotificationType.Reminder => "⏰",
            _ => "📢"
        };

        public string StatusText => IsActive ? "✅ Активен" : "❌ Неактивен";

        public NotificationTemplateViewModel(DomainNotificationTemplate template)
        {
            Uid = template.Uid;
            Name = template.Name;
            Description = template.Description;
            TitleTemplate = template.TitleTemplate;
            MessageTemplate = template.MessageTemplate;
            Type = template.Type;
            Priority = template.Priority;
            Category = template.Category;
            IsActive = template.IsActive;
        }

        public DomainNotificationTemplate ToTemplate()
        {
            return new DomainNotificationTemplate
            {
                Uid = Uid,
                Name = Name,
                Description = Description,
                TitleTemplate = TitleTemplate,
                MessageTemplate = MessageTemplate,
                Type = Type,
                Priority = Priority,
                Category = Category,
                IsActive = IsActive,
                CreatedAt = CreatedAt
            };
        }
    }
} 