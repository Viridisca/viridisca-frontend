using System;
using System.Threading.Tasks;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для навигации между представлениями
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Навигация к представлению
        /// </summary>
        Task NavigateToAsync(string viewName);
        
        /// <summary>
        /// Навигация к представлению с параметром
        /// </summary>
        Task NavigateToAsync(string viewName, object parameter);
        
        /// <summary>
        /// Навигация назад
        /// </summary>
        Task NavigateBackAsync();
        
        /// <summary>
        /// Текущий путь навигации
        /// </summary>
        string CurrentRoute { get; }
    }
} 