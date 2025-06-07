using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure.Navigation;
using ViridiscaUi.ViewModels.Bases.Navigations;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для страницы профиля пользователя
/// Современный дизайн с полной функциональностью
/// </summary>
[Route("profile", DisplayName = "Профиль", IconKey = "Account", Order = 10, Group = "Система", ShowInMenu = true)]
public class ProfileViewModel : RoutableViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IPersonService _personService;
    private readonly IDialogService _dialogService;
    private readonly IFileService _fileService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Сервис сессии пользователя
    /// </summary>
    protected IPersonSessionService PersonSessionService { get; private set; }

    #region Personal Information Properties

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Reactive] public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    [Reactive] public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество пользователя
    /// </summary>
    [Reactive] public string MiddleName { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения
    /// </summary>
    [Reactive] public DateTimeOffset? DateOfBirth { get; set; }

    /// <summary>
    /// Электронная почта
    /// </summary>
    [Reactive] public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Номер телефона
    /// </summary>
    [Reactive] public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// URL изображения профиля
    /// </summary>
    [Reactive] public string ProfileImageUrl { get; set; } = string.Empty;

    #endregion

    #region Password Change Properties

    /// <summary>
    /// Текущий пароль
    /// </summary>
    [Reactive] public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    [Reactive] public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение нового пароля
    /// </summary>
    [Reactive] public string ConfirmPassword { get; set; } = string.Empty;

    #endregion

    #region User Info Properties

    /// <summary>
    /// Роли пользователя
    /// </summary>
    [Reactive] public ObservableCollection<PersonRole> UserRoles { get; set; } = new();

    /// <summary>
    /// Дата регистрации
    /// </summary>
    [Reactive] public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// Дата последнего входа
    /// </summary>
    [Reactive] public DateTime? LastLoginDate { get; set; }

    #endregion

    #region State Properties

    /// <summary>
    /// Флаг загрузки данных
    /// </summary>
    [Reactive] public bool IsLoading { get; set; }

    /// <summary>
    /// Флаг сохранения
    /// </summary>
    [Reactive] public bool IsSaving { get; set; }

    /// <summary>
    /// Флаг изменения пароля
    /// </summary>
    [Reactive] public bool IsChangingPassword { get; set; }

    /// <summary>
    /// Флаг загрузки фото
    /// </summary>
    [Reactive] public bool IsUploadingPhoto { get; set; }

    #endregion

    #region Commands

    /// <summary>
    /// Команда сохранения профиля
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; private set; } = null!;

    /// <summary>
    /// Команда отмены изменений
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; } = null!;

    /// <summary>
    /// Команда изменения пароля
    /// </summary>
    public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; private set; } = null!;

    /// <summary>
    /// Команда загрузки фото
    /// </summary>
    public ReactiveCommand<Unit, Unit> UploadPhotoCommand { get; private set; } = null!;

    /// <summary>
    /// Команда удаления фото
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemovePhotoCommand { get; private set; } = null!;

    /// <summary>
    /// Команда обновления данных
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;

    #endregion

    public ProfileViewModel(
        IScreen hostScreen,
        IPersonSessionService personSessionService,
        IAuthService authService,
        IPersonService personService,
        IDialogService dialogService,
        IFileService fileService,
        INotificationService notificationService)
        : base(hostScreen)
    {
        PersonSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        InitializeCommands();
        
        // Загружаем данные сразу при создании ViewModel
        _ = LoadUserDataAsync();
    }

    #region Lifecycle Methods

    /// <summary>
    /// Вызывается при первой загрузке ViewModel
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        await LoadUserDataAsync();
    }

    /// <summary>
    /// Вызывается при активации ViewModel
    /// </summary>
    protected override void OnActivated()
    {
        base.OnActivated();
        _ = LoadUserDataAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Команда сохранения
        SaveCommand = CreateCommand(SaveProfileAsync, null, "Ошибка сохранения профиля");

        // Команда отмены
        CancelCommand = CreateCommand(CancelChangesAsync, null, "Ошибка отмены изменений");

        // Команда изменения пароля
        ChangePasswordCommand = CreateCommand(ChangePasswordAsync, null, "Ошибка изменения пароля");

        // Команда загрузки фото
        UploadPhotoCommand = CreateCommand(UploadPhotoAsync, null, "Ошибка загрузки фото");

        // Команда удаления фото
        RemovePhotoCommand = CreateCommand(RemovePhotoAsync, null, "Ошибка удаления фото");

        // Команда обновления
        RefreshCommand = CreateCommand(LoadUserDataAsync, null, "Ошибка обновления данных");
    }

    /// <summary>
    /// Загружает данные пользователя
    /// </summary>
    private async Task LoadUserDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Проверяем состояние сессии
            var currentPerson = PersonSessionService.CurrentPerson;
            
            if (currentPerson != null)
            {
                PopulateFromPerson(currentPerson);
                await LoadUserStatisticsAsync(currentPerson.Uid);
                ShowInfo("Данные профиля загружены");
            }
            else
            {
                // Очищаем все поля
                ClearProfileData();
                
                // Показываем сообщение пользователю
                SetError("Пользователь не авторизован. Пожалуйста, войдите в систему.");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки данных профиля");
            SetError("Критическая ошибка загрузки данных профиля", ex);
            
            // Очищаем данные при ошибке
            ClearProfileData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Загружает статистику пользователя
    /// </summary>
    private async Task LoadUserStatisticsAsync(Guid personUid)
    {
        try
        {
            // Загружаем последний вход из Account
            var account = await _personService.GetPersonAccountAsync(personUid);
            LastLoginDate = account?.LastLoginAt;
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки статистики пользователя");
            LastLoginDate = null;
        }
    }

    /// <summary>
    /// Сохраняет изменения профиля
    /// </summary>
    private async Task SaveProfileAsync()
    {
        try
        {
            IsSaving = true;

            var currentPerson = PersonSessionService.CurrentPerson;
            if (currentPerson == null)
            {
                SetError("Пользователь не найден");
                return;
            }

            // Обновляем данные
            currentPerson.FirstName = FirstName;
            currentPerson.LastName = LastName;
            currentPerson.MiddleName = MiddleName;
            currentPerson.DateOfBirth = DateOfBirth?.DateTime;
            currentPerson.Email = Email;
            currentPerson.PhoneNumber = PhoneNumber;

            var success = await _personService.UpdatePersonAsync(currentPerson);
            if (success)
            {
                await _dialogService.ShowInfoAsync("Успешно", "Профиль обновлен");
                ShowInfo("Профиль успешно сохранен");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Не удалось сохранить профиль");
                SetError("Ошибка сохранения профиля");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка сохранения профиля");
            await _dialogService.ShowErrorAsync("Ошибка", "Не удалось сохранить профиль");
            SetError("Ошибка сохранения профиля", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Отменяет изменения
    /// </summary>
    private async Task CancelChangesAsync()
    {
        try
        {
            var result = await _dialogService.ShowConfirmationAsync(
                "Отмена изменений",
                "Вы уверены, что хотите отменить все изменения?");

            if (result)
            {
                await LoadUserDataAsync();
                ShowInfo("Изменения отменены");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка отмены изменений");
            SetError("Ошибка отмены изменений", ex);
        }
    }

    /// <summary>
    /// Изменяет пароль
    /// </summary>
    private async Task ChangePasswordAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                await _dialogService.ShowWarningAsync("Внимание", "Введите текущий пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                await _dialogService.ShowWarningAsync("Внимание", "Введите новый пароль");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await _dialogService.ShowWarningAsync("Внимание", "Пароли не совпадают");
                return;
            }

            if (NewPassword.Length < 6)
            {
                await _dialogService.ShowWarningAsync("Внимание", "Пароль должен содержать минимум 6 символов");
                return;
            }

            IsChangingPassword = true;

            var currentPerson = PersonSessionService.CurrentPerson;
            if (currentPerson == null)
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Пользователь не найден");
                return;
            }

            var result = await _authService.ChangePasswordAsync(currentPerson.Uid, CurrentPassword, NewPassword);
            
            if (result)
            {
                ClearPasswordFields();
                await _dialogService.ShowInfoAsync("Успешно", "Пароль изменен");
                ShowInfo("Пароль успешно изменен");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Не удалось изменить пароль");
                SetError("Ошибка изменения пароля");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка изменения пароля");
            await _dialogService.ShowErrorAsync("Ошибка", "Не удалось изменить пароль");
            SetError("Ошибка изменения пароля", ex);
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    /// <summary>
    /// Загружает фото профиля
    /// </summary>
    private async Task UploadPhotoAsync()
    {
        try
        {
            IsUploadingPhoto = true;

            // Выбираем файл изображения
            var selectedFile = await _fileService.SelectImageFileAsync();
            if (selectedFile == null)
            {
                return;
            }

            // Проверяем размер файла (максимум 5 МБ)
            const long maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (selectedFile.Size > maxFileSize)
            {
                await _dialogService.ShowWarningAsync("Внимание", "Размер файла не должен превышать 5 МБ");
                return;
            }

            // Загружаем файл
            var uploadResult = await _fileService.UploadProfileImageAsync(selectedFile, PersonSessionService.CurrentPerson!.Uid);
            if (uploadResult.Success)
            {
                ProfileImageUrl = uploadResult.FilePath;
                await _dialogService.ShowInfoAsync("Успешно", "Фото профиля загружено");
                ShowInfo("Фото профиля успешно загружено");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка", $"Не удалось загрузить фото: {uploadResult.ErrorMessage}");
                SetError($"Ошибка загрузки фото: {uploadResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки фото");
            await _dialogService.ShowErrorAsync("Ошибка", "Не удалось загрузить фото");
            SetError("Ошибка загрузки фото", ex);
        }
        finally
        {
            IsUploadingPhoto = false;
        }
    }

    /// <summary>
    /// Удаляет фото профиля
    /// </summary>
    private async Task RemovePhotoAsync()
    {
        try
        {
            var result = await _dialogService.ShowConfirmationAsync(
                "Удаление фото",
                "Вы уверены, что хотите удалить фото профиля?");

            if (result)
            {
                ProfileImageUrl = string.Empty;
                await _dialogService.ShowInfoAsync("Успешно", "Фото профиля удалено");
                ShowInfo("Фото профиля удалено");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка удаления фото");
            await _dialogService.ShowErrorAsync("Ошибка", "Не удалось удалить фото");
            SetError("Ошибка удаления фото", ex);
        }
    }

    /// <summary>
    /// Заполняет поля из объекта Person
    /// </summary>
    private void PopulateFromPerson(Person person)
    {
        // Заполняем поля с проверкой
        FirstName = person.FirstName ?? string.Empty;
        LastName = person.LastName ?? string.Empty;
        MiddleName = person.MiddleName ?? string.Empty;
        DateOfBirth = person.DateOfBirth.HasValue ? new DateTimeOffset(person.DateOfBirth.Value) : null;
        Email = person.Email ?? string.Empty;
        PhoneNumber = person.PhoneNumber ?? string.Empty;
        RegistrationDate = person.CreatedAt;

        // Загружаем роли
        UserRoles.Clear();
        
        if (person.PersonRoles != null)
        {
            var activeRoles = person.PersonRoles.Where(pr => pr.IsActive).ToList();
            
            foreach (var personRole in activeRoles)
            {
                UserRoles.Add(personRole);
            }
        }
    }

    /// <summary>
    /// Очищает поля паролей
    /// </summary>
    private void ClearPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
    }

    /// <summary>
    /// Очищает данные профиля
    /// </summary>
    private void ClearProfileData()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        MiddleName = string.Empty;
        DateOfBirth = null;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        ProfileImageUrl = string.Empty;
        
        UserRoles.Clear();
        RegistrationDate = DateTime.MinValue;
        LastLoginDate = null;
    }

    #endregion
}
