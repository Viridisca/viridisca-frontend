using ReactiveUI;
using System.Reactive;

namespace ViridiscaUi.ViewModels.Pages;

/// <summary>
/// ViewModel для главной страницы
/// </summary>
public class HomeViewModel : RoutableViewModelBase
{
    public override string UrlPathSegment => "home";

    public string Title => "Главная страница";
    public string Description => "Добро пожаловать в ViridiscaUi LMS!";

    /// <summary>
    /// Команда для навигации к странице курсов
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToCoursesCommand { get; }

    public HomeViewModel(IScreen hostScreen) : base(hostScreen)
    {
        // Создаем команду для навигации к странице курсов
        NavigateToCoursesCommand = ReactiveCommand.CreateFromObservable(
            () => HostScreen.Router.Navigate.Execute(new CoursesViewModel(hostScreen))
        );
    }
} 