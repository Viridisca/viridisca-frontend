using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.ViewModels.System;
using DynamicData;
using DynamicData.Binding;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.System.Enums;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления расписанием
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("schedule", 
    DisplayName = "Расписание", 
    IconKey = "CalendarClock", 
    Order = 9,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление расписанием занятий")]
public class ScheduleViewModel : RoutableViewModelBase
{
    private readonly IScheduleSlotService _scheduleSlotService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IAcademicPeriodService _academicPeriodService;
    private readonly ITeacherService _teacherService;
    private readonly IGroupService _groupService;
    private readonly IDialogService _dialogService;
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;

    private readonly SourceList<ScheduleSlotViewModel> _scheduleSlotsSource = new();
    private readonly ReadOnlyObservableCollection<ScheduleSlotViewModel> _scheduleSlots;

    // === СВОЙСТВА ===
    
    [Reactive] public ScheduleSlotViewModel? SelectedSlot { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public bool HasErrors { get; set; }
    [Reactive] public int TotalSlots { get; set; }
    [Reactive] public DateTime SelectedDate { get; set; } = DateTime.Today;
    [Reactive] public DayOfWeek SelectedDayOfWeek { get; set; } = DateTime.Today.DayOfWeek;

    // Фильтры
    [Reactive] public Guid? SelectedTeacherUid { get; set; }
    [Reactive] public Guid? SelectedGroupUid { get; set; }
    [Reactive] public Guid? SelectedPeriodUid { get; set; }
    [Reactive] public string? SelectedRoom { get; set; }

    // Коллекции для фильтров
    public ObservableCollection<Teacher> AvailableTeachers { get; } = new();
    public ObservableCollection<Group> AvailableGroups { get; } = new();
    public ObservableCollection<AcademicPeriod> AvailablePeriods { get; } = new();
    public ObservableCollection<string> AvailableRooms { get; } = new();

    public ReadOnlyObservableCollection<ScheduleSlotViewModel> ScheduleSlots => _scheduleSlots;

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadScheduleCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> AddSlotCommand { get; private set; } = null!;
    public ReactiveCommand<ScheduleSlotViewModel, Unit> EditSlotCommand { get; private set; } = null!;
    public ReactiveCommand<ScheduleSlotViewModel, Unit> DeleteSlotCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportScheduleCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ImportScheduleCommand { get; private set; } = null!;

    // Дата начала для фильтрации
    [Reactive]
    public DateTime StartDate { get; set; } = DateTime.Today;

    // Дата окончания для фильтрации
    [Reactive]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

    public ScheduleViewModel(
        IScreen hostScreen,
        IScheduleSlotService scheduleSlotService,
        ICourseInstanceService courseInstanceService,
        IAcademicPeriodService academicPeriodService,
        ITeacherService teacherService,
        IGroupService groupService,
        IDialogService dialogService,
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService) : base(hostScreen)
    {
        _scheduleSlotService = scheduleSlotService ?? throw new ArgumentNullException(nameof(scheduleSlotService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _academicPeriodService = academicPeriodService ?? throw new ArgumentNullException(nameof(academicPeriodService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        // Настройка фильтрации и сортировки
        var filterPredicate = this.WhenAnyValue(
            x => x.SearchText,
            x => x.SelectedTeacherUid,
            x => x.SelectedGroupUid,
            x => x.SelectedPeriodUid,
            x => x.SelectedRoom,
            x => x.SelectedDayOfWeek)
            .Select(CreateFilterPredicate);

        _scheduleSlotsSource
            .Connect()
            .Filter(filterPredicate)
            .Sort(SortExpressionComparer<ScheduleSlotViewModel>.Ascending(x => x.DayOfWeek)
                .ThenByAscending(x => x.StartTime))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _scheduleSlots)
            .Subscribe();

        InitializeCommands();
    }

    private void InitializeCommands()
    {
        LoadScheduleCommand = CreateCommand(LoadScheduleAsync, null, "Ошибка загрузки расписания");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        
        AddSlotCommand = CreateCommand(AddSlotAsync, 
            this.WhenAnyValue(x => x.IsLoading).Select(loading => !loading),
            "Ошибка создания слота расписания");
            
        EditSlotCommand = CreateCommand<ScheduleSlotViewModel>(EditSlotAsync,
            this.WhenAnyValue(x => x.SelectedSlot).Select(slot => slot != null),
            "Ошибка редактирования слота");
            
        DeleteSlotCommand = CreateCommand<ScheduleSlotViewModel>(DeleteSlotAsync,
            this.WhenAnyValue(x => x.SelectedSlot).Select(slot => slot != null),
            "Ошибка удаления слота");
            
        ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");
        ExportScheduleCommand = CreateCommand(ExportScheduleAsync, null, "Ошибка экспорта расписания");
        ImportScheduleCommand = CreateCommand(ImportScheduleAsync, null, "Ошибка импорта расписания");
    }

    protected override async void OnFirstTimeLoaded()
    {
        await LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Загружаем справочные данные параллельно
            var teachersTask = LoadTeachersAsync();
            var groupsTask = LoadGroupsAsync();
            var periodsTask = LoadPeriodsAsync();
            var roomsTask = LoadRoomsAsync();
            var slotsTask = LoadScheduleAsync();

            await Task.WhenAll(teachersTask, groupsTask, periodsTask, roomsTask, slotsTask);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки начальных данных расписания");
            ShowError("Ошибка загрузки данных расписания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadScheduleAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            HasErrors = false;

            var slots = await _scheduleSlotService.GetAllAsync();
            var slotViewModels = slots.Select(s => new ScheduleSlotViewModel(s)).ToList();

            _scheduleSlotsSource.Clear();
            _scheduleSlotsSource.AddRange(slotViewModels);
            
            TotalSlots = slotViewModels.Count;

            LogInfo("Загружено {Count} слотов расписания", slotViewModels.Count);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки расписания");
            ErrorMessage = "Не удалось загрузить расписание";
            HasErrors = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTeachersAsync()
    {
        try
        {
            var teachers = await _teacherService.GetAllAsync();
            AvailableTeachers.Clear();
            foreach (var teacher in teachers)
            {
                AvailableTeachers.Add(teacher);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки преподавателей");
        }
    }

    private async Task LoadGroupsAsync()
    {
        try
        {
            var groups = await _groupService.GetAllAsync();
            AvailableGroups.Clear();
            foreach (var group in groups)
            {
                AvailableGroups.Add(group);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки групп");
        }
    }

    private async Task LoadPeriodsAsync()
    {
        try
        {
            var periods = await _academicPeriodService.GetAllAsync();
            AvailablePeriods.Clear();
            foreach (var period in periods)
            {
                AvailablePeriods.Add(period);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки академических периодов");
        }
    }

    private async Task LoadRoomsAsync()
    {
        try
        {
            var slots = await _scheduleSlotService.GetAllAsync();
            var rooms = slots.Where(s => !string.IsNullOrEmpty(s.Room))
                           .Select(s => s.Room!)
                           .Distinct()
                           .OrderBy(r => r)
                           .ToList();
            
            AvailableRooms.Clear();
            foreach (var room in rooms)
            {
                AvailableRooms.Add(room);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки аудиторий");
        }
    }

    private async Task RefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            await LoadInitialDataAsync();
            ShowSuccess("Данные расписания обновлены");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка обновления данных");
            ShowError("Ошибка обновления данных");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task AddSlotAsync()
    {
        try
        {
            IsLoading = true;

            var newSlot = new ScheduleSlot
            {
                Uid = Guid.NewGuid(),
                DayOfWeek = SelectedDayOfWeek,
                StartTime = TimeSpan.FromHours(9), // Значение по умолчанию
                EndTime = TimeSpan.FromHours(10),
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddMonths(6)
            };

            var result = await _dialogService.ShowScheduleSlotEditDialogAsync(newSlot);
            if (result != null && result is ScheduleSlot createdSlot)
            {
                var savedSlot = await _scheduleSlotService.CreateAsync(createdSlot);
                var slotViewModel = new ScheduleSlotViewModel(savedSlot);
                _scheduleSlotsSource.Add(slotViewModel);
                TotalSlots++;

                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.SendNotificationAsync(
                    currentPerson?.Uid ?? Guid.Empty,
                    "Слот расписания создан",
                    $"Новый слот расписания успешно создан",
                    ViridiscaUi.Domain.Models.System.Enums.NotificationType.Success,
                    ViridiscaUi.Domain.Models.System.Enums.NotificationPriority.Normal);

                ShowSuccess("Слот расписания создан");
                LogInfo("Schedule slot created successfully");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create schedule slot");
            ShowError("Не удалось создать слот расписания. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task EditSlotAsync(ScheduleSlotViewModel slotViewModel)
    {
        if (slotViewModel == null) return;

        try
        {
            IsLoading = true;

            var slot = new ScheduleSlot
            {
                Uid = slotViewModel.Uid,
                CourseInstanceUid = slotViewModel.CourseInstanceUid,
                DayOfWeek = slotViewModel.DayOfWeek,
                StartTime = slotViewModel.StartTime,
                EndTime = slotViewModel.EndTime,
                Room = slotViewModel.Room,
                ValidFrom = slotViewModel.ValidFrom,
                ValidTo = slotViewModel.ValidTo
            };

            var result = await _dialogService.ShowScheduleSlotEditDialogAsync(slot);
            if (result != null && result is ScheduleSlot updatedSlot)
            {
                await _scheduleSlotService.UpdateAsync(updatedSlot);
                await LoadScheduleAsync();

                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.SendNotificationAsync(
                    currentPerson?.Uid ?? Guid.Empty,
                    "Слот расписания обновлен",
                    $"Слот расписания успешно обновлен",
                    ViridiscaUi.Domain.Models.System.Enums.NotificationType.Success,
                    ViridiscaUi.Domain.Models.System.Enums.NotificationPriority.Normal);

                ShowSuccess("Слот расписания обновлен");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Concurrency conflict while updating schedule slot");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update schedule slot");
            ShowError("Не удалось обновить слот расписания. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteSlotAsync(ScheduleSlotViewModel slotViewModel)
    {
        if (slotViewModel == null) return;

        try
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить слот расписания?");

            if (confirmed == DialogResult.Yes)
            {
                await _scheduleSlotService.DeleteAsync(slotViewModel.Uid);
                _scheduleSlotsSource.Remove(slotViewModel);
                TotalSlots--;

                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.SendNotificationAsync(
                    currentPerson?.Uid ?? Guid.Empty,
                    "Слот расписания удален",
                    $"Слот расписания успешно удален",
                    ViridiscaUi.Domain.Models.System.Enums.NotificationType.Success,
                    ViridiscaUi.Domain.Models.System.Enums.NotificationPriority.Normal);

                ShowSuccess("Слот расписания удален");
                LogInfo("Schedule slot deleted successfully");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete schedule slot");
            ShowError("Не удалось удалить слот расписания. Попробуйте еще раз.");
        }
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedTeacherUid = null;
        SelectedGroupUid = null;
        SelectedPeriodUid = null;
        SelectedRoom = null;
        SelectedDayOfWeek = DateTime.Today.DayOfWeek;
        
        await Task.CompletedTask;
    }

    private async Task ExportScheduleAsync()
    {
        try
        {
            IsLoading = true;
            
            var fileName = await _scheduleSlotService.ExportScheduleAsync(StartDate, EndDate);
            
            var currentPerson = await _authService.GetCurrentPersonAsync();
            await _notificationService.SendNotificationAsync(
                currentPerson?.Uid ?? Guid.Empty,
                "Экспорт завершен",
                $"Расписание экспортировано в файл {fileName}",
                ViridiscaUi.Domain.Models.System.Enums.NotificationType.Success,
                ViridiscaUi.Domain.Models.System.Enums.NotificationPriority.Normal);

            ShowSuccess($"Расписание экспортировано в файл {fileName}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка экспорта расписания");
            ShowError("Ошибка экспорта расписания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ImportScheduleAsync()
    {
        try
        {
            IsLoading = true;
            
            var filePath = await _dialogService.ShowFilePickerAsync("Выберите файл расписания", new[] { "*.xlsx", "*.csv" });
            if (!string.IsNullOrEmpty(filePath))
            {
                var importResult = await _scheduleSlotService.ImportScheduleAsync(filePath);
                
                if (importResult.IsSuccess)
                {
                    await LoadScheduleAsync();
                    
                    var currentPerson = await _authService.GetCurrentPersonAsync();
                    await _notificationService.SendNotificationAsync(
                        currentPerson?.Uid ?? Guid.Empty,
                        "Импорт завершен",
                        $"Импортировано {importResult.ImportedCount} слотов расписания",
                        ViridiscaUi.Domain.Models.System.Enums.NotificationType.Success,
                        ViridiscaUi.Domain.Models.System.Enums.NotificationPriority.Normal);

                    ShowSuccess($"Импортировано {importResult.ImportedCount} слотов расписания");
                }
                else
                {
                    ShowError($"Ошибка импорта: {string.Join(", ", importResult.Errors)}");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка импорта расписания");
            ShowError("Ошибка импорта расписания");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Func<ScheduleSlotViewModel, bool> CreateFilterPredicate((string searchText, Guid? teacherUid, Guid? groupUid, Guid? periodUid, string? room, DayOfWeek dayOfWeek) filters)
    {
        return slot =>
        {
            // Поиск по тексту
            if (!string.IsNullOrWhiteSpace(filters.searchText))
            {
                var searchLower = filters.searchText.ToLowerInvariant();
                if (!slot.Room.ToLowerInvariant().Contains(searchLower))
                    return false;
            }

            // Фильтр по преподавателю
            if (filters.teacherUid.HasValue)
            {
                // Здесь нужно проверить через CourseInstance
                // Пока упрощенная проверка
            }

            // Фильтр по группе
            if (filters.groupUid.HasValue)
            {
                // Здесь нужно проверить через CourseInstance
                // Пока упрощенная проверка
            }

            // Фильтр по периоду
            if (filters.periodUid.HasValue)
            {
                // Здесь нужно проверить через CourseInstance
                // Пока упрощенная проверка
            }

            // Фильтр по аудитории
            if (!string.IsNullOrWhiteSpace(filters.room))
            {
                if (!slot.Room.Equals(filters.room, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            // Фильтр по дню недели
            if (slot.DayOfWeek != filters.dayOfWeek)
                return false;

            return true;
        };
    }

    /// <summary>
    /// Показывает сообщение об успехе
    /// </summary>
    private new void ShowSuccess(string message)
    {
        _notificationService?.ShowSuccess(message);
    }

    /// <summary>
    /// Показывает сообщение об ошибке
    /// </summary>
    private new void ShowError(string message)
    {
        _notificationService?.ShowError(message);
    }

    /// <summary>
    /// Логирует информационное сообщение
    /// </summary>
    private void LogInfo(string message)
    {
        StatusLogger.LogInfo(message, "ScheduleViewModel");
    }

    /// <summary>
    /// Логирует ошибку
    /// </summary>
    private void LogError(Exception ex, string message)
    {
        StatusLogger.LogError($"{message}: {ex.Message}", "ScheduleViewModel");
    }
}

/// <summary>
/// ViewModel для слота расписания
/// </summary>
public class ScheduleSlotViewModel : ReactiveObject
{
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public Guid CourseInstanceUid { get; set; }
    [Reactive] public DayOfWeek DayOfWeek { get; set; }
    [Reactive] public TimeSpan StartTime { get; set; }
    [Reactive] public TimeSpan EndTime { get; set; }
    [Reactive] public string Room { get; set; } = string.Empty;
    [Reactive] public DateTime ValidFrom { get; set; }
    [Reactive] public DateTime ValidTo { get; set; }
    [Reactive] public DateTime LastModifiedAt { get; set; }

    // Дополнительные свойства для отображения
    [Reactive] public string SubjectName { get; set; } = string.Empty;
    [Reactive] public string TeacherName { get; set; } = string.Empty;
    [Reactive] public string GroupName { get; set; } = string.Empty;

    public ScheduleSlotViewModel() { }

    public ScheduleSlotViewModel(ScheduleSlot slot)
    {
        Uid = slot.Uid;
        CourseInstanceUid = slot.CourseInstanceUid;
        DayOfWeek = slot.DayOfWeek;
        StartTime = slot.StartTime;
        EndTime = slot.EndTime;
        Room = slot.Room ?? string.Empty;
        ValidFrom = slot.ValidFrom;
        ValidTo = slot.ValidTo;
        LastModifiedAt = slot.LastModifiedAt.HasValue ? slot.LastModifiedAt.Value : DateTime.UtcNow;

        // Дополнительная информация будет загружена отдельно
        SubjectName = "Загрузка...";
        TeacherName = "Загрузка...";
        GroupName = "Загрузка...";
    }

    public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public string DayOfWeekName => DayOfWeek.ToString();
    public string ValidityPeriod => $"{ValidFrom:dd.MM.yyyy} - {ValidTo:dd.MM.yyyy}";
} 