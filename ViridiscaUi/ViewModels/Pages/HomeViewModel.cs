using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using ViridiscaUi.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.ViewModels.Pages;

/// <summary>
/// ViewModel для главной страницы
/// </summary>
public class HomeViewModel : ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment => "home";
    public IScreen HostScreen { get; }

    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public string Title => "Главная страница";
    public string Description => "Добро пожаловать в ViridiscaUi LMS!";

    /// <summary>
    /// Команда для навигации к странице курсов
    /// </summary>
    public ReactiveCommand<Unit, IRoutableViewModel> NavigateToCoursesCommand { get; }

    public HomeViewModel(INavigationService navigationService, IServiceProvider serviceProvider, IScreen hostScreen)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        HostScreen = hostScreen;

        // Создаем команду для навигации к странице курсов
        NavigateToCoursesCommand = ReactiveCommand.CreateFromObservable(
            () => {
                var coursesViewModel = _serviceProvider.GetRequiredService<ViridiscaUi.ViewModels.Education.CoursesViewModel>();
                return HostScreen.Router.Navigate.Execute(coursesViewModel);
            }
        );
    }
} 