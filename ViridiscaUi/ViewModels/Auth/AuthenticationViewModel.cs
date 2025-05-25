using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.ViewModels.Auth
{
    /// <summary>
    /// ViewModel для объединенного представления авторизации (вход + регистрация)
    /// </summary>
    public class AuthenticationViewModel : RoutableViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IRoleService _roleService;
        
        /// <summary>
        /// URL-сегмент для навигации
        /// </summary>
        public override string UrlPathSegment => "auth";

        /// <summary>
        /// Режим регистрации (false = вход, true = регистрация)
        /// </summary>
        [Reactive]
        public bool IsRegistrationMode { get; set; }

        /// <summary>
        /// Имя пользователя (логин)
        /// </summary>
        [Reactive]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email (для регистрации)
        /// </summary>
        [Reactive]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя (для регистрации)
        /// </summary>
        [Reactive]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя (для регистрации)
        /// </summary>
        [Reactive]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Reactive]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение пароля (для регистрации)
        /// </summary>
        [Reactive]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Выбранная роль пользователя (для регистрации)
        /// </summary>
        [Reactive]
        public Role? SelectedRole { get; set; }

        /// <summary>
        /// Список доступных ролей (для регистрации)
        /// </summary>
        [Reactive]
        public ObservableCollection<Role> AvailableRoles { get; set; } = new();

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        [Reactive]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Флаг, указывающий на процесс обработки
        /// </summary>
        [Reactive]
        public bool IsProcessing { get; set; } = false;

        /// <summary>
        /// Флаг загрузки ролей
        /// </summary>
        [Reactive]
        public bool IsLoadingRoles { get; set; } = false;

        /// <summary>
        /// Текст кнопки действия
        /// </summary>
        [ObservableAsProperty]
        public string ActionButtonText { get; }

        /// <summary>
        /// Команда основного действия (вход/регистрация)
        /// </summary>
        public ReactiveCommand<Unit, Unit> ActionCommand { get; }

        /// <summary>
        /// Команда переключения на режим входа
        /// </summary>
        public ReactiveCommand<Unit, Unit> SwitchToLoginCommand { get; }

        /// <summary>
        /// Команда переключения на режим регистрации
        /// </summary>
        public ReactiveCommand<Unit, Unit> SwitchToRegisterCommand { get; }

        /// <summary>
        /// Создает новый экземпляр ViewModel для авторизации
        /// </summary>
        public AuthenticationViewModel(IAuthService authService, INavigationService navigationService, IRoleService roleService, IScreen hostScreen) 
            : base(hostScreen)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

            // Команды переключения режимов
            SwitchToLoginCommand = ReactiveCommand.Create(() => 
            {
                IsRegistrationMode = false;
                ClearErrors();
            });

            SwitchToRegisterCommand = ReactiveCommand.Create(() => 
            {
                IsRegistrationMode = true;
                ClearErrors();
                // Загружаем роли при переключении на режим регистрации
                _ = LoadRolesAsync();
            });

            // Динамический текст кнопки действия
            this.WhenAnyValue(x => x.IsRegistrationMode)
                .Select(isReg => isReg ? "Зарегистрироваться" : "Войти")
                .ToPropertyEx(this, x => x.ActionButtonText);

            // Проверка возможности выполнения команды действия
            var canExecuteAction = this.WhenAnyValue(
                x => x.Username,
                x => x.Password,
                x => x.IsRegistrationMode,
                x => x.Email,
                x => x.FirstName,
                x => x.LastName,
                x => x.ConfirmPassword,
                x => x.SelectedRole,
                x => x.IsProcessing,
                (username, password, isReg, email, firstName, lastName, confirmPassword, selectedRole, isProcessing) =>
                {
                    if (isProcessing || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                        return false;

                    if (isReg)
                    {
                        return !string.IsNullOrWhiteSpace(email) &&
                               !string.IsNullOrWhiteSpace(firstName) &&
                               !string.IsNullOrWhiteSpace(lastName) &&
                               password == confirmPassword &&
                               selectedRole != null &&
                               password.Length >= 6;
                    }

                    return true;
                });

            // Создание команды действия
            ActionCommand = ReactiveCommand.CreateFromTask(ExecuteActionAsync, canExecuteAction);
        }

        /// <summary>
        /// Загружает список доступных ролей
        /// </summary>
        private async Task LoadRolesAsync()
        {
            try
            {
                IsLoadingRoles = true;
                Console.WriteLine("AuthenticationViewModel: Начинаем загрузку ролей...");
                
                var roles = await _roleService.GetAllRolesAsync();
                Console.WriteLine($"AuthenticationViewModel: Получено ролей: {roles.Count()}");
                
                AvailableRoles.Clear();
                foreach (var role in roles)
                {
                    AvailableRoles.Add(role);
                    Console.WriteLine($"AuthenticationViewModel: Добавлена роль: {role.Name}");
                }

                // Устанавливаем роль студента по умолчанию
                SelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == "Student");
                Console.WriteLine($"AuthenticationViewModel: Роль по умолчанию: {SelectedRole?.Name ?? "null"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AuthenticationViewModel: Ошибка загрузки ролей: {ex.Message}");
                ErrorMessage = $"Ошибка загрузки ролей: {ex.Message}";
            }
            finally
            {
                IsLoadingRoles = false;
                Console.WriteLine("AuthenticationViewModel: Загрузка ролей завершена");
            }
        }

        /// <summary>
        /// Выполняет основное действие (вход или регистрацию)
        /// </summary>
        private async Task ExecuteActionAsync()
        {
            try
            {
                IsProcessing = true;
                ErrorMessage = null;

                if (IsRegistrationMode)
                {
                    await ExecuteRegistrationAsync();
                }
                else
                {
                    await ExecuteLoginAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Выполняет вход в систему
        /// </summary>
        private async Task ExecuteLoginAsync()
        {
            var result = await _authService.AuthenticateAsync(Username, Password);

            if (result.Success)
            {
                Console.WriteLine($"AuthenticationViewModel: Успешная авторизация пользователя: {result.User?.Email}");
                // Навигация теперь происходит автоматически через MainViewModel при изменении CurrentUserObservable
                // await _navigationService.NavigateToAsync("home"); - убираем эту строку
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Неверное имя пользователя или пароль";
            }
        }

        /// <summary>
        /// Выполняет регистрацию пользователя
        /// </summary>
        private async Task ExecuteRegistrationAsync()
        {
            // Дополнительная проверка на совпадение паролей
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            if (SelectedRole == null)
            {
                ErrorMessage = "Выберите роль";
                return;
            }

            var result = await _authService.RegisterAsync(Username, Email, Password, FirstName, LastName, SelectedRole.Uid);

            if (result.Success)
            {
                // После успешной регистрации переключаемся на режим входа
                IsRegistrationMode = false;
                ClearForm();
                ErrorMessage = null;
                
                // Можно также показать сообщение об успехе
                // Или автоматически выполнить вход
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Ошибка при регистрации";
            }
        }

        /// <summary>
        /// Очищает ошибки
        /// </summary>
        private void ClearErrors()
        {
            ErrorMessage = null;
        }

        /// <summary>
        /// Очищает форму
        /// </summary>
        private void ClearForm()
        {
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            // Оставляем остальные поля для удобства пользователя
        }
    }
} 