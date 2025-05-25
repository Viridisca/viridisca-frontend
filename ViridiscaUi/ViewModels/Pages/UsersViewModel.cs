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

    public UsersViewModel(IScreen hostScreen) : base(hostScreen)
    {
        // Инициализация без навигационных команд
    }
} 