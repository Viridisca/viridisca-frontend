using System;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Infrastructure.Navigation;

namespace ViridiscaUi.ViewModels.Bases.Navigation_New;

/// <summary>
/// ViewModel для элемента навигации в сайдбаре
/// </summary>
public class NavigationItemViewModel : ReactiveObject
{
    /// <summary>
    /// Маршрут навигации
    /// </summary>
    [Reactive] public NavigationRoute? Route { get; set; }

    /// <summary>
    /// Отображаемое имя
    /// </summary>
    [Reactive] public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Ключ иконки
    /// </summary>
    [Reactive] public string? IconKey { get; set; }

    /// <summary>
    /// Путь для навигации
    /// </summary>
    [Reactive] public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Активен ли элемент
    /// </summary>
    [Reactive] public bool IsActive { get; set; }

    /// <summary>
    /// Включен ли элемент
    /// </summary>
    [Reactive] public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Есть ли бейдж
    /// </summary>
    [Reactive] public bool HasBadge { get; set; }

    /// <summary>
    /// Текст бейджа
    /// </summary>
    [Reactive] public string? BadgeText { get; set; }

    /// <summary>
    /// Команда навигации
    /// </summary>
    public ReactiveCommand<Unit, Unit>? NavigateCommand { get; set; }

    public NavigationItemViewModel()
    {
    }

    public NavigationItemViewModel(NavigationRoute route)
    {
        Route = route;
        DisplayName = route.DisplayName;
        IconKey = route.IconKey;
        Path = route.Path;
        NavigateCommand = route.NavigateCommand;
    }
}
