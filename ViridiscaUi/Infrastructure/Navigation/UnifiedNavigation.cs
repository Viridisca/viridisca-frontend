using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace ViridiscaUi.Infrastructure.Navigation;

#region Route Attribute

/// <summary>
/// Атрибут для определения маршрута навигации для ViewModel
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RouteAttribute : Attribute
{
    public string Path { get; }
    public string? DisplayName { get; set; }
    public string? IconKey { get; set; }
    public int Order { get; set; } = 0;
    public string? Group { get; set; }
    public string[]? RequiredRoles { get; set; }
    public bool ShowInMenu { get; set; } = true;
    public string? ParentRoute { get; set; }
    public string? Description { get; set; }
    public string? Shortcut { get; set; }
    public string[]? Tags { get; set; }
    public bool IsBeta { get; set; } = false;
    public bool RequiresConfirmation { get; set; } = false;

    public RouteAttribute(string path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }
}

#endregion

#region Navigation Route Model

/// <summary>
/// Модель маршрута навигации
/// </summary>
public class NavigationRoute
{
    public string Path { get; }
    public Type ViewModelType { get; }
    public string DisplayName { get; }
    public string? IconKey { get; }
    public int Order { get; }
    public string? Group { get; }
    public string[] RequiredRoles { get; }
    public bool ShowInMenu { get; }
    public string? ParentRoute { get; }
    public string? Description { get; }
    public string? Shortcut { get; }
    public string[] Tags { get; }
    public bool IsBeta { get; }
    public bool RequiresConfirmation { get; }
    
    /// <summary>
    /// Команда навигации для данного маршрута
    /// </summary>
    public ReactiveCommand<Unit, Unit>? NavigateCommand { get; set; }
    
    /// <summary>
    /// Отображаемое имя для UI (alias для DisplayName)
    /// </summary>
    public string Label => DisplayName;

    public NavigationRoute(
        string path,
        Type viewModelType,
        string displayName,
        string? iconKey = null,
        int order = 0,
        string? group = null,
        string[]? requiredRoles = null,
        bool showInMenu = true,
        string? parentRoute = null,
        string? description = null,
        string? shortcut = null,
        string[]? tags = null,
        bool isBeta = false,
        bool requiresConfirmation = false)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        IconKey = iconKey;
        Order = order;
        Group = group;
        RequiredRoles = requiredRoles ?? Array.Empty<string>();
        ShowInMenu = showInMenu;
        ParentRoute = parentRoute;
        Description = description;
        Shortcut = shortcut;
        Tags = tags ?? Array.Empty<string>();
        IsBeta = isBeta;
        RequiresConfirmation = requiresConfirmation;
    }
}

#endregion

#region Unified Navigation Service

/// <summary>
/// Единый сервис навигации, объединяющий всю функциональность
/// </summary>
public interface IUnifiedNavigationService
{
    // Основная навигация
    Task<bool> NavigateToAsync(string path);
    Task<bool> NavigateToAsync<TViewModel>() where TViewModel : class, IRoutableViewModel;
    Task<bool> NavigateAndResetAsync(string path);
    Task<bool> NavigateAndResetAsync<TViewModel>() where TViewModel : class, IRoutableViewModel;
    Task<bool> GoBackAsync();
    void ClearNavigationStack();
    bool CanGoBack { get; }

    // Работа с маршрутами
    NavigationRoute? GetRoute(string path);
    NavigationRoute? GetRoute<TViewModel>() where TViewModel : class, IRoutableViewModel;
    IEnumerable<NavigationRoute> GetMenuRoutes(string[]? userRoles = null);
    IEnumerable<NavigationRoute> GetChildRoutes(string parentPath, string[]? userRoles = null);
    IEnumerable<NavigationRoute> GetRoutesByGroup(string group, string[]? userRoles = null);
    IEnumerable<NavigationRoute> GetRoutesByTags(string[] tags, string[]? userRoles = null);

    // Команды навигации
    ReactiveCommand<Unit, IRoutableViewModel> CreateNavigationCommand(string path);
    ReactiveCommand<Unit, IRoutableViewModel> CreateNavigationCommand<TViewModel>() where TViewModel : class, IRoutableViewModel;

    // Инициализация
    void Initialize(IScreen screen);
    void ScanAndRegisterRoutes();
}

/// <summary>
/// Реализация единого сервиса навигации
/// </summary>
public class UnifiedNavigationService : IUnifiedNavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<NavigationRoute> _routes = new();
    private IScreen? _screen;

    public IReadOnlyList<NavigationRoute> Routes => _routes.AsReadOnly();
    public bool CanGoBack => _screen?.Router.NavigationStack.Count > 1;

    public UnifiedNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public void Initialize(IScreen screen)
    {
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        StatusLogger.LogDebug("Navigation service initialized", "UnifiedNavigation");
    }

    public void ScanAndRegisterRoutes()
    {
        StatusLogger.LogInfo("Сканирование маршрутов...", "UnifiedNavigation");
        
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            Assembly.GetEntryAssembly()
        }.Where(a => a != null).Distinct();

        var registeredCount = 0;
        
        foreach (var assembly in assemblies)
        {
            var viewModelTypes = assembly.GetTypes()
                .Where(type => 
                    type.IsClass && 
                    !type.IsAbstract && 
                    typeof(IRoutableViewModel).IsAssignableFrom(type) &&
                    type.GetCustomAttribute<RouteAttribute>() != null);

            foreach (var viewModelType in viewModelTypes)
            {
                try
                {
                    RegisterRoute(viewModelType);
                    registeredCount++;
                }
                catch (Exception ex)
                {
                    StatusLogger.LogWarning($"Не удалось зарегистрировать маршрут для {viewModelType.Name}: {ex.Message}", "UnifiedNavigation");
                }
            }
        }
        
        StatusLogger.LogSuccess($"Зарегистрировано {registeredCount} маршрутов", "UnifiedNavigation");
    }

    private void RegisterRoute(Type viewModelType)
    {
        var routeAttribute = viewModelType.GetCustomAttribute<RouteAttribute>();
        if (routeAttribute == null) return;

        // Проверяем дублирование
        if (_routes.Any(r => r.Path.Equals(routeAttribute.Path, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Маршрут '{routeAttribute.Path}' уже зарегистрирован");
        }

        var displayName = routeAttribute.DisplayName ?? viewModelType.Name.Replace("ViewModel", "");
        
        var route = new NavigationRoute(
            routeAttribute.Path,
            viewModelType,
            displayName,
            routeAttribute.IconKey,
            routeAttribute.Order,
            routeAttribute.Group,
            routeAttribute.RequiredRoles,
            routeAttribute.ShowInMenu,
            routeAttribute.ParentRoute,
            routeAttribute.Description,
            routeAttribute.Shortcut,
            routeAttribute.Tags,
            routeAttribute.IsBeta,
            routeAttribute.RequiresConfirmation);
            
        _routes.Add(route);
        StatusLogger.LogDebug($"Зарегистрирован маршрут: {route.Path} -> {route.ViewModelType.Name}", "UnifiedNavigation");
    }

    public async Task<bool> NavigateToAsync(string path)
    {
        try
        {
            var command = CreateNavigationCommand(path);
            await command.Execute();
            StatusLogger.LogInfo($"Переход к {path}", "UnifiedNavigation");
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации к {path}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> NavigateToAsync<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        try
        {
            var command = CreateNavigationCommand<TViewModel>();
            await command.Execute();
            StatusLogger.LogInfo($"Переход к {typeof(TViewModel).Name}", "UnifiedNavigation");
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации к {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> NavigateAndResetAsync(string path)
    {
        try
        {
            _screen?.Router.NavigationStack.Clear();
            return await NavigateToAsync(path);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации с сбросом к {path}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> NavigateAndResetAsync<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        try
        {
            _screen?.Router.NavigationStack.Clear();
            return await NavigateToAsync<TViewModel>();
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации с сбросом к {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> GoBackAsync()
    {
        try
        {
            if (!CanGoBack)
            {
                StatusLogger.LogWarning("Невозможно вернуться назад - это первая страница", "UnifiedNavigation");
                return false;
            }

            await _screen!.Router.NavigateBack.Execute();
            StatusLogger.LogInfo("Возврат назад", "UnifiedNavigation");
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка возврата назад: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public void ClearNavigationStack()
    {
        _screen?.Router.NavigationStack.Clear();
        StatusLogger.LogInfo("Стек навигации очищен", "UnifiedNavigation");
    }

    public NavigationRoute? GetRoute(string path)
    {
        return _routes.FirstOrDefault(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
    }

    public NavigationRoute? GetRoute<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        return _routes.FirstOrDefault(r => r.ViewModelType == typeof(TViewModel));
    }

    public IEnumerable<NavigationRoute> GetMenuRoutes(string[]? userRoles = null)
    {
        var menuRoutes = _routes.Where(r => r.ShowInMenu);
        
        if (userRoles != null && userRoles.Length > 0)
        {
            menuRoutes = menuRoutes.Where(r => 
                r.RequiredRoles.Length == 0 || 
                r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }
        
        return menuRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);
    }

    public IEnumerable<NavigationRoute> GetChildRoutes(string parentPath, string[]? userRoles = null)
    {
        var childRoutes = _routes.Where(r => 
            !string.IsNullOrEmpty(r.ParentRoute) && 
            r.ParentRoute.Equals(parentPath, StringComparison.OrdinalIgnoreCase));
        
        if (userRoles != null && userRoles.Length > 0)
        {
            childRoutes = childRoutes.Where(r => 
                r.RequiredRoles.Length == 0 || 
                r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }
        
        return childRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);
    }

    public IEnumerable<NavigationRoute> GetRoutesByGroup(string group, string[]? userRoles = null)
    {
        var groupRoutes = _routes.Where(r => 
            !string.IsNullOrEmpty(r.Group) && 
            r.Group.Equals(group, StringComparison.OrdinalIgnoreCase));
        
        if (userRoles != null && userRoles.Length > 0)
        {
            groupRoutes = groupRoutes.Where(r => 
                r.RequiredRoles.Length == 0 || 
                r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }
        
        return groupRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);
    }

    public IEnumerable<NavigationRoute> GetRoutesByTags(string[] tags, string[]? userRoles = null)
    {
        var taggedRoutes = _routes.Where(r => 
            r.Tags.Length > 0 && 
            r.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        
        if (userRoles != null && userRoles.Length > 0)
        {
            taggedRoutes = taggedRoutes.Where(r => 
                r.RequiredRoles.Length == 0 || 
                r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }
        
        return taggedRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);
    }

    public ReactiveCommand<Unit, IRoutableViewModel> CreateNavigationCommand(string path)
    {
        if (_screen == null)
            throw new InvalidOperationException("Navigation service not initialized. Call Initialize() first.");

        return ReactiveCommand.CreateFromObservable(() =>
        {
            var route = GetRoute(path);
            if (route == null)
            {
                StatusLogger.LogError($"Маршрут '{path}' не найден", "UnifiedNavigation");
                return Observable.Return<IRoutableViewModel>(null!);
            }

            try
            {
                var viewModel = (IRoutableViewModel)_serviceProvider.GetRequiredService(route.ViewModelType);
                return _screen.Router.Navigate.Execute(viewModel);
            }
            catch (Exception ex)
            {
                StatusLogger.LogError($"Ошибка создания ViewModel для маршрута '{path}': {ex.Message}", "UnifiedNavigation");
                return Observable.Return<IRoutableViewModel>(null!);
            }
        });
    }

    public ReactiveCommand<Unit, IRoutableViewModel> CreateNavigationCommand<TViewModel>() 
        where TViewModel : class, IRoutableViewModel
    {
        if (_screen == null)
            throw new InvalidOperationException("Navigation service not initialized. Call Initialize() first.");

        return ReactiveCommand.CreateFromObservable(() =>
        {
            try
            {
                var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
                return _screen.Router.Navigate.Execute(viewModel);
            }
            catch (Exception ex)
            {
                StatusLogger.LogError($"Ошибка создания {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
                return Observable.Return<IRoutableViewModel>(null!);
            }
        });
    }
}

#endregion 