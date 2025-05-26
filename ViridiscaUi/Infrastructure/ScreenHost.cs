using System;
using ReactiveUI;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Простая реализация IScreen для ReactiveUI навигации
/// </summary>
public class ScreenHost : IScreen
{
    public RoutingState Router { get; }

    public ScreenHost(RoutingState router)
    {
        Router = router ?? throw new ArgumentNullException(nameof(router));
    }
} 