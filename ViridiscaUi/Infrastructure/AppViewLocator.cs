using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;
using System;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Pages;
using ViridiscaUi.Views.Auth;
using ViridiscaUi.Views.Pages;

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
            
        // Сопоставление ViewModel с соответствующей View
        if (viewModel is LoginViewModel loginVm)
            return new LoginView { DataContext = loginVm };
            
        if (viewModel is HomeViewModel homeVm)
            return new HomeView { DataContext = homeVm };
            
        if (viewModel is CoursesViewModel coursesVm)
            return new CoursesView { DataContext = coursesVm };
            
        if (viewModel is UsersViewModel usersVm)
            return new UsersView { DataContext = usersVm }; 

        // Если нет явного сопоставления, пытаемся найти View по названию
        var viewModelName = viewModel.GetType().FullName!;
        var viewTypeName = viewModelName
            .Replace("ViewModel", "View")
            .Replace("ViridiscaUi.ViewModels", "ViridiscaUi.Views");
            
        var viewType = Type.GetType(viewTypeName);
        
        if (viewType != null)
        {
            return Activator.CreateInstance(viewType) as IViewFor;
        }
        
        return null;
         
        // Если View не найдено, выбрасываем исключение
        throw new ArgumentOutOfRangeException(
            nameof(viewModel),
            $"Не удалось найти представление для {viewModel.GetType().Name}"
        );
    } 

    /// <summary>
    /// Создает элемент управления для ViewModel
    /// </summary>
    public Control Build(object data)
    {
        var name = data.GetType().FullName!
            .Replace("ViewModel", "View")
            .Replace("ViridiscaUi.ViewModels", "ViridiscaUi.Views");

        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = $"Не найдено представление для {data.GetType().Name}" };
    }

    /// <summary>
    /// Проверяет, подходит ли шаблон для данного объекта
    /// </summary>
    public bool Match(object data)
    {
        return data is ViewModelBase;
    }
}
