using System;
using System.Threading.Tasks;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Интерфейс для сервиса навигации
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Текущий путь навигации
        /// </summary>
        string CurrentRoute { get; }
        
        /// <summary>
        /// Наблюдаемый объект для отслеживания изменений маршрута
        /// </summary>
        IObservable<string> RouteChanged { get; }
        
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
    }
} 