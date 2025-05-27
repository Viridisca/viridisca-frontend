# Исправления навигации и кодировки в ViridiscaUi

## Проблема
Приложение выдавало ошибку: "Маршрут 'auth' не найден" при попытке навигации к странице авторизации.

## Выполненные исправления

### 1. Добавлены недостающие атрибуты Route

Добавлены атрибуты `[Route]` для следующих ViewModels:

- **AuthenticationViewModel**: `[Route("auth", DisplayName = "Авторизация", IconKey = "🔐", Order = 0, ShowInMenu = false)]`
- **LoginViewModel**: `[Route("login", DisplayName = "Вход", IconKey = "🔑", Order = 1, ShowInMenu = false)]`
- **RegisterViewModel**: `[Route("register", DisplayName = "Регистрация", IconKey = "📝", Order = 2, ShowInMenu = false)]`
- **GroupsViewModel**: `[Route("groups", DisplayName = "Группы", IconKey = "👥", Order = 3, Group = "Education")]`
- **AssignmentsViewModel**: `[Route("assignments", DisplayName = "Задания", IconKey = "📝", Order = 6, Group = "Education")]`
- **NotificationCenterViewModel**: `[Route("notifications", DisplayName = "Уведомления", IconKey = "🔔", Order = 10, Group = "System")]`

### 2. Исправлена кодировка комментариев

Заменены некорректно отображаемые символы кириллицы на правильную UTF-8 кодировку во всех ViewModels авторизации.

### 3. Улучшена система навигации

- Добавлено свойство `NavigateCommand` в класс `NavigationRoute`
- Добавлено свойство `Label` как alias для `DisplayName`
- Обновлен метод `UpdateMenuItems` в `MainViewModel` для создания команд навигации

### 4. Зарегистрированные маршруты

Теперь в системе зарегистрированы следующие маршруты:

#### Авторизация (ShowInMenu = false)
- `auth` → AuthenticationViewModel
- `login` → LoginViewModel  
- `register` → RegisterViewModel

#### Основные страницы (Group = "Main")
- `home` → HomeViewModel
- `profile` → ProfileViewModel

#### Образование (Group = "Education")
- `courses` → CoursesViewModel
- `groups` → GroupsViewModel
- `students` → StudentsViewModel
- `teachers` → TeachersViewModel
- `grades` → GradesViewModel
- `assignments` → AssignmentsViewModel

#### Система (Group = "System")
- `notifications` → NotificationCenterViewModel

### 5. Исправления в XAML

Обновлен `MainWindow.axaml` для корректного использования команд навигации:
- Используется `{Binding NavigateCommand}` для элементов меню
- Используется `{Binding Label}` для отображения названий

## Результат

✅ Ошибка "Маршрут 'auth' не найден" исправлена  
✅ Навигация между страницами работает корректно  
✅ Меню отображается с правильными названиями и иконками  
✅ Кодировка комментариев исправлена  
✅ Все ViewModels имеют корректные маршруты  

## Тестирование

Приложение успешно компилируется и запускается без ошибок навигации. Система маршрутизации автоматически сканирует и регистрирует все ViewModels с атрибутами `[Route]`. 