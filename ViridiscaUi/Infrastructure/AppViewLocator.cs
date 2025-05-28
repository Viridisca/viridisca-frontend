using Avalonia.Controls;
using ReactiveUI;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.Views.Auth;
using ViridiscaUi.Windows;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Views.Education;
using ViridiscaUi.Views.Common;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.Views.System;
using ViridiscaUi.Views.Common.System;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Единый локатор представлений для ReactiveUI
/// Автоматически связывает ViewModels с соответствующими Views
/// </summary>
public class ReactiveViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        if (viewModel is null)
            return null;

        return viewModel switch
        {
            // Auth ViewModels
            LoginViewModel => new LoginView(),
            RegisterViewModel => new RegisterView(),
            AuthenticationViewModel => new AuthenticationView(),

            // Profile ViewModels
            ProfileViewModel => new ProfileView(),

            // Main ViewModels
            MainViewModel => new MainWindow(),
            HomeViewModel => new HomeView(),

            // Education ViewModels
            CoursesViewModel => new CoursesView(),
            AssignmentsViewModel => new AssignmentsView(),
            GradesViewModel => new GradesView(),
            TeachersViewModel => new TeachersView(),
            GroupsViewModel => new GroupsView(),
            StudentsViewModel => new StudentsView(),

            // System ViewModels
            NotificationCenterViewModel => new NotificationCenterView(),

            // Fallback - создаем простой UserControl с TextBlock
            _ => new FallbackView { DataContext = viewModel }
        };
    }
}

/// <summary>
/// Fallback view для неизвестных ViewModels
/// </summary>
public class FallbackView : UserControl, IViewFor
{
    public FallbackView()
    {
        Content = new TextBlock 
        { 
            Text = "Представление не найдено",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }

    public object? ViewModel 
    { 
        get => DataContext; 
        set => DataContext = value; 
    }
}
