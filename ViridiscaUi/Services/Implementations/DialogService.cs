using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.Library;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.Views.Education;
using ViridiscaUi.Views.System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls.Templates;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.Windows;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Domain.Models.System.Enums;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с диалогами
/// </summary>
public class DialogService : IDialogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DialogService> _logger;

    public DialogService(IServiceProvider serviceProvider, ILogger<DialogService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        await ShowMessageBoxAsync(title, message, "Warning");
    }

    /// <summary>
    /// Показывает запрос подтверждения
    /// </summary>
    // Удален старый метод ShowConfirmationAsync(string, string) -> bool
    // Используйте новые методы ShowConfirmationAsync(...) -> DialogResult в конце файла

    /// <summary>
    /// Показывает диалог с вводом текста
    /// </summary>
    public async Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "")
    {
        var mainWindow = GetOwnerWindow();
        if (mainWindow == null) return null;

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
            Spacing = 15
        };

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };

        var textBox = new TextBox
        {
            Text = defaultValue,
            Margin = new Avalonia.Thickness(0, 0, 0, 15)
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var okButton = new Button
        {
            Content = "OK",
            MinWidth = 80,
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Content = "Отмена",
            MinWidth = 80,
            IsCancel = true
        };

        string? result = null;

        okButton.Click += (s, e) =>
        {
            result = textBox.Text;
            dialog.Close();
        };

        cancelButton.Click += (s, e) => dialog.Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        stackPanel.Children.Add(messageText);
        stackPanel.Children.Add(textBox);
        stackPanel.Children.Add(buttonPanel);

        dialog.Content = stackPanel;

        await dialog.ShowDialog(mainWindow);
        return result;
    }

    /// <summary>
    /// Показывает диалог с вводом текста (алиас для ShowInputDialogAsync)
    /// </summary>
    public async Task<string?> ShowTextInputDialogAsync(string title, string message, string defaultValue = "")
    {
        return await ShowInputDialogAsync(title, message, defaultValue);
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

    public async Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase viewModel) where TResult : class
    {
        try
        {
            var mainWindow = GetOwnerWindow();
            if (mainWindow == null) return default;

            var dialog = new Window
            {
                Title = GetEditorTitle(viewModel),
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = true
            };

            var tcs = new TaskCompletionSource<TResult?>();
            var content = CreateEditorContent<TResult>(viewModel, tcs, dialog);
            dialog.Content = content;

            ConfigureDialog(dialog);

            // Показываем диалог и ждем результат
            _ = dialog.ShowDialog(mainWindow);
            return await tcs.Task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при показе диалога редактирования");
            return default;
        }
    }

    /// <summary>
    /// Показывает диалог редактирования группы
    /// </summary>
    public async Task<Group?> ShowGroupEditDialogAsync(Group? group = null)
    {
        try
        {
            // Создаем копию группы для редактирования или новую группу
            var editableGroup = group != null ? new Group
            {
                Uid = group.Uid,
                Code = group.Code ?? $"GRP_{DateTime.Now:yyyyMMddHHmmss}",
                Name = group.Name ?? "Новая группа",
                Description = group.Description ?? "Описание группы",
                MaxStudents = group.MaxStudents > 0 ? group.MaxStudents : 25,
                IsActive = group.IsActive,
                CuratorUid = group.CuratorUid,
                CreatedAt = group.CreatedAt == default ? DateTime.UtcNow : group.CreatedAt,
                LastModifiedAt = DateTime.UtcNow
            } : new Group
            {
                Uid = Guid.NewGuid(),
                Code = $"GRP_{DateTime.Now:yyyyMMddHHmmss}",
                Name = "Новая группа",
                Description = "Описание новой группы",
                MaxStudents = 25,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            // Простая реализация диалога (для компиляции)
            // В реальном приложении здесь будет полноценный UI диалог
            
            // Валидация базовых полей
            if (string.IsNullOrWhiteSpace(editableGroup.Name))
            {
                editableGroup.Name = "Новая группа";
            }
            
            if (string.IsNullOrWhiteSpace(editableGroup.Code))
            {
                editableGroup.Code = $"GRP_{DateTime.Now:yyyyMMddHHmmss}";
            }

            if (editableGroup.MaxStudents <= 0)
            {
                editableGroup.MaxStudents = 25;
            }

            // Возвращаем отредактированную группу
            return editableGroup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in group edit dialog");
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
    public async Task<CourseInstance?> ShowCourseEditDialogAsync(CourseInstance courseInstance)
    {
        try
        {
            var viewModel = new CourseEditorViewModel(
                _serviceProvider.GetRequiredService<ICourseInstanceService>(),
                _serviceProvider.GetRequiredService<ITeacherService>(),
                _serviceProvider.GetRequiredService<IUnifiedNavigationService>(),
                _serviceProvider.GetRequiredService<IScreen>());

            if (courseInstance != null)
            {
                // TODO: Load course instance data into the view model
                // viewModel.LoadCourseInstance(courseInstance);
            }

            var dialog = new CourseEditDialog { DataContext = viewModel };
            var result = await dialog.ShowDialog<CourseInstance?>(GetOwnerWindow());

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing course editor dialog");
            return null;
        }
    }
    
    public async Task<object?> ShowCourseEnrollmentDialogAsync(CourseInstance courseInstance, IEnumerable<Student> allStudents)
    {
        await ShowInfoAsync("Запись на курс", "Диалог записи студентов на курс будет реализован в следующей версии");
        return null;
    }
    
    // Диалоги для заданий
    public async Task<object?> ShowSubmissionsViewDialogAsync(Assignment assignment, IEnumerable<Submission> submissions)
    {
        // TODO: Реализовать диалог просмотра сдач
        await Task.Delay(100);
        return null;
    }
    
    // Удален старый метод ShowBulkGradingDialogAsync(IEnumerable<Submission> submissions) -> IEnumerable<object>?
    // Используйте новый метод ShowBulkGradingDialogAsync(IEnumerable<Submission> submissions) в конце файла
    
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
    public async Task<object?> ShowCourseContentManagementDialogAsync(CourseInstance courseInstance)
    {
        await ShowInfoAsync("Управление содержимым", "Диалог управления содержимым курса будет реализован в следующей версии");
        return null;
    }

    /// <summary>
    /// Показывает диалог управления студентами курса
    /// </summary>
    public async Task<object?> ShowCourseStudentsManagementDialogAsync(CourseInstance courseInstance, IEnumerable<Student> allStudents)
    {
        // TODO: Реализовать диалог управления студентами курса
        await Task.Delay(1);
        return null;
    }

    /// <summary>
    /// Показывает диалог статистики курса
    /// </summary>
    public async Task<object?> ShowCourseStatisticsDialogAsync(CourseInstance courseInstance)
    {
        var stats = $"Курс: {courseInstance.Subject?.Name}\n" +
                   $"Группа: {courseInstance.Group?.Name}\n" +
                   $"Преподаватель: {courseInstance.Teacher?.Person?.FirstName} {courseInstance.Teacher?.Person?.LastName}\n" +
                   $"Период: {courseInstance.AcademicPeriod?.Name}";
        
        await ShowInfoAsync("Статистика курса", stats);
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования оценки
    /// </summary>
    // Удален старый метод ShowGradeEditDialogAsync(Grade grade, ...) -> Grade?
    // Используйте новый метод ShowGradeEditDialogAsync(...) -> DialogResult в конце файла

    /// <summary>
    /// Показывает диалог массового выставления оценок
    /// </summary>
    public async Task<IEnumerable<Grade>?> ShowBulkGradingDialogAsync(IEnumerable<CourseInstance> courseInstances, IEnumerable<Assignment> assignments)
    {
        // TODO: Реализовать диалог массового оценивания
        await Task.Delay(100);
        return new List<Grade>();
    }

    /// <summary>
    /// Показывает диалог редактирования преподавателя
    /// </summary>
    // Удален старый метод ShowTeacherEditDialogAsync(Teacher? teacher) -> Teacher?
    // Используйте новый метод ShowTeacherEditDialogAsync(Teacher? teacher) -> DialogResult в конце файла

    /// <summary>
    /// Показывает диалог с подробной информацией о преподавателе
    /// </summary>
    public async Task<string?> ShowTeacherDetailsDialogAsync(Teacher teacher)
    {
        var details = $"Преподаватель: {teacher.Person?.FirstName} {teacher.Person?.LastName}\n" +
                     $"Email: {teacher.Person?.Email}\n" +
                     $"Специализация: {teacher.Specialization}\n" +
                     $"Зарплата: {teacher.Salary:C}";
        
        await ShowInfoAsync("Детали преподавателя", details);
        return null;
    }

    /// <summary>
    /// Показывает диалог редактора с кастомным содержимым
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
    private static Grid CreateEditorContent<TResult>(ViewModelBase editorViewModel, TaskCompletionSource<TResult?> tcs, Window window) where TResult : class
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // Область контента
        var contentArea = CreateEditorContentArea(editorViewModel);
        Grid.SetRow(contentArea, 0);
        grid.Children.Add(contentArea);

        // Панель кнопок
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(10),
            Spacing = 10
        };

        var saveButton = new Button
        {
            Content = "Сохранить",
            MinWidth = 80,
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Content = "Отмена",
            MinWidth = 80,
            IsCancel = true
        };

        buttonPanel.Children.Add(saveButton);
        buttonPanel.Children.Add(cancelButton);

        Grid.SetRow(buttonPanel, 1);
        grid.Children.Add(buttonPanel);

        // Настройка команд
        SetupEditorCommands(editorViewModel, saveButton, cancelButton, tcs, window);

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
    /// Показывает диалог редактирования департамента
    /// </summary>
    public async Task<Department?> ShowDepartmentEditDialogAsync(Department department)
    {
        try
        {
            // Создаем копию департамента для редактирования
            var editableDepartment = new Department
            {
                Uid = department.Uid,
                Name = department.Name ?? "Новый отдел",
                Code = department.Code ?? $"DEPT_{DateTime.Now:yyyyMMddHHmmss}",
                Description = department.Description ?? "Описание отдела",
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt == default ? DateTime.UtcNow : department.CreatedAt,
                LastModifiedAt = DateTime.UtcNow
            };

            // Простая реализация диалога через консольный ввод (для компиляции)
            // В реальном приложении здесь будет полноценный UI диалог
            
            // Валидация базовых полей
            if (string.IsNullOrWhiteSpace(editableDepartment.Name))
            {
                editableDepartment.Name = "Новый отдел";
            }
            
            if (string.IsNullOrWhiteSpace(editableDepartment.Code))
            {
                editableDepartment.Code = $"DEPT_{DateTime.Now:yyyyMMddHHmmss}";
            }

            // Возвращаем отредактированный департамент
            return editableDepartment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in department edit dialog");
            return null;
        }
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
    public async Task<object?> ShowCourseDetailsDialogAsync(CourseInstance courseInstance)
    {
        try
        {
            var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
            var navigationService = _serviceProvider.GetRequiredService<IUnifiedNavigationService>();
            var hostScreen = _serviceProvider.GetRequiredService<IScreen>();
            var editorViewModel = new CourseEditorViewModel(
                _serviceProvider.GetRequiredService<ICourseInstanceService>(), 
                teacherService, 
                navigationService,
                hostScreen);
            
            var dialog = new ViridiscaUi.Views.Education.CourseDetailsDialog(editorViewModel);
            ConfigureDialog(dialog);
            
            var result = await dialog.ShowDialog<object?>(GetOwnerWindow());
            
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

    public async Task<object?> ShowCourseAnalyticsDialogAsync(CourseInstance courseInstance)
    {
        // TODO: Реализовать диалог аналитики экземпляра курса
        await Task.Delay(100);
        return null;
    }

    public async Task ShowScheduleConflictsDialogAsync(IEnumerable<object> conflicts)
    {
        var message = $"Обнаружено {conflicts.Count()} конфликтов в расписании";
        await ShowWarningAsync("Конфликты расписания", message);
    }

    public async Task<string?> ShowOpenFileDialogAsync(string title, string filter = "")
    {
        var filters = string.IsNullOrEmpty(filter) ? new[] { "*.*" } : filter.Split('|');
        return await ShowFileOpenDialogAsync(title, filters);
    }

    public async Task<string?> ShowSaveFileDialogAsync(string title, string filter = "", string defaultFileName = "")
    {
        var filters = string.IsNullOrEmpty(filter) ? new[] { "*.*" } : filter.Split('|');
        return await ShowFileSaveDialogAsync(title, defaultFileName, filters);
    }

    // Реализация методов интерфейса IDialogService
    public async Task<DialogResult> ShowMessageAsync(string title, string message)
    {
        await ShowInfoAsync(title, message);
        return DialogResult.OK;
    }

    public async Task<DialogResult> ShowConfirmationAsync(string title, string message)
    {
        var result = await ShowConfirmationDialogAsync(title, message);
        return result ? DialogResult.Yes : DialogResult.No;
    }

    public async Task<DialogResult> ShowConfirmationAsync(string title, string message, DialogButtons buttons)
    {
        var result = await ShowConfirmationDialogAsync(title, message);
        return result ? DialogResult.Yes : DialogResult.No;
    }

    public async Task<string?> ShowInputAsync(string title, string message, string defaultValue = "")
    {
        return await ShowInputDialogAsync(title, message, defaultValue);
    }

    public async Task ShowValidationErrorsAsync(string title, IEnumerable<string> errors)
    {
        var message = string.Join("\n", errors);
        await ShowErrorAsync(title, message);
    }

    public async Task ShowValidationErrorsAsync(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            await ShowValidationErrorsAsync("Ошибки валидации", validationResult.Errors);
        }
    }

    // Специализированные диалоги редактирования с правильными возвращаемыми типами
    
    /// <summary>
    /// Показывает диалог редактирования студента
    /// </summary>
    public async Task<Student?> ShowStudentEditDialogAsync(Student? student = null)
    {
        try
        {
            // Создаем копию студента для редактирования или нового студента
            var editableStudent = student != null ? new Student
            {
                Uid = student.Uid,
                StudentCode = student.StudentCode ?? $"ST{DateTime.Now:yy}{DateTime.Now:MMdd}{DateTime.Now:HHmm}",
                PersonUid = student.PersonUid,
                Person = student.Person != null ? new Person
                {
                    Uid = student.Person.Uid,
                    FirstName = student.Person.FirstName ?? "Имя",
                    LastName = student.Person.LastName ?? "Фамилия",
                    MiddleName = student.Person.MiddleName,
                    Email = student.Person.Email ?? "student@example.com",
                    PhoneNumber = student.Person.PhoneNumber,
                    DateOfBirth = student.Person.DateOfBirth,
                    Address = student.Person.Address,
                    CreatedAt = student.Person.CreatedAt == default ? DateTime.UtcNow : student.Person.CreatedAt,
                    LastModifiedAt = DateTime.UtcNow
                } : null,
                GroupUid = student.GroupUid,
                CurriculumUid = student.CurriculumUid,
                EnrollmentDate = student.EnrollmentDate,
                GraduationDate = student.GraduationDate,
                Status = student.Status,
                GPA = student.GPA,
                CreatedAt = student.CreatedAt == default ? DateTime.UtcNow : student.CreatedAt,
                LastModifiedAt = DateTime.UtcNow
            } : new Student
            {
                Uid = Guid.NewGuid(),
                StudentCode = $"ST{DateTime.Now:yy}{DateTime.Now:MMdd}{DateTime.Now:HHmm}",
                PersonUid = Guid.NewGuid(),
                Person = new Person
                {
                    Uid = Guid.NewGuid(),
                    FirstName = "Новый",
                    LastName = "Студент",
                    Email = "newstudent@example.com",
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                },
                EnrollmentDate = DateTime.Now,
                Status = ViridiscaUi.Domain.Models.Education.Enums.StudentStatus.Active,
                GPA = 0.0,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            // Простая реализация диалога (для компиляции)
            // В реальном приложении здесь будет полноценный UI диалог
            
            // Валидация базовых полей
            if (editableStudent.Person != null)
            {
                if (string.IsNullOrWhiteSpace(editableStudent.Person.FirstName))
                {
                    editableStudent.Person.FirstName = "Имя";
                }
                
                if (string.IsNullOrWhiteSpace(editableStudent.Person.LastName))
                {
                    editableStudent.Person.LastName = "Фамилия";
                }
                
                if (string.IsNullOrWhiteSpace(editableStudent.Person.Email))
                {
                    editableStudent.Person.Email = "student@example.com";
                }
            }
            
            if (string.IsNullOrWhiteSpace(editableStudent.StudentCode))
            {
                editableStudent.StudentCode = $"ST{DateTime.Now:yy}{DateTime.Now:MMdd}{DateTime.Now:HHmm}";
            }

            // Возвращаем отредактированного студента
            return editableStudent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in student edit dialog");
            return null;
        }
    }

    /// <summary>
    /// Показывает диалог редактирования преподавателя
    /// </summary>
    public async Task<Teacher?> ShowTeacherEditDialogAsync(Teacher? teacher = null)
    {
        await ShowInfoAsync("Преподаватели", "Диалог редактирования преподавателей будет реализован в следующей версии");
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования предмета
    /// </summary>
    public async Task<Subject?> ShowSubjectEditDialogAsync(Subject? subject = null)
    {
        await ShowInfoAsync("Предметы", "Диалог редактирования предметов будет реализован в следующей версии");
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования задания
    /// </summary>
    public async Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment? assignment = null)
    {
        await ShowInfoAsync("Задания", "Диалог редактирования заданий будет реализован в следующей версии");
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования оценки
    /// </summary>
    public async Task<Grade?> ShowGradeEditDialogAsync(Grade grade, IEnumerable<Student> students, IEnumerable<Assignment> assignments)
    {
        await ShowInfoAsync("Оценки", "Диалог редактирования оценок будет реализован в следующей версии");
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования экзамена
    /// </summary>
    public async Task<Exam?> ShowExamEditDialogAsync(Exam? exam = null)
    {
        _logger.LogWarning("ShowExamEditDialogAsync not implemented yet");
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования слота расписания
    /// </summary>
    public async Task<ScheduleSlot?> ShowScheduleSlotEditDialogAsync(ScheduleSlot? scheduleSlot = null)
    {
        _logger.LogWarning("ShowScheduleSlotEditDialogAsync not implemented yet");
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования учебного плана
    /// </summary>
    public async Task<Curriculum?> ShowCurriculumEditDialogAsync(Curriculum? curriculum = null)
    {
        _logger.LogWarning("ShowCurriculumEditDialogAsync not implemented yet");
        return null;
    }

    /// <summary>
    /// Показывает диалог редактирования библиотечного ресурса
    /// </summary>
    public async Task<LibraryResource?> ShowLibraryResourceEditDialogAsync(LibraryResource? resource = null)
    {
        await ShowInfoAsync("Библиотека", "Диалог редактирования библиотечных ресурсов будет реализован в следующей версии");
        return null;
    }

    /// <summary>
    /// Показывает диалог создания займа библиотеки
    /// </summary>
    public async Task<object?> ShowCreateLoanDialogAsync()
    {
        // TODO: Реализовать диалог создания займа
        await Task.Delay(100);
        return null;
    }

    /// <summary>
    /// Показывает диалог продления займа
    /// </summary>
    public async Task<object?> ShowExtendLoanDialogAsync(object loan)
    {
        // TODO: Реализовать диалог продления займа
        await Task.Delay(100);
        return null;
    }

    /// <summary>
    /// Показывает диалог просроченных займов
    /// </summary>
    public async Task ShowOverdueLoansDialogAsync()
    {
        // TODO: Реализовать диалог просроченных займов
        await Task.Delay(100);
    }

    /// <summary>
    /// Показывает диалог массового оценивания сдач
    /// </summary>
    public async Task<IEnumerable<object>?> ShowBulkGradingDialogAsync(IEnumerable<Submission> submissions)
    {
        // TODO: Реализовать диалог массового оценивания
        await Task.Delay(100);
        return new List<object>();
    }

    /// <summary>
    /// Показывает диалог статистики преподавателя
    /// </summary>
    public async Task<object?> ShowTeacherStatisticsDialogAsync(string title, object statistics)
    {
        // TODO: Реализовать диалог статистики
        await Task.Delay(100);
        return null;
    }

    /// <summary>
    /// Показывает диалог деталей студента
    /// </summary>
    public async Task ShowStudentDetailsDialogAsync(Student student)
    {
        var details = $"Студент: {student.Person?.FirstName} {student.Person?.LastName}\n" +
                     $"Email: {student.Person?.Email}\n" +
                     $"Группа: {student.Group?.Name ?? "Не назначена"}\n" +
                     $"Статус: {student.Status}";
        
        await ShowInfoAsync("Детали студента", details);
    }

    /// <summary>
    /// Показывает диалог управления курсами преподавателя
    /// </summary>
    public async Task<object?> ShowTeacherCoursesManagementDialogAsync(Teacher teacher, IEnumerable<CourseInstance> courseInstances)
    {
        // TODO: Реализовать диалог управления курсами
        await Task.Delay(100);
        return null;
    }

    /// <summary>
    /// Показывает диалог управления группами преподавателя
    /// </summary>
    public async Task<object?> ShowTeacherGroupsManagementDialogAsync(Teacher teacher, IEnumerable<Group> groups)
    {
        // TODO: Реализовать диалог управления группами
        await Task.Delay(100);
        return null;
    }

    /// <summary>
    /// Показывает диалог массового редактирования студентов
    /// </summary>
    public async Task<BulkEditResult?> ShowBulkEditDialogAsync(BulkEditOptions options)
    {
        // Простая реализация для демонстрации
        var message = "Выберите параметры для массового редактирования:\n\n";
        
        if (options.CanChangeGroup)
            message += "• Изменить группу\n";
        if (options.CanChangeStatus)
            message += "• Изменить статус\n";
        if (options.CanChangeAcademicYear)
            message += "• Изменить академический год\n";
            
        var result = await ShowConfirmationAsync("Массовое редактирование", message);
        
        if (result == DialogResult.Yes)
        {
            // Возвращаем простой результат для демонстрации
            return new BulkEditResult
            {
                NewStatus = Domain.Models.Education.Enums.StudentStatus.Active, // Пример
                NewAcademicYear = 1 // Пример
            };
        }
        
        return null;
    }

    /// <summary>
    /// Показывает диалог выбора экземпляров курсов
    /// </summary>
    public async Task<IEnumerable<CourseInstance>?> ShowCourseInstanceSelectionDialogAsync(IEnumerable<CourseInstance> courseInstances)
    {
        // TODO: Реализовать диалог выбора экземпляров курсов
        await Task.Delay(100); // Заглушка для async
        return courseInstances.Take(1); // Временная заглушка
    }

    /// <summary>
    /// Показывает диалог выбора групп
    /// </summary>
    public async Task<IEnumerable<Group>?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups)
    {
        // TODO: Реализовать диалог выбора групп
        await Task.Delay(100); // Заглушка для async
        return groups.Take(1); // Временная заглушка
    }

    /// <summary>
    /// Показывает диалог выбора файла
    /// </summary>
    public async Task<string?> ShowFilePickerAsync(string title, string[] fileTypes)
    {
        // TODO: Реализовать диалог выбора файла
        await Task.Delay(100); // Заглушка для async
        return null; // Временная заглушка
    }

    /// <summary>
    /// Показывает диалог редактирования слота расписания с дополнительными параметрами
    /// </summary>
    public async Task<ScheduleSlot?> ShowScheduleSlotEditDialogAsync(ScheduleSlot? scheduleSlot, IEnumerable<CourseInstance> courseInstances, IEnumerable<string> rooms)
    {
        // TODO: Реализовать диалог редактирования слота расписания с дополнительными параметрами
        await Task.Delay(100); // Заглушка для async
        return scheduleSlot; // Временная заглушка
    }
}
