using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса статистики
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IStudentService _studentService;
    private readonly ITeacherService _teacherService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly IAssignmentService _assignmentService;
    private readonly IGroupService _groupService;
    private readonly ISubjectService _subjectService;
    private readonly IDepartmentService _departmentService;
    private readonly INotificationService _notificationService;
    private readonly IPersonSessionService _personSessionService;
    private readonly ILogger<StatisticsService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public StatisticsService(
        IStudentService studentService,
        ITeacherService teacherService,
        ICourseInstanceService courseInstanceService,
        IAssignmentService assignmentService,
        IGroupService groupService,
        ISubjectService subjectService,
        IDepartmentService departmentService,
        INotificationService notificationService,
        IPersonSessionService personSessionService,
        ILogger<StatisticsService> logger,
        ApplicationDbContext dbContext)
    {
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        _teacherService = teacherService ?? throw new ArgumentNullException(nameof(teacherService));
        _courseInstanceService = courseInstanceService ?? throw new ArgumentNullException(nameof(courseInstanceService));
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _personSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Получает общую статистику системы
    /// </summary>
    public async Task<SystemStatistics> GetSystemStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Начинается загрузка системной статистики");

            // Параллельно загружаем все данные для оптимизации производительности
            var studentsTask = GetTotalStudentsAsync();
            var teachersTask = GetTotalTeachersAsync();
            var coursesTask = GetTotalCoursesAsync();
            var assignmentsTask = GetTotalAssignmentsAsync();
            var groupsTask = GetTotalGroupsAsync();
            var subjectsTask = GetTotalSubjectsAsync();
            var departmentsTask = GetTotalDepartmentsAsync();
            var activeCoursesTask = GetActiveCoursesAsync();
            var activeStudentsTask = GetActiveStudentsAsync();

            await Task.WhenAll(
                studentsTask, teachersTask, coursesTask, assignmentsTask,
                groupsTask, subjectsTask, departmentsTask, activeCoursesTask, activeStudentsTask);

            var systemStats = new SystemStatistics
            {
                TotalStudents = await _dbContext.Students.CountAsync(),
                TotalTeachers = await _dbContext.Teachers.CountAsync(),
                TotalCourses = await _dbContext.CourseInstances.CountAsync(),
                TotalAssignments = await _dbContext.Assignments.CountAsync(),
                TotalSubjects = await _dbContext.Subjects.CountAsync(),
                TotalGroups = await _dbContext.Groups.CountAsync(),
                TotalDepartments = await _dbContext.Departments.CountAsync(),
                ActiveStudents = await _dbContext.Students.Where(s => s.IsActive).CountAsync(),
                ActiveCourses = await _dbContext.CourseInstances.Where(ci => ci.IsActive).CountAsync()
            };

            _logger.LogInformation("Системная статистика успешно загружена: {Students} студентов, {Teachers} преподавателей, {Courses} курсов, {Assignments} заданий",
                systemStats.TotalStudents, systemStats.TotalTeachers, systemStats.TotalCourses, systemStats.TotalAssignments);

            return systemStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке системной статистики");
            
            // Возвращаем пустую статистику в случае ошибки
            return new SystemStatistics
            {
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Получает статистику для конкретного пользователя
    /// </summary>
    public async Task<UserStatistics> GetUserStatisticsAsync(Guid userUid)
    {
        try
        {
            _logger.LogInformation("Загрузка пользовательской статистики для: {UserUid}", userUid);

            var statistics = new UserStatistics
            {
                UserUid = userUid
            };

            // Загружаем количество непрочитанных уведомлений
            try
            {
                statistics.UnreadNotifications = await _notificationService.GetUnreadCountAsync(userUid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось загрузить количество непрочитанных уведомлений для пользователя: {UserUid}", userUid);
                statistics.UnreadNotifications = 0;
            }

            // Дополнительная статистика в зависимости от роли пользователя будет добавлена позже
            // TODO: Определить роль пользователя и загрузить соответствующую статистику

            _logger.LogInformation("Пользовательская статистика загружена для: {UserUid}", userUid);
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке пользовательской статистики для: {UserUid}", userUid);
            
            return new UserStatistics
            {
                UserUid = userUid
            };
        }
    }

    /// <summary>
    /// Получает статистику активности
    /// </summary>
    public async Task<ActivityStatistics> GetActivityStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Загрузка статистики активности");

            var statistics = new ActivityStatistics
            {
                OnlineUsersCount = await GetOnlineUsersCountAsync(),
                ActiveSessions = await GetActiveSessionsCountAsync(),
                TodayActions = await GetTodayActionsCountAsync(),
                WeekActions = await GetWeekActionsCountAsync(),
                PeakActivityTime = await GetPeakActivityTimeAsync()
            };

            _logger.LogInformation("Статистика активности загружена: {OnlineUsers} пользователей онлайн", statistics.OnlineUsersCount);
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке статистики активности");
            
            return new ActivityStatistics
            {
                OnlineUsersCount = 1 // Минимум текущий пользователь
            };
        }
    }

    #region Private Helper Methods

    private async Task<int> GetTotalStudentsAsync()
    {
        try
        {
            var students = await _studentService.GetAllStudentsAsync();
            return students.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество студентов");
            return 0;
        }
    }

    private async Task<int> GetTotalTeachersAsync()
    {
        try
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return teachers.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество преподавателей");
            return 0;
        }
    }

    private async Task<int> GetTotalCoursesAsync()
    {
        try
        {
            var courses = await _courseInstanceService.GetAllCoursesAsync();
            return courses.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество курсов");
            return 0;
        }
    }

    private async Task<int> GetTotalAssignmentsAsync()
    {
        try
        {
            var assignments = await _assignmentService.GetAllAssignmentsAsync();
            return assignments.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество заданий");
            return 0;
        }
    }

    private async Task<int> GetTotalGroupsAsync()
    {
        try
        {
            var groups = await _groupService.GetAllAsync();
            return groups.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество групп");
            return 0;
        }
    }

    private async Task<int> GetTotalSubjectsAsync()
    {
        try
        {
            var subjects = await _subjectService.GetAllSubjectsAsync();
            return subjects.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество предметов");
            return 0;
        }
    }

    private async Task<int> GetTotalDepartmentsAsync()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return departments.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество департаментов");
            return 0;
        }
    }

    private async Task<int> GetActiveCoursesAsync()
    {
        try
        {
            var courses = await _courseInstanceService.GetAllCoursesAsync();
            return courses.Count(c => c.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество активных курсов");
            return 0;
        }
    }

    private async Task<int> GetActiveStudentsAsync()
    {
        try
        {
            var students = await _studentService.GetStudentsByStatusAsync(StudentStatus.Active);
            return students.Count();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество активных студентов");
            return 0;
        }
    }

    private async Task<int> GetOnlineUsersCountAsync()
    {
        try
        {
            // TODO: Реализовать подсчет онлайн пользователей через UserSessionService
            // Пока возвращаем заглушку
            await Task.CompletedTask;
            return Math.Max(1, new Random().Next(5, 25)); // Имитация от 5 до 25 пользователей онлайн
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество пользователей онлайн");
            return 1;
        }
    }

    private async Task<int> GetActiveSessionsCountAsync()
    {
        try
        {
            // TODO: Реализовать подсчет активных сессий
            await Task.CompletedTask;
            return Math.Max(1, new Random().Next(3, 15));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество активных сессий");
            return 1;
        }
    }

    private async Task<int> GetTodayActionsCountAsync()
    {
        try
        {
            // TODO: Реализовать подсчет действий за сегодня
            await Task.CompletedTask;
            return new Random().Next(50, 200);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество действий за сегодня");
            return 0;
        }
    }

    private async Task<int> GetWeekActionsCountAsync()
    {
        try
        {
            // TODO: Реализовать подсчет действий за неделю
            await Task.CompletedTask;
            return new Random().Next(300, 1500);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось получить количество действий за неделю");
            return 0;
        }
    }

    private async Task<TimeSpan?> GetPeakActivityTimeAsync()
    {
        try
        {
            // TODO: Реализовать определение пикового времени активности
            await Task.CompletedTask;
            // Возвращаем время между 10:00 и 16:00 как наиболее вероятное пиковое время
            var hour = new Random().Next(10, 17);
            var minute = new Random().Next(0, 60);
            return new TimeSpan(hour, minute, 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось определить пиковое время активности");
            return null;
        }
    }

    #endregion
} 