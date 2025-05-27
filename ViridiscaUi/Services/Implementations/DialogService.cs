using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ViridiscaUi.Services.Interfaces;
using Avalonia.Media;
using Avalonia.Layout;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Views.Education.Students;
using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Domain.Models.System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls.Templates;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с диалоговыми окнами
/// </summary>
public class DialogService(IServiceProvider serviceProvider) : IDialogService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <summary>
    /// Получает активное окно для использования в качестве владельца диалогов
    /// </summary>
    private Window GetOwnerWindow()
    {
        // Пытаемся найти главное окно через ApplicationLifetime
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow ?? throw new InvalidOperationException("MainWindow is not available");
        }
        
        throw new InvalidOperationException("Application lifetime is not available");
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
            ownerWindow.Closing -= OnOwnerClosing;
        }
        
        ownerWindow.Closing += OnOwnerClosing;
        
        // Также подписываемся на закрытие диалога, чтобы очистить обработчики
        dialog.Closed += (s, e) =>
        {
            ownerWindow.Closing -= OnOwnerClosing;
        };
    }

    /// <summary>
    /// Показывает информационное сообщение
    /// </summary>
    public async Task ShowInfoAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message },
                    new Button { Content = "OK" }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        ConfigureDialog(dialog);

        var okButton = ((dialog.Content as StackPanel)?.Children[1]) as Button;
        if (okButton != null)
        {
            void OnClick(object? sender, RoutedEventArgs args)
            {
                dialog.Close();
                okButton.Click -= OnClick;
            }
            okButton.Click += OnClick;
        }

        await dialog.ShowDialog(GetOwnerWindow());
    }

    /// <summary>
    /// Показывает сообщение об ошибке
    /// </summary>
    public async Task ShowErrorAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message, Foreground = new SolidColorBrush(Colors.Red) },
                    new Button { Content = "OK" }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        ConfigureDialog(dialog);

        var okButton = ((dialog.Content as StackPanel)?.Children[1]) as Button;
        if (okButton != null)
        {
            void OnClick(object? sender, RoutedEventArgs args)
            {
                dialog.Close();
                okButton.Click -= OnClick;
            }
            okButton.Click += OnClick;
        }

        await dialog.ShowDialog(GetOwnerWindow());
    }

    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    public async Task ShowWarningAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message, Foreground = new SolidColorBrush(Colors.Orange) },
                    new Button { Content = "OK" }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        ConfigureDialog(dialog);

        var okButton = ((dialog.Content as StackPanel)?.Children[1]) as Button;
        if (okButton != null)
        {
            void OnClick(object? sender, RoutedEventArgs args)
            {
                dialog.Close();
                okButton.Click -= OnClick;
            }
            okButton.Click += OnClick;
        }

        await dialog.ShowDialog(GetOwnerWindow());
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
            
            studentEditor.SaveCommand.Subscribe(student =>
            {
                tcs.SetResult(student);
                window.Close();
            });

            studentEditor.CancelCommand.Subscribe(_ =>
            {
                tcs.SetResult(null);
                window.Close();
            });

            await window.ShowDialog(GetOwnerWindow());
            return (TResult?)(object?)await tcs.Task;
        }

        throw new ArgumentException($"Unsupported view model type: {viewModel.GetType()}");
    }

    public async Task<Student?> ShowStudentEditorDialogAsync(Student? student = null)
    {
        var editorViewModel = new StudentEditorViewModel(_serviceProvider.GetRequiredService<IGroupService>(), student);
        return await ShowDialogAsync<Student>(editorViewModel);
    }

    // Диалоги для групп
    public async Task<Group?> ShowGroupEditDialogAsync(Group group)
    {
        var editorViewModel = new GroupEditorViewModel(_serviceProvider.GetRequiredService<ITeacherService>(), group);
        var result = await ShowEditorDialogAsync<Group>(editorViewModel);
        return result;
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
    public async Task<Course?> ShowCourseEditDialogAsync(Course course)
    {
        var editorViewModel = new CourseEditorViewModel(_serviceProvider.GetRequiredService<ITeacherService>(), course);
        var result = await ShowEditorDialogAsync<Course>(editorViewModel);
        return result;
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
                            new Button { Content = confirmText },
                            new Button { Content = cancelText }
                        }
                    }
                }
            },
            SizeToContent = SizeToContent.WidthAndHeight
        };

        ConfigureDialog(dialog);

        var buttonPanel = ((dialog.Content as StackPanel)?.Children[1]) as StackPanel;
        var confirmButton = buttonPanel?.Children[0] as Button;
        var cancelButton = buttonPanel?.Children[1] as Button;

        if (confirmButton != null)
        {
            void OnConfirmClick(object? sender, RoutedEventArgs args)
            {
                tcs.SetResult(true);
                dialog.Close();
                confirmButton.Click -= OnConfirmClick;
            }
            confirmButton.Click += OnConfirmClick;
        }

        if (cancelButton != null)
        {
            void OnCancelClick(object? sender, RoutedEventArgs args)
            {
                tcs.SetResult(false);
                dialog.Close();
                cancelButton.Click -= OnCancelClick;
            }
            cancelButton.Click += OnCancelClick;
        }

        await dialog.ShowDialog(GetOwnerWindow());
        return await tcs.Task;
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
        var dialog = new OpenFileDialog
        {
            Title = title,
            AllowMultiple = false
        };

        if (filters?.Length > 0)
        {
            var fileTypeFilters = new List<FileDialogFilter>();
            foreach (var filter in filters)
            {
                var parts = filter.Split('|');
                if (parts.Length == 2)
                {
                    fileTypeFilters.Add(new FileDialogFilter
                    {
                        Name = parts[0],
                        Extensions = parts[1].Split(',').Select(ext => ext.Trim().TrimStart('*', '.')).ToList()
                    });
                }
                else
                {
                    // Простой формат типа "*.xlsx"
                    var extension = filter.TrimStart('*', '.');
                    fileTypeFilters.Add(new FileDialogFilter
                    {
                        Name = $"{extension.ToUpper()} files",
                        Extensions = new List<string> { extension }
                    });
                }
            }
            dialog.Filters = fileTypeFilters;
        }

        var result = await dialog.ShowAsync(GetOwnerWindow());
        return result?.FirstOrDefault();
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
    public async Task<Teacher?> ShowTeacherEditDialogAsync(Teacher teacher)
    {
        var editorViewModel = new TeacherEditorViewModel(teacher);
        var result = await ShowEditorDialogAsync<Teacher>(editorViewModel);
        return result;
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
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true
        };

        ConfigureDialog(window);

        var tcs = new TaskCompletionSource<T?>();

        // Создаем универсальный контент для редактора
        var content = CreateEditorContent(editorViewModel, tcs, window);
        window.Content = content;

        await window.ShowDialog(GetOwnerWindow());
        return await tcs.Task;
    }

    /// <summary>
    /// Получает заголовок для диалога редактирования
    /// </summary>
    private static string GetEditorTitle(ViewModelBase viewModel)
    {
        return viewModel switch
        {
            GroupEditorViewModel group => group.Title,
            CourseEditorViewModel course => course.Title,
            TeacherEditorViewModel teacher => teacher.Title,
            AssignmentEditorViewModel assignment => assignment.WindowTitle,
            GradeEditorViewModel grade => grade.Title,
            _ => "Редактирование"
        };
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
        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = "Студент:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("Students"),
                        [!ComboBox.SelectedItemProperty] = new Binding("SelectedStudent"),
                        DisplayMemberBinding = new Binding("FullName")
                    },
                    
                    new TextBlock { Text = "Задание:", FontWeight = FontWeight.SemiBold },
                    new ComboBox 
                    { 
                        [!ItemsControl.ItemsSourceProperty] = new Binding("Assignments"),
                        [!ComboBox.SelectedItemProperty] = new Binding("SelectedAssignment"),
                        DisplayMemberBinding = new Binding("Title")
                    },
                    
                    new TextBlock { Text = "Оценка:", FontWeight = FontWeight.SemiBold },
                    new NumericUpDown { [!NumericUpDown.ValueProperty] = new Binding("Value"), Minimum = 0 },
                    
                    new TextBlock { Text = "Максимальный балл:", FontWeight = FontWeight.SemiBold },
                    new NumericUpDown { [!NumericUpDown.ValueProperty] = new Binding("MaxValue"), Minimum = 1 },
                    
                    new TextBlock { Text = "Комментарий:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Comment"), AcceptsReturn = true, Height = 60 },
                    
                    new TextBlock { Text = "Обратная связь:", FontWeight = FontWeight.SemiBold },
                    new TextBox { [!TextBox.TextProperty] = new Binding("Feedback"), AcceptsReturn = true, Height = 80 }
                }
            }
        };
    }
}
