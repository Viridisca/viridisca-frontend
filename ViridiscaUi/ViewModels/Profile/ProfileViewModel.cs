using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Navigation;

namespace ViridiscaUi.ViewModels.Profile
{
    /// <summary>
    /// ViewModel для страницы профиля пользователя
    /// Следует принципам SOLID и чистой архитектуры
    /// </summary>
    [Route("profile", DisplayName = "Профиль", IconKey = "Profile", Order = 10, ShowInMenu = false)]
    public class ProfileViewModel : RoutableViewModelBase
    {
        

        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IFileService _fileService;

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
        public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; private set; } = null!;

        /// <summary>
        /// Команда для изменения изображения профиля
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangeProfileImageCommand { get; private set; } = null!;

        public ProfileViewModel(
            IScreen hostScreen,
            IUserService userService,
            IAuthService authService,
            IFileService fileService) : base(hostScreen)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));

            InitializeCommands();
            
            LogInfo("ProfileViewModel initialized");
        }

        #region Lifecycle Methods

        /// <summary>
        /// Вызывается при первой загрузке ViewModel
        /// </summary>
        protected override async Task OnFirstTimeLoadedAsync()
        {
            await LoadProfileDataAsync();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Инициализирует команды
        /// </summary>
        private void InitializeCommands()
        {
            // Используем стандартизированные методы создания команд из ViewModelBase
            SaveProfileCommand = CreateCommand(SaveProfileAsync, null, "Ошибка сохранения профиля");
            ChangeProfileImageCommand = CreateCommand(ChangeProfileImageAsync, null, "Ошибка изменения изображения профиля");
        }

        /// <summary>
        /// Загружает данные профиля пользователя
        /// </summary>
        private async Task LoadProfileDataAsync()
        {
            LogInfo("Loading profile data");
            
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                ShowError("Не удалось загрузить данные профиля");
                LogWarning("Failed to load current user data");
                return;
            }

            FirstName = currentUser.FirstName;
            LastName = currentUser.LastName;
            MiddleName = currentUser.MiddleName;
            PhoneNumber = currentUser.PhoneNumber;
            ProfileImageUrl = currentUser.ProfileImageUrl;
            Email = currentUser.Email;
            Role = currentUser.Role?.Name ?? "Не назначена";
            
            LogInfo("Profile data loaded successfully for user: {Email}", currentUser.Email);
        }

        /// <summary>
        /// Сохраняет изменения в профиле
        /// </summary>
        private async Task SaveProfileAsync()
        {
            LogInfo("Saving profile changes");
            
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                ShowError("Не удалось получить данные пользователя");
                LogWarning("Failed to get current user for profile update");
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
                ShowSuccess("Профиль успешно обновлен");
                LogInfo("Profile updated successfully for user: {Email}", currentUser.Email);
            }
            else
            {
                ShowError("Не удалось обновить профиль");
                LogWarning("Failed to update profile for user: {Email}", currentUser.Email);
            }
        }

        /// <summary>
        /// Изменяет изображение профиля
        /// </summary>
        private async Task ChangeProfileImageAsync()
        {
            LogInfo("Changing profile image");
            
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                ShowError("Не удалось получить данные пользователя");
                LogWarning("Failed to get current user for image change");
                return;
            }

            // TODO: Реализовать выбор и загрузку изображения
            ShowInfo("Функция изменения изображения профиля будет доступна в следующем обновлении");
            LogInfo("Profile image change requested for user: {Email}", currentUser.Email);
        }

        #endregion
    }
} 