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
    public class ProfileViewModel : RoutableViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;

        /// <summary>
        /// URL-сегмент для навигации
        /// </summary>
        public override string UrlPathSegment => "profile";

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
            IDialogService dialogService,
            IAuthService authService) : base(screen)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

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
                await _dialogService.ShowErrorAsync("Ошибка", "Не удалось загрузить данные профиля");
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
                    await _dialogService.ShowErrorAsync("Ошибка", "Не удалось получить данные пользователя");
                    return;
                }

                var success = await _userService.UpdateProfileAsync(
                    currentUser.Uid,
                    FirstName,
                    LastName,
                    MiddleName,
                    PhoneNumber
                );

                if (success)
                {
                    await _dialogService.ShowInfoAsync("Успех", "Профиль успешно обновлен");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Ошибка", "Не удалось обновить профиль");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка", $"Произошла ошибка при обновлении профиля: {ex.Message}");
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
                    await _dialogService.ShowErrorAsync("Ошибка", "Не удалось получить данные пользователя");
                    return;
                }

                // TODO: Реализовать выбор и загрузку изображения
                await _dialogService.ShowInfoAsync("Информация", "Функция изменения изображения профиля будет доступна в следующем обновлении");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Ошибка", $"Произошла ошибка при изменении изображения: {ex.Message}");
            }
        }
    }
} 