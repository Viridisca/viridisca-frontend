# Исправления регистрации ViewModels в DI контейнере

## Проблема
Приложение выдавало ошибки при попытке навигации к различным страницам:
- `No service for type 'ViridiscaUi.ViewModels.System.NotificationCenterViewModel' has been registered`
- `No service for type 'ViridiscaUi.ViewModels.Education.AssignmentsViewModel' has been registered`
- `No service for type 'ViridiscaUi.ViewModels.Education.GradesViewModel' has been registered`
- `No service for type 'ViridiscaUi.ViewModels.Education.TeachersViewModel' has been registered`
- `No service for type 'ViridiscaUi.ViewModels.Students.StudentsViewModel' has been registered`
- `No service for type 'ViridiscaUi.ViewModels.Education.GroupsViewModel' has been registered`

## Выполненные исправления

### 1. Обновлена регистрация ViewModels в DI контейнере

**Файл**: `ViridiscaUi/DI/DependencyInjectionExtensions.cs`

Добавлены недостающие using statements:
```csharp
using ViridiscaUi.ViewModels.Students;
using ViridiscaUi.ViewModels.System;
```

Добавлены недостающие регистрации ViewModels в методе `AddViewModels()`:
```csharp
// Education ViewModels
services.AddTransient<CoursesViewModel>();
services.AddTransient<GroupsViewModel>();
services.AddTransient<TeachersViewModel>();
services.AddTransient<AssignmentsViewModel>();
services.AddTransient<GradesViewModel>();

// Students ViewModels
services.AddTransient<StudentsViewModel>();

// System ViewModels
services.AddTransient<NotificationCenterViewModel>();
```

### 2. Исправлен StudentsView

**Проблема**: StudentsView имел расширение `.broken` и не работал.

**Исправления**:
- Создан новый `StudentsView.axaml` с корректной разметкой
- Создан `StudentsView.axaml.cs` с правильным code-behind
- Удалены сломанные файлы `.broken`
- Исправлены привязки данных и команды
- Добавлены эмодзи-иконки вместо отсутствующих PathIcon

### 3. Проверены атрибуты Route

Все ViewModels имеют корректные атрибуты `[Route]`:
- ✅ **GradesViewModel**: `[Route("grades", DisplayName = "Оценки", IconKey = "Grade", Order = 5, Group = "Education")]`
- ✅ **TeachersViewModel**: `[Route("teachers", DisplayName = "Преподаватели", IconKey = "Teacher", Order = 4, Group = "Education")]`
- ✅ **StudentsViewModel**: `[Route("students", DisplayName = "Студенты", IconKey = "Student", Order = 3, Group = "Education")]`
- ✅ **GroupsViewModel**: `[Route("groups", DisplayName = "Группы", IconKey = "👥", Order = 3, Group = "Education")]`
- ✅ **AssignmentsViewModel**: `[Route("assignments", DisplayName = "Задания", IconKey = "📝", Order = 6, Group = "Education")]`
- ✅ **NotificationCenterViewModel**: `[Route("notifications", DisplayName = "Уведомления", IconKey = "🔔", Order = 10, Group = "System")]`

### 4. Структура регистрации ViewModels

Теперь все ViewModels организованы по категориям:

```csharp
// Main ViewModels
services.AddTransient<MainViewModel>();
services.AddTransient<HomeViewModel>();

// Auth ViewModels
services.AddTransient<LoginViewModel>();
services.AddTransient<RegisterViewModel>();
services.AddTransient<AuthenticationViewModel>();

// Profile ViewModels
services.AddTransient<ProfileViewModel>();

// Education ViewModels
services.AddTransient<CoursesViewModel>();
services.AddTransient<GroupsViewModel>();
services.AddTransient<TeachersViewModel>();
services.AddTransient<AssignmentsViewModel>();
services.AddTransient<GradesViewModel>();

// Students ViewModels
services.AddTransient<StudentsViewModel>();

// System ViewModels
services.AddTransient<NotificationCenterViewModel>();
```

## Результат

✅ Все ViewModels зарегистрированы в DI контейнере  
✅ Навигация между страницами работает корректно  
✅ Ошибки "No service for type" исправлены  
✅ StudentsView восстановлен и работает  
✅ Проект собирается без ошибок  
✅ Приложение запускается успешно  

## Тестирование

Приложение успешно компилируется и запускается. Система навигации автоматически сканирует и регистрирует все ViewModels с атрибутами `[Route]`, а DI контейнер корректно создает экземпляры ViewModels при навигации.

Все страницы теперь доступны для навигации:
- 🏠 Главная (home)
- 👤 Профиль (profile)
- 📚 Курсы (courses)
- 👥 Группы (groups)
- 👨‍🎓 Студенты (students)
- 👨‍🏫 Преподаватели (teachers)
- 📊 Оценки (grades)
- 📝 Задания (assignments)
- 🔔 Уведомления (notifications) 