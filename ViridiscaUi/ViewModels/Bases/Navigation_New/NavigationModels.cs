using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Infrastructure.Navigation;

namespace ViridiscaUi.ViewModels.Bases.Navigation_New;

/// <summary>
/// Группа меню для сайдбара
/// </summary>
public class MenuGroup : ReactiveObject
{
    [Reactive] public string GroupName { get; set; } = string.Empty;
    [Reactive] public int Order { get; set; }
    [Reactive] public ObservableCollection<NavigationRoute> Items { get; set; } = new();
}

/// <summary>
/// Быстрое действие в сайдбаре
/// </summary>
public class QuickAction : ReactiveObject
{
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public string IconKey { get; set; } = string.Empty;
    [Reactive] public string Path { get; set; } = string.Empty;
    [Reactive] public string? Shortcut { get; set; }
    [Reactive] public bool IsEnabled { get; set; } = true;
    [Reactive] public int Order { get; set; }
}

/// <summary>
/// Избранный элемент
/// </summary>
public class FavoriteItem : ReactiveObject
{
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Path { get; set; } = string.Empty;
    [Reactive] public string? IconKey { get; set; }
    [Reactive] public DateTime AddedAt { get; set; }
    [Reactive] public int Order { get; set; }
}

/// <summary>
/// Недавний элемент
/// </summary>
public class RecentItem : ReactiveObject
{
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Path { get; set; } = string.Empty;
    [Reactive] public string? IconKey { get; set; }
    [Reactive] public DateTime AccessedAt { get; set; }
    [Reactive] public int AccessCount { get; set; }
}

/// <summary>
/// Результат поиска
/// </summary>
public class SearchResult : ReactiveObject
{
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Path { get; set; } = string.Empty;
    [Reactive] public string? IconKey { get; set; }
    [Reactive] public string? Description { get; set; }
    [Reactive] public string? Group { get; set; }
    [Reactive] public double Relevance { get; set; }
    [Reactive] public string HighlightedName { get; set; } = string.Empty;
} 