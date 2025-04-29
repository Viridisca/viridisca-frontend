using ReactiveUI;
using System.Reactive;

namespace ViridiscaUi.ViewModels.Pages;

/// <summary>
/// ViewModel для страницы с пользователями
/// </summary>
public class UsersViewModel : RoutableViewModelBase
{
    public override string UrlPathSegment => "users";

    public string Title => "Пользователи";
    public string Description => "Управление пользователями системы";

    /// <summary>
    /// Команда для возврата назад
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> GoBackCommand { get; }

    public UsersViewModel(IScreen hostScreen) : base(hostScreen)
    {
        // Команда для возврата на предыдущую страницу
        GoBackCommand = hostScreen.Router.NavigateBack;
    }
} 