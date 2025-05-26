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
    /// Конструктор по умолчанию для fallback режима
    /// </summary>
    public CoursesViewModel() : base(null)
    {
        // Инициализация без навигационных команд
    }

    public CoursesViewModel(IScreen hostScreen) : base(hostScreen)
    {
        // Инициализация без навигационных команд
    }
} 