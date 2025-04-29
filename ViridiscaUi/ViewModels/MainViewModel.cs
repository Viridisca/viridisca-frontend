using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using ViridiscaUi.ViewModels.Pages;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel, реализующая IScreen для управления навигацией
/// </summary>
public class MainViewModel : ViewModelBase, IScreen
{
    /// <summary>
    /// RoutingState для управления навигацией
    /// </summary>
    public RoutingState Router { get; } = new RoutingState();

    /// <summary>
    /// Команда для перехода на главную страницу
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToHomeCommand { get; }

    /// <summary>
    /// Команда для перехода на страницу курсов
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToCoursesCommand { get; }

    /// <summary>
    /// Команда для перехода на страницу пользователей
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToUsersCommand { get; }

    /// <summary>
    /// Команда для возврата назад
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> GoBackCommand { get; }

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public MainViewModel()
    {
        // Инициализация команд навигации
        NavigateToHomeCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new HomeViewModel(this))
        );

        NavigateToCoursesCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new CoursesViewModel(this))
        );

        NavigateToUsersCommand = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new UsersViewModel(this))
        );

        // Команда для возврата назад по стеку навигации
        GoBackCommand = Router.NavigateBack;

        // Стартовая навигация на главную страницу
        NavigateToHomeCommand.Execute().Subscribe(_ => { });
    }
}
