using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Services.Implementations;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.ViewModels.Components;
using ViridiscaUi.Infrastructure.Navigation;
using ReactiveUI;
using Avalonia.Controls;
using ViridiscaUi.ViewModels.Bases.Navigations;
using ViridiscaUi.Windows;
using Microsoft.Extensions.Configuration;
using ViridiscaUi.Configuration;
using System;

namespace ViridiscaUi.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core infrastructure
        services.AddLogging(configuration);
        services.AddDatabase(configuration);
        services.AddReactiveUI();

        // Add Lazy Resolution support for circular dependencies
        services.AddLazyResolution();

        // Application services
        services.AddEducationServices();
        services.AddAuthenticationServices();
        services.AddSystemServices();

        // ViewModels
        services.AddViewModels();

        // Windows and Views
        services.AddWindows();

        return services;
    }

    /// <summary>
    /// Добавляет поддержку Lazy Resolution для разрыва циклических зависимостей
    /// </summary>
    private static IServiceCollection AddLazyResolution(this IServiceCollection services)
    {
        return services.AddTransient(typeof(Lazy<>), typeof(LazilyResolved<>));
    }

    /// <summary>
    /// Реализация Lazy Resolution для DI контейнера
    /// </summary>
    private class LazilyResolved<T> : Lazy<T>
    {
        public LazilyResolved(IServiceProvider serviceProvider) 
            : base(serviceProvider.GetRequiredService<T>)
        {
        }
    }

    private static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);

            // Можно добавить дополнительные провайдеры логирования
            // builder.AddSerilog();
        });

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Register PostgreSQL database context
        services.AddPostgreSqlDatabase(configuration);
        return services;
    }

    private static IServiceCollection AddReactiveUI(this IServiceCollection services)
    {
        // Register ReactiveUI ViewLocator for RoutedViewHost
        services.AddSingleton<IViewLocator, ReactiveViewLocator>();

        // Register IScreen as separate implementation to avoid circular dependency
        services.AddSingleton<IScreen>(provider =>
        {
            var screen = new RoutingState();
            return new ScreenHost(screen);
        });

        return services;
    }

    private static IServiceCollection AddEducationServices(this IServiceCollection services)
    {
        // Education services (Scoped для работы с EF Core)
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ICourseInstanceService, CourseInstanceService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAcademicPeriodService, AcademicPeriodService>();
        services.AddScoped<IScheduleSlotService, ScheduleSlotService>();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        // Auth services (Singleton for PersonSessionService alignment)
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddSingleton<IAuthService, AuthService>();

        return services;
    }

    private static IServiceCollection AddSystemServices(this IServiceCollection services)
    {
        // System services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        // Singleton services for application state
        services.AddSingleton<IPersonSessionService, PersonSessionService>();

        // Unified navigation system (replaces old navigation services)
        services.AddSingleton<IUnifiedNavigationService, UnifiedNavigationService>();

        services.AddSingleton<IStatusService, StatusService>();
        services.AddSingleton<IDialogService, DialogService>();

        return services;
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // Main ViewModels (Singleton to ensure single instance for Observable subscriptions)
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomeViewModel>();

        // Component ViewModels
        services.AddTransient<ViridiscaUi.ViewModels.Components.StatusBarViewModel>();

        // Auth ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<AuthenticationViewModel>();

        // Profile ViewModels
        services.AddTransient<ProfileViewModel>();

        // Education ViewModels
        services.AddTransient<CoursesViewModel>();
        services.AddTransient<GroupsViewModel>();
        services.AddTransient<TeachersViewModel>();
        services.AddTransient<AssignmentsViewModel>();
        services.AddTransient<GradesViewModel>();
        services.AddTransient<SubjectsViewModel>();
        services.AddTransient<ScheduleViewModel>();

        // Education Editor ViewModels
        services.AddTransient<GroupEditorViewModel>();
        services.AddTransient<CourseEditorViewModel>();
        services.AddTransient<TeacherEditorViewModel>();
        services.AddTransient<AssignmentEditorViewModel>();
        services.AddTransient<GradeEditorViewModel>();
        services.AddTransient<SubjectEditorViewModel>();

        // Students ViewModels
        services.AddTransient<StudentsViewModel>();
        services.AddTransient<StudentEditorViewModel>();

        // System ViewModels
        services.AddTransient<NotificationCenterViewModel>();
        services.AddTransient<DepartmentsViewModel>();
        // services.AddTransient<DepartmentEditorViewModel>(); // TODO: Create when needed

        return services;
    }

    private static IServiceCollection AddWindows(this IServiceCollection services)
    {
        // Windows (Singleton for main window, Transient for dialogs)
        services.AddSingleton<MainWindow>();

        return services;
    }
}