using System.Threading.Tasks;

namespace ViridiscaUi.ViewModels.Bases.Navigation_New;

/// <summary>
/// Интерфейс для ViewModels, которые могут принимать параметры
/// </summary>
public interface IParameterizedViewModel
{
    /// <summary>
    /// Параметр, переданный в ViewModel
    /// </summary>
    object? Parameter { get; }

    /// <summary>
    /// Результат выполнения операции
    /// </summary>
    object? Result { get; }

    /// <summary>
    /// Инициализирован ли ViewModel
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Инициализация с параметром
    /// </summary>
    /// <param name="parameter">Параметр для инициализации</param>
    void Initialize(object parameter);

    /// <summary>
    /// Асинхронная инициализация с параметром
    /// </summary>
    /// <param name="parameter">Параметр для инициализации</param>
    Task InitializeAsync(object parameter);
} 