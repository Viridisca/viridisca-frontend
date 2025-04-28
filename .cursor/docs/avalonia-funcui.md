# Использование Avalonia.FuncUI в проекте ViridiscaUi

Avalonia.FuncUI - это функциональная обертка над Avalonia UI, которая позволяет создавать пользовательский интерфейс в более декларативном стиле. В данном документе описаны принципы и правила использования Avalonia.FuncUI в проекте ViridiscaUi.

## Основные концепции Avalonia.FuncUI

### 1. Компонентная модель

Avalonia.FuncUI использует компонентную модель, где каждый UI-элемент представлен как функция, возвращающая компонент:

```csharp
// Пример компонента в C#
public static IView StudentCard(Student student)
{
    return Component(ctx =>
    {
        var isExpanded = ctx.UseState(false);

        return StackPanel.Create(
            Border.Create(
                Grid.Create(
                    TextBlock.Create(
                        TextBlock.Text(student.FullName),
                        TextBlock.FontWeight(FontWeight.Bold)
                    ),
                    Button.Create(
                        Button.Content(isExpanded.Current ? "Скрыть" : "Показать"),
                        Button.OnClick(_ => isExpanded.Set(!isExpanded.Current))
                    )
                )
            ),
            StackPanel.Create(
                StackPanel.IsVisible(isExpanded.Current),
                TextBlock.Create(TextBlock.Text($"Email: {student.Email}")),
                TextBlock.Create(TextBlock.Text($"Группа: {student.Group}")),
                TextBlock.Create(TextBlock.Text($"Статус: {student.Status}"))
            )
        );
    });
}
```

### 2. Управление состоянием

В Avalonia.FuncUI используется концепция хуков для управления состоянием:

```csharp
// Использование состояния
public static IView Counter()
{
    return Component(ctx =>
    {
        var count = ctx.UseState(0);

        return StackPanel.Create(
            Button.Create(
                Button.Content("-"),
                Button.OnClick(_ => count.Set(count.Current - 1))
            ),
            TextBlock.Create(
                TextBlock.Text(count.Current.ToString()),
                TextBlock.HorizontalAlignment(HorizontalAlignment.Center)
            ),
            Button.Create(
                Button.Content("+"),
                Button.OnClick(_ => count.Set(count.Current + 1))
            )
        );
    });
}
```

### 3. Создание элементов управления

Все элементы управления в Avalonia.FuncUI создаются с помощью статических методов:

```csharp
// Создание элемента Button
Button.Create(
    Button.Content("Нажми меня"),
    Button.Background("Blue"),
    Button.Foreground("White"),
    Button.OnClick(e => HandleClick(e))
)

// Создание элемента TextBox
TextBox.Create(
    TextBox.Text(text),
    TextBox.Watermark("Введите значение"),
    TextBox.OnTextChanged(e => HandleTextChanged(e))
)
```

## Правила использования Avalonia.FuncUI в ViridiscaUi

### 1. Структура проекта

Для интеграции Avalonia.FuncUI в существующую структуру проекта, следуйте этим правилам:

```
ViridiscaUi/
├── Components/               # Переиспользуемые FuncUI компоненты
│   ├── Common/               # Общие компоненты (кнопки, поля ввода и т.д.)
│   ├── Students/             # Компоненты для работы со студентами
│   └── Courses/              # Компоненты для работы с курсами
├── Pages/                    # Страницы приложения на FuncUI
├── ViewModels/               # ViewModels для страниц
├── Services/                 # Сервисы
└── Extensions/               # Расширения для FuncUI
```

### 2. Определение идентификаторов

ViridiscaUi использует `Guid` для идентификаторов вместо `int`. Всегда используйте `Guid` в моделях и методах API:

```csharp
public class Student
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // другие свойства
}
```

### 3. Именование компонентов

Следуйте этим правилам именования для компонентов:

1. Имя компонента должно отражать его назначение (`StudentCard`, `CourseList`, `GradeEditor`)
2. Методы, создающие компоненты, должны быть статическими
3. Методы, создающие компоненты, должны возвращать тип `IView`

```csharp
public static class StudentComponents
{
    public static IView StudentList(IEnumerable<Student> students, Action<Guid> onSelect)
    {
        return Component(ctx => {
            // реализация компонента
        });
    }

    public static IView StudentEditor(Student student, Action<Student> onSave)
    {
        return Component(ctx => {
            // реализация компонента
        });
    }
}
```

### 4. Обработка событий и передача параметров

Для передачи событий и параметров между компонентами используйте делегаты:

```csharp
// Определение компонента с параметрами
public static IView StudentCard(Student student, Action<Guid> onSelect, Action<Guid> onDelete)
{
    return Component(ctx => {
        return StackPanel.Create(
            TextBlock.Create(TextBlock.Text(student.FullName)),
            StackPanel.Create(
                StackPanel.Orientation(Orientation.Horizontal),
                Button.Create(
                    Button.Content("Выбрать"),
                    Button.OnClick(_ => onSelect(student.Uid))
                ),
                Button.Create(
                    Button.Content("Удалить"),
                    Button.OnClick(_ => onDelete(student.Uid))
                )
            )
        );
    });
}

// Использование компонента
StudentCard(
    selectedStudent,
    uid => SelectStudent(uid),
    uid => DeleteStudent(uid)
)
```

### 5. Работа с коллекциями

Для отображения списков используйте `ItemsControl` или `ListBox`:

```csharp
public static IView StudentList(IEnumerable<Student> students, Action<Guid> onSelect)
{
    return Component(ctx => {
        return ListBox.Create(
            ListBox.DataItems(students),
            ListBox.ItemTemplate(student => 
                StudentCard(
                    (Student)student, 
                    onSelect,
                    _ => { } // пустой обработчик удаления
                )
            ),
            ListBox.OnSelectionChanged(e => {
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is Student selected)
                {
                    onSelect(selected.Uid);
                }
            })
        );
    });
}
```

### 6. Компоновка элементов управления

Используйте контейнеры компоновки для организации элементов:

```csharp
// Использование Grid
Grid.Create(
    Grid.ColumnDefinitions("Auto, *, Auto"),
    Grid.RowDefinitions("Auto, *"),
    TextBlock.Create(
        Grid.Column(0),
        Grid.Row(0),
        TextBlock.Text("Заголовок")
    ),
    Button.Create(
        Grid.Column(2),
        Grid.Row(0),
        Button.Content("Действие")
    ),
    ListBox.Create(
        Grid.Column(0),
        Grid.ColumnSpan(3),
        Grid.Row(1),
        // содержимое ListBox
    )
)

// Использование DockPanel
DockPanel.Create(
    TextBlock.Create(
        DockPanel.Dock(Dock.Top),
        TextBlock.Text("Верхняя панель")
    ),
    Button.Create(
        DockPanel.Dock(Dock.Bottom),
        Button.Content("Нижняя кнопка")
    ),
    StackPanel.Create(
        // содержимое центральной области
    )
)
```

### 7. Стилизация компонентов

Определяйте стили через отдельные методы для повторного использования:

```csharp
// Определение стилей
public static class Styles
{
    public static IView PrimaryButton(string content, Action<RoutedEventArgs> onClick)
    {
        return Button.Create(
            Button.Content(content),
            Button.Background("#0078D7"),
            Button.Foreground("White"),
            Button.Padding(10, 5),
            Button.HorizontalContentAlignment(HorizontalAlignment.Center),
            Button.VerticalContentAlignment(VerticalAlignment.Center),
            Button.OnClick(onClick)
        );
    }

    public static IView HeaderText(string text)
    {
        return TextBlock.Create(
            TextBlock.Text(text),
            TextBlock.FontSize(24),
            TextBlock.FontWeight(FontWeight.Bold),
            TextBlock.Margin(0, 0, 0, 10)
        );
    }
}

// Использование стилей
StackPanel.Create(
    Styles.HeaderText("Список студентов"),
    // другие элементы
    Styles.PrimaryButton("Добавить", _ => AddStudent())
)
```

### 8. Работа с формами

Используйте состояния для работы с формами:

```csharp
public static IView StudentForm(Student? student, Action<Student> onSave, Action onCancel)
{
    return Component(ctx => {
        var firstName = ctx.UseState(student?.FirstName ?? "");
        var lastName = ctx.UseState(student?.LastName ?? "");
        var email = ctx.UseState(student?.Email ?? "");
        
        // Валидация формы
        var isValid = firstName.Current.Length > 0 && lastName.Current.Length > 0;
        
        return StackPanel.Create(
            TextBox.Create(
                TextBox.Text(firstName.Current),
                TextBox.Watermark("Имя"),
                TextBox.OnTextChanged(e => firstName.Set(e.Text))
            ),
            TextBox.Create(
                TextBox.Text(lastName.Current),
                TextBox.Watermark("Фамилия"),
                TextBox.OnTextChanged(e => lastName.Set(e.Text))
            ),
            TextBox.Create(
                TextBox.Text(email.Current),
                TextBox.Watermark("Email"),
                TextBox.OnTextChanged(e => email.Set(e.Text))
            ),
            StackPanel.Create(
                StackPanel.Orientation(Orientation.Horizontal),
                StackPanel.HorizontalAlignment(HorizontalAlignment.Right),
                StackPanel.Margin(0, 10, 0, 0),
                Button.Create(
                    Button.Content("Отмена"),
                    Button.OnClick(_ => onCancel())
                ),
                Button.Create(
                    Button.Content("Сохранить"),
                    Button.IsEnabled(isValid),
                    Button.OnClick(_ => {
                        var result = student ?? new Student { Uid = Guid.NewGuid() };
                        result.FirstName = firstName.Current;
                        result.LastName = lastName.Current;
                        result.Email = email.Current;
                        onSave(result);
                    })
                )
            )
        );
    });
}
```

### 9. Интеграция с ReactiveUI

Хотя Avalonia.FuncUI имеет собственную систему управления состоянием, ее можно интегрировать с ReactiveUI:

```csharp
public static IView ReactiveView<TViewModel>(TViewModel viewModel) where TViewModel : ReactiveObject
{
    return Component(ctx => {
        // Подписка на изменения свойств
        ctx.UseEffect(() => {
            var disposable = viewModel.WhenAnyValue(vm => vm.SomeProperty)
                .Subscribe(value => {
                    // обработка изменений
                });
                
            return () => disposable.Dispose();
        }, Array.Empty<object>());
        
        return StackPanel.Create(
            // UI компоненты
        );
    });
}
```

### 10. Навигация между страницами

Реализуйте навигацию с помощью состояния контейнера:

```csharp
public enum Page
{
    Dashboard,
    Students,
    Courses,
    Settings
}

public static IView MainView()
{
    return Component(ctx => {
        var currentPage = ctx.UseState(Page.Dashboard);
        
        IView pageContent = currentPage.Current switch
        {
            Page.Dashboard => DashboardPage(),
            Page.Students => StudentsPage(),
            Page.Courses => CoursesPage(),
            Page.Settings => SettingsPage(),
            _ => DashboardPage()
        };
        
        return DockPanel.Create(
            // Боковое меню
            StackPanel.Create(
                DockPanel.Dock(Dock.Left),
                StackPanel.Width(200),
                StackPanel.Background("#2D3748"),
                Button.Create(
                    Button.Content("Панель управления"),
                    Button.OnClick(_ => currentPage.Set(Page.Dashboard))
                ),
                Button.Create(
                    Button.Content("Студенты"),
                    Button.OnClick(_ => currentPage.Set(Page.Students))
                ),
                Button.Create(
                    Button.Content("Курсы"),
                    Button.OnClick(_ => currentPage.Set(Page.Courses))
                ),
                Button.Create(
                    Button.Content("Настройки"),
                    Button.OnClick(_ => currentPage.Set(Page.Settings))
                )
            ),
            // Основной контент
            pageContent
        );
    });
}
```

## Интеграция Avalonia.FuncUI с существующим проектом

### 1. Добавление пакетов

Добавьте следующие пакеты в проект ViridiscaUi:

```xml
<PackageReference Include="Avalonia.FuncUI" Version="1.0.0" />
<PackageReference Include="Avalonia.FuncUI.Elmish" Version="1.0.0" />
```

### 2. Создание обертки для существующих ViewModels

Для интеграции с существующими ViewModels используйте адаптеры:

```csharp
public static class ViewModelAdapter
{
    public static IView CreateView<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        // Адаптер для конкретного типа ViewModel
        if (viewModel is StudentsViewModel studentsViewModel)
        {
            return StudentsView(studentsViewModel);
        }
        
        if (viewModel is CourseViewModel courseViewModel)
        {
            return CourseView(courseViewModel);
        }
        
        // Если ViewModel не поддерживается, возвращаем заглушку
        return TextBlock.Create(
            TextBlock.Text($"ViewModel типа {typeof(TViewModel).Name} не поддерживается")
        );
    }
    
    private static IView StudentsView(StudentsViewModel viewModel)
    {
        return Component(ctx => {
            // Подписка на изменения коллекции
            ctx.UseEffect(() => {
                var disposable = viewModel.Students.Connect()
                    .Subscribe(_ => ctx.ForceUpdate());
                    
                return () => disposable.Dispose();
            }, Array.Empty<object>());
            
            return StackPanel.Create(
                // UI на основе данных из viewModel
            );
        });
    }
}
```

### 3. Постепенная миграция компонентов

При миграции существующего приложения на Avalonia.FuncUI, следуйте этим шагам:

1. Начните с малых, изолированных компонентов
2. Создайте FuncUI обертки для существующих XAML-контролов
3. Постепенно заменяйте XAML-контролы на FuncUI компоненты
4. Сохраняйте существующую структуру проекта и именование

```csharp
// Пример обертки для существующего XAML-контрола
public static IView StudentCardAdapter(Student student)
{
    return Component(ctx => {
        return ContentControl.Create(
            ContentControl.Content(student),
            ContentControl.ContentTemplate(new DataTemplate(typeof(StudentCardControl)))
        );
    });
}
```

## Примеры типичных пользовательских интерфейсов

### Страница со списком и деталями

```csharp
public static IView StudentsPage()
{
    return Component(ctx => {
        var students = ctx.UseState<List<Student>>(new List<Student>());
        var selectedStudent = ctx.UseState<Student?>(null);
        
        // Загрузка данных при монтировании компонента
        ctx.UseEffect(() => {
            // Асинхронная загрузка данных
            Task.Run(async () => {
                var loadedStudents = await StudentService.GetAllStudentsAsync();
                students.Set(loadedStudents.ToList());
            });
        }, Array.Empty<object>()); // Пустой массив означает, что эффект запустится только при монтировании
        
        return Grid.Create(
            Grid.ColumnDefinitions("350, *"),
            Grid.RowDefinitions("Auto, *"),
            
            // Заголовок списка
            TextBlock.Create(
                Grid.Column(0),
                Grid.Row(0),
                TextBlock.Text("Список студентов"),
                TextBlock.FontSize(20),
                TextBlock.FontWeight(FontWeight.Bold),
                TextBlock.Margin(10)
            ),
            
            // Список студентов
            ListBox.Create(
                Grid.Column(0),
                Grid.Row(1),
                ListBox.Items(students.Current.Select(s => 
                    TextBlock.Create(TextBlock.Text($"{s.LastName} {s.FirstName}"))
                )),
                ListBox.SelectedIndex(students.Current.FindIndex(s => 
                    s.Uid == selectedStudent.Current?.Uid
                )),
                ListBox.OnSelectionChanged(e => {
                    if (e.AddedItems.Count > 0 && e.AddedItems[0] is Student selected)
                    {
                        selectedStudent.Set(selected);
                    }
                })
            ),
            
            // Панель деталей
            Border.Create(
                Grid.Column(1),
                Grid.Row(0),
                Grid.RowSpan(2),
                Border.BorderBrush("#CCCCCC"),
                Border.BorderThickness(1, 0, 0, 0),
                Border.Padding(20),
                
                selectedStudent.Current != null
                    ? StudentDetails(selectedStudent.Current)
                    : TextBlock.Create(
                        TextBlock.Text("Выберите студента из списка"),
                        TextBlock.HorizontalAlignment(HorizontalAlignment.Center),
                        TextBlock.VerticalAlignment(VerticalAlignment.Center),
                        TextBlock.Opacity(0.5)
                    )
            )
        );
    });
}

public static IView StudentDetails(Student student)
{
    return StackPanel.Create(
        TextBlock.Create(
            TextBlock.Text($"{student.LastName} {student.FirstName}"),
            TextBlock.FontSize(24),
            TextBlock.FontWeight(FontWeight.Bold),
            TextBlock.Margin(0, 0, 0, 20)
        ),
        Grid.Create(
            Grid.ColumnDefinitions("Auto, *"),
            Grid.RowDefinitions("Auto, Auto, Auto, Auto"),
            Grid.RowSpacing(10),
            Grid.ColumnSpacing(10),
            
            TextBlock.Create(
                Grid.Column(0),
                Grid.Row(0),
                TextBlock.Text("Email:"),
                TextBlock.FontWeight(FontWeight.Bold)
            ),
            TextBlock.Create(
                Grid.Column(1),
                Grid.Row(0),
                TextBlock.Text(student.Email)
            ),
            
            TextBlock.Create(
                Grid.Column(0),
                Grid.Row(1),
                TextBlock.Text("Группа:"),
                TextBlock.FontWeight(FontWeight.Bold)
            ),
            TextBlock.Create(
                Grid.Column(1),
                Grid.Row(1),
                TextBlock.Text(student.Group)
            ),
            
            TextBlock.Create(
                Grid.Column(0),
                Grid.Row(2),
                TextBlock.Text("Статус:"),
                TextBlock.FontWeight(FontWeight.Bold)
            ),
            TextBlock.Create(
                Grid.Column(1),
                Grid.Row(2),
                TextBlock.Text(student.Status.ToString())
            )
        )
    );
}
```

### Форма редактирования

```csharp
public static IView StudentEditorPage(Student? student, Action<Student> onSave, Action onCancel)
{
    return Component(ctx => {
        var isNewStudent = student == null;
        var title = isNewStudent ? "Добавление студента" : "Редактирование студента";
        
        var firstName = ctx.UseState(student?.FirstName ?? "");
        var lastName = ctx.UseState(student?.LastName ?? "");
        var middleName = ctx.UseState(student?.MiddleName ?? "");
        var email = ctx.UseState(student?.Email ?? "");
        var phone = ctx.UseState(student?.Phone ?? "");
        var group = ctx.UseState(student?.Group ?? "");
        
        // Валидация
        var isEmailValid = ctx.UseComputedState(() => 
            string.IsNullOrEmpty(email.Current) || email.Current.Contains("@")
        );
        
        var isFormValid = ctx.UseComputedState(() => 
            !string.IsNullOrEmpty(firstName.Current) &&
            !string.IsNullOrEmpty(lastName.Current) &&
            isEmailValid.Current
        );
        
        var handleSave = () => {
            var result = student ?? new Student { Uid = Guid.NewGuid() };
            result.FirstName = firstName.Current;
            result.LastName = lastName.Current;
            result.MiddleName = middleName.Current;
            result.Email = email.Current;
            result.Phone = phone.Current;
            result.Group = group.Current;
            
            onSave(result);
        };
        
        return DockPanel.Create(
            // Заголовок
            TextBlock.Create(
                DockPanel.Dock(Dock.Top),
                TextBlock.Text(title),
                TextBlock.FontSize(24),
                TextBlock.FontWeight(FontWeight.Bold),
                TextBlock.Margin(0, 0, 0, 20)
            ),
            
            // Кнопки действий
            StackPanel.Create(
                DockPanel.Dock(Dock.Bottom),
                StackPanel.Orientation(Orientation.Horizontal),
                StackPanel.HorizontalAlignment(HorizontalAlignment.Right),
                StackPanel.Margin(0, 20, 0, 0),
                StackPanel.Spacing(10),
                
                Button.Create(
                    Button.Content("Отмена"),
                    Button.OnClick(_ => onCancel())
                ),
                Button.Create(
                    Button.Content(isNewStudent ? "Создать" : "Сохранить"),
                    Button.IsEnabled(isFormValid.Current),
                    Button.Background("#0078D7"),
                    Button.Foreground("White"),
                    Button.OnClick(_ => handleSave())
                )
            ),
            
            // Форма
            ScrollViewer.Create(
                Grid.Create(
                    Grid.ColumnDefinitions("Auto, *"),
                    Grid.RowDefinitions("Auto, Auto, Auto, Auto, Auto, Auto"),
                    Grid.RowSpacing(15),
                    Grid.ColumnSpacing(10),
                    
                    // Фамилия
                    TextBlock.Create(
                        Grid.Column(0),
                        Grid.Row(0),
                        TextBlock.Text("Фамилия:"),
                        TextBlock.VerticalAlignment(VerticalAlignment.Center)
                    ),
                    TextBox.Create(
                        Grid.Column(1),
                        Grid.Row(0),
                        TextBox.Text(lastName.Current),
                        TextBox.OnTextChanged(e => lastName.Set(e.Text))
                    ),
                    
                    // Имя
                    TextBlock.Create(
                        Grid.Column(0),
                        Grid.Row(1),
                        TextBlock.Text("Имя:"),
                        TextBlock.VerticalAlignment(VerticalAlignment.Center)
                    ),
                    TextBox.Create(
                        Grid.Column(1),
                        Grid.Row(1),
                        TextBox.Text(firstName.Current),
                        TextBox.OnTextChanged(e => firstName.Set(e.Text))
                    ),
                    
                    // Отчество
                    TextBlock.Create(
                        Grid.Column(0),
                        Grid.Row(2),
                        TextBlock.Text("Отчество:"),
                        TextBlock.VerticalAlignment(VerticalAlignment.Center)
                    ),
                    TextBox.Create(
                        Grid.Column(1),
                        Grid.Row(2),
                        TextBox.Text(middleName.Current),
                        TextBox.OnTextChanged(e => middleName.Set(e.Text))
                    ),
                    
                    // Email
                    TextBlock.Create(
                        Grid.Column(0),
                        Grid.Row(3),
                        TextBlock.Text("Email:"),
                        TextBlock.VerticalAlignment(VerticalAlignment.Center)
                    ),
                    StackPanel.Create(
                        Grid.Column(1),
                        Grid.Row(3),
                        StackPanel.Spacing(5),
                        
                        TextBox.Create(
                            TextBox.Text(email.Current),
                            TextBox.OnTextChanged(e => email.Set(e.Text))
                        ),
                        
                        // Отображаем ошибку валидации
                        TextBlock.Create(
                            TextBlock.IsVisible(!isEmailValid.Current),
                            TextBlock.Text("Некорректный email адрес"),
                            TextBlock.Foreground("Red"),
                            TextBlock.FontSize(12)
                        )
                    ),
                    
                    // Телефон
                    TextBlock.Create(
                        Grid.Column(0),
                        Grid.Row(4),
                        TextBlock.Text("Телефон:"),
                        TextBlock.VerticalAlignment(VerticalAlignment.Center)
                    ),
                    TextBox.Create(
                        Grid.Column(1),
                        Grid.Row(4),
                        TextBox.Text(phone.Current),
                        TextBox.OnTextChanged(e => phone.Set(e.Text))
                    ),
                    
                    // Группа
                    TextBlock.Create(
                        Grid.Column(0),
                        Grid.Row(5),
                        TextBlock.Text("Группа:"),
                        TextBlock.VerticalAlignment(VerticalAlignment.Center)
                    ),
                    TextBox.Create(
                        Grid.Column(1),
                        Grid.Row(5),
                        TextBox.Text(group.Current),
                        TextBox.OnTextChanged(e => group.Set(e.Text))
                    )
                )
            )
        );
    });
}
```

## Заключение

Avalonia.FuncUI предоставляет мощный функциональный подход к созданию UI в приложении ViridiscaUi. Следуя приведенным правилам и рекомендациям, вы сможете создавать гибкие, масштабируемые и легко поддерживаемые компоненты пользовательского интерфейса. 