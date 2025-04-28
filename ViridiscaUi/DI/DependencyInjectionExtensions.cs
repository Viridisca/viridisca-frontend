using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Implementations;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services)
    {
        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        
        // Register data services
        services.AddSingleton<LocalDbContext>();
        
        // Register education services
        services.AddSingleton<IStudentService, StudentService>();
        services.AddSingleton<IGroupService, GroupService>();
        services.AddSingleton<ITeacherService, TeacherService>();
        services.AddSingleton<ICourseService, CourseService>();
        services.AddSingleton<IEnrollmentService, EnrollmentService>();
        services.AddSingleton<IAssignmentService, AssignmentService>();
        services.AddSingleton<ISubmissionService, SubmissionService>();
        
        // Register auth services
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IRoleService, RoleService>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IRolePermissionService, RolePermissionService>();
        
        // Эти сервисы будут реализованы позже, не регистрируем их сейчас
        // services.AddSingleton<IAuthService, AuthService>();
        // services.AddSingleton<INavigationService, NavigationService>();
        // services.AddSingleton<IDialogService, DialogService>();
        
        return services;
    }
} 