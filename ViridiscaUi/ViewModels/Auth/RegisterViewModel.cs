using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.ViewModels.Auth
{
    /// <summary>
    /// ViewModel для страницы регистрации пользователя
    /// </summary>
    public class RegisterViewModel : RoutableViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        /// <summary>
        /// URL-сегмент для навигации
        /// </summary>
        public override string UrlPathSegment => "register";

        /// <summary>
        /// Имя пользователя (логин)
        /// </summary>
        [Reactive]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта
        /// </summary>
        [Reactive]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        [Reactive]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        [Reactive]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Reactive]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        [Reactive]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        [Reactive]
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, указывающий на процесс регистрации
        /// </summary>
        [Reactive]
        public bool IsRegistering { get; set; } = false;

        /// <summary>
        /// Команда для регистрации пользователя
        /// </summary>
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }

        /// <summary>
        /// Команда для перехода на страницу входа
        /// </summary>
        public ReactiveCommand<Unit, Unit> GoToLoginCommand { get; }

        /// <summary>
        /// Создает новый экземпляр ViewModel для регистрации пользователя
        /// </summary>
        /// <param name="authService">Сервис аутентификации</param>
        /// <param name="navigationService">Сервис навигации</param>
        /// <param name="hostScreen">Родительский экран</param>
        public RegisterViewModel(IAuthService authService, INavigationService navigationService, IScreen hostScreen) 
            : base(hostScreen)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Проверка возможности выполнения команды регистрации
            var canRegister = this.WhenAnyValue(
                x => x.Username,
                x => x.Email,
                x => x.FirstName,
                x => x.LastName,
                x => x.Password,
                x => x.ConfirmPassword,
                x => x.IsRegistering,
                (username, email, firstName, lastName, password, confirmPassword, isRegistering) =>
                    !string.IsNullOrWhiteSpace(username) &&
                    !string.IsNullOrWhiteSpace(email) &&
                    !string.IsNullOrWhiteSpace(firstName) &&
                    !string.IsNullOrWhiteSpace(lastName) &&
                    !string.IsNullOrWhiteSpace(password) &&
                    password == confirmPassword &&
                    !isRegistering
            );

            // Создание команды регистрации
            RegisterCommand = ReactiveCommand.CreateFromTask(RegisterAsync, canRegister);
            
            // Команда для перехода на страницу входа
            GoToLoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await _navigationService.NavigateToAsync("login");
            });
        }

        /// <summary>
        /// Асинхронный метод для выполнения регистрации
        /// </summary>
        private async Task RegisterAsync()
        {
            try
            {
                IsRegistering = true;
                ErrorMessage = string.Empty;

                // Дополнительная проверка на совпадение паролей
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Пароли не совпадают";
                    return;
                }

                var result = await _authService.RegisterAsync(
                    Username, 
                    Email, 
                    Password, 
                    FirstName, 
                    LastName);

                if (result.Success)
                {
                    // Если регистрация успешная, перейти на страницу входа
                    await _navigationService.NavigateToAsync("login");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка регистрации: {ex.Message}";
            }
            finally
            {
                IsRegistering = false;
            }
        }
    }
} 