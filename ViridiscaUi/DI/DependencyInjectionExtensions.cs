using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Implementations;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Pages;

namespace ViridiscaUi.DI;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services)
    {
        // Register ReactiveUI ViewLocator
        services.AddSingleton<IViewLocator, Infrastructure.AppViewLocator>();
         
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
         
        // Сервисы
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IRoleService, RoleService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IRolePermissionService, RolePermissionService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<CoursesViewModel>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();

        return services;
    }
} 