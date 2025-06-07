namespace ViridiscaUi.ViewModels.Bases.Navigation_New
{
    /// <summary>
    /// Represents a ViewModel that can produce a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IResultantViewModel<out TResult>
    {
        /// <summary>
        /// Gets the result produced by the ViewModel.
        /// </summary>
        TResult Result { get; }
    }
} 