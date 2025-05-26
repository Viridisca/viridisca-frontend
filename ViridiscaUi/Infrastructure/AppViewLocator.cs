using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;
using System;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Pages;
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.ViewModels.Profile;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Views.Auth;
using ViridiscaUi.Views.Common;
using ViridiscaUi.Views.Education;
using ViridiscaUi.Views.Education.Students;
using ViridiscaUi.Views.Administration;
using ViridiscaUi.Views.Common.Profile;
using ViridiscaUi.Windows;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// ViewLocator для маршрутизации ViewModel к соответствующим View
/// </summary>
public class AppViewLocator : IViewLocator, IDataTemplate
{
    /// <summary>
    /// Возвращает View для заданной ViewModel
    /// </summary>
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    {
        if (viewModel == null)
            return null;
            
        // Сопоставление ViewModel с соответствующей View согласно новой структуре
        if (viewModel is MainViewModel mainVm)
            return new MainView { DataContext = mainVm };
            
        if (viewModel is AuthenticationViewModel authVm)
            return new AuthenticationView { DataContext = authVm };
            
        if (viewModel is LoginViewModel loginVm)
            return new LoginView { DataContext = loginVm };
            
        if (viewModel is RegisterViewModel registerVm)
            return new RegisterView { DataContext = registerVm };
            
        if (viewModel is HomeViewModel homeVm)
            return new HomeView { DataContext = homeVm };
            
        // Правильный маппинг для CoursesViewModel из Education namespace
        if (viewModel is ViridiscaUi.ViewModels.Education.CoursesViewModel educationCoursesVm)
            return new CoursesView { DataContext = educationCoursesVm };
            
        // Маппинг для CoursesViewModel из Pages namespace (если есть)
        if (viewModel is ViridiscaUi.ViewModels.Pages.CoursesViewModel pagesCoursesVm)
            return new CoursesView { DataContext = pagesCoursesVm };
            
        if (viewModel is UsersViewModel usersVm)
            return new UsersView { DataContext = usersVm }; 

        if (viewModel is StudentsViewModel studentsVm)
            return new StudentsView { DataContext = studentsVm };

        if (viewModel is TeachersViewModel teachersVm)
            return new TeachersView { DataContext = teachersVm };

        if (viewModel is GradesViewModel gradesVm)
            return new GradesView { DataContext = gradesVm };

        if (viewModel is ProfileViewModel profileVm)
            return new ProfileView { DataContext = profileVm };

        // Если нет явного сопоставления, пытаемся найти View по модульной структуре
        var viewModelName = viewModel.GetType().FullName!;
        var viewType = TryFindViewByModularStructure(viewModelName);
        
        if (viewType != null)
        {
            var view = Activator.CreateInstance(viewType) as IViewFor;
            if (view != null)
            {
                if (view is Control control)
                {
                    control.DataContext = viewModel;
                }
                return view;
            }
        }
        
        // Если View не найдено, выбрасываем исключение
        throw new ArgumentOutOfRangeException(
            nameof(viewModel),
            $"Не удалось найти представление для {viewModel.GetType().Name}"
        );
    }

    /// <summary>
    /// Попытка найти View по модульной структуре
    /// </summary>
    private Type? TryFindViewByModularStructure(string viewModelName)
    {
        var viewName = viewModelName.Replace("ViewModel", "View");
        
        // Определяем модуль и ищем в соответствующей папке
        if (viewModelName.Contains(".Auth."))
        {
            return Type.GetType(viewName.Replace("ViridiscaUi.ViewModels.Auth", "ViridiscaUi.Views.Auth"));
        }
        else if (viewModelName.Contains(".Students."))
        {
            return Type.GetType(viewName.Replace("ViridiscaUi.ViewModels.Students", "ViridiscaUi.Views.Education.Students"));
        }
        else if (viewModelName.Contains(".Profile."))
        {
            return Type.GetType(viewName.Replace("ViridiscaUi.ViewModels.Profile", "ViridiscaUi.Views.Common.Profile"));
        }
        else if (viewModelName.Contains(".Pages."))
        {
            // Определяем специфическое местоположение для каждой страницы
            if (viewName.Contains("Home"))
                return Type.GetType(viewName.Replace("ViridiscaUi.ViewModels.Pages", "ViridiscaUi.Views.Common"));
            else if (viewName.Contains("Courses"))
                return Type.GetType(viewName.Replace("ViridiscaUi.ViewModels.Pages", "ViridiscaUi.Views.Education"));
            else if (viewName.Contains("Users"))
                return Type.GetType(viewName.Replace("ViridiscaUi.ViewModels.Pages", "ViridiscaUi.Views.Administration"));
        }
        
        // Fallback: пытаемся найти в общих папках
        return Type.GetType(viewName.Replace("ViridiscaUi.ViewModels", "ViridiscaUi.Views.Common"));
    }

    /// <summary>
    /// Создает элемент управления для ViewModel
    /// </summary>
    public Control? Build(object? data)
    {
        if (data == null)
            return null;

        try
        {
            var view = ResolveView(data);
            return view as Control;
        }
        catch
        {
            return new TextBlock { Text = $"Не найдено представление для {data.GetType().Name}" };
        }
    }

    /// <summary>
    /// Проверяет, подходит ли шаблон для данного объекта
    /// </summary>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
