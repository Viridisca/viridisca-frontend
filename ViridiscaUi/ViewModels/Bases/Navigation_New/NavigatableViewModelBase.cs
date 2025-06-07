using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.Bases.Navigation_New;

/// <summary>
/// Расширенный базовый класс для ViewModels с навигационными возможностями
/// </summary>
public abstract class NavigatableViewModelBase : RoutableViewModelBase
{
    protected readonly IUnifiedNavigationService NavigationService;

    /// <summary>
    /// Команда возврата назад
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; protected set; }

    /// <summary>
    /// Команда навигации к главной странице
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoHomeCommand { get; protected set; }

    /// <summary>
    /// Можно ли вернуться назад
    /// </summary>
    public bool CanGoBack => NavigationService?.CanGoBack ?? false;

    protected NavigatableViewModelBase(
        IScreen hostScreen, 
        IUnifiedNavigationService navigationService) 
        : base(hostScreen)
    {
        NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        InitializeNavigationCommands();
    }

    private void InitializeNavigationCommands()
    {
        // Команда возврата назад
        GoBackCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await NavigationService.GoBackAsync();
        });

        // Команда навигации к главной
        GoHomeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await NavigationService.NavigateToAsync("home");
        });
    }

    /// <summary>
    /// Навигация к указанному пути
    /// </summary>
    /// <param name="path">Путь для навигации</param>
    protected virtual async Task NavigateToAsync(string path)
    {
        try
        {
            await NavigationService.NavigateToAsync(path);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка навигации к {Path}", path);
        }
    }

    /// <summary>
    /// Навигация к указанному ViewModel
    /// </summary>
    /// <typeparam name="TViewModel">Тип ViewModel</typeparam>
    protected virtual async Task NavigateToAsync<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        try
        {
            await NavigationService.NavigateToAsync<TViewModel>();
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка навигации к {ViewModelType}", typeof(TViewModel).Name);
        }
    }

    /// <summary>
    /// Навигация с сбросом стека
    /// </summary>
    /// <param name="path">Путь для навигации</param>
    protected virtual async Task NavigateAndResetAsync(string path)
    {
        try
        {
            await NavigationService.NavigateAndResetAsync(path);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка навигации с сбросом к {Path}", path);
        }
    }
}
