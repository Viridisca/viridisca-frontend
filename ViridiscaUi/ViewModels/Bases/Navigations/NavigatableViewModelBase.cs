using System;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Infrastructure.Navigation;

namespace ViridiscaUi.ViewModels.Bases.Navigations;

/// <summary>
/// Базовый класс для ViewModels с поддержкой навигации через новый UnifiedNavigationService
/// </summary>
/// <remarks>
/// Создает новый экземпляр ViewModel с поддержкой навигации
/// </remarks>
/// <param name="hostScreen">IScreen хост</param>
/// <param name="navigationService">Единый сервис навигации</param>
public abstract class NavigatableViewModelBase(IScreen hostScreen, IUnifiedNavigationService navigationService) : RoutableViewModelBase(hostScreen)
{
    protected readonly IUnifiedNavigationService NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

    /// <summary>
    /// Навигация к маршруту по пути
    /// </summary>
    protected override async Task NavigateToAsync(string path)
    {
        try
        {
            await NavigationService.NavigateToAsync(path);
        }
        catch (Exception ex)
        {
            SetError($"Ошибка навигации к {path}", ex);
        }
    }

    /// <summary>
    /// Навигация к ViewModel по типу
    /// </summary>
    protected override async Task NavigateToAsync<TViewModel>()
    {
        try
        {
            await NavigationService.NavigateToAsync<TViewModel>();
        }
        catch (Exception ex)
        {
            SetError($"Ошибка навигации к {typeof(TViewModel).Name}", ex);
        }
    }

    /// <summary>
    /// Навигация с заменой текущего стека
    /// </summary>
    protected async Task<bool> NavigateAndResetAsync(string path)
    {
        try
        {
            return await NavigationService.NavigateAndResetAsync(path);
        }
        catch (Exception ex)
        {
            SetError($"Ошибка навигации с заменой к {path}", ex);
            return false;
        }
    }

    /// <summary>
    /// Навигация с заменой текущего стека по типу ViewModel
    /// </summary>
    protected async Task<bool> NavigateAndResetAsync<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        try
        {
            return await NavigationService.NavigateAndResetAsync<TViewModel>();
        }
        catch (Exception ex)
        {
            SetError($"Ошибка навигации с заменой к {typeof(TViewModel).Name}", ex);
            return false;
        }
    }
} 