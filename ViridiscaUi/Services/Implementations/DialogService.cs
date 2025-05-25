using System;
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

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Реализация сервиса для работы с диалоговыми окнами
/// </summary>
public class DialogService(Window owner, IServiceProvider serviceProvider) : IDialogService
{
    private readonly Window _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

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
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

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

        await dialog.ShowDialog(_owner);
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
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

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

        await dialog.ShowDialog(_owner);
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
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

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

        await dialog.ShowDialog(_owner);
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
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

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

        await dialog.ShowDialog(_owner);
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
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

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

        await dialog.ShowDialog(_owner);
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
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

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

        await dialog.ShowDialog(_owner);
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

            await window.ShowDialog(_owner);
            return (TResult?)(object?)await tcs.Task;
        }

        throw new ArgumentException($"Unsupported view model type: {viewModel.GetType()}");
    }

    public async Task<Student?> ShowStudentEditorDialogAsync(Student? student = null)
    {
        var viewModel = student != null 
            ? ActivatorUtilities.CreateInstance<StudentEditorViewModel>(_serviceProvider, student)
            : ActivatorUtilities.CreateInstance<StudentEditorViewModel>(_serviceProvider);

        return await ShowDialogAsync<Student>(viewModel);
    }
}
