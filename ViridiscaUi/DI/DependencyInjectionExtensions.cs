using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Implementations;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Pages;
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.ViewModels.Profile;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Windows;
using ViridiscaUi.Services;
using Microsoft.Extensions.Configuration;
using ViridiscaUi.Configuration;

namespace ViridiscaUi.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Register ReactiveUI ViewLocator
        services.AddSingleton<IViewLocator, AppViewLocator>();
         
        // Register PostgreSQL database context
        services.AddPostgreSqlDatabase(configuration);
        
        // Register education services (Scoped для работы с EF Core)
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IImportService, ImportService>();
        
        // Register auth services (Scoped для работы с EF Core)
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IAuthService, AuthService>();
        
        // Register system services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileService, FileService>();
        
        // Register user session service (Singleton для сохранения состояния)
        services.AddSingleton<IUserSessionService, UserSessionService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IStatusService, StatusService>();
        services.AddSingleton<IDialogService, DialogService>();
        
        // ViewModels
        services.AddSingleton<MainViewModel>();
        
        // Register IScreen as separate implementation to avoid circular dependency
        services.AddSingleton<IScreen>(provider => 
        {
            var screen = new RoutingState();
            return new ScreenHost(screen);
        });
        
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ViridiscaUi.ViewModels.Education.CoursesViewModel>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<AuthenticationViewModel>();
        services.AddTransient<StudentsViewModel>();
        services.AddTransient<StudentEditorViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<TeachersViewModel>();
        services.AddTransient<GradesViewModel>();
 
        return services;
    }
} 