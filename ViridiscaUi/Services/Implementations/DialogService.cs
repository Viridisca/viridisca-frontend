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
using ViridiscaUi.Views.Education.Students;
using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Domain.Models.System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

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
        // TODO: Реализовать диалог редактирования студента
        await Task.Delay(100);
        return student;
    }

    // Диалоги для групп
    public async Task<Group?> ShowGroupEditDialogAsync(Group group)
    {
        // TODO: Реализовать диалог редактирования группы
        await Task.Delay(100);
        return group;
    }
    
    public async Task<Teacher?> ShowTeacherSelectionDialogAsync(IEnumerable<Teacher> teachers)
    {
        // TODO: Реализовать диалог выбора преподавателя
        await Task.Delay(100);
        return teachers.FirstOrDefault();
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
        // TODO: Реализовать диалог редактирования курса
        await Task.Delay(100);
        return course;
    }
    
    public async Task<object?> ShowCourseEnrollmentDialogAsync(Course course, IEnumerable<Student> allStudents)
    {
        // TODO: Реализовать диалог записи на курс
        await Task.Delay(100);
        return new object();
    }
    
    public async Task<Group?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups)
    {
        // TODO: Реализовать диалог выбора группы
        await Task.Delay(100);
        return groups.FirstOrDefault();
    }
    
    // Диалоги для заданий
    public async Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment assignment)
    {
        // TODO: Реализовать диалог редактирования задания
        await Task.Delay(100);
        return assignment;
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
        // TODO: Реализовать диалог редактирования оценки
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог массового выставления оценок
    /// </summary>
    public async Task<IEnumerable<Grade>?> ShowBulkGradingDialogAsync(IEnumerable<Course> courses, IEnumerable<Assignment> assignments)
    {
        // TODO: Реализовать диалог массового выставления оценок
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования преподавателя
    /// </summary>
    public async Task<Teacher?> ShowTeacherEditDialogAsync(Teacher teacher)
    {
        // TODO: Реализовать диалог редактирования преподавателя
        await Task.Delay(1);
        return null;
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
}
