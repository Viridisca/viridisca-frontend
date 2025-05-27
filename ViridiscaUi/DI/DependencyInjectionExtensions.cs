using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Implementations;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Profile;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.Windows;
using Microsoft.Extensions.Configuration;
using ViridiscaUi.Configuration;

namespace ViridiscaUi.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core infrastructure
        services.AddLogging(configuration);
        services.AddDatabase(configuration);
        services.AddReactiveUI();

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
        services.AddScoped<ICourseService, CourseService>(); 
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IImportService, ImportService>();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        // Auth services (Scoped для работы с EF Core)
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    private static IServiceCollection AddSystemServices(this IServiceCollection services)
    {
        // System services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileService, FileService>();

        // Singleton services for application state
        services.AddSingleton<IUserSessionService, UserSessionService>();

        // Unified navigation system (replaces old navigation services)
        services.AddSingleton<IUnifiedNavigationService, UnifiedNavigationService>();

        services.AddSingleton<IStatusService, StatusService>();
        services.AddSingleton<IDialogService, DialogService>();

        return services;
    }

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // Main ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();

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

        // Education Editor ViewModels
        services.AddTransient<GroupEditorViewModel>();
        services.AddTransient<CourseEditorViewModel>();
        services.AddTransient<TeacherEditorViewModel>();
        services.AddTransient<AssignmentEditorViewModel>();
        services.AddTransient<GradeEditorViewModel>();

        // Students ViewModels
        services.AddTransient<StudentsViewModel>();
        services.AddTransient<StudentEditorViewModel>();

        // System ViewModels
        services.AddTransient<NotificationCenterViewModel>();

        return services;
    }

    private static IServiceCollection AddWindows(this IServiceCollection services)
    {
        // Windows (Singleton for main window, Transient for dialogs)
        services.AddSingleton<MainWindow>();

        return services;
    }
}