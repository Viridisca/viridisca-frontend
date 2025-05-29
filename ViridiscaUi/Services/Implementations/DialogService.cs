using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ViridiscaUi.Services.Interfaces;
using Avalonia.Media;
using Avalonia.Layout;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Views.Education;
using ViridiscaUi.Domain.Models.System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls.Templates;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Windows;
using ViridiscaUi.Views.System;


namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с диалогами
/// </summary>
public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Получает активное окно для использования в качестве владельца диалогов
    /// </summary>
    private Window? GetOwnerWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    /// <summary>
    /// Настраивает диалоговое окно с правильным владельцем и обработчиками событий
    /// </summary>
    private void ConfigureDialog(Window dialog)
    {
        var ownerWindow = GetOwnerWindow();
        
        // Устанавливаем владельца и позицию
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        // Подписываемся на закрытие главного окна, чтобы закрыть диалог
        void OnOwnerClosing(object? sender, WindowClosingEventArgs e)
        {
            if (dialog.IsVisible)
            {
                dialog.Close();
            }
            ownerWindow?.Closing -= OnOwnerClosing;
        }
        
        ownerWindow?.Closing += OnOwnerClosing;
        
        // Также подписываемся на закрытие диалога, чтобы очистить обработчики
        dialog.Closed += (s, e) =>
        {
            ownerWindow?.Closing -= OnOwnerClosing;
        };
    }

    /// <summary>
    /// Показывает информационное сообщение
    /// </summary>
    public async Task ShowInfoAsync(string title, string message)
    {
        await ShowMessageBoxAsync(title, message, "Информация");
    }

    /// <summary>
    /// Показывает сообщение об ошибке
    /// </summary>
    public async Task ShowErrorAsync(string title, string message)
    {
        await ShowMessageBoxAsync(title, message, "Ошибка");
    }

    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    public async Task ShowWarningAsync(string title, string message)
    {
        await ShowMessageBoxAsync(title, message, "Предупреждение");
    }

    /// <summary>
    /// Показывает запрос подтверждения
    /// </summary>
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();
        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new Button { Content = "Да" },
                            new Button { Content = "Нет" }
                        }
                    }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        ConfigureDialog(dialog);

        var buttons = ((dialog.Content as StackPanel)?.Children[1] as StackPanel)?.Children;
        if (buttons != null)
        {
            var yesButton = buttons[0] as Button;
            var noButton = buttons[1] as Button;

            if (yesButton != null)
            {
                void OnYesClick(object? sender, RoutedEventArgs args)
                {
                    tcs.SetResult(true);
                    dialog.Close();
                    yesButton.Click -= OnYesClick;
                }
                yesButton.Click += OnYesClick;
            }

            if (noButton != null)
            {
                void OnNoClick(object? sender, RoutedEventArgs args)
                {
                    tcs.SetResult(false);
                    dialog.Close();
                    noButton.Click -= OnNoClick;
                }
                noButton.Click += OnNoClick;
            }
        }

        await dialog.ShowDialog(GetOwnerWindow());
        return await tcs.Task;
    }

    /// <summary>
    /// Показывает диалог с вводом текста
    /// </summary>
    public async Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "")
    {
        var tcs = new TaskCompletionSource<string?>();
        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message },
                    new TextBox { Text = defaultValue },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new Button { Content = "OK" },
                            new Button { Content = "Отмена" }
                        }
                    }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        ConfigureDialog(dialog);

        var buttons = ((dialog.Content as StackPanel)?.Children[2] as StackPanel)?.Children;
        if (buttons != null)
        {
            var okButton = buttons[0] as Button;
            var cancelButton = buttons[1] as Button;

            if (okButton != null)
            {
                void OnOkClick(object? sender, RoutedEventArgs args)
                {
                    var textBox = ((dialog.Content as StackPanel)?.Children[1]) as TextBox;
                    tcs.SetResult(textBox?.Text);
                    dialog.Close();
                    okButton.Click -= OnOkClick;
                }
                okButton.Click += OnOkClick;
            }

            if (cancelButton != null)
            {
                void OnCancelClick(object? sender, RoutedEventArgs args)
                {
                    tcs.SetResult(null);
                    dialog.Close();
                    cancelButton.Click -= OnCancelClick;
                }
                cancelButton.Click += OnCancelClick;
            }
        }

        await dialog.ShowDialog(GetOwnerWindow());
        return await tcs.Task;
    }

    /// <summary>
    /// Показывает диалог с выбором из списка
    /// </summary>
    public async Task<T?> ShowSelectionDialogAsync<T>(string title, string message, T[] items)
    {
        var tcs = new TaskCompletionSource<T?>();
        var listBox = new ListBox();
        foreach (var item in items)
        {
            listBox.Items.Add(item);
        }

        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message },
                    listBox,
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new Button { Content = "Выбрать" },
                            new Button { Content = "Отмена" }
                        }
                    }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        ConfigureDialog(dialog);

        var buttons = ((dialog.Content as StackPanel)?.Children[2] as StackPanel)?.Children;
        if (buttons != null)
        {
            var selectButton = buttons[0] as Button;
            var cancelButton = buttons[1] as Button;

            if (selectButton != null)
            {
                void OnSelectClick(object? sender, RoutedEventArgs args)
                {
                    tcs.SetResult((T?)listBox.SelectedItem);
                    dialog.Close();
                    selectButton.Click -= OnSelectClick;
                }
                selectButton.Click += OnSelectClick;
            }

            if (cancelButton != null)
            {
                void OnCancelClick(object? sender, RoutedEventArgs args)
                {
                    tcs.SetResult(default);
                    dialog.Close();
                    cancelButton.Click -= OnCancelClick;
                }
                cancelButton.Click += OnCancelClick;
            }
        }

        await dialog.ShowDialog(GetOwnerWindow());
        return await tcs.Task;
    }

    public async Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase viewModel)
    {
        if (viewModel is StudentEditorViewModel studentEditor)
        {
            var window = new StudentEditorWindow
            {
                ViewModel = studentEditor
            };
            
            ConfigureDialog(window);
            
            var tcs = new TaskCompletionSource<Student?>();
            
            studentEditor.SaveCommand.Subscribe(_ =>
            {
                tcs.SetResult(studentEditor.CurrentStudent);
                window.Close();
            });

            studentEditor.CancelCommand.Subscribe(_ =>
            {
                tcs.SetResult(null);
                window.Close();
            });

            window.Title = studentEditor.FormTitle;

            await window.ShowDialog(GetOwnerWindow());
            return (TResult?)(object?)await tcs.Task;
        }
        
        if (viewModel is DepartmentEditDialogViewModel departmentEditViewModel)
        {
            var dialog = new DepartmentEditDialog(departmentEditViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<bool?>(GetOwnerWindow());
            return (TResult?)(object?)(result == true ? departmentEditViewModel.Department : null);
        }
        
        if (viewModel is DepartmentDetailsDialogViewModel departmentDetailsViewModel)
        {
            var dialog = new DepartmentDetailsDialog(departmentDetailsViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<bool?>(GetOwnerWindow());
            return (TResult?)(object?)result;
        }

        throw new ArgumentException($"Unsupported view model type: {viewModel.GetType()}");
    }

    public async Task<Student?> ShowStudentEditDialogAsync(Student? student = null)
    {
        var mainWindow = GetOwnerWindow();
        if (mainWindow == null) return null;

        try
        {
            var studentService = _serviceProvider.GetRequiredService<IStudentService>();
            var groupService = _serviceProvider.GetRequiredService<IGroupService>();
            
            var viewModel = new StudentEditorViewModel(studentService, groupService, student);

            var dialog = new StudentEditDialog
            {
                DataContext = viewModel
            };

            var result = await dialog.ShowDialog<bool?>(mainWindow);
            
            if (result == true)
            {
                return viewModel.CurrentStudent;
            }

            return null;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования студента: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Показывает диалог редактирования группы
    /// </summary>
    public async Task<Group?> ShowGroupEditDialogAsync(Group group)
    {
        try
        {
            var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
            var editorViewModel = new GroupEditorViewModel(teacherService, group);
            
            var dialog = new ViridiscaUi.Views.Education.GroupEditDialog(editorViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<Group?>(GetOwnerWindow());
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования группы: {ex.Message}");
            return null;
        }
    }
    
    public async Task<Teacher?> ShowTeacherSelectionDialogAsync(IEnumerable<Teacher> teachers)
    {
        var teachersList = teachers.ToArray();
        if (!teachersList.Any())
        {
            await ShowWarningAsync("Предупреждение", "Нет доступных преподавателей для выбора");
            return null;
        }

        return await ShowSelectionDialogAsync("Выбор преподавателя", "Выберите преподавателя:", teachersList);
    }
    
    public async Task<object?> ShowGroupStudentsManagementDialogAsync(Group group, IEnumerable<Student> allStudents)
    {
        // TODO: Реализовать диалог управления студентами группы
        await Task.Delay(100);
        return new object();
    }
    
    // Диалоги для курсов
    public async Task<Course?> ShowCourseEditDialogAsync(Course? course = null)
    {
        try
        {
            var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
            var courseService = _serviceProvider.GetRequiredService<ICourseService>();
            var editorViewModel = new CourseEditorViewModel(courseService, teacherService, course);
            
            var dialog = new ViridiscaUi.Views.Education.CourseEditDialog(editorViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<Course?>(GetOwnerWindow());
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования курса: {ex.Message}");
            return null;
        }
    }
    
    public async Task<object?> ShowCourseEnrollmentDialogAsync(Course course, IEnumerable<Student> allStudents)
    {
        // TODO: Реализовать диалог записи на курс
        await Task.Delay(100);
        return new object();
    }
    
    public async Task<Group?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups)
    {
        var groupsList = groups.ToArray();
        if (!groupsList.Any())
        {
            await ShowWarningAsync("Предупреждение", "Нет доступных групп для выбора");
            return null;
        }

        return await ShowSelectionDialogAsync("Выбор группы", "Выберите группу:", groupsList);
    }
    
    // Диалоги для заданий
    public async Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment assignment)
    {
        var editorViewModel = new AssignmentEditorViewModel(_serviceProvider.GetRequiredService<ICourseService>(), assignment);
        var result = await ShowEditorDialogAsync<Assignment>(editorViewModel);
        return result;
    }
    
    public async Task<object?> ShowSubmissionsViewDialogAsync(Assignment assignment, IEnumerable<Submission> submissions)
    {
        // TODO: Реализовать диалог просмотра сдач
        await Task.Delay(100);
        return new object();
    }
    
    public async Task<IEnumerable<object>?> ShowBulkGradingDialogAsync(IEnumerable<Submission> submissions)
    {
        // TODO: Реализовать диалог массового оценивания
        await Task.Delay(100);
        return new List<object>();
    }
    
    // Диалоги для уведомлений
    public async Task<NotificationTemplate?> ShowNotificationTemplateEditDialogAsync(NotificationTemplate template)
    {
        // TODO: Реализовать диалог редактирования шаблона уведомления
        await Task.Delay(100);
        return template;
    }
    
    public async Task<Dictionary<string, object>?> ShowTemplateParametersDialogAsync(NotificationTemplate template)
    {
        // TODO: Реализовать диалог параметров шаблона
        await Task.Delay(100);
        return new Dictionary<string, object>();
    }
    
    public async Task<ReminderData?> ShowCreateReminderDialogAsync()
    {
        // TODO: Реализовать диалог создания напоминания
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает запрос подтверждения с кастомными кнопками
    /// </summary>
    public async Task<bool> ShowConfirmationDialogAsync(string title, string message, string confirmText = "Да", string cancelText = "Нет")
    {
        var mainWindow = GetOwnerWindow();
        if (mainWindow == null) return false;

        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var stackPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20
        };

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10
        };

        var confirmButton = new Button
        {
            Content = confirmText,
            MinWidth = 80,
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Content = cancelText,
            MinWidth = 80,
            IsCancel = true
        };

        bool result = false;

        confirmButton.Click += (s, e) =>
        {
            result = true;
            dialog.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            result = false;
            dialog.Close();
        };

        buttonPanel.Children.Add(confirmButton);
        buttonPanel.Children.Add(cancelButton);
        
        stackPanel.Children.Add(messageText);
        stackPanel.Children.Add(buttonPanel);
        
        dialog.Content = stackPanel;

        await dialog.ShowDialog(mainWindow);
        return result;
    }

    /// <summary>
    /// Показывает диалог с вводом текста (альтернативное название)
    /// </summary>
    public async Task<string?> ShowTextInputDialogAsync(string title, string message, string defaultValue = "")
    {
        return await ShowInputDialogAsync(title, message, defaultValue);
    }

    /// <summary>
    /// Показывает диалог выбора файла для открытия
    /// </summary>
    public async Task<string?> ShowFileOpenDialogAsync(string title, string[] filters)
    {
        var mainWindow = GetOwnerWindow();
        if (mainWindow?.StorageProvider == null) return null;

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = CreateFileTypeFilters(filters)
        };

        var files = await mainWindow.StorageProvider.OpenFilePickerAsync(options);
        return files?.FirstOrDefault()?.Path.LocalPath;
    }

    /// <summary>
    /// Показывает диалог сохранения файла
    /// </summary>
    public async Task<string?> ShowFileSaveDialogAsync(string title, string defaultFileName, string[] fileTypes)
    {
        var mainWindow = GetOwnerWindow();
        if (mainWindow?.StorageProvider == null) return null;

        var options = new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = defaultFileName,
            FileTypeChoices = CreateFileTypeFilters(fileTypes)
        };

        var file = await mainWindow.StorageProvider.SaveFilePickerAsync(options);
        return file?.Path.LocalPath;
    }

    /// <summary>
    /// Показывает диалог управления содержимым курса
    /// </summary>
    public async Task<object?> ShowCourseContentManagementDialogAsync(Course course)
    {
        // TODO: Реализовать диалог управления содержимым курса
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог управления студентами курса
    /// </summary>
    public async Task<object?> ShowCourseStudentsManagementDialogAsync(Course course, IEnumerable<Student> students)
    {
        // TODO: Реализовать диалог управления студентами курса
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог статистики курса
    /// </summary>
    public async Task<object?> ShowCourseStatisticsDialogAsync(string title, CourseStatistics statistics)
    {
        // TODO: Реализовать диалог статистики курса
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования оценки
    /// </summary>
    public async Task<Grade?> ShowGradeEditDialogAsync(Grade grade, IEnumerable<Student> students, IEnumerable<Assignment> assignments)
    {
        var editorViewModel = new GradeEditorViewModel(
            _serviceProvider.GetRequiredService<IStudentService>(),
            _serviceProvider.GetRequiredService<IAssignmentService>(),
            _serviceProvider.GetRequiredService<ITeacherService>(),
            grade);
        var result = await ShowEditorDialogAsync<Grade>(editorViewModel);
        return result;
    }

    /// <summary>
    /// Показывает диалог массового выставления оценок
    /// </summary>
    public async Task<IEnumerable<Grade>?> ShowBulkGradingDialogAsync(IEnumerable<Course> courses, IEnumerable<Assignment> assignments)
    {
        // TODO: Реализовать диалог массового оценивания
        await Task.Delay(100);
        return new List<Grade>();
    }

    /// <summary>
    /// Показывает диалог редактирования преподавателя
    /// </summary>
    public async Task<Teacher?> ShowTeacherEditDialogAsync(Teacher? teacher = null)
    {
        try
        {
            var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
            var navigationService = _serviceProvider.GetRequiredService<IUnifiedNavigationService>();
            var screen = _serviceProvider.GetRequiredService<IScreen>();
            
            // Создаем ViewModel для диалога без навигации
            var editorViewModel = new TeacherEditorViewModel(screen, teacherService, navigationService, this);
            
            if (teacher != null)
            {
                editorViewModel.PopulateForm(teacher);
                editorViewModel.IsEditMode = true;
                editorViewModel.FormTitle = "Редактирование преподавателя";
            }
            else
            {
                editorViewModel.IsEditMode = false;
                editorViewModel.FormTitle = "Создание преподавателя";
            }
            
            var dialog = new ViridiscaUi.Views.Education.TeacherEditDialog(editorViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<Teacher?>(GetOwnerWindow());
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования преподавателя: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Показывает диалог с подробной информацией о преподавателе
    /// </summary>
    public async Task<string?> ShowTeacherDetailsDialogAsync(Teacher teacher)
    {
        try
        {
            var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
            var navigationService = _serviceProvider.GetRequiredService<IUnifiedNavigationService>();
            var screen = _serviceProvider.GetRequiredService<IScreen>();
            
            var editorViewModel = new TeacherEditorViewModel(screen, teacherService, navigationService, this);
            editorViewModel.PopulateForm(teacher);
            editorViewModel.FormTitle = "Информация о преподавателе";
            
            var dialog = new ViridiscaUi.Views.Education.TeacherDetailsDialog(editorViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<string?>(GetOwnerWindow());
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог деталей преподавателя: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Показывает диалог управления курсами преподавателя
    /// </summary>
    public async Task<object?> ShowTeacherCoursesManagementDialogAsync(Teacher teacher, IEnumerable<Course> courses)
    {
        // TODO: Реализовать диалог управления курсами преподавателя
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог управления группами преподавателя
    /// </summary>
    public async Task<object?> ShowTeacherGroupsManagementDialogAsync(Teacher teacher, IEnumerable<Group> groups)
    {
        // TODO: Реализовать диалог управления группами преподавателя
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог статистики преподавателя
    /// </summary>
    public async Task<object?> ShowTeacherStatisticsDialogAsync(string title, object statistics)
    {
        // TODO: Реализовать диалог статистики преподавателя
        await Task.Delay(1);
        return null;
    }

    // === ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ===

    /// <summary>
    /// Универсальный метод для показа диалогов редактирования
    /// </summary>
    private async Task<T?> ShowEditorDialogAsync<T>(ViewModelBase editorViewModel) where T : class
    {
        var window = new Window
        {
            Title = GetEditorTitle(editorViewModel),
            Width = 600,
            Height = 500,
            Content = editorViewModel,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var result = await window.ShowDialog<T?>(GetOwnerWindow());
        return result;
    }

    /// <summary>
    /// Создает контент для диалога редактирования
    /// </summary>
    private static Grid CreateEditorContent<T>(ViewModelBase editorViewModel, TaskCompletionSource<T?> tcs, Window window) where T : class
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(16)
        };

        // Основной контент (будет заполнен в зависимости от типа ViewModel)
        var contentArea = CreateEditorContentArea(editorViewModel);
        Grid.SetRow(contentArea, 0);
        grid.Children.Add(contentArea);

        // Кнопки
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Thickness(0, 16, 0, 0)
        };

        var saveButton = new Button
        {
            Content = "Сохранить",
            Padding = new Thickness(16, 8),
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Content = "Отмена",
            Padding = new Thickness(16, 8),
            IsCancel = true
        };

        // Привязываем команды
        SetupEditorCommands(editorViewModel, saveButton, cancelButton, tcs, window);

        buttonPanel.Children.Add(cancelButton);
        buttonPanel.Children.Add(saveButton);

        Grid.SetRow(buttonPanel, 1);
        grid.Children.Add(buttonPanel);

        return grid;
    }

    /// <summary>
    /// Создает область контента для редактора
    /// </summary>
    private static Control CreateEditorContentArea(ViewModelBase editorViewModel)
    {
        return editorViewModel switch
        {
            GroupEditorViewModel => CreateGroupEditorContent(),
            CourseEditorViewModel => CreateCourseEditorContent(),
            TeacherEditorViewModel => CreateTeacherEditorContent(),
            AssignmentEditorViewModel => CreateAssignmentEditorContent(),
            GradeEditorViewModel => CreateGradeEditorContent(),
            _ => new TextBlock { Text = "Редактор не реализован" }
        };
    }

    /// <summary>
    /// Настраивает команды для кнопок редактора
    /// </summary>
    private static void SetupEditorCommands<T>(ViewModelBase editorViewModel, Button saveButton, Button cancelButton, TaskCompletionSource<T?> tcs, Window window) where T : class
    {
        switch (editorViewModel)
        {
            case GroupEditorViewModel groupEditor:
                saveButton.Command = groupEditor.SaveCommand;
                cancelButton.Command = groupEditor.CancelCommand;
                
                groupEditor.SaveCommand.Subscribe(result =>
                {
                    tcs.SetResult(result as T);
                    window.Close();
                });
                
                groupEditor.CancelCommand.Subscribe(_ =>
                {
                    tcs.SetResult(null);
                    window.Close();
                });
                break;

            case CourseEditorViewModel courseEditor:
                saveButton.Command = courseEditor.SaveCommand;
                cancelButton.Command = courseEditor.CancelCommand;
                
                courseEditor.SaveCommand.Subscribe(result =>
                {
                    tcs.SetResult(result as T);
                    window.Close();
                });
                
                courseEditor.CancelCommand.Subscribe(_ =>
                {
                    tcs.SetResult(null);
                    window.Close();
                });
                break;

            case TeacherEditorViewModel teacherEditor:
                saveButton.Command = teacherEditor.SaveCommand;
                cancelButton.Command = teacherEditor.CancelCommand;
                
                teacherEditor.SaveCommand.Subscribe(result =>
                {
                    tcs.SetResult(result as T);
                    window.Close();
                });
                
                teacherEditor.CancelCommand.Subscribe(_ =>
                {
                    tcs.SetResult(null);
                    window.Close();
                });
                break;

            case AssignmentEditorViewModel assignmentEditor:
                saveButton.Command = assignmentEditor.SaveCommand;
                cancelButton.Command = assignmentEditor.CancelCommand;
                
                assignmentEditor.SaveCommand.Subscribe(result =>
                {
                    tcs.SetResult(result as T);
                    window.Close();
                });
                
                assignmentEditor.CancelCommand.Subscribe(_ =>
                {
                    tcs.SetResult(null);
                    window.Close();
                });
                break;

            case GradeEditorViewModel gradeEditor:
                saveButton.Command = gradeEditor.SaveCommand;
                cancelButton.Command = gradeEditor.CancelCommand;
                
                gradeEditor.SaveCommand.Subscribe(result =>
                {
                    tcs.SetResult(result as T);
                    window.Close();
                });
                
                gradeEditor.CancelCommand.Subscribe(_ =>
                {
                    tcs.SetResult(null);
                    window.Close();
                });
                break;
        }
    }

    // === СОЗДАНИЕ КОНТЕНТА ДЛЯ РЕДАКТОРОВ ===

    private static Control CreateGroupEditorContent()
    {
        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = "Название группы:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Name") },
                    
                    new TextBlock { Text = "Код группы:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Code") },
                    
                    new TextBlock { Text = "Описание:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Description"), AcceptsReturn = true, Height = 80 },
                    
                    new TextBlock { Text = "Год:", FontWeight = FontWeight.SemiBold },
                    new NumericUpDown { [!NumericUpDown.ValueProperty] = new Binding("Year"), Minimum = 2020, Maximum = 2030 },
                    
                    new TextBlock { Text = "Максимум студентов:", FontWeight = FontWeight.SemiBold },
                    new NumericUpDown { [!NumericUpDown.ValueProperty] = new Binding("MaxStudents"), Minimum = 1, Maximum = 100 },
                    
                    new TextBlock { Text = "Куратор:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("Teachers"),
                        [!ComboBox.SelectedItemProperty] = new Binding("SelectedCurator"),
                        DisplayMemberBinding = new Binding("FullName")
                    }
                }
            }
        };
    }

    private static Control CreateCourseEditorContent()
    {
        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = "Название курса:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Name") },
                    
                    new TextBlock { Text = "Код курса:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Code") },
                    
                    new TextBlock { Text = "Описание:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Description"), AcceptsReturn = true, Height = 80 },
                    
                    new TextBlock { Text = "Категория:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("Categories"),
                        [!ComboBox.SelectedItemProperty] = new Binding("Category")
                    },
                    
                    new TextBlock { Text = "Кредиты:", FontWeight = FontWeight.SemiBold },
                    new NumericUpDown { [!NumericUpDown.ValueProperty] = new Binding("Credits"), Minimum = 1, Maximum = 10 },
                    
                    new TextBlock { Text = "Преподаватель:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("Teachers"),
                        [!ComboBox.SelectedItemProperty] = new Binding("SelectedTeacher"),
                        DisplayMemberBinding = new Binding("FullName")
                    }
                }
            }
        };
    }

    private static Control CreateTeacherEditorContent()
    {
        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = "Имя:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("FirstName") },
                    
                    new TextBlock { Text = "Фамилия:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("LastName") },
                    
                    new TextBlock { Text = "Отчество:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("MiddleName") },
                    
                    new TextBlock { Text = "Email:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Email") },
                    
                    new TextBlock { Text = "Телефон:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("PhoneNumber") },
                    
                    new TextBlock { Text = "Специализация:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("Specializations"),
                        [!ComboBox.SelectedItemProperty] = new Binding("Specialization")
                    },
                    
                    new TextBlock { Text = "Академическое звание:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("AcademicTitles"),
                        [!ComboBox.SelectedItemProperty] = new Binding("AcademicTitle")
                    }
                }
            }
        };
    }

    private static Control CreateAssignmentEditorContent()
    {
        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = "Название задания:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Title") },
                    
                    new TextBlock { Text = "Описание:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Description"), AcceptsReturn = true, Height = 80 },
                    
                    new TextBlock { Text = "Курс:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("Courses"),
                        [!ComboBox.SelectedItemProperty] = new Binding("SelectedCourse"),
                        DisplayMemberBinding = new Binding("Name")
                    },
                    
                    new TextBlock { Text = "Тип задания:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("AssignmentTypes"),
                        [!ComboBox.SelectedItemProperty] = new Binding("Type")
                    },
                    
                    new TextBlock { Text = "Максимальный балл:", FontWeight = FontWeight.SemiBold },
                    new NumericUpDown { [!NumericUpDown.ValueProperty] = new Binding("MaxScore"), Minimum = 1, Maximum = 1000 },
                    
                    new TextBlock { Text = "Срок сдачи:", FontWeight = FontWeight.SemiBold },
                    new DatePicker { [!DatePicker.SelectedDateProperty] = new Binding("DueDate") }
                }
            }
        };
    }

    private static Control CreateGradeEditorContent()
    {
        return new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "Редактирование оценки" },
                new TextBox { Watermark = "Значение оценки" },
                new TextBox { Watermark = "Комментарий" },
                new DatePicker { }
            }
        };
    }

    /// <summary>
    /// Показывает диалог с подробной информацией о студенте
    /// </summary>
    public async Task ShowStudentDetailsDialogAsync(Student student)
    {
        var mainWindow = GetOwnerWindow();
        if (mainWindow == null) return;

        try
        {
            var studentService = _serviceProvider.GetRequiredService<IStudentService>();
            var groupService = _serviceProvider.GetRequiredService<IGroupService>();
            
            var viewModel = new StudentEditorViewModel(studentService, groupService, student);

            var dialog = new StudentDetailsDialog(viewModel);

            await dialog.ShowDialog(mainWindow);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог деталей студента: {ex.Message}");
        }
    }

    /// <summary>
    /// Показывает диалог редактирования департамента
    /// </summary>
    public async Task<Department?> ShowDepartmentEditDialogAsync(Department department)
    {
        var departmentService = _serviceProvider.GetRequiredService<IDepartmentService>();
        var dialogViewModel = new DepartmentEditDialogViewModel(department, departmentService, isEdit: true);
        
        var dialog = new ViridiscaUi.Views.System.DepartmentEditDialog(dialogViewModel);
        ConfigureDialog(dialog);
        
        var result = await dialog.ShowDialog<bool?>(GetOwnerWindow());
        
        return result == true ? dialogViewModel.Department : null;
    }

    /// <summary>
    /// Показывает диалог создания нового департамента
    /// </summary>
    public async Task<Department?> ShowDepartmentCreateDialogAsync()
    {
        var departmentService = _serviceProvider.GetRequiredService<IDepartmentService>();
        var newDepartment = new Department
        {
            Uid = Guid.NewGuid(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
        
        var dialogViewModel = new DepartmentEditDialogViewModel(newDepartment, departmentService, isEdit: false);
        
        var dialog = new ViridiscaUi.Views.System.DepartmentEditDialog(dialogViewModel);
        ConfigureDialog(dialog);
        
        var result = await dialog.ShowDialog<bool?>(GetOwnerWindow());
        
        return result == true ? dialogViewModel.Department : null;
    }

    /// <summary>
    /// Показывает диалог с деталями департамента
    /// </summary>
    public async Task ShowDepartmentDetailsDialogAsync(Department department)
    {
        try
        {
            var departmentService = _serviceProvider.GetRequiredService<IDepartmentService>();
            var statistics = await departmentService.GetDepartmentStatisticsAsync(department.Uid);
            
            var dialogViewModel = new DepartmentDetailsDialogViewModel(department, statistics);
            
            var dialog = new ViridiscaUi.Views.System.DepartmentDetailsDialog(dialogViewModel);
            ConfigureDialog(dialog);
            
            await dialog.ShowDialog(GetOwnerWindow());
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось загрузить детали департамента: {ex.Message}");
        }
    }

    /// <summary>
    /// Показывает диалог редактирования предмета
    /// </summary>
    public async Task<Subject?> ShowSubjectEditDialogAsync(Subject subject)
    {
        try
        {
            var departmentService = _serviceProvider.GetRequiredService<IDepartmentService>();
            var editorViewModel = new SubjectEditorViewModel(departmentService, subject);
            
            var dialog = new ViridiscaUi.Views.Education.SubjectEditDialog
            {
                DataContext = editorViewModel
            };
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<Subject?>(GetOwnerWindow());
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог редактирования предмета: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Показывает диалог с деталями предмета
    /// </summary>
    public async Task ShowSubjectDetailsDialogAsync(Subject subject)
    {
        try
        {
            var subjectViewModel = SubjectViewModel.FromSubject(subject);
            
            var dialog = new ViridiscaUi.Views.Education.SubjectDetailsDialog
            {
                DataContext = subjectViewModel
            };
            ConfigureDialog(dialog);
            
            await dialog.ShowDialog(GetOwnerWindow());
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось загрузить детали предмета: {ex.Message}");
        }
    }

    /// <summary>
    /// Показывает диалог с подробной информацией о группе
    /// </summary>
    public async Task<string?> ShowGroupDetailsDialogAsync(Group group)
    {
        try
        {
            var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
            var editorViewModel = new GroupEditorViewModel(teacherService, group);
            
            var dialog = new ViridiscaUi.Views.Education.GroupDetailsDialog(editorViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<string?>(GetOwnerWindow());
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог деталей группы: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Показывает диалог с подробной информацией о курсе
    /// </summary>
    public async Task<string?> ShowCourseDetailsDialogAsync(Course course)
    {
        try
        {
            var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
            var editorViewModel = new CourseEditorViewModel(
                _serviceProvider.GetRequiredService<ICourseService>(), 
                teacherService, 
                course);
            
            var dialog = new ViridiscaUi.Views.Education.CourseDetailsDialog(editorViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<string?>(GetOwnerWindow());
            
            return result;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Ошибка", $"Не удалось открыть диалог деталей курса: {ex.Message}");
            return null;
        }
    }

    private string GetEditorTitle(ViewModelBase editorViewModel)
    {
        return editorViewModel switch
        {
            GroupEditorViewModel => "Редактирование группы",
            CourseEditorViewModel => "Редактирование курса",
            TeacherEditorViewModel => "Редактирование преподавателя",
            AssignmentEditorViewModel => "Редактирование задания",
            GradeEditorViewModel => "Редактирование оценки",
            _ => "Редактирование"
        };
    }

    private async Task ShowMessageBoxAsync(string title, string message, string type)
    {
        var mainWindow = GetOwnerWindow();
        if (mainWindow == null) return;

        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var stackPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20
        };

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var okButton = new Button
        {
            Content = "OK",
            MinWidth = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            IsDefault = true
        };

        okButton.Click += (s, e) => dialog.Close();

        stackPanel.Children.Add(messageText);
        stackPanel.Children.Add(okButton);
        
        dialog.Content = stackPanel;

        await dialog.ShowDialog(mainWindow);
    }

    private static List<FilePickerFileType> CreateFileTypeFilters(string[] fileTypes)
    {
        var filters = new List<FilePickerFileType>();

        foreach (var fileType in fileTypes)
        {
            var filter = fileType.ToLowerInvariant() switch
            {
                "xlsx" or "excel" => new FilePickerFileType("Excel файлы")
                {
                    Patterns = new[] { "*.xlsx", "*.xls" }
                },
                "csv" => new FilePickerFileType("CSV файлы")
                {
                    Patterns = new[] { "*.csv" }
                },
                "pdf" => new FilePickerFileType("PDF файлы")
                {
                    Patterns = new[] { "*.pdf" }
                },
                "json" => new FilePickerFileType("JSON файлы")
                {
                    Patterns = new[] { "*.json" }
                },
                "xml" => new FilePickerFileType("XML файлы")
                {
                    Patterns = new[] { "*.xml" }
                },
                _ => new FilePickerFileType($"{fileType.ToUpperInvariant()} файлы")
                {
                    Patterns = new[] { $"*.{fileType.ToLowerInvariant()}" }
                }
            };

            filters.Add(filter);
        }

        // Добавляем "Все файлы" в конце
        filters.Add(new FilePickerFileType("Все файлы")
        {
            Patterns = new[] { "*.*" }
        });

        return filters;
    }

    /// <summary>
    /// Показывает диалог редактора студента (упрощенная версия)
    /// </summary>
    public async Task<Student?> ShowStudentEditorDialogAsync(Student? student = null)
    {
        // Используем тот же диалог что и ShowStudentEditDialogAsync
        return await ShowStudentEditDialogAsync(student);
    }
}

