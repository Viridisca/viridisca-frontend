using ReactiveUI;
using System.Reactive;

namespace ViridiscaUi.ViewModels.Bases.Navigations;

/// <summary>
/// ViewModel для элемента навигационного меню
/// </summary>
public class NavigationItemViewModel : ViewModelBase
{
    /// <summary>
    /// Текст элемента навигации
    /// </summary>
    public string Label { get; }
    
    /// <summary>
    /// Ключ иконки элемента навигации
    /// </summary>
    public string IconKey { get; }
    
    /// <summary>
    /// Команда навигации
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateCommand { get; }
    
    /// <summary>
    /// Создает новый элемент навигационного меню
    /// </summary>
    /// <param name="label">Текст элемента</param>
    /// <param name="iconKey">Ключ иконки</param>
    /// <param name="navigateCommand">Команда навигации</param>
    public NavigationItemViewModel(
        string label, 
        string iconKey, 
        ReactiveCommand<Unit, IRoutableViewModel> navigateCommand)
    {
        Label = label;
        IconKey = iconKey;
        NavigateCommand = navigateCommand;
    }
} 