# Компоненты и Модели Данных LMS Системы ViridiscaUi

## Оглавление

1.  [Введение](#введение)
2.  [Основные Модули LMS](#основные-модули-lms)
    *   [2.1 Модуль "Студенты"](#21-модуль-студенты)
        *   [Модели Данных](#модели-данных-студенты)
        *   [ViewModels (ReactiveUI)](#viewmodels-reactiveui-студенты)
        *   [Views (XAML)](#views-xaml-студенты)
    *   [2.2 Модуль "Курсы"](#22-модуль-курсы)
        *   [Модели Данных](#модели-данных-курсы)
        *   [ViewModels (ReactiveUI)](#viewmodels-reactiveui-курсы)
        *   [Views (XAML)](#views-xaml-курсы)
    *   [2.3 Модуль "Преподаватели"](#23-модуль-преподаватели)
        *   [Модели Данных](#модели-данных-преподаватели)
        *   [ViewModels (ReactiveUI)](#viewmodels-reactiveui-преподаватели)
        *   [Views (XAML)](#views-xaml-преподаватели)
    *   [2.4 Модуль "Оценки и Успеваемость"](#24-модуль-оценки-и-успеваемость)
        *   [Модели Данных](#модели-данных-оценки)
        *   [ViewModels (ReactiveUI)](#viewmodels-reactiveui-оценки)
        *   [Views (XAML)](#views-xaml-оценки)
    *   [2.5 Модуль "Аутентификация и Авторизация"](#25-модуль-аутентификация-и-авторизация)
        *   [Модели Данных](#модели-данных-аутентификация)
        *   [ViewModels (ReactiveUI)](#viewmodels-reactiveui-аутентификация)
        *   [Views (XAML)](#views-xaml-аутентификация)
3.  [Пользовательский Интерфейс](#пользовательский-интерфейс)
    *   [3.1 Главное Окно (Пример)](#31-главное-окно-пример)
    *   [3.2 Панель Мониторинга (Пример)](#32-панель-мониторинга-пример)
4.  [Общие Рекомендации по Реализации LMS](#общие-рекомендации-по-реализации-lms)
5.  [Заключение](#заключение)

## 1. Введение

Данный документ описывает основные функциональные модули, модели данных, компоненты ViewModel и примеры представлений (View) для системы управления обучением (LMS) ViridiscaUi. Цель документа - предоставить четкую структуру и рекомендации для разработки ключевых частей приложения, используя C#, Avalonia UI и ReactiveUI.

Все идентификаторы сущностей используют тип `Guid` и именуются `Uid` для обеспечения уникальности и соответствия стандартам проекта.

## 2. Основные Модули LMS

### 2.1 Модуль "Студенты"

Отвечает за управление информацией о студентах.

#### Модели Данных (Студенты)

Расположение: `ViridiscaUi.Domain/Models/Education`

```csharp
using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Auth; // Для User

namespace ViridiscaUi.Domain.Models.Education
{
    // Базовая сущность с аудитом
    public abstract class AuditableEntity
    {
        public Guid Uid { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; } // Uid пользователя
        public Guid? UpdatedBy { get; set; } // Uid пользователя
    }

    public class Student : AuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public Guid? GroupId { get; set; } // Связь с группой
        public Group? Group { get; set; }
        public StudentStatus Status { get; set; } = StudentStatus.Enrolled;
        public Guid? UserId { get; set; } // Связь с аккаунтом пользователя
        public User? User { get; set; }

        // Навигационные свойства
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }

    public class Group : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CuratorId { get; set; } // Связь с преподавателем-куратором
        public Teacher? Curator { get; set; }

        // Навигационные свойства
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }

    public enum StudentStatus
    {
        Enrolled, // Зачислен
        Active,   // Обучается
        OnLeave,  // В академическом отпуске
        Graduated,// Выпустился
        Expelled  // Отчислен
    }
}
```

#### ViewModels (ReactiveUI) (Студенты)

Расположение: `ViridiscaUi/ViewModels/Students`

```csharp
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Services; // Предполагаемые сервисы

namespace ViridiscaUi.ViewModels.Students
{
    public class StudentViewModel : ViewModelBase
    {
        public Guid Uid { get; }
        [Reactive] public string FirstName { get; set; }
        [Reactive] public string LastName { get; set; }
        [Reactive] public string? MiddleName { get; set; }
        [Reactive] public string Email { get; set; }
        [Reactive] public string? GroupName { get; set; }
        [Reactive] public StudentStatus Status { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        public StudentViewModel(Student student)
        {
            Uid = student.Uid;
            FirstName = student.FirstName;
            LastName = student.LastName;
            MiddleName = student.MiddleName;
            Email = student.Email;
            GroupName = student.Group?.Name;
            Status = student.Status;
        }
    }

    public class StudentsViewModel : RoutableViewModelBase // Базовый класс с поддержкой навигации
    {
        private readonly IStudentService _studentService;
        private readonly IDialogService _dialogService;
        private readonly SourceList<StudentViewModel> _studentsSource = new SourceList<StudentViewModel>();
        private readonly ReadOnlyObservableCollection<StudentViewModel> _students;

        public ReadOnlyObservableCollection<StudentViewModel> Students => _students;

        [Reactive] public StudentViewModel? SelectedStudent { get; set; }
        [Reactive] public string SearchTerm { get; set; } = string.Empty;
        [ObservableAsProperty] public bool IsLoading { get; }

        public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddStudentCommand { get; }
        public ReactiveCommand<StudentViewModel, Unit> EditStudentCommand { get; }
        public ReactiveCommand<StudentViewModel, Unit> DeleteStudentCommand { get; }

        public StudentsViewModel(IScreen hostScreen, IStudentService studentService, IDialogService dialogService)
            : base(hostScreen)
        {
            _studentService = studentService;
            _dialogService = dialogService;

            // Фильтрация по поисковой строке
            var filter = this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(300), RxApp.MainThreadScheduler)
                .Select(term => term?.Trim())
                .DistinctUntilChanged()
                .Select(term => new Func<StudentViewModel, bool>(vm =>
                    string.IsNullOrWhiteSpace(term) ||
                    vm.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    vm.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    vm.GroupName?.Contains(term, StringComparison.OrdinalIgnoreCase) == true
                ));

            _studentsSource.Connect()
                .Filter(filter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _students)
                .Subscribe();

            // Команда загрузки студентов
            LoadStudentsCommand = ReactiveCommand.CreateFromTask(LoadStudentsAsync);
            LoadStudentsCommand.IsExecuting.ToPropertyEx(this, x => x.IsLoading);

            // Команда добавления студента
            AddStudentCommand = ReactiveCommand.CreateFromTask(AddStudentAsync);

            // Команда редактирования студента
            var canEdit = this.WhenAnyValue(x => x.SelectedStudent).Select(s => s != null);
            EditStudentCommand = ReactiveCommand.CreateFromTask<StudentViewModel>(EditStudentAsync, canEdit);

            // Команда удаления студента
            DeleteStudentCommand = ReactiveCommand.CreateFromTask<StudentViewModel>(DeleteStudentAsync, canEdit);

            LoadStudentsCommand.Execute().Subscribe(); // Загрузка при инициализации
        }

        private async Task LoadStudentsAsync()
        {
            var studentsData = await _studentService.GetAllStudentsAsync(); // Метод вашего сервиса
            _studentsSource.Edit(innerList =>
            {
                innerList.Clear();
                innerList.AddRange(studentsData.Select(s => new StudentViewModel(s)));
            });
        }

        private async Task AddStudentAsync()
        {
            var studentEditorVm = new StudentEditorViewModel(); // ViewModel для окна/диалога редактирования
            var result = await _dialogService.ShowDialogAsync(studentEditorVm);
            if (result != null) // Если пользователь сохранил
            {
                // Логика сохранения нового студента через сервис
                await _studentService.AddStudentAsync(result);
                await LoadStudentsAsync(); // Перезагрузка списка
            }
        }

        private async Task EditStudentAsync(StudentViewModel studentVm)
        {
            if (studentVm == null) return;
            var studentData = await _studentService.GetStudentByUidAsync(studentVm.Uid);
            if (studentData == null) return; // Студент не найден

            var studentEditorVm = new StudentEditorViewModel(studentData);
            var result = await _dialogService.ShowDialogAsync(studentEditorVm);
            if (result != null)
            {
                // Логика обновления студента через сервис
                await _studentService.UpdateStudentAsync(result);
                await LoadStudentsAsync(); // Перезагрузка списка
            }
        }

        private async Task DeleteStudentAsync(StudentViewModel studentVm)
        {
            if (studentVm == null) return;
            var confirm = await _dialogService.ShowConfirmationAsync("Удаление студента", $"Вы уверены, что хотите удалить студента {studentVm.FullName}?");
            if (confirm)
            {
                // Логика удаления студента через сервис
                await _studentService.DeleteStudentAsync(studentVm.Uid);
                await LoadStudentsAsync(); // Перезагрузка списка
            }
        }
    }
}
```

#### Views (XAML) (Студенты)

Расположение: `ViridiscaUi/Views/Students`

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="using:ViridiscaUi.ViewModels.Students"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="600"
             x:Class="ViridiscaUi.Views.Students.StudentsView"
             x:DataType="vm:StudentsViewModel">

    <DockPanel LastChildFill="True">
        <!-- Панель инструментов -->
        <Border DockPanel.Dock="Top" Padding="10" BorderBrush="LightGray" BorderThickness="0 0 0 1">
            <Grid ColumnDefinitions="*,Auto">
                <TextBox Grid.Column="0" Watermark="Поиск студента..." Text="{Binding SearchTerm}" VerticalAlignment="Center" Margin="0,0,10,0"/>
                <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="10">
                    <Button Content="Обновить" Command="{Binding LoadStudentsCommand}">
                        <ToolTip.Tip>Загрузить список студентов</ToolTip.Tip>
                    </Button>
                    <Button Content="Добавить студента" Command="{Binding AddStudentCommand}" Classes="accent"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Индикатор загрузки -->
        <ProgressBar DockPanel.Dock="Top" IsIndeterminate="True" IsVisible="{Binding IsLoading}" Height="4" Margin="0"/>

        <!-- Список студентов -->
        <DataGrid ItemsSource="{Binding Students}"
                  SelectedItem="{Binding SelectedStudent}"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  CanUserSortColumns="True"
                  CanUserReorderColumns="True"
                  GridLinesVisibility="Horizontal"
                  Margin="10">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ФИО" Binding="{Binding FullName}" Width="*"/>
                <DataGridTextColumn Header="Email" Binding="{Binding Email}" Width="*"/>
                <DataGridTextColumn Header="Группа" Binding="{Binding GroupName}" Width="150"/>
                <DataGridTextColumn Header="Статус" Binding="{Binding Status}" Width="120"/>
                <!-- Колонка с кнопками действий -->
                <DataGridTemplateColumn Header="Действия" Width="180">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate DataType="vm:StudentViewModel">
                            <StackPanel Orientation="Horizontal" Spacing="5" HorizontalAlignment="Center">
                                <Button Content="Редакт." 
                                        Command="{Binding $parent[DataGrid].DataContext.EditStudentCommand}"
                                        CommandParameter="{Binding}"
                                        Padding="8,4"/>
                                <Button Content="Удалить" 
                                        Command="{Binding $parent[DataGrid].DataContext.DeleteStudentCommand}"
                                        CommandParameter="{Binding}"
                                        Padding="8,4"
                                        Classes="danger"/>
                            </StackPanel>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>
    </DockPanel>
</UserControl>
```

### 2.2 Модуль "Курсы"

Отвечает за управление учебными курсами, модулями и уроками.

#### Модели Данных (Курсы)

Расположение: `ViridiscaUi.Domain/Models/Education`

```csharp
using System;
using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Education
{
    public class Course : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public Guid? TeacherId { get; set; } // Связь с основным преподавателем
        public Teacher? Teacher { get; set; }

        // Навигационные свойства
        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

    public enum CourseStatus
    {
        Draft,     // Черновик
        Published, // Опубликован
        Active,    // Проводится
        Completed, // Завершен
        Archived   // В архиве
    }

    public class Module : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public Guid CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        // Навигационные свойства
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }

    public class Lesson : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; } // Основной материал урока
        public int Order { get; set; } 
        public LessonType Type { get; set; } = LessonType.Lecture;
        public TimeSpan? Duration { get; set; } // Опциональная длительность
        public Guid ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;

        // Навигационные свойства
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

    public enum LessonType
    {
        Lecture,  // Лекция
        Practice, // Практическое занятие
        Quiz,     // Тест
        Assignment,// Задание
        Video,    // Видеоурок
        Resource  // Дополнительные материалы
    }

    // Сущность для связи Студент-Курс (Зачисление)
    public class Enrollment : AuditableEntity
    {
        public Guid StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;
        public Guid CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
        public double? FinalGrade { get; set; } // Итоговая оценка за курс
    }

    public enum EnrollmentStatus
    {
        Active,    // Активное зачисление
        Completed, // Курс пройден
        Withdrawn  // Отчислен с курса
    }
}
```

#### ViewModels (ReactiveUI) (Курсы)

*Пример ViewModel для списка курсов (аналогично StudentsViewModel)*

#### Views (XAML) (Курсы)

*Пример View для списка курсов (аналогично StudentsView)*

### 2.3 Модуль "Преподаватели"

Отвечает за управление информацией о преподавателях.

#### Модели Данных (Преподаватели)

Расположение: `ViridiscaUi.Domain/Models/Education`

```csharp
using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Auth; // Для User

namespace ViridiscaUi.Domain.Models.Education
{
    public class Teacher : AuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Bio { get; set; } // Краткая биография
        public string? Specialization { get; set; } // Специализация
        public Guid? UserId { get; set; } // Связь с аккаунтом пользователя
        public User? User { get; set; }

        // Навигационные свойства
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>(); // Курсы, которые ведет преподаватель
        public virtual ICollection<Group> CuratedGroups { get; set; } = new List<Group>(); // Группы, которые курирует

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}
```

#### ViewModels (ReactiveUI) (Преподаватели)

*Пример ViewModel для списка преподавателей (аналогично StudentsViewModel)*

#### Views (XAML) (Преподаватели)

*Пример View для списка преподавателей (аналогично StudentsView)*

### 2.4 Модуль "Оценки и Успеваемость"

Отвечает за управление заданиями, их сдачей и оценками.

#### Модели Данных (Оценки)

Расположение: `ViridiscaUi.Domain/Models/Education`

```csharp
using System;
using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Education
{
    public class Assignment : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; } // Срок сдачи
        public double MaxScore { get; set; } = 100.0; // Максимальный балл
        public AssignmentType Type { get; set; } = AssignmentType.Homework;
        public Guid CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
        public Guid? LessonId { get; set; } // Опциональная связь с уроком
        public virtual Lesson? Lesson { get; set; }

        // Навигационные свойства
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public enum AssignmentType
    {
        Homework,   // Домашнее задание
        Quiz,       // Тест
        Exam,       // Экзамен
        Project,    // Проект
        LabWork     // Лабораторная работа
    }

    public class Submission : AuditableEntity
    {
        public Guid StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;
        public Guid AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; } = null!;
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        public string? Content { get; set; } // Текст ответа или ссылка на файл
        public double? Score { get; set; } // Оценка
        public string? Feedback { get; set; } // Комментарий преподавателя
        public Guid? GradedById { get; set; } // Uid преподавателя, поставившего оценку
        public Teacher? GradedBy { get; set; }
        public DateTime? GradedDate { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    }

    public enum SubmissionStatus
    {
        Draft,        // Черновик (не сдано)
        Submitted,    // Сдано
        Late,         // Сдано с опозданием
        UnderReview,  // На проверке
        Graded,       // Оценено
        Returned      // Возвращено на доработку
    }
}
```

#### ViewModels (ReactiveUI) (Оценки)

*Пример ViewModel для журнала оценок или управления заданиями* 

#### Views (XAML) (Оценки)

*Пример View для отображения журнала оценок* 

### 2.5 Модуль "Аутентификация и Авторизация"

Отвечает за управление пользователями, ролями и правами доступа.

#### Модели Данных (Аутентификация)

Расположение: `ViridiscaUi.Domain/Models/Auth`

```csharp
using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Education; // Для Student, Teacher

namespace ViridiscaUi.Domain.Models.Auth
{
    public class User : AuditableEntity // Используем общий AuditableEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Хеш пароля
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpiry { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }

        // Связи с профилями
        public virtual Student? StudentProfile { get; set; }
        public virtual Teacher? TeacherProfile { get; set; }
        // Возможно, другие профили (Администратор и т.д.)
    }

    public class Role : AuditableEntity
    {
        public string Name { get; set; } = string.Empty; // e.g., "Administrator", "Teacher", "Student"
        public string? Description { get; set; }

        // Навигационные свойства
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }

    public class Permission : AuditableEntity
    {
        public string Name { get; set; } = string.Empty; // e.g., "ViewStudents", "EditCourse", "DeleteUser"
        public string? Description { get; set; }

        // Навигационные свойства
        public virtual ICollection<RolePermission> Roles { get; set; } = new List<RolePermission>();
    }

    // Сущность для связи Роль-Разрешение
    public class RolePermission : AuditableEntity
    {
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
        public Guid PermissionId { get; set; }
        public virtual Permission Permission { get; set; } = null!;
    }
}
```

#### ViewModels (ReactiveUI) (Аутентификация)

```csharp
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Services; // Предполагаемые сервисы

namespace ViridiscaUi.ViewModels.Auth
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [Reactive] public string Username { get; set; } = string.Empty;
        [Reactive] public string Password { get; set; } = string.Empty;
        [Reactive] public string? ErrorMessage { get; set; }
        [ObservableAsProperty] public bool IsLoggingIn { get; }

        public ReactiveCommand<Unit, bool> LoginCommand { get; }

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            var canLogin = this.WhenAnyValue(
                x => x.Username,
                x => x.Password,
                (user, pass) => !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass)
            );

            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);
            LoginCommand.IsExecuting.ToPropertyEx(this, x => x.IsLoggingIn);

            // Обработка результата команды
            LoginCommand.Subscribe(success =>
            {
                if (success)
                {
                    ErrorMessage = null;
                    _navigationService.NavigateToMainArea(); // Переход к основному интерфейсу
                }
                else
                {
                    ErrorMessage = "Неверное имя пользователя или пароль.";
                }
            });

            // Обработка ошибок команды
            LoginCommand.ThrownExceptions.Subscribe(ex =>
            {
                // Логирование ошибки
                ErrorMessage = "Произошла ошибка при входе.";
            });
        }

        private async Task<bool> LoginAsync()
        {
            ErrorMessage = null; // Сброс ошибки перед попыткой входа
            return await _authService.LoginAsync(Username, Password);
        }
    }
}
```

#### Views (XAML) (Аутентификация)

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:ViridiscaUi.ViewModels.Auth"
             x:Class="ViridiscaUi.Views.Auth.LoginView"
             x:DataType="vm:LoginViewModel">
    
    <Border Background="#F0F0F0"
            CornerRadius="8"
            Padding="30"
            Width="400"
            HorizontalAlignment="Center"
            VerticalAlignment="Center"
            BoxShadow="0 5 15 0 #20000000">
        
        <DockPanel LastChildFill="True">
            <!-- Заголовок -->
            <TextBlock DockPanel.Dock="Top"
                      Text="Вход в Viridisca LMS"
                      FontSize="24"
                      FontWeight="SemiBold"
                      HorizontalAlignment="Center"
                      Margin="0 0 0 25" />

            <!-- Сообщение об ошибке -->
             <TextBlock DockPanel.Dock="Top"
                       Text="{Binding ErrorMessage}"
                       Foreground="Red"
                       IsVisible="{Binding ErrorMessage, Converter={x:Static StringConverters.IsNotNullOrEmpty}}"
                       TextWrapping="Wrap"
                       Margin="0 0 0 15"/>

            <!-- Кнопка входа -->
            <Button DockPanel.Dock="Bottom"
                   Content="Войти"
                   Command="{Binding LoginCommand}"
                   HorizontalAlignment="Stretch"
                   HorizontalContentAlignment="Center"
                   Padding="0 12"
                   Margin="0 20 0 0"
                   Classes="accent"/>
            
             <!-- Индикатор загрузки -->
            <ProgressBar DockPanel.Dock="Bottom" 
                         IsIndeterminate="True" 
                         IsVisible="{Binding IsLoggingIn}"
                         Margin="0 10 0 0"/>

            <!-- Поля ввода -->
            <StackPanel Spacing="15">
                <TextBox Text="{Binding Username}" 
                         Watermark="Имя пользователя или Email"
                         UseFloatingWatermark="True"/>
                <TextBox Text="{Binding Password}" 
                         Watermark="Пароль"
                         PasswordChar="•"
                         UseFloatingWatermark="True"/>
                <!-- Можно добавить CheckBox "Запомнить меня" -->
            </StackPanel>
        </DockPanel>
    </Border>
</UserControl>
```

## 3. Пользовательский Интерфейс

### 3.1 Главное Окно (Пример)

Расположение: `ViridiscaUi/Views/MainWindow.axaml`

```xaml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:ViridiscaUi.ViewModels"
        x:Class="ViridiscaUi.Views.MainWindow"
        x:DataType="vm:MainViewModel"
        Icon="/Assets/appicon.ico" <!-- Укажите путь к иконке -->
        Title="Viridisca LMS"
        WindowStartupLocation="CenterScreen"
        MinWidth="1024" MinHeight="768"
        Width="1200" Height="800">

    <Design.DataContext>
        <!-- Предоставляет ViewModel для дизайнера -->
        <vm:DesignTimeMainViewModel/> 
    </Design.DataContext>

    <Grid ColumnDefinitions="Auto,*">
        <!-- Боковое меню (Навигация) -->
        <Border Grid.Column="0" Background="#2D3748" MinWidth="220">
            <DockPanel>
                <!-- Логотип и название -->
                <StackPanel DockPanel.Dock="Top" Margin="15">
                    <!-- <Image Source="/Assets/logo.png" Height="40" /> -->
                    <TextBlock Text="Viridisca LMS"
                              Foreground="White"
                              FontSize="20"
                              FontWeight="Bold"
                              HorizontalAlignment="Center"
                              Margin="0 10 0 15" />
                </StackPanel>
                
                <!-- Пункты меню -->
                <ScrollViewer VerticalScrollBarVisibility="Auto">
                    <!-- Используйте ItemsControl или ListBox для привязки к коллекции пунктов меню в ViewModel -->
                    <StackPanel Margin="5 0">
                        <Button Content="Панель мониторинга" Command="{Binding NavigateToDashboardCommand}" Classes="menu-item"/>
                        <Button Content="Студенты" Command="{Binding NavigateToStudentsCommand}" Classes="menu-item"/>
                        <Button Content="Курсы" Command="{Binding NavigateToCoursesCommand}" Classes="menu-item"/>
                        <Button Content="Преподаватели" Command="{Binding NavigateToTeachersCommand}" Classes="menu-item"/>
                        <Button Content="Оценки" Command="{Binding NavigateToGradesCommand}" Classes="menu-item"/>
                        <!-- Разделитель -->
                        <Separator Height="1" Background="#4A5568" Margin="10"/> 
                        <Button Content="Настройки" Command="{Binding NavigateToSettingsCommand}" Classes="menu-item"/>
                    </StackPanel>
                </ScrollViewer>
            </DockPanel>
        </Border>
        
        <!-- Основное содержимое -->
        <Panel Grid.Column="1">
             <!-- Верхняя панель (Header) -->
             <Border DockPanel.Dock="Top" 
                    Background="White" 
                    BorderBrush="LightGray" 
                    BorderThickness="0 0 0 1"
                    Padding="15">
                <Grid ColumnDefinitions="*,Auto">
                    <!-- Заголовок текущего раздела -->
                    <TextBlock Grid.Column="0"
                              Text="{Binding CurrentPageTitle}" <!-- Привязка к заголовку -->
                              FontSize="20"
                              FontWeight="SemiBold"
                              VerticalAlignment="Center" />
                    
                    <!-- Информация о пользователе и кнопка выхода -->
                    <StackPanel Grid.Column="1"
                              Orientation="Horizontal"
                              Spacing="10" VerticalAlignment="Center">
                        <TextBlock Text="{Binding CurrentUser.FullName}" VerticalAlignment="Center" />
                        <Button Content="Выйти" Command="{Binding LogoutCommand}" Padding="8 4"/>
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Контейнер для отображения страниц/представлений -->
            <!-- Используйте RouterView из ReactiveUI для навигации -->
            <TransitioningContentControl Margin="0,65,0,0" <!-- Отступ под Header -->
                                       Content="{Binding Router.CurrentViewModel}"
                                       Padding="20"/>
        </Panel>
    </Grid>
    
    <Window.Styles>
        <Style Selector="Button.menu-item">
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="Foreground" Value="#E2E8F0" />
            <Setter Property="HorizontalAlignment" Value="Stretch" />
            <Setter Property="HorizontalContentAlignment" Value="Left" />
            <Setter Property="Padding" Value="15 12" />
            <Setter Property="Margin" Value="5 2" />
            <Setter Property="CornerRadius" Value="4"/>
        </Style>
        <Style Selector="Button.menu-item:pointerover /template/ ContentPresenter">
            <Setter Property="Background" Value="#4A5568" />
            <Setter Property="TextBlock.Foreground" Value="White" />
        </Style>
        <Style Selector="Button.menu-item:pressed /template/ ContentPresenter">
            <Setter Property="Background" Value="#2D3748" />
        </Style>
        <!-- Добавьте другие стили (например, для кнопок accent, danger) -->
    </Window.Styles>
</Window>
```

### 3.2 Панель Мониторинга (Пример)

*Ранее представленный XAML-пример для Dashboard из `lms-components.md` нуждается в адаптации под Avalonia UI, если он был предназначен для WPF. Следует использовать контролы Avalonia и соответствующие стили.* 

```xaml
<!-- Пример структуры Dashboard для Avalonia -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:ViridiscaUi.ViewModels"
             x:Class="ViridiscaUi.Views.DashboardView"
             x:DataType="vm:DashboardViewModel">

    <ScrollViewer>
        <StackPanel Spacing="20">
            <!-- Приветствие -->
            <TextBlock Text="{Binding WelcomeMessage}" Classes="h1"/>
            
            <!-- Статистика -->
            <UniformGrid Columns="3" Spacing="15">
                <Border Classes="card">
                    <StackPanel>
                        <TextBlock Text="Активные студенты" Classes="card-title"/>
                        <TextBlock Text="{Binding ActiveStudentCount}" Classes="card-value"/>
                    </StackPanel>
                </Border>
                <Border Classes="card">
                     <StackPanel>
                        <TextBlock Text="Активные курсы" Classes="card-title"/>
                        <TextBlock Text="{Binding ActiveCourseCount}" Classes="card-value"/>
                    </StackPanel>
                </Border>
                 <Border Classes="card">
                     <StackPanel>
                        <TextBlock Text="Предстоящие задания" Classes="card-title"/>
                        <TextBlock Text="{Binding UpcomingAssignmentsCount}" Classes="card-value"/>
                    </StackPanel>
                </Border>
            </UniformGrid>

            <!-- Графики (используйте подходящую библиотеку, например LiveChartsCore.SkiaSharpView.Avalonia) -->
            <Grid ColumnDefinitions="*,*" RowDefinitions="Auto" Gap="20">
                <Border Grid.Column="0" Classes="card">
                    <StackPanel>
                        <TextBlock Text="Распределение студентов по статусам" Classes="card-title" Margin="0,0,0,10"/>
                        <!-- Placeholder для графика -->
                        <Panel MinHeight="200" Background="#EEE"/>
                    </StackPanel>
                </Border>
                <Border Grid.Column="1" Classes="card">
                     <StackPanel>
                        <TextBlock Text="Активность курсов" Classes="card-title" Margin="0,0,0,10"/>
                        <!-- Placeholder для графика -->
                        <Panel MinHeight="200" Background="#EEE"/>
                    </StackPanel>
                </Border>
            </Grid>

            <!-- Последние уведомления/активности -->
            <Border Classes="card">
                <StackPanel>
                    <TextBlock Text="Последние уведомления" Classes="card-title"/>
                    <ItemsControl ItemsSource="{Binding RecentNotifications}">
                         <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Padding="10" Margin="0 5">
                                    <TextBlock Text="{Binding Message}" TextWrapping="Wrap"/>
                                    <!-- Добавьте дату и тип уведомления -->
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>

     <UserControl.Styles>
        <!-- Определите стили для h1, card, card-title, card-value -->
        <Style Selector="TextBlock.h1">
            <Setter Property="FontSize" Value="24"/>
            <Setter Property="FontWeight" Value="Bold"/>
            <Setter Property="Margin" Value="0 0 0 15"/>
        </Style>
         <Style Selector="Border.card">
            <Setter Property="Background" Value="White"/>
            <Setter Property="CornerRadius" Value="4"/>
            <Setter Property="Padding" Value="15"/>
            <Setter Property="BoxShadow" Value="0 2 4 0 #1A000000"/>
        </Style>
        <Style Selector="TextBlock.card-title">
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Opacity" Value="0.7"/>
             <Setter Property="Margin" Value="0 0 0 5"/>
        </Style>
        <Style Selector="TextBlock.card-value">
            <Setter Property="FontSize" Value="28"/>
            <Setter Property="FontWeight" Value="Bold"/>
        </Style>
    </UserControl.Styles>

</UserControl>
```

## 4. Общие Рекомендации по Реализации LMS

1.  **Аудит**: Внедрите логирование изменений для ключевых сущностей (кто, когда и что изменил), используя поля `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`.
2.  **Валидация**: Реализуйте надежную валидацию данных как на уровне ViewModel (например, с использованием `ReactiveUI.ValidationHelpers`), так и на уровне сервисов.
3.  **Асинхронность**: Всегда используйте асинхронные операции (`async/await`) для взаимодействия с базой данных и другими внешними ресурсами, чтобы не блокировать UI-поток.
4.  **Кэширование**: Применяйте стратегии кэширования для часто запрашиваемых данных (например, списки ролей, статусов, категорий), чтобы снизить нагрузку на базу данных.
5.  **Поиск и Фильтрация**: Обеспечьте гибкие механизмы поиска и фильтрации для всех списков данных.
6.  **Права Доступа**: Тщательно продумайте систему разрешений (Permissions) и их связь с ролями (Roles) для гранулярного контроля доступа к функциям и данным.
7.  **Уведомления**: Разработайте механизм уведомлений для пользователей (например, о новых заданиях, оценках, сообщениях).
8.  **Отчетность**: Предусмотрите возможность генерации отчетов (успеваемость, посещаемость, активность пользователей).
9.  **Локализация**: Заложите возможность локализации интерфейса и данных для поддержки нескольких языков.
10. **Тестирование**: Пишите модульные тесты для сервисов и ViewModels, а также интеграционные тесты для ключевых сценариев.
11. **Обработка ошибок**: Реализуйте централизованную обработку ошибок и их логирование.

## 5. Заключение

Этот документ представляет собой основу для разработки компонентов LMS ViridiscaUi. Он определяет структуру данных и базовые компоненты, которые могут быть расширены и доработаны в процессе разработки. Следование этим рекомендациям поможет создать согласованное, масштабируемое и поддерживаемое приложение. 

## Общие принципы

### Адаптивная верстка

1. **Запрещенные практики**
   - Не используйте фиксированные размеры (Width, Height)
   - Не используйте проценты в Grid
   - Не используйте абсолютные значения для размеров

2. **Рекомендуемые подходы**
   - Используйте Grid с `*` для адаптивного распределения пространства
   - Используйте DockPanel для сложных layouts
   - Используйте ScrollViewer для прокручиваемого контента
   - Используйте WrapPanel для адаптивных списков

### Обработка текста

1. **Предотвращение обрезания**
   - Всегда используйте `TextWrapping="Wrap"`
   - Устанавливайте `TextTrimming="None"`
   - Используйте `MaxWidth` с привязкой к родительскому контейнеру

2. **Адаптивные контейнеры**
   - Используйте ScrollViewer для длинного контента
   - Настраивайте видимость скроллбаров
   - Используйте DockPanel для сложных layouts

## Компоненты

### 1. Базовые компоненты

#### 1.1 TextBlock
```xml
<!-- Правильный подход к тексту -->
<TextBlock TextWrapping="Wrap" 
           TextTrimming="None"
           MaxWidth="{Binding RelativeSource={RelativeSource AncestorType=Control}, Path=ActualWidth}">
    <TextBlock.Text>
        <MultiBinding StringFormat="{}{0} - {1}">
            <Binding Path="Title" />
            <Binding Path="Description" />
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

#### 1.2 Image
```xml
<!-- Правильный подход к изображениям -->
<Image Stretch="Uniform" 
       StretchDirection="DownOnly"
       MaxWidth="{Binding RelativeSource={RelativeSource AncestorType=Control}, Path=ActualWidth}">
    <Image.Source>
        <Binding Path="ImageSource" />
    </Image.Source>
</Image>
```

### 2. Контейнеры

#### 2.1 Grid
```xml
<!-- Правильное использование Grid -->
<Grid RowDefinitions="Auto,*,Auto" ColumnDefinitions="*,Auto">
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Header" />
    <ScrollViewer Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="2">
        <StackPanel>
            <!-- Content -->
        </StackPanel>
    </ScrollViewer>
    <Button Grid.Row="2" Grid.Column="1" Content="Action" />
</Grid>
```

#### 2.2 DockPanel
```xml
<!-- Правильное использование DockPanel -->
<DockPanel>
    <Menu DockPanel.Dock="Top">
        <!-- Menu items -->
    </Menu>
    <ToolBar DockPanel.Dock="Top">
        <!-- Toolbar items -->
    </ToolBar>
    <StatusBar DockPanel.Dock="Bottom">
        <!-- Status items -->
    </StatusBar>
    <Grid>
        <!-- Main content -->
    </Grid>
</DockPanel>
```

### 3. Списки и таблицы

#### 3.1 ItemsControl
```xml
<!-- Правильный подход к спискам -->
<ItemsControl Items="{Binding Items}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <UniformGrid Columns="3" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Margin="4" Padding="8">
                <StackPanel>
                    <TextBlock Text="{Binding Title}" 
                             TextWrapping="Wrap" 
                             TextTrimming="None" />
                    <TextBlock Text="{Binding Description}" 
                             TextWrapping="Wrap" 
                             TextTrimming="None" />
                </StackPanel>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

#### 3.2 DataGrid
```xml
<!-- Правильный подход к таблицам -->
<DataGrid AutoGenerateColumns="False"
          CanUserResizeColumns="True"
          CanUserReorderColumns="True"
          HorizontalScrollBarVisibility="Auto"
          VerticalScrollBarVisibility="Auto">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Name" 
                           Binding="{Binding Name}"
                           Width="*" />
        <DataGridTextColumn Header="Description" 
                           Binding="{Binding Description}"
                           Width="2*" />
    </DataGrid.Columns>
</DataGrid>
```

### 4. Пользовательские компоненты

#### 4.1 Композиция
```xml
<!-- Правильный подход к композиции -->
<StackPanel>
    <local:HeaderView />
    <local:ContentArea>
        <local:DataGrid />
    </local:ContentArea>
    <local:StatusBar />
</StackPanel>
```

#### 4.2 Стили
```xml
<!-- Правильный подход к стилям -->
<Styles>
    <Style Selector="TextBlock.header">
        <Setter Property="FontSize" Value="16" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Margin" Value="0,0,0,8" />
    </Style>
</Styles>
```

## Рекомендации по производительности

1. Используйте виртуализацию для больших списков
2. Применяйте ленивую загрузку для тяжелых компонентов
3. Используйте кэширование для часто используемых данных
4. Оптимизируйте привязки данных, избегая сложных преобразований

## Отладка UI

1. Используйте `Debug.WriteLine` для отслеживания изменений в привязках
2. Применяйте временные границы для визуализации layout
3. Используйте инструменты разработчика Avalonia для инспекции UI