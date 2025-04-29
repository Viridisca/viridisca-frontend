using System;
using Avalonia.Controls;
using ReactiveUI;
using ViridiscaUi.ViewModels.Pages;
using ViridiscaUi.Views.Pages;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// ViewLocator для маршрутизации ViewModel к соответствующим View
/// </summary>
public class ViewLocator : IViewLocator
{
    /// <summary>
    /// Находит View для указанной ViewModel
    /// </summary>
    /// <param name="viewModel">ViewModel, для которой требуется найти View</param>
    /// <param name="contract">Дополнительная информация для нахождения View (не используется)</param>
    /// <returns>Представление, соответствующее ViewModel</returns>
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        if (viewModel == null)
            return null;

        // Сопоставление ViewModel с View
        if (viewModel is HomeViewModel home)
            return new HomeView { DataContext = home };
        if (viewModel is CoursesViewModel courses)
            return new CoursesView { DataContext = courses };
        if (viewModel is UsersViewModel users)
            return new UsersView { DataContext = users };

        // Если View не найдено, выбрасываем исключение
        throw new ArgumentOutOfRangeException(
            nameof(viewModel), 
            $"Не удалось найти представление для {viewModel.GetType().Name}"
        );
    }
} 