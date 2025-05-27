using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная страница приложения
/// Отображает общую информацию, учет и быстрый доступ к основным функциям
/// </summary>
[Route("home", 
    DisplayName = "Главная", 
    IconKey = "Home", 
    Order = 1,
    Group = "Main",
    Description = "Главная страница с общей информацией и учетными данными",
    Tags = new[] { "main", "dashboard", "overview" })]
public class HomeViewModel : RoutableViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;

    #region Properties

    /// <summary>
    /// Заголовок главной страницы
    /// </summary>
    [Reactive] public string Title { get; set; } = "Добро пожаловать в ViridiscaUi LMS";

    /// <summary>
    /// Описание главной страницы
    /// </summary>
    [Reactive] public string Description { get; set; } = "Система управления обучением для современного образования";

    /// <summary>
    /// Информация о текущем пользователе
    /// </summary>
    [Reactive] public CurrentUserInfo? CurrentUser { get; set; }

    /// <summary>
    /// Общий обзор системы
    /// </summary>
    [Reactive] public SystemOverview SystemOverview { get; set; } = new();

    /// <summary>
    /// Быстрые ссылки для пользователя
    /// </summary>
    public ObservableCollection<QuickLink> QuickLinks { get; } = new();

    /// <summary>
    /// Последние новости
    /// </summary>
    public ObservableCollection<NewsItem> LatestNews { get; } = new();

    /// <summary>
    /// Уведомления пользователя
    /// </summary>
    public ObservableCollection<UserNotification> Notifications { get; } = new();

    /// <summary>
    /// Предстоящие события
    /// </summary>
    public ObservableCollection<UpcomingEvent> UpcomingEvents { get; } = new();

    /// <summary>
    /// Статистика по ролям
    /// </summary>
    [Reactive] public RoleSpecificStats? RoleStats { get; set; }

    /// <summary>
    /// Последние активности
    /// </summary>
    public ObservableCollection<RecentActivity> RecentActivities { get; } = new();

    /// <summary>
    /// Команда навигации к курсам
    /// </summary>
    public ReactiveCommand<Unit, Unit> NavigateToCoursesCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к студентам
    /// </summary>
    public ReactiveCommand<Unit, Unit> NavigateToStudentsCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к заданиям
    /// </summary>
    public ReactiveCommand<Unit, Unit> NavigateToAssignmentsCommand { get; private set; } = null!;

    // Свойства для статистических карточек
    public string TotalStudents => SystemOverview?.TotalStudents.ToString() ?? "0";
    public string ActiveCourses => SystemOverview?.ActiveCourses.ToString() ?? "0";
    public string TotalTeachers => SystemOverview?.TotalTeachers.ToString() ?? "0";
    public string PendingAssignments => RoleStats?.Stats.GetValueOrDefault("PendingAssignments", 0).ToString() ?? "0";
    public string WelcomeMessage => CurrentUser?.WelcomeMessage ?? "Добро пожаловать в систему!";

    #endregion

    #region Commands

    /// <summary>
    /// Команда обновления данных
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к быстрой ссылке
    /// </summary>
    public ReactiveCommand<QuickLink, Unit> NavigateToQuickLinkCommand { get; private set; } = null!;

    /// <summary>
    /// Команда просмотра новости
    /// </summary>
    public ReactiveCommand<NewsItem, Unit> ViewNewsCommand { get; private set; } = null!;

    /// <summary>
    /// Команда отметки уведомления как прочитанного
    /// </summary>
    public ReactiveCommand<UserNotification, Unit> MarkNotificationReadCommand { get; private set; } = null!;

    #endregion

    /// <summary>
    /// Конструктор
    /// </summary>
    public HomeViewModel(
        IScreen hostScreen,
        INotificationService notificationService,
        IUserService userService) : base(hostScreen)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        InitializeCommands();
        SetupPropertyNotifications();
        StatusLogger.LogInfo("HomeViewModel инициализирована", GetType().Name);
    }

    #region Lifecycle Methods

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await LoadHomeDataAsync();
        StatusLogger.LogInfo("Данные главной страницы загружены", GetType().Name);
    }

    #endregion

    #region Private Methods

    private void InitializeCommands()
    {
        RefreshCommand = CreateCommand(LoadHomeDataAsync, null, "Ошибка при обновлении данных");

        NavigateToQuickLinkCommand = CreateCommand<QuickLink>(NavigateToQuickLinkAsync, null, "Ошибка навигации");

        ViewNewsCommand = CreateCommand<NewsItem>(ViewNewsAsync, null, "Ошибка при просмотре новости");

        MarkNotificationReadCommand = CreateCommand<UserNotification>(MarkNotificationReadAsync, null, "Ошибка при отметке уведомления как прочитанного");

        NavigateToCoursesCommand = CreateCommand(NavigateToCoursesAsync, null, "Ошибка при навигации к курсам");

        NavigateToStudentsCommand = CreateCommand(NavigateToStudentsAsync, null, "Ошибка при навигации к студентам");

        NavigateToAssignmentsCommand = CreateCommand(NavigateToAssignmentsAsync, null, "Ошибка при навигации к заданиям");
    }

    private void SetupPropertyNotifications()
    {
        // Уведомления об изменении computed properties
        this.WhenAnyValue(x => x.SystemOverview)
            .Subscribe(_ => 
            {
                this.RaisePropertyChanged(nameof(TotalStudents));
                this.RaisePropertyChanged(nameof(ActiveCourses));
                this.RaisePropertyChanged(nameof(TotalTeachers));
            });

        this.WhenAnyValue(x => x.RoleStats)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(PendingAssignments)));

        this.WhenAnyValue(x => x.CurrentUser)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(WelcomeMessage)));
    }

    private async Task LoadHomeDataAsync()
    {
        try
        {
            SetLoading(true, "Загрузка данных главной страницы...");

            // Загружаем информацию о текущем пользователе
            CurrentUser = await LoadCurrentUserAsync();

            // Загружаем общий обзор системы
            SystemOverview = await LoadSystemOverviewAsync();

            // Загружаем быстрые ссылки для ролей пользователей
            var quickLinks = await LoadQuickLinksAsync();
            QuickLinks.Clear();
            foreach (var link in quickLinks)
            {
                QuickLinks.Add(link);
            }

            // Загружаем последние новости
            var news = await LoadLatestNewsAsync();
            LatestNews.Clear();
            foreach (var newsItem in news)
            {
                LatestNews.Add(newsItem);
            }

            // Загружаем уведомления пользователя
            var notifications = await LoadNotificationsAsync();
            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }

            // Загружаем предстоящие события
            var events = await LoadUpcomingEventsAsync();
            UpcomingEvents.Clear();
            foreach (var eventItem in events)
            {
                UpcomingEvents.Add(eventItem);
            }

            // Загружаем статистику по ролям
            if (CurrentUser != null)
            {
                RoleStats = await LoadRoleStatsAsync(CurrentUser.PrimaryRole);
            }

            // Загружаем последние активности
            var activities = await LoadRecentActivitiesAsync();
            RecentActivities.Clear();
            foreach (var activity in activities)
            {
                RecentActivities.Add(activity);
            }

            ShowSuccess("Данные главной страницы обновлены");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке данных главной страницы", ex);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task<CurrentUserInfo?> LoadCurrentUserAsync()
    {
        // TODO: Implement actual user loading
        await Task.Delay(100);
        return new CurrentUserInfo
        {
            Id = Guid.NewGuid(),
            FirstName = "Пользователь",
            LastName = "Системы",
            Email = "user@viridisca.com",
            Roles = new[] { "Student" },
            PrimaryRole = "Student",
            LastLoginAt = DateTime.Now
        };
    }

    private async Task<SystemOverview> LoadSystemOverviewAsync()
    {
        // TODO: Implement actual system overview loading
        await Task.Delay(100);
        return new SystemOverview
        {
            TotalUsers = 1250,
            TotalStudents = 980,
            TotalTeachers = 85,
            TotalCourses = 45,
            ActiveCourses = 38,
            OnlineUsers = 156,
            LastUpdated = DateTime.Now,
            SystemStatus = "Работает"
        };
    }

    private async Task<QuickLink[]> LoadQuickLinksAsync()
    {
        // TODO: Implement actual quick links loading
        await Task.Delay(100);
        return new[]
        {
            new QuickLink { Title = "Моё курси", Route = "courses", IconKey = "Book", Color = "#2196F3" },
            new QuickLink { Title = "Расписание", Route = "schedule", IconKey = "Calendar", Color = "#4CAF50" },
            new QuickLink { Title = "Оценки", Route = "grades", IconKey = "Grade", Color = "#FF9800" }
        };
    }

    private async Task<NewsItem[]> LoadLatestNewsAsync()
    {
        // TODO: Implement actual news loading
        await Task.Delay(100);
        return Array.Empty<NewsItem>();
    }

    private async Task<UserNotification[]> LoadNotificationsAsync()
    {
        // TODO: Implement actual notifications loading
        await Task.Delay(100);
        return Array.Empty<UserNotification>();
    }

    private async Task<UpcomingEvent[]> LoadUpcomingEventsAsync()
    {
        // TODO: Implement actual events loading
        await Task.Delay(100);
        return Array.Empty<UpcomingEvent>();
    }

    private async Task<RoleSpecificStats?> LoadRoleStatsAsync(string role)
    {
        // TODO: Implement actual role stats loading
        await Task.Delay(100);
        return new RoleSpecificStats
        {
            RoleName = role,
            Stats = new Dictionary<string, object>
            {
                ["CompletedCourses"] = 5,
                ["AverageGrade"] = 4.2,
                ["AttendanceRate"] = 0.95,
                ["PendingAssignments"] = 12
            },
            KeyMetrics = new[] { "CompletedCourses", "AverageGrade", "AttendanceRate", "PendingAssignments" }
        };
    }

    private async Task<RecentActivity[]> LoadRecentActivitiesAsync()
    {
        // TODO: Implement actual activities loading
        await Task.Delay(100);
        return Array.Empty<RecentActivity>();
    }

    private async Task NavigateToQuickLinkAsync(QuickLink link)
    {
        try
        {
            await NavigateToAsync(link.Route);
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при навигации к '{link.Title}'", ex);
        }
    }

    private async Task ViewNewsAsync(NewsItem news)
    {
        try
        {
            await NavigateToAsync($"news/{news.Id}");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при просмотре новости '{news.Title}'", ex);
        }
    }

    private async Task MarkNotificationReadAsync(UserNotification notification)
    {
        try
        {
            SetLoading(true, "Отметка уведомления как прочитанного...");

            // TODO: Implement actual notification marking
            await Task.Delay(500);
            notification.IsRead = true;

            ShowInfo($"Уведомление '{notification.Title}' отмечено как прочитанное");
        }
        catch (Exception ex)
        {
            SetError($"Ошибка при отметке уведомления '{notification.Title}'", ex);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task NavigateToCoursesAsync()
    {
        try
        {
            await NavigateToAsync("courses");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при навигации к курсам", ex);
        }
    }

    private async Task NavigateToStudentsAsync()
    {
        try
        {
            await NavigateToAsync("students");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при навигации к студентам", ex);
        }
    }

    private async Task NavigateToAssignmentsAsync()
    {
        try
        {
            await NavigateToAsync("assignments");
        }
        catch (Exception ex)
        {
            SetError("Ошибка при навигации к заданиям", ex);
        }
    }

    #endregion
}

#region Models

/// <summary>
/// Информация о текущем пользователе
/// </summary>
public class CurrentUserInfo
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string? PhotoUrl { get; set; }
    public DateTime LastLoginAt { get; set; }
    public string PrimaryRole { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
    public string WelcomeMessage => $"Добро пожаловать, {FirstName}!";
}

/// <summary>
/// Общий обзор системы
/// </summary>
public class SystemOverview
{
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int OnlineUsers { get; set; }
    public DateTime LastUpdated { get; set; }
    public string SystemStatus { get; set; } = "Работает";
}

/// <summary>
/// Быстрая ссылка
/// </summary>
public class QuickLink
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsNew { get; set; }
}

/// <summary>
/// Новость
/// </summary>
public class NewsItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public NewsCategory Category { get; set; }
    public bool IsImportant { get; set; }
    public int ViewCount { get; set; }
}

/// <summary>
/// Уведомление пользователя
/// </summary>
public class UserNotification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public string? IconKey { get; set; }
}

/// <summary>
/// Предстоящее событие
/// </summary>
public class UpcomingEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public bool IsRequired { get; set; }
    public int DaysUntil { get; set; }
    public string? IconKey { get; set; }
}

/// <summary>
/// Статистика по ролям
/// </summary>
public class RoleSpecificStats
{
    public string RoleName { get; set; } = string.Empty;
    public Dictionary<string, object> Stats { get; set; } = new();
    public string[] KeyMetrics { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Последняя активность
/// </summary>
public class RecentActivity
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ActivityType Type { get; set; }
    public string? UserName { get; set; }
    public string? EntityName { get; set; }
    public string? IconKey { get; set; }
}

/// <summary>
/// Категория новости
/// </summary>
public enum NewsCategory
{
    General,
    Academic,
    Administrative,
    Events,
    System
}

/// <summary>
/// Приоритет уведомления
/// </summary>
public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>
/// Тип уведомления
/// </summary>
public enum NotificationType
{
    Info,
    Warning,
    Success,
    Error,
    Reminder
}

/// <summary>
/// Тип события
/// </summary>
public enum EventType
{
    Academic,
    Administrative,
    Social,
    Meeting,
    Deadline,
    Holiday
}

/// <summary>
/// Тип активности
/// </summary>
public enum ActivityType
{
    Login,
    CourseAccess,
    GradeUpdate,
    Assignment,
    Message,
    System
}

#endregion
