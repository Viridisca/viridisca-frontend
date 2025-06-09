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
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System.Enums;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная страница приложения
/// Отображает общую информацию, учет и быстрый доступ к основным функциям
/// </summary>
[Route("home", 
    DisplayName = "Главная", 
    IconKey = "Home", 
    Order = 0,
    Group = "Основное",
    ShowInMenu = true,
    Description = "Главная страница системы",
    Tags = new[] { "main", "dashboard", "overview" })]
public class HomeViewModel : RoutableViewModelBase
{
    private readonly IPersonService _personService;
    private readonly IAuthService _authService;
    private readonly INotificationService _notificationService;
    private readonly IScheduleSlotService _scheduleSlotService;
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGradeService _gradeService;
    private readonly IDialogService _dialogService;

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
    /// Имя пользователя
    /// </summary>
    [Reactive] public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    [Reactive] public string UserRole { get; set; } = string.Empty;

    /// <summary>
    /// Системный обзор с основной статистикой
    /// </summary>
    [Reactive] public SystemOverview? SystemOverviewData { get; set; }

    /// <summary>
    /// Статистические карточки для отображения
    /// </summary>
    [Reactive] public ObservableCollection<StatisticCardViewModel> SystemOverview { get; set; } = new();

    /// <summary>
    /// Быстрые ссылки для навигации
    /// </summary>
    public ObservableCollection<QuickLinkViewModel> QuickLinks { get; } = new();

    /// <summary>
    /// Последние новости
    /// </summary>
    public ObservableCollection<NewsItemViewModel> LatestNews { get; } = new();

    /// <summary>
    /// Последние новости (алиас для совместимости)
    /// </summary>
    public ObservableCollection<NewsItemViewModel> News => LatestNews;

    /// <summary>
    /// Уведомления пользователя
    /// </summary>
    public ObservableCollection<NotificationItemViewModel> Notifications { get; } = new();

    /// <summary>
    /// Предстоящие события
    /// </summary>
    public ObservableCollection<EventItemViewModel> UpcomingEvents { get; } = new();

    /// <summary>
    /// Статистика по ролям
    /// </summary>
    [Reactive] public RoleSpecificStats? RoleStatsData { get; set; }

    /// <summary>
    /// Статистические карточки для ролей
    /// </summary>
    [Reactive] public ObservableCollection<StatisticCardViewModel> RoleStats { get; set; } = new();

    /// <summary>
    /// Последние активности
    /// </summary>
    public ObservableCollection<ActivityItemViewModel> RecentActivities { get; } = new();

    /// <summary>
    /// Количество непрочитанных уведомлений
    /// </summary>
    [Reactive] public int UnreadNotificationsCount { get; set; }

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
    public string TotalStudents => SystemOverviewData?.TotalStudents.ToString() ?? "0";
    public string ActiveCourses => SystemOverviewData?.ActiveCourses.ToString() ?? "0";
    public string TotalTeachers => SystemOverviewData?.TotalTeachers.ToString() ?? "0";
    public string PendingAssignments => RoleStatsData?.Stats.GetValueOrDefault("PendingAssignments", 0).ToString() ?? "0";
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
    public ReactiveCommand<QuickLinkViewModel, Unit> NavigateToQuickLinkCommand { get; private set; } = null!;

    /// <summary>
    /// Команда просмотра новости
    /// </summary>
    public ReactiveCommand<NewsItemViewModel, Unit> ViewNewsCommand { get; private set; } = null!;

    /// <summary>
    /// Команда отметки уведомления как прочитанного
    /// </summary>
    public ReactiveCommand<NotificationItemViewModel, Unit> MarkNotificationReadCommand { get; private set; } = null!;

    #endregion

    /// <summary>
    /// Конструктор
    /// </summary>
    public HomeViewModel(
        IScreen hostScreen,
        IPersonService personService,
        IAuthService authService,
        INotificationService notificationService,
        IScheduleSlotService scheduleSlotService,
        IStudentService studentService,
        ITeacherService teacherService,
        ICourseInstanceService courseInstanceService,
        IEnrollmentService enrollmentService,
        IGradeService gradeService,
        IDialogService dialogService) : base(hostScreen)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _scheduleSlotService = scheduleSlotService ?? throw new ArgumentNullException(nameof(scheduleSlotService));
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
        _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

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

        NavigateToQuickLinkCommand = CreateCommand<QuickLinkViewModel>(NavigateToQuickLinkAsync, null, "Ошибка навигации");

        ViewNewsCommand = CreateCommand<NewsItemViewModel>(ViewNewsAsync, null, "Ошибка при просмотре новости");

        MarkNotificationReadCommand = CreateCommand<NotificationItemViewModel>(MarkNotificationReadAsync, null, "Ошибка при отметке уведомления как прочитанного");

        NavigateToCoursesCommand = CreateCommand(NavigateToCoursesAsync, null, "Ошибка при навигации к курсам");

        NavigateToStudentsCommand = CreateCommand(NavigateToStudentsAsync, null, "Ошибка при навигации к студентам");

        NavigateToAssignmentsCommand = CreateCommand(NavigateToAssignmentsAsync, null, "Ошибка при навигации к заданиям");
    }

    private void SetupPropertyNotifications()
    {
        // Уведомления об изменении computed properties
        this.WhenAnyValue(x => x.SystemOverviewData)
            .Subscribe(_ => 
            {
                this.RaisePropertyChanged(nameof(TotalStudents));
                this.RaisePropertyChanged(nameof(ActiveCourses));
                this.RaisePropertyChanged(nameof(TotalTeachers));
            });

        this.WhenAnyValue(x => x.RoleStatsData)
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
            await LoadUserDataAsync();

            // Загружаем общий обзор системы
            await LoadSystemOverviewAsync();

            // Загружаем быстрые ссылки для ролей пользователей
            await LoadQuickLinksAsync();

            // Загружаем последние новости
            await LoadNewsAsync();

            // Загружаем уведомления пользователя
            await LoadNotificationsAsync();

            // Загружаем предстоящие события
            await LoadUpcomingEventsAsync();

            // Загружаем статистику по ролям
            await LoadRoleStatsAsync();

            // Загружаем последние активности
            await LoadRecentActivitiesAsync();

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

    private async Task LoadUserDataAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                CurrentUser = new CurrentUserInfo
                {
                    Id = currentPerson.Uid,
                    FirstName = currentPerson.FirstName,
                    LastName = currentPerson.LastName,
                    Email = currentPerson.Email ?? string.Empty,
                    Roles = currentPerson.PersonRoles?.Select(pr => pr.Role?.Name ?? string.Empty).Where(r => !string.IsNullOrEmpty(r)).ToArray() ?? Array.Empty<string>(),
                    PrimaryRole = currentPerson.PersonRoles?.FirstOrDefault()?.Role?.Name ?? "Пользователь",
                    LastLoginAt = DateTime.UtcNow
                };
                
                UserName = $"{currentPerson.FirstName} {currentPerson.LastName}";
                UserRole = currentPerson.PersonRoles?.FirstOrDefault()?.Role?.Name ?? "Пользователь";
                
                LogInfo("Данные пользователя загружены: {UserName}", UserName);
            }
            else
            {
                UserName = "Гость";
                UserRole = "Не авторизован";
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки данных пользователя");
            UserName = "Ошибка загрузки";
            UserRole = "Неизвестно";
        }
    }

    private async Task LoadSystemOverviewAsync()
    {
        try
        {
            // Загружаем основную статистику системы
            var studentsCount = await GetStudentsCountAsync();
            var teachersCount = await GetTeachersCountAsync();
            var coursesCount = await GetCoursesCountAsync();
            var activeSessionsCount = await GetActiveSessionsCountAsync();

            // Создаем объект SystemOverview
            SystemOverviewData = new SystemOverview
            {
                TotalStudents = studentsCount,
                TotalTeachers = teachersCount,
                TotalCourses = coursesCount,
                ActiveCourses = coursesCount, // Пока считаем все курсы активными
                OnlineUsers = activeSessionsCount,
                LastUpdated = DateTime.UtcNow,
                SystemStatus = "Работает"
            };

            // Обновляем статистические карточки
            SystemOverview.Clear();
            SystemOverview.Add(new StatisticCardViewModel("Студенты", studentsCount.ToString(), "AccountMultiple", "Общее количество студентов"));
            SystemOverview.Add(new StatisticCardViewModel("Преподаватели", teachersCount.ToString(), "AccountTie", "Общее количество преподавателей"));
            SystemOverview.Add(new StatisticCardViewModel("Курсы", coursesCount.ToString(), "BookOpenPageVariant", "Общее количество курсов"));
            SystemOverview.Add(new StatisticCardViewModel("Активные сессии", activeSessionsCount.ToString(), "AccountClock", "Количество активных пользователей"));

            LogInfo("Обзор системы загружен");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки обзора системы");
        }
    }

    private async Task LoadQuickLinksAsync()
    {
        try
        {
            QuickLinks.Clear();
            
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                var userRoles = currentPerson.PersonRoles?.Select(pr => pr.Role?.Name).Where(r => r != null).ToArray() ?? new string[0];
                
                // Добавляем быстрые ссылки в зависимости от роли
                if (userRoles.Contains("Admin") || userRoles.Contains("SystemAdmin"))
                {
                    QuickLinks.Add(new QuickLinkViewModel("Управление пользователями", "AccountMultiple", "students"));
                    QuickLinks.Add(new QuickLinkViewModel("Системные настройки", "Cog", "system-settings"));
                }
                
                if (userRoles.Contains("Teacher") || userRoles.Contains("Admin"))
                {
                    QuickLinks.Add(new QuickLinkViewModel("Мои курсы", "BookOpenPageVariant", "courses"));
                    QuickLinks.Add(new QuickLinkViewModel("Расписание", "CalendarClock", "schedule"));
                }
                
                if (userRoles.Contains("Student"))
                {
                    QuickLinks.Add(new QuickLinkViewModel("Мои оценки", "StarCircle", "grades"));
                    QuickLinks.Add(new QuickLinkViewModel("Задания", "ClipboardText", "assignments"));
                }
                
                // Общие ссылки для всех
                QuickLinks.Add(new QuickLinkViewModel("Библиотека", "LibraryShelves", "library"));
                QuickLinks.Add(new QuickLinkViewModel("Профиль", "Account", "profile"));
            }

            LogInfo("Быстрые ссылки загружены: {Count}", QuickLinks.Count);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки быстрых ссылок");
        }
    }

    private async Task LoadNewsAsync()
    {
        try
        {
            News.Clear();
            
            // Загружаем последние новости из системы уведомлений
            var notifications = await _notificationService.GetRecentNotificationsAsync(10);
            foreach (var notification in notifications.Take(5))
            {
                News.Add(new NewsItemViewModel
                {
                    Title = notification.Title,
                    Content = notification.Message,
                    Date = notification.CreatedAt,
                    Author = "Система"
                });
            }

            LogInfo("Новости загружены: {Count}", News.Count);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки новостей");
        }
    }

    private async Task LoadNotificationsAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                // Заглушка для уведомлений - создаем тестовые данные
                Notifications.Clear();
                
                // Добавляем несколько тестовых уведомлений
                    Notifications.Add(new NotificationItemViewModel
                    {
                    Id = Guid.NewGuid(),
                    Title = "Добро пожаловать!",
                    Message = "Добро пожаловать в систему ViridiscaUi LMS",
                    Date = DateTime.Now.AddHours(-1),
                    Type = NotificationType.Info,
                    Priority = NotificationPriority.Normal,
                    IsRead = false
                    });

                UnreadNotificationsCount = Notifications.Count(n => !n.IsRead);
            }

            LogInfo("Уведомления загружены");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки уведомлений");
        }
    }

    private async Task LoadUpcomingEventsAsync()
    {
        try
        {
            UpcomingEvents.Clear();
            
            // Загружаем предстоящие события из расписания
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                var upcomingSlots = await _scheduleSlotService.GetUpcomingSlotsAsync(currentPerson.Uid, 5);
                foreach (var slot in upcomingSlots)
                {
                    UpcomingEvents.Add(new EventItemViewModel
                    {
                        Title = slot.CourseInstance?.Subject?.Name ?? "Занятие",
                        Description = $"Аудитория: {slot.Room}",
                        Date = GetNextOccurrence(slot.DayOfWeek, slot.StartTime),
                        Type = "Занятие"
                    });
                }
            }

            LogInfo("Предстоящие события загружены: {Count}", UpcomingEvents.Count);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки предстоящих событий");
        }
    }

    private async Task LoadRoleStatsAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                var userRoles = currentPerson.PersonRoles?.Select(pr => pr.Role?.Name).Where(r => r != null).ToArray() ?? new string[0];
                
                RoleStats.Clear();
                
                if (userRoles.Contains("Student"))
                {
                    var studentStats = await GetStudentStatsAsync(currentPerson.Uid);
                    RoleStats.Add(new StatisticCardViewModel("Мои курсы", studentStats.CoursesCount.ToString(), "BookOpenPageVariant", "Количество активных курсов"));
                    RoleStats.Add(new StatisticCardViewModel("Средний балл", studentStats.AverageGrade.ToString("F1"), "StarCircle", "Средний балл по всем предметам"));
                }
                
                if (userRoles.Contains("Teacher"))
                {
                    var teacherStats = await GetTeacherStatsAsync(currentPerson.Uid);
                    RoleStats.Add(new StatisticCardViewModel("Мои группы", teacherStats.GroupsCount.ToString(), "AccountMultiple", "Количество групп"));
                    RoleStats.Add(new StatisticCardViewModel("Студенты", teacherStats.StudentsCount.ToString(), "Account", "Общее количество студентов"));
                }
            }

            LogInfo("Статистика роли загружена");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки статистики роли");
        }
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            RecentActivities.Clear();
            
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                // Заглушка для активностей - создаем тестовые данные
                    RecentActivities.Add(new ActivityItemViewModel
                    {
                    Id = Guid.NewGuid(),
                    Title = "Вход в систему",
                    Description = "Пользователь вошел в систему",
                    Date = DateTime.Now.AddMinutes(-30),
                    Type = "Login",
                    UserName = $"{currentPerson.FirstName} {currentPerson.LastName}"
                    });
            }

            LogInfo("Последние активности загружены: {Count}", RecentActivities.Count);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки последних активностей");
        }
    }

    private async Task MarkNotificationReadAsync(NotificationItemViewModel notification)
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

    // Вспомогательные методы для получения статистики
    private async Task<int> GetStudentsCountAsync()
    {
        try
        {
            var students = await _studentService.GetAllAsync();
            return students.Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetTeachersCountAsync()
    {
        try
        {
            var teachers = await _teacherService.GetAllAsync();
            return teachers.Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetCoursesCountAsync()
    {
        try
        {
            var courses = await _courseInstanceService.GetAllAsync();
            return courses.Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetActiveSessionsCountAsync()
    {
        try
        {
            // Примерная реализация - можно расширить
            return await Task.FromResult(Random.Shared.Next(10, 50));
        }
        catch
        {
            return 0;
        }
    }

    private async Task<(int CoursesCount, double AverageGrade)> GetStudentStatsAsync(Guid studentPersonUid)
    {
        try
        {
            // Заглушка - возвращаем тестовые данные
            return (5, 4.2);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка получения статистики студента");
        }
        
        return (0, 0.0);
    }

    private async Task<(int GroupsCount, int StudentsCount)> GetTeacherStatsAsync(Guid teacherPersonUid)
    {
        try
        {
            // Заглушка - возвращаем тестовые данные
            return (3, 45);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка получения статистики преподавателя");
        }
        
        return (0, 0);
    }

    private DateTime GetNextOccurrence(DayOfWeek dayOfWeek, TimeSpan time)
    {
        var today = DateTime.Today;
        var daysUntilTarget = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0 && DateTime.Now.TimeOfDay > time)
        {
            daysUntilTarget = 7;
        }
        
        return today.AddDays(daysUntilTarget).Add(time);
    }

    /// <summary>
    /// Навигация к быстрой ссылке
    /// </summary>
    private async Task NavigateToQuickLinkAsync(QuickLinkViewModel quickLink)
    {
        if (quickLink == null) return;

        try
        {
            await NavigateToAsync(quickLink.Route);
            LogInfo("Навигация к быстрой ссылке: {Route}", quickLink.Route);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка навигации к быстрой ссылке: {Route}", quickLink.Route);
            ShowError($"Не удалось перейти к {quickLink.Title}");
        }
    }

    /// <summary>
    /// Просмотр новости
    /// </summary>
    private async Task ViewNewsAsync(NewsItemViewModel newsItem)
    {
        if (newsItem == null) return;

        try
        {
            // Здесь можно открыть диалог с подробностями новости
            // или перейти к странице новостей
            await _dialogService.ShowInfoAsync("Новость", newsItem.Content);
            LogInfo("Просмотр новости: {Title}", newsItem.Title);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при просмотре новости: {Title}", newsItem.Title);
            ShowError("Не удалось открыть новость");
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
    public string PrimaryRole { get; set; } = string.Empty;
    public DateTime LastLoginAt { get; set; }

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

/// <summary>
/// Статистическая карточка
/// </summary>
public class StatisticCardViewModel : ReactiveObject
{
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Value { get; set; } = string.Empty;
    [Reactive] public string IconKey { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string Color { get; set; } = "Primary";

    public StatisticCardViewModel() { }

    public StatisticCardViewModel(string title, string value, string iconKey, string description)
    {
        Title = title;
        Value = value;
        IconKey = iconKey;
        Description = description;
    }
}

/// <summary>
/// ViewModel для быстрой ссылки
/// </summary>
public class QuickLinkViewModel : ReactiveObject
{
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string Route { get; set; } = string.Empty;
    [Reactive] public string IconKey { get; set; } = string.Empty;
    [Reactive] public string Color { get; set; } = "Primary";
    [Reactive] public int Order { get; set; }
    [Reactive] public bool IsNew { get; set; }

    public QuickLinkViewModel() { }

    public QuickLinkViewModel(string title, string iconKey, string route)
    {
        Title = title;
        IconKey = iconKey;
        Route = route;
    }
}

/// <summary>
/// ViewModel для новости
/// </summary>
public class NewsItemViewModel : ReactiveObject
{
    [Reactive] public Guid Id { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Summary { get; set; } = string.Empty;
    [Reactive] public string Content { get; set; } = string.Empty;
    [Reactive] public DateTime Date { get; set; }
    [Reactive] public string Author { get; set; } = string.Empty;
    [Reactive] public string? ImageUrl { get; set; }
    [Reactive] public bool IsImportant { get; set; }
    [Reactive] public int ViewCount { get; set; }

    public string FormattedDate => Date.ToString("dd.MM.yyyy HH:mm");
}

/// <summary>
/// ViewModel для уведомления
/// </summary>
public class NotificationItemViewModel : ReactiveObject
{
    [Reactive] public Guid Id { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Message { get; set; } = string.Empty;
    [Reactive] public DateTime Date { get; set; }
    [Reactive] public NotificationPriority Priority { get; set; }
    [Reactive] public NotificationType Type { get; set; }
    [Reactive] public bool IsRead { get; set; }
    [Reactive] public string? ActionUrl { get; set; }
    [Reactive] public string? IconKey { get; set; }

    public string FormattedDate => Date.ToString("dd.MM.yyyy HH:mm");
    public string PriorityText => Priority.ToString();
    public string TypeText => Type.ToString();
}

/// <summary>
/// ViewModel для события
/// </summary>
public class EventItemViewModel : ReactiveObject
{
    [Reactive] public Guid Id { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public DateTime Date { get; set; }
    [Reactive] public string Location { get; set; } = string.Empty;
    [Reactive] public string Type { get; set; } = string.Empty;
    [Reactive] public bool IsRequired { get; set; }
    [Reactive] public string? IconKey { get; set; }

    public string FormattedDate => Date.ToString("dd.MM.yyyy HH:mm");
    public int DaysUntil => (Date.Date - DateTime.Today).Days;
}

/// <summary>
/// ViewModel для активности
/// </summary>
public class ActivityItemViewModel : ReactiveObject
{
    [Reactive] public Guid Id { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public DateTime Date { get; set; }
    [Reactive] public string Type { get; set; } = string.Empty;
    [Reactive] public string? UserName { get; set; }
    [Reactive] public string? EntityName { get; set; }
    [Reactive] public string? IconKey { get; set; }

    public string FormattedDate => Date.ToString("dd.MM.yyyy HH:mm");
}

#endregion
