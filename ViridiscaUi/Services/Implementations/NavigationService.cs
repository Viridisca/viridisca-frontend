using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса навигации
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly BehaviorSubject<string> _routeChangedSubject = new BehaviorSubject<string>(string.Empty);
        private string _currentRoute = string.Empty;
        
        /// <summary>
        /// Текущий путь навигации
        /// </summary>
        public string CurrentRoute => _currentRoute;
        
        /// <summary>
        /// Наблюдаемый объект для отслеживания изменений маршрута
        /// </summary>
        public IObservable<string> RouteChanged => _routeChangedSubject.AsObservable();
        
        /// <summary>
        /// Навигация к представлению
        /// </summary>
        public Task NavigateToAsync(string viewName)
        {
            _currentRoute = viewName;
            _routeChangedSubject.OnNext(viewName);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Навигация к представлению с параметром
        /// </summary>
        public Task NavigateToAsync(string viewName, object parameter)
        {
            _currentRoute = viewName;
            _routeChangedSubject.OnNext(viewName);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Навигация назад
        /// </summary>
        public Task NavigateBackAsync()
        {
            _currentRoute = "back";
            _routeChangedSubject.OnNext("back");
            return Task.CompletedTask;
        }
    }
} 