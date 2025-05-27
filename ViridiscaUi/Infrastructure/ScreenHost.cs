using System;
using ReactiveUI;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Простая реализация IScreen для ReactiveUI навигации
/// </summary>
public class ScreenHost(RoutingState router) : IScreen
{
    public RoutingState Router => router ?? throw new ArgumentNullException(nameof(router));
} 