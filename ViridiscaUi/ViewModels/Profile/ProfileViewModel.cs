using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.ViewModels.Profile
{
    /// <summary>
    /// ViewModel для страницы профиля пользователя
    /// </summary>
    public class ProfileViewModel : ViewModelBase, IRoutableViewModel
    {
        public string? UrlPathSegment => "profile";
        public IScreen HostScreen { get; }

        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IStatusService _statusService;

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
        /// Отчество пользователя
        /// </summary>
        [Reactive]
        public string MiddleName { get; set; } = string.Empty;

        /// <summary>
        /// Номер телефона
        /// </summary>
        [Reactive]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// URL изображения профиля
        /// </summary>
        [Reactive]
        public string ProfileImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта
        /// </summary>
        [Reactive]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Роль пользователя
        /// </summary>
        [Reactive]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Команда для сохранения изменений профиля
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; }

        /// <summary>
        /// Команда для изменения изображения профиля
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangeProfileImageCommand { get; }

        public ProfileViewModel(
            IScreen screen,
            IUserService userService,
            IAuthService authService,
            IStatusService statusService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
            HostScreen = screen ?? throw new ArgumentNullException(nameof(screen));

            // Инициализация команд
            SaveProfileCommand = ReactiveCommand.CreateFromTask(SaveProfileAsync);
            ChangeProfileImageCommand = ReactiveCommand.CreateFromTask(ChangeProfileImageAsync);

            // Загрузка данных профиля
            LoadProfileData();
        }

        /// <summary>
        /// Загружает данные профиля пользователя
        /// </summary>
        private async void LoadProfileData()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                _statusService.ShowError("Не удалось загрузить данные профиля", "ProfileViewModel");
                return;
            }

            FirstName = currentUser.FirstName;
            LastName = currentUser.LastName;
            MiddleName = currentUser.MiddleName;
            PhoneNumber = currentUser.PhoneNumber;
            ProfileImageUrl = currentUser.ProfileImageUrl;
            Email = currentUser.Email;
            Role = currentUser.Role?.Name ?? "Не назначена";
        }

        /// <summary>
        /// Сохраняет изменения в профиле
        /// </summary>
        private async Task SaveProfileAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    _statusService.ShowError("Не удалось получить данные пользователя", "ProfileViewModel");
                    return;
                }

                // Обновляем данные пользователя
                currentUser.FirstName = FirstName;
                currentUser.LastName = LastName;
                currentUser.MiddleName = MiddleName;
                currentUser.PhoneNumber = PhoneNumber;

                var success = await _userService.UpdateUserAsync(currentUser);
                if (success)
                {
                    _statusService.ShowInfo("Профиль успешно обновлен", "ProfileViewModel");
                }
                else
                {
                    _statusService.ShowError("Не удалось обновить профиль", "ProfileViewModel");
                }
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Произошла ошибка при обновлении профиля: {ex.Message}", "ProfileViewModel");
            }
        }

        /// <summary>
        /// Изменяет изображение профиля
        /// </summary>
        private async Task ChangeProfileImageAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    _statusService.ShowError("Не удалось получить данные пользователя", "ProfileViewModel");
                    return;
                }

                // TODO: Реализовать выбор и загрузку изображения
                _statusService.ShowInfo("Функция изменения изображения профиля будет доступна в следующем обновлении", "ProfileViewModel");
            }
            catch (Exception ex)
            {
                _statusService.ShowError($"Произошла ошибка при изменении изображения: {ex.Message}", "ProfileViewModel");
            }
        }
    }
} 