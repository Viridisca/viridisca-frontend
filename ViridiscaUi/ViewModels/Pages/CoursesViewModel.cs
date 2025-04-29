using ReactiveUI;
using System.Reactive;

namespace ViridiscaUi.ViewModels.Pages;

/// <summary>
/// ViewModel для страницы с курсами
/// </summary>
public class CoursesViewModel : RoutableViewModelBase
{
    public override string UrlPathSegment => "courses";

    public string Title => "Курсы";
    public string Description => "Список доступных курсов";

    /// <summary>
    /// Команда для навигации к странице пользователей
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToUsersCommand { get; }

    /// <summary>
    /// Команда для возврата назад
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> GoBackCommand { get; }

    public CoursesViewModel(IScreen hostScreen) : base(hostScreen)
    {
        // Создаем команду для навигации к странице пользователей
        NavigateToUsersCommand = ReactiveCommand.CreateFromObservable(
            () => HostScreen.Router.Navigate.Execute(new UsersViewModel(hostScreen))
        );

        // Команда для возврата на предыдущую страницу
        GoBackCommand = hostScreen.Router.NavigateBack;
    }
} 