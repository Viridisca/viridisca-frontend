using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases;

namespace ViridiscaUi.ViewModels.Bases.Navigation_New;

/// <summary>
/// Улучшенный ViewModel для Sidebar с поддержкой всех новых функций навигации
/// </summary>
public class EnhancedSidebarViewModel : ViewModelBase
{
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IPersonSessionService _personSessionService;
    private readonly CompositeDisposable _disposables = new();

    #region Properties

    /// <summary>
    /// Группированные элементы меню
    /// </summary>
    [Reactive] public ObservableCollection<MenuGroup> GroupedMenuItems { get; set; } = new();

    /// <summary>
    /// Быстрые действия
    /// </summary>
    [Reactive] public ObservableCollection<QuickAction> QuickActions { get; set; } = new();

    /// <summary>
    /// Избранные элементы
    /// </summary>
    [Reactive] public ObservableCollection<FavoriteItem> Favorites { get; set; } = new();

    /// <summary>
    /// Недавние элементы
    /// </summary>
    [Reactive] public ObservableCollection<RecentItem> Recent { get; set; } = new();

    /// <summary>
    /// Результаты поиска
    /// </summary>
    [Reactive] public ObservableCollection<SearchResult> SearchResults { get; set; } = new();

    /// <summary>
    /// Текст поиска
    /// </summary>
    [Reactive] public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Показывать ли секцию быстрых действий
    /// </summary>
    [Reactive] public bool ShowQuickActions { get; set; } = true;

    /// <summary>
    /// Показывать ли секцию избранного
    /// </summary>
    [Reactive] public bool ShowFavorites { get; set; } = true;

    /// <summary>
    /// Показывать ли секцию недавних
    /// </summary>
    [Reactive] public bool ShowRecent { get; set; } = true;

    /// <summary>
    /// Показывать ли результаты поиска
    /// </summary>
    [Reactive] public bool ShowSearchResults { get; set; } = false;

    /// <summary>
    /// Свернут ли sidebar
    /// </summary>
    [Reactive] public bool IsCollapsed { get; set; } = false;

    /// <summary>
    /// Информация о текущем пользователе
    /// </summary>
    [Reactive] public CurrentUserInfo? CurrentUser { get; set; }

    /// <summary>
    /// Роли текущего пользователя
    /// </summary>
    [Reactive] public string[]? UserRoles { get; set; }

    #endregion

    #region Commands

    /// <summary>
    /// Команда поиска
    /// </summary>
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;

    /// <summary>
    /// Команда очистки поиска
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; private set; } = null!;

    /// <summary>
    /// Команда переключения состояния сайдбара
    /// </summary>
    public ReactiveCommand<Unit, Unit> ToggleCollapseCommand { get; private set; } = null!;

    /// <summary>
    /// Команда обновления данных
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// Команда добавления в избранное
    /// </summary>
    public ReactiveCommand<NavigationRoute, Unit> AddToFavoritesCommand { get; private set; } = null!;

    /// <summary>
    /// Команда удаления из избранного
    /// </summary>
    public ReactiveCommand<string, Unit> RemoveFromFavoritesCommand { get; private set; } = null!;

    /// <summary>
    /// Команда очистки недавних элементов
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClearRecentCommand { get; private set; } = null!;

    #endregion

    public EnhancedSidebarViewModel(
        IUnifiedNavigationService navigationService,
        IPersonSessionService personSessionService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _personSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));

        // Инициализация команд
        InitializeCommands();

        // Подписка на изменения пользователя
        SubscribeToUserChanges();

        // Подписка на изменения поиска
        SubscribeToSearchChanges();

        // Подписка на изменения навигации
        SubscribeToNavigationChanges();

        // Начальная загрузка данных
        _ = LoadDataAsync();

        StatusLogger.LogInfo("Enhanced Sidebar ViewModel initialized", "EnhancedSidebar");
    }

    #region Initialization

    private void InitializeCommands()
    {
        // Команда поиска
        SearchCommand = ReactiveCommand.CreateFromTask<string>(async query =>
        {
            await PerformSearchAsync(query);
        });

        // Команда очистки поиска
        var canClearSearch = this.WhenAnyValue(x => x.SearchQuery)
            .Select(query => !string.IsNullOrWhiteSpace(query));

        ClearSearchCommand = ReactiveCommand.Create(() =>
        {
            SearchQuery = string.Empty;
            ShowSearchResults = false;
            SearchResults.Clear();
        }, canClearSearch);

        // Команда переключения состояния
        ToggleCollapseCommand = ReactiveCommand.Create(() =>
        {
            IsCollapsed = !IsCollapsed;
            StatusLogger.LogDebug($"Sidebar collapsed: {IsCollapsed}", "EnhancedSidebar");
        });

        // Команда обновления
        RefreshCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await LoadDataAsync();
        });

        // Команда добавления в избранное
        AddToFavoritesCommand = ReactiveCommand.CreateFromTask<NavigationRoute>(async route =>
        {
            // TODO: Реализовать добавление в избранное через настройки пользователя
            await Task.CompletedTask;
            StatusLogger.LogInfo($"Добавлено в избранное: {route.DisplayName}", "EnhancedSidebar");
        });

        // Команда удаления из избранного
        RemoveFromFavoritesCommand = ReactiveCommand.CreateFromTask<string>(async route =>
        {
            // TODO: Реализовать удаление из избранного через настройки пользователя
            await Task.CompletedTask;
            StatusLogger.LogInfo($"Удалено из избранного: {route}", "EnhancedSidebar");
        });

        // Команда очистки недавних
        ClearRecentCommand = ReactiveCommand.Create(() =>
        {
            // TODO: Реализовать очистку недавних через настройки пользователя
            Recent.Clear();
            StatusLogger.LogInfo("Очищены недавние элементы", "EnhancedSidebar");
        });
    }

    private void SubscribeToUserChanges()
    {
        _personSessionService.WhenAnyValue(x => x.CurrentPerson)
            .Subscribe(async person =>
            {
                if (person != null)
                {
                    CurrentUser = new CurrentUserInfo
                    {
                        Id = person.Uid,
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        Email = person.Email,
                        PrimaryRole = person.PersonRoles?.FirstOrDefault(pr => pr.IsActive)?.Role?.Name ?? "Unknown",
                        Roles = person.PersonRoles?.Where(pr => pr.IsActive)
                            .Select(pr => pr.Role?.Name ?? "Unknown")
                            .ToArray() ?? Array.Empty<string>(),
                        LastLoginAt = DateTime.UtcNow
                    };

                    UserRoles = CurrentUser.Roles;
                    await LoadDataAsync();
                }
                else
                {
                    CurrentUser = null;
                    UserRoles = null;
                    ClearAllData();
                }
            })
            .DisposeWith(_disposables);
    }

    private void SubscribeToSearchChanges()
    {
        this.WhenAnyValue(x => x.SearchQuery)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .DistinctUntilChanged()
            .Where(query => !string.IsNullOrWhiteSpace(query))
            .Subscribe(async query =>
            {
                await PerformSearchAsync(query);
            })
            .DisposeWith(_disposables);
    }

    private void SubscribeToNavigationChanges()
    {
        // TODO: Подписка на события навигации для обновления недавних элементов
        // Когда будет доступно в IUnifiedNavigationService
        StatusLogger.LogDebug("Navigation changes subscription initialized", "EnhancedSidebar");
    }

    #endregion

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            await Task.WhenAll(
                LoadMenuItemsAsync(),
                LoadQuickActionsAsync(),
                LoadFavoritesAsync(),
                LoadRecentAsync()
            );

            StatusLogger.LogInfo("Sidebar data loaded successfully", "EnhancedSidebar");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error loading sidebar data: {ex.Message}", "EnhancedSidebar");
        }
    }

    private async Task LoadMenuItemsAsync()
    {
        try
        {
            var menuRoutes = _navigationService.GetMenuRoutes(UserRoles);
            
            GroupedMenuItems.Clear();
            if (menuRoutes != null)
            {
                var groupedItems = menuRoutes
                    .GroupBy(route => route.Group ?? "Основное")
                    .OrderBy(group => group.Min(r => r.Order))
                    .Select(group => new MenuGroup
                    {
                        GroupName = group.Key,
                        Order = group.Min(r => r.Order),
                        Items = new ObservableCollection<NavigationRoute>(
                            group.OrderBy(r => r.Order)
                                 .ThenBy(r => r.DisplayName))
                    });

                foreach (var group in groupedItems)
                {
                    GroupedMenuItems.Add(group);
                }
            }

            StatusLogger.LogDebug($"Loaded {GroupedMenuItems.Count} menu groups", "EnhancedSidebar");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error loading menu items: {ex.Message}", "EnhancedSidebar");
        }
    }

    private async Task LoadQuickActionsAsync()
    {
        try
        {
            // TODO: Реализовать загрузку быстрых действий когда будет доступно в IUnifiedNavigationService
            QuickActions.Clear();
            
            // Добавляем базовые быстрые действия
            QuickActions.Add(new QuickAction
            {
                Name = "Создать студента",
                Description = "Быстрое создание нового студента",
                IconKey = "AccountPlus",
                Path = "student-editor",
                Order = 1
            });

            StatusLogger.LogDebug($"Loaded {QuickActions.Count} quick actions", "EnhancedSidebar");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error loading quick actions: {ex.Message}", "EnhancedSidebar");
        }
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            if (CurrentUser == null) return;

            // TODO: Реализовать загрузку избранного когда будет доступно в IUnifiedNavigationService
            Favorites.Clear();

            StatusLogger.LogDebug($"Loaded {Favorites.Count} favorites", "EnhancedSidebar");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error loading favorites: {ex.Message}", "EnhancedSidebar");
        }
    }

    private async Task LoadRecentAsync()
    {
        try
        {
            if (CurrentUser == null) return;

            // TODO: Реализовать загрузку недавних элементов когда будет доступно в IUnifiedNavigationService
            Recent.Clear();

            StatusLogger.LogDebug($"Loaded {Recent.Count} recent items", "EnhancedSidebar");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error loading recent items: {ex.Message}", "EnhancedSidebar");
        }
    }

    #endregion

    #region Search

    private async Task PerformSearchAsync(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ShowSearchResults = false;
                SearchResults.Clear();
                return;
            }

            // TODO: Реализовать поиск когда будет доступно в IUnifiedNavigationService
            SearchResults.Clear();
            
            // Простой поиск по существующим маршрутам
            var allRoutes = _navigationService.GetMenuRoutes(UserRoles);
            var filteredRoutes = allRoutes
                .Where(r => r.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (r.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .Take(20)
                .Select(r => new SearchResult
                {
                    Name = r.DisplayName,
                    Path = r.Path,
                    IconKey = r.IconKey,
                    Description = r.Description,
                    Group = r.Group,
                    Relevance = 1.0,
                    HighlightedName = r.DisplayName
                });

            foreach (var result in filteredRoutes)
            {
                SearchResults.Add(result);
            }

            ShowSearchResults = SearchResults.Count > 0;
            
            StatusLogger.LogDebug($"Search for '{query}' returned {SearchResults.Count} results", "EnhancedSidebar");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error performing search: {ex.Message}", "EnhancedSidebar");
        }
    }

    #endregion

    #region Helper Methods

    private void ClearAllData()
    {
        GroupedMenuItems.Clear();
        QuickActions.Clear();
        Favorites.Clear();
        Recent.Clear();
        SearchResults.Clear();
        SearchQuery = string.Empty;
        ShowSearchResults = false;
    }

    /// <summary>
    /// Получение статистики для отображения
    /// </summary>
    public SidebarStatistics GetStatistics()
    {
        return new SidebarStatistics
        {
            TotalMenuItems = GroupedMenuItems.SelectMany(g => g.Items).Count(),
            TotalQuickActions = QuickActions.Count,
            TotalFavorites = Favorites.Count,
            TotalRecent = Recent.Count,
            IsUserLoggedIn = CurrentUser != null
        };
    }

    #endregion

    #region Cleanup

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposables?.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}

#region Supporting Models

/// <summary>
/// Информация о текущем пользователе для Sidebar
/// </summary>
public class CurrentUserInfo
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PrimaryRole { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public DateTime LastLoginAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
}

/// <summary>
/// Статистика Sidebar
/// </summary>
public class SidebarStatistics
{
    public int TotalMenuItems { get; set; }
    public int TotalQuickActions { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalRecent { get; set; }
    public bool IsUserLoggedIn { get; set; }
}

#endregion 