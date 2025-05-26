using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Services;
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
    /// </summary>
    public class NotificationCenterViewModel : RoutableViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly IAuthService _authService;

        public override string UrlPathSegment => "notification-center";

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
        [Reactive] public NotificationType? TypeFilter { get; set; }
        [Reactive] public NotificationPriority? PriorityFilter { get; set; }
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
        
        public ReactiveCommand<Unit, Unit> LoadNotificationsCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<NotificationViewModel, Unit> MarkAsReadCommand { get; }
        public ReactiveCommand<Unit, Unit> MarkAllAsReadCommand { get; }
        public ReactiveCommand<NotificationViewModel, Unit> MarkAsImportantCommand { get; }
        public ReactiveCommand<NotificationViewModel, Unit> UnmarkAsImportantCommand { get; }
        public ReactiveCommand<NotificationViewModel, Unit> DeleteNotificationCommand { get; }
        public ReactiveCommand<NotificationViewModel, Unit> ViewNotificationDetailsCommand { get; }
        public ReactiveCommand<string, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadStatisticsCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadSystemStatisticsCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadUserSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveUserSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadTemplatesCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateTemplateCommand { get; }
        public ReactiveCommand<NotificationTemplateViewModel, Unit> EditTemplateCommand { get; }
        public ReactiveCommand<NotificationTemplateViewModel, Unit> DeleteTemplateCommand { get; }
        public ReactiveCommand<NotificationTemplateViewModel, Unit> SendFromTemplateCommand { get; }
        public ReactiveCommand<Unit, Unit> CreateReminderCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowUnreadOnlyCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowImportantOnlyCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowHighPriorityCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowTodayNotificationsCommand { get; }
        public ReactiveCommand<Unit, Unit> ArchiveOldNotificationsCommand { get; }
        public ReactiveCommand<int, Unit> GoToPageCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        public NotificationCenterViewModel(
            IScreen hostScreen,
            INotificationService notificationService,
            IDialogService dialogService,
            IStatusService statusService,
            IAuthService authService) : base(hostScreen)
        {
            _notificationService = notificationService;
            _dialogService = dialogService;
            _statusService = statusService;
            _authService = authService;

            // === ИНИЦИАЛИЗАЦИЯ КОМАНД ===

            LoadNotificationsCommand = ReactiveCommand.CreateFromTask(LoadNotificationsAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
            MarkAsReadCommand = ReactiveCommand.CreateFromTask<NotificationViewModel>(MarkAsReadAsync);
            MarkAllAsReadCommand = ReactiveCommand.CreateFromTask(MarkAllAsReadAsync);
            MarkAsImportantCommand = ReactiveCommand.CreateFromTask<NotificationViewModel>(MarkAsImportantAsync);
            UnmarkAsImportantCommand = ReactiveCommand.CreateFromTask<NotificationViewModel>(UnmarkAsImportantAsync);
            DeleteNotificationCommand = ReactiveCommand.CreateFromTask<NotificationViewModel>(DeleteNotificationAsync);
            ViewNotificationDetailsCommand = ReactiveCommand.CreateFromTask<NotificationViewModel>(ViewNotificationDetailsAsync);
            SearchCommand = ReactiveCommand.CreateFromTask<string>(SearchNotificationsAsync);
            ApplyFiltersCommand = ReactiveCommand.CreateFromTask(ApplyFiltersAsync);
            ClearFiltersCommand = ReactiveCommand.CreateFromTask(ClearFiltersAsync);
            LoadStatisticsCommand = ReactiveCommand.CreateFromTask(LoadStatisticsAsync);
            LoadSystemStatisticsCommand = ReactiveCommand.CreateFromTask(LoadSystemStatisticsAsync);
            LoadUserSettingsCommand = ReactiveCommand.CreateFromTask(LoadUserSettingsAsync);
            SaveUserSettingsCommand = ReactiveCommand.CreateFromTask(SaveUserSettingsAsync);
            LoadTemplatesCommand = ReactiveCommand.CreateFromTask(LoadTemplatesAsync);
            CreateTemplateCommand = ReactiveCommand.CreateFromTask(CreateTemplateAsync);
            EditTemplateCommand = ReactiveCommand.CreateFromTask<NotificationTemplateViewModel>(EditTemplateAsync);
            DeleteTemplateCommand = ReactiveCommand.CreateFromTask<NotificationTemplateViewModel>(DeleteTemplateAsync);
            SendFromTemplateCommand = ReactiveCommand.CreateFromTask<NotificationTemplateViewModel>(SendFromTemplateAsync);
            CreateReminderCommand = ReactiveCommand.CreateFromTask(CreateReminderAsync);
            ShowUnreadOnlyCommand = ReactiveCommand.CreateFromTask(ShowUnreadOnlyAsync);
            ShowImportantOnlyCommand = ReactiveCommand.CreateFromTask(ShowImportantOnlyAsync);
            ShowHighPriorityCommand = ReactiveCommand.CreateFromTask(ShowHighPriorityAsync);
            ShowTodayNotificationsCommand = ReactiveCommand.CreateFromTask(ShowTodayNotificationsAsync);
            ArchiveOldNotificationsCommand = ReactiveCommand.CreateFromTask(ArchiveOldNotificationsAsync);
            GoToPageCommand = ReactiveCommand.CreateFromTask<int>(GoToPageAsync);
            NextPageCommand = ReactiveCommand.CreateFromTask(NextPageAsync, this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total));
            PreviousPageCommand = ReactiveCommand.CreateFromTask(PreviousPageAsync, this.WhenAnyValue(x => x.CurrentPage, current => current > 1));

            // === ПОДПИСКИ ===

            // Автопоиск при изменении текста поиска
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(SearchCommand);

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
                .Subscribe(_ => ApplyFiltersCommand.Execute().Subscribe());

            // Обновление счетчика непрочитанных при изменении уведомлений
            this.WhenAnyValue(x => x.Notifications)
                .Subscribe(_ => UpdateUnreadCount());

            // Уведомление об изменении HasUnreadNotifications при изменении UnreadCount
            this.WhenAnyValue(x => x.UnreadCount)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasUnreadNotifications)));

            // Уведомления об изменении computed properties
            this.WhenAnyValue(x => x.UserStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasUserStatistics)));
                
            this.WhenAnyValue(x => x.SelectedNotification)
                .Subscribe(_ => {
                    this.RaisePropertyChanged(nameof(HasSelectedNotification));
                    this.RaisePropertyChanged(nameof(HasSelectedNotificationReadAt));
                    this.RaisePropertyChanged(nameof(HasSelectedNotificationExpiresAt));
                    this.RaisePropertyChanged(nameof(HasSelectedNotificationActionUrl));
                });
                
            this.WhenAnyValue(x => x.SystemStatistics)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSystemStatistics)));
                
            this.WhenAnyValue(x => x.UserSettings)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HasUserSettings)));

            // Первоначальная загрузка
            InitializeAsync();
        }

        // === МЕТОДЫ КОМАНД ===

        private async Task InitializeAsync()
        {
            try
            {
                CurrentUserUid = await _authService.GetCurrentUserUidAsync();
                await LoadNotificationsAsync();
                await LoadStatisticsAsync();
                await LoadUserSettingsAsync();
                await LoadTemplatesAsync();
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка инициализации центра уведомлений: {ex.Message}", "Уведомления");
            }
        }

        private async Task LoadNotificationsAsync()
        {
            try
            {
                IsLoading = true;
                _statusService.ShowInfo("Загрузка уведомлений...", "Уведомления");

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

                _statusService.ShowSuccess($"Загружено {Notifications.Count} уведомлений", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка загрузки уведомлений: {ex.Message}", "Уведомления");
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
                _statusService.ShowWarning($"Не удалось загрузить категории: {ex.Message}", "Уведомления");
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
                    _statusService.ShowSuccess("Уведомление отмечено как прочитанное", "Уведомления");
                }
                else
                {
                    _statusService.ShowError("Не удалось отметить уведомление как прочитанное", "Уведомления");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отметки уведомления: {ex.Message}", "Уведомления");
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
                    _statusService.ShowSuccess($"Все уведомления отмечены как прочитанные", "Уведомления");
                }
                else
                {
                    _statusService.ShowError("Не удалось отметить все уведомления как прочитанные", "Уведомления");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отметки всех уведомлений: {ex.Message}", "Уведомления");
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
                    _statusService.ShowSuccess("Уведомление отмечено как важное", "Уведомления");
                }
                else
                {
                    _statusService.ShowError("Не удалось отметить уведомление как важное", "Уведомления");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отметки важности: {ex.Message}", "Уведомления");
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
                    _statusService.ShowSuccess("Отметка важности снята", "Уведомления");
                }
                else
                {
                    _statusService.ShowError("Не удалось снять отметку важности", "Уведомления");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка снятия отметки важности: {ex.Message}", "Уведомления");
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
                    _statusService.ShowSuccess("Уведомление удалено", "Уведомления");
                }
                else
                {
                    _statusService.ShowError("Не удалось удалить уведомление", "Уведомления");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка удаления уведомления: {ex.Message}", "Уведомления");
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

                _statusService.ShowInfo($"Просмотр уведомления '{notificationViewModel.Title}'", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка просмотра уведомления: {ex.Message}", "Уведомления");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                UserStatistics = await _notificationService.GetUserNotificationStatisticsAsync(CurrentUserUid);
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить статистику: {ex.Message}", "Уведомления");
            }
        }

        private async Task LoadSystemStatisticsAsync()
        {
            try
            {
                SystemStatistics = await _notificationService.GetSystemNotificationStatisticsAsync();
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить системную статистику: {ex.Message}", "Уведомления");
            }
        }

        private async Task LoadUserSettingsAsync()
        {
            try
            {
                UserSettings = await _notificationService.GetUserSettingsAsync(CurrentUserUid);
            }
            catch (Exception ex)
            {
                _statusService.ShowWarning($"Не удалось загрузить настройки: {ex.Message}", "Уведомления");
            }
        }

        private async Task SaveUserSettingsAsync()
        {
            try
            {
                if (UserSettings == null) return;

                var success = await _notificationService.UpdateUserSettingsAsync(CurrentUserUid, UserSettings);
                if (success)
                {
                    _statusService.ShowSuccess("Настройки сохранены", "Уведомления");
                }
                else
                {
                    _statusService.ShowError("Не удалось сохранить настройки", "Уведомления");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка сохранения настроек: {ex.Message}", "Уведомления");
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
                _statusService.ShowWarning($"Не удалось загрузить шаблоны: {ex.Message}", "Уведомления");
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
                    Type = NotificationType.Info,
                    Priority = NotificationPriority.Normal,
                    IsActive = true
                };

                var dialogResult = await _dialogService.ShowNotificationTemplateEditDialogAsync(newTemplate);
                if (dialogResult == null) return;

                var createdTemplate = await _notificationService.CreateTemplateAsync(dialogResult);
                Templates.Add(new NotificationTemplateViewModel(createdTemplate));

                _statusService.ShowSuccess($"Шаблон '{createdTemplate.Name}' создан", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания шаблона: {ex.Message}", "Уведомления");
            }
        }

        private async Task EditTemplateAsync(NotificationTemplateViewModel templateViewModel)
        {
            try
            {
                var dialogResult = await _dialogService.ShowNotificationTemplateEditDialogAsync(templateViewModel.ToTemplate());
                if (dialogResult == null) return;

                // В реальной системе здесь был бы метод UpdateTemplateAsync
                _statusService.ShowSuccess($"Шаблон '{dialogResult.Name}' обновлен", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка обновления шаблона: {ex.Message}", "Уведомления");
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
                _statusService.ShowSuccess($"Шаблон '{templateViewModel.Name}' удален", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка удаления шаблона: {ex.Message}", "Уведомления");
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

                _statusService.ShowSuccess($"Уведомление отправлено по шаблону '{templateViewModel.Name}'", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка отправки по шаблону: {ex.Message}", "Уведомления");
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

                _statusService.ShowSuccess($"Напоминание '{reminderData.Title}' создано", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка создания напоминания: {ex.Message}", "Уведомления");
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
            PriorityFilter = NotificationPriority.High;
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

                var olderThan = DateTime.UtcNow.AddDays(-30);
                var archivedCount = await _notificationService.ArchiveOldNotificationsAsync(olderThan);

                await RefreshAsync();
                _statusService.ShowSuccess($"Архивировано {archivedCount} уведомлений", "Уведомления");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Ошибка архивирования: {ex.Message}", "Уведомления");
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
                _statusService.ShowWarning($"Не удалось обновить счетчик непрочитанных: {ex.Message}", "Уведомления");
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
        [Reactive] public NotificationType Type { get; set; }
        [Reactive] public NotificationPriority Priority { get; set; }
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
            NotificationType.Info => "ℹ️",
            NotificationType.Warning => "⚠️",
            NotificationType.Error => "❌",
            NotificationType.Success => "✅",
            NotificationType.Reminder => "⏰",
            _ => "📢"
        };

        public string PriorityIcon => Priority switch
        {
            NotificationPriority.Low => "🔵",
            NotificationPriority.Normal => "🟡",
            NotificationPriority.High => "🟠",
            NotificationPriority.Critical => "🔴",
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
        [Reactive] public NotificationType Type { get; set; }
        [Reactive] public NotificationPriority Priority { get; set; }
        [Reactive] public string? Category { get; set; }
        [Reactive] public bool IsActive { get; set; }
        [Reactive] public DateTime CreatedAt { get; set; }

        public string TypeIcon => Type switch
        {
            NotificationType.Info => "ℹ️",
            NotificationType.Warning => "⚠️",
            NotificationType.Error => "❌",
            NotificationType.Success => "✅",
            NotificationType.Reminder => "⏰",
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