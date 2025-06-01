using System;
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
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("profile", DisplayName = "Профиль", IconKey = "Account", Order = 10, Group = "Система", ShowInMenu = true)]
public class ProfileViewModel : RoutableViewModelBase
{
    

    private readonly IAuthService _authService;
    private readonly IPersonService _personService;

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
    /// Дата рождения
    /// </summary>
    [Reactive]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Полное имя
    /// </summary>
    [Reactive]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания
    /// </summary>
    [Reactive]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего изменения
    /// </summary>
    [Reactive]
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Флаг изменения пароля
    /// </summary>
    [Reactive]
    public bool IsChangingPassword { get; set; }

    /// <summary>
    /// Флаг сохранения
    /// </summary>
    [Reactive]
    public bool IsSaving { get; set; }

    /// <summary>
    /// Флаг загрузки
    /// </summary>
    [Reactive]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Команда для сохранения изменений профиля
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; private set; } = null!;

    /// <summary>
    /// Команда для изменения изображения профиля
    /// </summary>
    public ReactiveCommand<Unit, Unit> ChangeProfileImageCommand { get; private set; } = null!;

    /// <summary>
    /// Команда для изменения пароля
    /// </summary>
    public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; private set; } = null!;

    /// <summary>
    /// Текущий пароль
    /// </summary>
    [Reactive]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    [Reactive]
    public string NewPassword { get; set; } = string.Empty;

    public ProfileViewModel(
        IScreen hostScreen,
        IAuthService authService,
        IPersonService personService) : base(hostScreen)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));

        InitializeCommands();
        
        LogInfo("ProfileViewModel initialized");
    }

    #region Lifecycle Methods

    /// <summary>
    /// Вызывается при первой загрузке ViewModel
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        await LoadUserDataAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Используем стандартизированные методы создания команд из ViewModelBase
        SaveProfileCommand = CreateCommand(SaveChangesAsync, null, "Ошибка сохранения профиля");
        ChangeProfileImageCommand = CreateCommand(ChangeProfileImageAsync, null, "Ошибка изменения изображения профиля");
        ChangePasswordCommand = CreateCommand(ChangePasswordAsync, null, "Ошибка изменения пароля");
    }

    /// <summary>
    /// Загружает данные пользователя
    /// </summary>
    private async Task LoadUserDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Используем новый метод для получения текущего пользователя
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                PopulateFromPerson(currentPerson);
            }
            else
            {
                SetError("Не удалось загрузить данные пользователя");
            }
        }
        catch (Exception ex)
        {
            SetError("Ошибка при загрузке данных пользователя", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Сохраняет изменения в профиле
    /// </summary>
    private async Task SaveChangesAsync()
    {
        try
        {
            IsSaving = true;
            ClearError();

            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson == null)
            {
                SetError("Пользователь не найден");
                return;
            }

            // Обновляем данные Person
            currentPerson.FirstName = FirstName;
            currentPerson.LastName = LastName;
            currentPerson.MiddleName = MiddleName;
            currentPerson.Email = Email;
            currentPerson.PhoneNumber = PhoneNumber;
            currentPerson.DateOfBirth = BirthDate ?? DateTime.MinValue;
            currentPerson.LastModifiedAt = DateTime.UtcNow;

            // Используем новый метод для обновления Person
            var success = await _personService.UpdatePersonAsync(currentPerson);
            if (success)
            {
                ShowSuccess("Профиль обновлен");
                LogInfo("Profile updated for person: {PersonUid}", currentPerson.Uid);
            }
            else
            {
                SetError("Не удалось обновить профиль");
            }
        }
        catch (Exception ex)
        {
            SetError("Ошибка при сохранении профиля", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Изменяет изображение профиля
    /// </summary>
    private async Task ChangeProfileImageAsync()
    {
        LogInfo("Changing profile image");
        
        var currentPerson = await _authService.GetCurrentPersonAsync();
        if (currentPerson == null)
        {
            ShowError("Не удалось получить данные пользователя");
            LogWarning("Failed to get current user for image change");
            return;
        }

        // TODO: Реализовать выбор и загрузку изображения
        ShowInfo("Функция изменения изображения профиля будет доступна в следующем обновлении");
        LogInfo("Profile image change requested for person: {PersonUid}", currentPerson.Uid);
    }

    /// <summary>
    /// Изменяет пароль
    /// </summary>
    private async Task ChangePasswordAsync()
    {
        try
        {
            IsChangingPassword = true;
            ClearError();

            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson == null)
            {
                SetError("Пользователь не найден");
                return;
            }

            // Используем новый метод для смены пароля (передаем PersonUid)
            var success = await _authService.ChangePasswordAsync(currentPerson.Uid, CurrentPassword, NewPassword);
            if (success)
            {
                ShowSuccess("Пароль изменен");
                ClearPasswordFields();
                LogInfo("Password changed for person: {PersonUid}", currentPerson.Uid);
            }
            else
            {
                SetError("Не удалось изменить пароль. Проверьте текущий пароль.");
            }
        }
        catch (Exception ex)
        {
            SetError("Ошибка при смене пароля", ex);
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    private void PopulateFromPerson(Person person)
    {
        FirstName = person.FirstName;
        LastName = person.LastName;
        MiddleName = person.MiddleName ?? string.Empty;
        Email = person.Email;
        PhoneNumber = person.PhoneNumber ?? string.Empty;
        BirthDate = person.DateOfBirth == DateTime.MinValue ? null : person.DateOfBirth;
        
        // Дополнительная информация
        FullName = $"{person.LastName} {person.FirstName} {person.MiddleName}".Trim();
        CreatedAt = person.CreatedAt;
        LastModifiedAt = person.LastModifiedAt;
    }

    /// <summary>
    /// Очищает поля пароля
    /// </summary>
    private void ClearPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
    }

    #endregion
}
