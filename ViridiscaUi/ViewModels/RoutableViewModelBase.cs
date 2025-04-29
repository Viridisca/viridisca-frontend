using ReactiveUI;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Базовый класс для ViewModel с поддержкой маршрутизации
/// </summary>
public abstract class RoutableViewModelBase : ViewModelBase, IRoutableViewModel
{
    /// <summary>
    /// Уникальный идентификатор маршрута ViewModel
    /// </summary>
    public abstract string UrlPathSegment { get; }
    
    /// <summary>
    /// Ссылка на IScreen, который владеет этой ViewModel
    /// </summary>
    public IScreen HostScreen { get; }

    /// <summary>
    /// Создает новый экземпляр маршрутизируемой ViewModel
    /// </summary>
    /// <param name="hostScreen">IScreen хост</param>
    protected RoutableViewModelBase(IScreen hostScreen)
    {
        HostScreen = hostScreen;
    }
} 